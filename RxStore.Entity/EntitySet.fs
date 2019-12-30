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

    let private withMany func { Time = time; Item = map } entitySet =
        map
        |> Map.fold
            (fun entitySet key item -> entitySet |> func key { Time = time; Item = item })
            entitySet        


    let without key entitySet = entitySet |> Map.remove key

    let withoutMany keys entitySet =
        keys |> Set.fold (fun entitySet key -> entitySet |> without key) entitySet


    let withPending key stamp entitySet =
        withStamp key (stamp |> Stamp.mapTo Pending) entitySet

    let withManyPending stamp entitySet =
        withMany withPending stamp entitySet    


    let withCompleted key stamp entitySet =
        withStamp key (stamp |> Stamp.map Completed) entitySet

    let withManyCompleted stamp entitySet =
        withMany withCompleted stamp entitySet    


    let withFailed key stamp entitySet =
        withCompleted key (stamp |> Stamp.map Error) entitySet

    let withManyFailed stamp entitySet =
        withMany withFailed stamp entitySet    


    let withSuccessful key stamp entitySet =
        withCompleted key (stamp |> Stamp.map Ok) entitySet

    let withManySuccessful stamp entitySet =
        withMany withSuccessful stamp entitySet


    let withAbsent key stamp entitySet =
        withSuccessful key (stamp |> Stamp.mapTo None) entitySet

    let withManyAbsent stamp entitySet =
        withMany withAbsent stamp entitySet    


    let withPresent key stamp entitySet =
        withSuccessful key (stamp |> Stamp.map Some) entitySet

    let withManyPresent stamp entitySet =
        withMany withPresent stamp entitySet
