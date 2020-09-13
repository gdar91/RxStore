namespace RxStore.Entity


type Completion<'Item> =
| Pending
| Completed of 'Item


module Completion =

    [<CompiledName "OfPending">]
    let ofPending = Pending

    [<CompiledName "OfPending">]
    let ofPendingWithWitness<'a> (witness: 'a) = Completion<'a>.Pending


    [<CompiledName "OfCompleted">]
    let ofCompleted item = Completed item


    [<CompiledName "Map">]
    let map mapping completion =
        match completion with
        | Pending -> Pending
        | Completed item -> Completed (mapping item)
