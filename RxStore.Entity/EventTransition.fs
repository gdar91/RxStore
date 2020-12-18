namespace RxStore


type EventTransition<'State, 'Event> =
    { State: 'State;
      Event: 'Event }


module EventTransition =

    [<CompiledName("OfValues")>]
    let ofValues state event =
        { State = state;
          Event = event }

    [<CompiledName("AsUpdateState")>]
    let ofTransitionState<'State, 'Event>
        (transition: EventTransition<'State, 'Event>)
        : EventUpdate<'State, 'Event> =
            State transition.State

    [<CompiledName("AsUpdateEvent")>]
    let ofTransitionEvent<'State, 'Event>
        (transition: EventTransition<'State, 'Event>)
        : EventUpdate<'State, 'Event> =
            Event transition.Event

    [<CompiledName("AsUpdateConfirmation")>]
    let ofTransitionConfirmation<'State, 'Event>
        (transition: EventTransition<'State, 'Event>)
        : EventUpdate<'State, 'Event> =
            Confirmation
