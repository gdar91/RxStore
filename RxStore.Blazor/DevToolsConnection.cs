using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Reflection;
using Microsoft.FSharpLu.Json;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RxStore
{
    internal interface IDevToolsConnection : IDisposable
    {
        void Start();
    }


    internal sealed class DevToolsConnection<TStore> : IDevToolsConnection where TStore : Store
    {
        private readonly IDevToolsConnection inner;


        public DevToolsConnection(IJSRuntime jsRuntime, TStore store)
        {
            inner = (IDevToolsConnection) typeof(DevToolsConnection<,,>)
                .MakeGenericType(new[]
                {
                    typeof(TStore),
                    store.StateType,
                    store.ActionType
                })
                .GetConstructor(new[]
                {
                    typeof(IJSRuntime),
                    typeof(TStore)
                })
                .Invoke(new object[]
                {
                    jsRuntime,
                    store
                });
        }


        public void Start() => inner.Start();

        public void Dispose() => inner.Dispose();
    }


    internal sealed class DevToolsConnection<TStore, TState, TAction> : IDevToolsConnection
        where TStore : Store<TState, TAction>
    {
        private readonly IJSRuntime jsRuntime;

        private readonly IConnectableObservable<bool> observable;


        private IDisposable connection;


        public DevToolsConnection(IJSRuntime jSRuntime, TStore store)
        {
            this.jsRuntime = jSRuntime;

            observable = store.OutStateTransitions
                .Select(next => Observable.FromAsync(cancellationToken => next switch
                {
                    Store<TState, TAction>.StateTransition.Initial { State: var state } =>
                        OnInitialState(state, cancellationToken),
                    
                    Store<TState, TAction>.StateTransition.ByAction { State: var state, Action: var action} =>
                        OnAction(state, action, cancellationToken),
                    
                    _ => Task.FromResult(true)
                }))
                .Concat()
                .Publish();
        }


        public void Start()
        {
            lock (this)
            {
                connection = connection ?? observable.Connect();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                using var resource = connection;

                connection = null;
            }
        }




        private const string JsInteropObjectName = "___RxStore___DevTools___";

        private static readonly string instanceName = typeof(TStore).Name;


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

        private async Task<bool> OnAction(TState state, TAction action, CancellationToken cancellationToken)
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






        static DevToolsConnection()
        {
            new System.ComponentModel.TypeConverter();
            new System.ComponentModel.ArrayConverter();
            // new System.ComponentModel.BaseNumberConverter();
            new System.ComponentModel.BooleanConverter();
            new System.ComponentModel.ByteConverter();
            new System.ComponentModel.CharConverter();
            new System.ComponentModel.CollectionConverter();
            new System.ComponentModel.ComponentConverter(typeof(object));
            new System.ComponentModel.CultureInfoConverter();
            new System.ComponentModel.DateTimeConverter();
            new System.ComponentModel.DecimalConverter();
            new System.ComponentModel.DoubleConverter();
            new System.ComponentModel.EnumConverter(typeof(System.DayOfWeek));
            new System.ComponentModel.ExpandableObjectConverter();
            new System.ComponentModel.Int16Converter();
            new System.ComponentModel.Int32Converter();
            new System.ComponentModel.Int64Converter();
            // new System.ComponentModel.NullableConverter(typeof(int?));
            new System.ComponentModel.SByteConverter();
            new System.ComponentModel.SingleConverter();
            new System.ComponentModel.StringConverter();
            new System.ComponentModel.TimeSpanConverter();
            new System.ComponentModel.UInt16Converter();
            new System.ComponentModel.UInt32Converter();
            new System.ComponentModel.UInt64Converter();
        }
    }
}
