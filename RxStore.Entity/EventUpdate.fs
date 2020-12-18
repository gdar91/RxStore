namespace RxStore


type EventUpdate<'State, 'Event> =
| State of 'State
| Event of 'Event
| Confirmation


module EventUpdate =
    
    type Action =
    | Stay
    | Advance
    | RaiseMismatch
