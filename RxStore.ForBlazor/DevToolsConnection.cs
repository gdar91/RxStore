using Microsoft.FSharp.Reflection;
using Microsoft.FSharpLu.Json;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RxStore
{
    internal static class DevToolsConnection<TEvent, TState>
    {
        private const string JsInteropObjectName = "__RxStore_ForBlazor_DevTools__";


        public static IObservable<Unit> CreateObservable(
            IJSRuntime jSRuntime,
            Store<TEvent, TState> store,
            string instanceName
        )
        {
            return store.StateTransitions
                .Select(next => Observable.FromAsync(cancellationToken => next switch
                {
                    Store<TEvent, TState>.StateTransition.Initial { State: var state } =>
                        OnInitial(state, cancellationToken),

                    Store<TEvent, TState>.StateTransition.ByEvent { State: var state, Event: var @event } =>
                        OnEvent(@event, state, cancellationToken),

                    _ => Task.FromResult(true)
                }))
                .Concat()
                .Select(next => Unit.Default)
                .IgnoreElements()
                .Publish()
                .RefCount();


            async Task<bool> OnInitial(TState state, CancellationToken cancellationToken)
            {
                var stateJson = JsonOfState(state);

                return await jSRuntime.InvokeAsync<bool>(
                    $"{JsInteropObjectName}.{nameof(OnInitial)}",
                    cancellationToken,
                    instanceName,
                    stateJson
                );
            }

            async Task<bool> OnEvent(TEvent @event, TState state, CancellationToken cancellationToken)
            {
                var eventJson = JsonOfEvent(@event);
                var stateJson = JsonOfState(state);

                return await jSRuntime.InvokeAsync<bool>(
                    $"{JsInteropObjectName}.{nameof(OnEvent)}",
                    cancellationToken,
                    instanceName,
                    eventJson,
                    stateJson
                );
            }
        }




        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new CompactUnionJsonConverter(true, false)
            }
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);


        public static string JsonOfState(TState state) =>
            JsonConvert.SerializeObject(state, SerializerSettings);

        public static string JsonOfEvent(TEvent @event)
        {
            var (typeNames, type, value) = TypeNameHierarchyOfEvent(
                new List<string>(),
                @event?.GetType() ?? typeof(TEvent),
                @event
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
                JTokenType.Object => (JObject)valueJToken,
                _ => JObject.FromObject(new { Item = value }, Serializer)
            };

            var eventJObject = new JObject() { ["type"] = typeName };

            foreach (var property in valueJObject.Properties())
            {
                eventJObject.Add(property.Name, property.Value);
            }

            var eventJson = JsonConvert.SerializeObject(eventJObject, SerializerSettings);

            return eventJson;
        }

        public static (List<string>, Type, object) TypeNameHierarchyOfEvent(
            List<string> typeNames,
            Type type,
            object value
        )
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

            if (
                !FSharpType.IsUnion(type, bindingFlags) ||
                type.Namespace.StartsWith(nameof(Microsoft))
            )
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

                1 when fields.Single().Name == "Item" => TypeNameHierarchyOfEvent(
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
