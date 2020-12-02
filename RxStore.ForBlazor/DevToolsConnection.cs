using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Reflection;
using Microsoft.JSInterop;

namespace RxStore
{
    //internal static class DevToolsConnection<TStore, TState, TAction> where TStore : Store<TState, TAction>
    //{
    //    private const string JsInteropObjectName = "___RxStore___DevTools___";

    //    public static IConnectableObservable<Unit> CreateObservable(
    //        IJSRuntime jSRuntime,
    //        TStore store
    //    )
    //    {
    //        var instanceName = typeof(TStore).Name;

    //        return store.OutStateTransitions
    //            .Select(next => Observable.FromAsync(cancellationToken => next switch
    //            {
    //                Store<TState, TAction>.StateTransition.Initial { State: var state } =>
    //                    OnInitialState(state, cancellationToken),

    //                Store<TState, TAction>.StateTransition.ByAction { State: var state, Action: var action } =>
    //                    OnAction(state, action, cancellationToken),

    //                _ => Task.FromResult(true)
    //            }))
    //            .Concat()
    //            .Select(next => Unit.Default)
    //            .IgnoreElements()
    //            .Publish();


    //        async Task<bool> OnInitialState(TState state, CancellationToken cancellationToken)
    //        {
    //            var stateJson = JsonOfState(state);

    //            return await jSRuntime.InvokeAsync<bool>(
    //                $"{JsInteropObjectName}.{nameof(OnInitialState)}",
    //                cancellationToken,
    //                instanceName,
    //                stateJson
    //            );
    //        }

    //        async Task<bool> OnAction(TState state, TAction action, CancellationToken cancellationToken)
    //        {
    //            var actionJson = JsonOfAction(action);
    //            var stateJson = JsonOfState(state);

    //            return await jSRuntime.InvokeAsync<bool>(
    //                $"{JsInteropObjectName}.{nameof(OnAction)}",
    //                cancellationToken,
    //                instanceName,
    //                actionJson,
    //                stateJson
    //            );
    //        }
    //    }




    //    private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    //    {
    //        Converters =
    //        {
    //            new CompactUnionJsonConverter(false, true)
    //        }
    //    };

    //    private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);


    //    public static string JsonOfState(TState state) =>
    //        JsonConvert.SerializeObject(state, SerializerSettings);

    //    public static string JsonOfAction(TAction action)
    //    {
    //        var (typeNames, type, value) = TypeNameHierarchyOfAction(
    //            new List<string>(),
    //            action?.GetType() ?? typeof(TAction),
    //            action
    //        );

    //        var typeName =
    //            typeNames.Count == 0
    //                ? type.Name // TODO non-object single-value actions (int)
    //                : string.Join(":", typeNames);

    //        var valueJToken = value == null
    //            ? JValue.CreateNull()
    //            : JToken.FromObject(value, Serializer);

    //        var valueJObject = valueJToken.Type switch 
    //        {
    //            JTokenType.Null => new JObject(),
    //            JTokenType.Object => (JObject) valueJToken,
    //            _ => JObject.FromObject(new { Item = value }, Serializer)
    //        };

    //        var actionJObject = new JObject() { ["type"] = typeName };

    //        foreach (var property in valueJObject.Properties())
    //        {
    //            actionJObject.Add(property.Name, property.Value);
    //        }

    //        var actionJson = JsonConvert.SerializeObject(actionJObject, SerializerSettings);

    //        return actionJson;
    //    }

    //    public static (List<string>, Type, object) TypeNameHierarchyOfAction(
    //        List<string> typeNames,
    //        Type type,
    //        object value
    //    )
    //    {
    //        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

    //        if (
    //            !FSharpType.IsUnion(type, bindingFlags) ||
    //            type.Namespace.StartsWith(nameof(Microsoft))
    //        )
    //        {
    //            return (typeNames, type, value);
    //        }

    //        var tuple = FSharpValue.GetUnionFields(value, type, bindingFlags);
    //        var unionCaseInfo = tuple.Item1;
    //        var values = tuple.Item2;
    //        var fields = unionCaseInfo.GetFields();

    //        var currentTypeNames = new List<string>(typeNames)
    //        {
    //            unionCaseInfo.Name
    //        };

    //        return values.Length switch
    //        {
                
    //            0 => (currentTypeNames, typeof(object), new object()),

    //            1 when fields.Single().Name == "Item" => TypeNameHierarchyOfAction(
    //                currentTypeNames,
    //                fields.Single().PropertyType,
    //                values.Single()
    //            ),

    //            _ => (
    //                currentTypeNames,
    //                typeof(IDictionary<string, object>),
    //                Enumerable
    //                    .Zip(
    //                        unionCaseInfo.GetFields().Select(field => field.Name),
    //                        values,
    //                        (First, Second) => (First, Second)
    //                    )
    //                    .ToDictionary(pair => pair.First, pair => pair.Second)
    //            )

    //        };
    //    }
    //}
}
