namespace RxStore.Entity


type Completion<'Item> =
| Pending
| Completed of 'Item


module Completion =

    let ofPending = Pending

    let ofCompleted item = Completed item
