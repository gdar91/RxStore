namespace RxStore.Entity


type EntitySet<'Value> = EntitySet<Key, 'Value>

and EntitySet<'Key, 'Value> when 'Key : comparison =
    Map<'Key, 'Value option Result Completion Stamp>


and Key = System.Guid

and Result<'Item> = Result<'Item, ErrorItem>

and ErrorItem = string


module EntitySet =


    let empty<'Value> = EntitySet<'Value> []

    let empty2<'Key, 'Value when 'Key : comparison> = EntitySet<'Key, 'Value> []


    let resultOfOk<'a> (item: 'a) = Result<'a, ErrorItem>.Ok item

    let resultOfError<'a> item = Result<'a, ErrorItem>.Error item


    let at key map = map |> Map.tryFind key


    let private mapApplyFuncAsSeq seqFunc map =
        map
        |> Map.toSeq
        |> seqFunc
        |> Map.ofSeq    


    let private pendingItemsSeq entitySeq =
        entitySeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Pending -> Some (key, stamp |> Stamp.mapTo ())
                | Completed _ -> None)

    let pendingItems entitySet =
        entitySet |> mapApplyFuncAsSeq pendingItemsSeq


    let private completedItemsSeq entitySeq =
        entitySeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Pending -> None
                | Completed item -> Some (key, stamp |> Stamp.mapTo item))

    let completedItems entitySet =
        entitySet |> mapApplyFuncAsSeq completedItemsSeq


    let private failedItemsSeq entitySeq =
        entitySeq
        |> completedItemsSeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Ok _ -> None
                | Error item -> Some (key, stamp |> Stamp.mapTo item))

    let failedItems entitySet =
        entitySet |> mapApplyFuncAsSeq failedItemsSeq


    let private successfulItemsSeq entitySeq =
        entitySeq
        |> completedItemsSeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Ok item -> Some (key, stamp |> Stamp.mapTo item)
                | Error _ -> None)

    let successfulItems entitySet =
        entitySet |> mapApplyFuncAsSeq successfulItemsSeq


    let private absentItemsSeq entitySeq =
        entitySeq
        |> successfulItemsSeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Some _ -> None
                | None -> Some (key, stamp |> Stamp.mapTo ()))

    let absentItems entitySet =
        entitySet |> mapApplyFuncAsSeq absentItemsSeq


    let private presentItemsSeq entitySeq =
        entitySeq
        |> successfulItemsSeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Some item -> Some (key, stamp |> Stamp.mapTo item)
                | None -> None)

    let presentItems entitySet =
        entitySet |> mapApplyFuncAsSeq presentItemsSeq




    let withStamp key stamp entitySet =
        match entitySet |> Map.tryFind key with
        | Some oldStamp when oldStamp.Time >= stamp.Time -> entitySet
        | Some _
        | None -> entitySet |> Map.add key stamp

    let without key entitySet = entitySet |> Map.remove key

    let withPending key stamp entitySet =
        withStamp key (stamp |> Stamp.mapTo Pending) entitySet

    let withCompleted key stamp entitySet =
        withStamp key (stamp |> Stamp.map Completed) entitySet    

    let withFailed key stamp entitySet =
        withCompleted key (stamp |> Stamp.map Error) entitySet

    let withSuccessful key stamp entitySet =
        withCompleted key (stamp |> Stamp.map Ok) entitySet
    
    let withAbsent key stamp entitySet =
        withSuccessful key (stamp |> Stamp.mapTo None) entitySet

    let withPresent key stamp entitySet =
        withSuccessful key (stamp |> Stamp.map Some) entitySet

    let withManyPresent { Time = time; Item = map } entitySet =
        map
        |> Map.fold
            (fun entitySet key data ->
                entitySet
                |> withPresent key { Time = time; Item = data })
            entitySet
