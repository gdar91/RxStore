using Microsoft.FSharp.Reflection;
using Microsoft.FSharpLu.Json;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RxStore.DevTools
{
    public sealed class DevTools<TState, TAction> : IEffects<TState, TAction>
    {
        private readonly Store<TState, TAction> store;

        private readonly IJSRuntime jsRuntime;


        public DevTools(Store<TState, TAction> store, IJSRuntime jsRuntime)
        {
            this.store = store;
            this.jsRuntime = jsRuntime;
        }


        public IEnumerable<IObservable<TAction>> GetEffects(IObservable<TAction> actions)
        {

            yield return store.ActionStates
                .Select(tuple => Observable.FromAsync(cancellationToken =>
                    OnAction(tuple.action, tuple.state, cancellationToken)
                ))
                .StartWith(Observable.FromAsync(cancellationToken =>
                    OnInitialState(store.initialState, cancellationToken)
                ))
                .Concat()
                .TakeWhile(success => success)
                .IgnoreElementsAs<TAction>();

        }




        private const string JsInteropObjectName = "___RxStore___DevTools___";
        
        private static readonly string instanceName = $"{typeof(TState).Name}, {typeof(TAction).Name}";


        private async Task<bool> OnInitialState(TState state, CancellationToken cancellationToken)
        {
            var stateJson = JsonOfState(state);

            return await jsRuntime.InvokeAsync<bool>(
                $"{JsInteropObjectName}.{nameof(OnInitialState)}",
                cancellationToken,
                instanceName,
                stateJson
            );
        }

        private async Task<bool> OnAction(TAction action, TState state, CancellationToken cancellationToken)
        {
            var actionJson = JsonOfAction(action);
            var stateJson = JsonOfState(state);

            return await jsRuntime.InvokeAsync<bool>(
                $"{JsInteropObjectName}.{nameof(OnAction)}",
                cancellationToken,
                instanceName,
                actionJson,
                stateJson
            );
        }




        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new CompactUnionJsonConverter(false, true)
            }
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);

        private static string JsonOfState(TState state)
        {
            return JsonConvert.SerializeObject(state, SerializerSettings);
        }

        private static string JsonOfAction(TAction action)
        {
            var (typeNames, type, value) = TypeNameHierarchyOfAction(
                new List<string>(),
                action?.GetType() ?? typeof(TAction),
                action
            );

            var typeName =
                typeNames.Count == 0
                    ? type.Name // TODO non-object single-value actions (int)
                    : string.Join(":", typeNames);

            var valueJToken = value == null
                ? JValue.CreateNull()
                : JToken.FromObject(value, Serializer);

            var valueJObject = valueJToken.Type switch 
            {
                JTokenType.Null => new JObject(),
                JTokenType.Object => (JObject) valueJToken,
                _ => JObject.FromObject(new { Item = value }, Serializer)
            };

            var actionJObject = new JObject() { ["type"] = typeName };

            foreach (var property in valueJObject.Properties())
            {
                actionJObject.Add(property.Name, property.Value);
            }

            var actionJson = JsonConvert.SerializeObject(actionJObject, SerializerSettings);

            return actionJson;
        }

        private static (List<string>, Type, object) TypeNameHierarchyOfAction(
            List<string> typeNames,
            Type type,
            object value
        )
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

            if (!FSharpType.IsUnion(type, bindingFlags) || type.Namespace.StartsWith(nameof(Microsoft)))
            {
                return (typeNames, type, value);
            }

            var tuple = FSharpValue.GetUnionFields(value, type, bindingFlags);
            var unionCaseInfo = tuple.Item1;
            var values = tuple.Item2;
            var fields = unionCaseInfo.GetFields();

            var currentTypeNames = new List<string>(typeNames)
            {
                unionCaseInfo.Name
            };

            return values.Length switch
            {
                
                0 => (currentTypeNames, typeof(object), new object()),

                1 when fields.Single().Name == "Item" => TypeNameHierarchyOfAction(
                    currentTypeNames,
                    fields.Single().PropertyType,
                    values.Single()
                ),

                _ => (
                    currentTypeNames,
                    typeof(IDictionary<string, object>),
                    Enumerable
                        .Zip(
                            unionCaseInfo.GetFields().Select(field => field.Name),
                            values,
                            (First, Second) => (First, Second)
                        )
                        .ToDictionary(pair => pair.First, pair => pair.Second)
                )

            };
        }
    }
}
