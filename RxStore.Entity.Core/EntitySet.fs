namespace RxStore.Entity


type EntitySet<'Value> = EntitySet<Key, 'Value>

and EntitySet<'Key, 'Value> when 'Key : comparison =
    EntitySet of Map<'Key, 'Value EntityInfo>


and Key = System.Guid


module EntitySet =

    [<CompiledName("Empty")>]
    let empty<'Key, 'Value when 'Key : comparison> : EntitySet<'Key, 'Value> =
        EntitySet (Map.empty)


    let private mapApplyFuncAsSeq seqFunc map =
        map
        |> Map.toSeq
        |> seqFunc
        |> Map.ofSeq


    [<CompiledName("One")>]
    let one key (EntitySet map) = map |> Map.tryFind key

    [<CompiledName("Many")>]
    let many (EntitySet map) = map


    [<CompiledName("OneCompleted")>]
    let oneCompleted key (EntitySet map) =
        map
        |> Map.tryFind key
        |> Option.bind (fun entityInfo -> entityInfo.LatestCompletedOption)

    let private manyCompletedSeq seq =
        seq
        |> Seq.choose
            (fun (key, entityInfo) ->
                entityInfo.LatestCompletedOption
                |> Option.map (fun latestCompletedOption -> key, latestCompletedOption))

    [<CompiledName("ManyCompleted")>]
    let manyCompleted entitySet = entitySet |> mapApplyFuncAsSeq manyCompletedSeq


    [<CompiledName("OneSuccessful")>]
    let oneSuccessful key entitySet =
        entitySet
        |> oneCompleted key
        |> Option.bind (fun completedEntityInfo -> completedEntityInfo.LatestSuccessfulOption)

    let private manySuccessfulSeq seq =
        seq
        |> manyCompletedSeq
        |> Seq.choose
            (fun (key, completedEntityInfo) ->
                completedEntityInfo.LatestSuccessfulOption
                |> Option.map (fun latestSuccessfulOption -> key, latestSuccessfulOption))

    [<CompiledName("ManySuccessful")>]
    let manySuccessful entitySet = entitySet|> mapApplyFuncAsSeq manySuccessfulSeq


    [<CompiledName("OnePresent")>]
    let onePresent key entitySet =
        entitySet
        |> oneSuccessful key
        |> Option.bind
            (fun stamp ->
                match stamp.Item with
                | Some item -> Some (stamp |> Stamp.mapTo item)
                | None -> None)

    let private manyPresentSeq seq =
        seq
        |> manySuccessfulSeq
        |> Seq.choose
            (fun (key, stamp) ->
                match stamp.Item with
                | Some item -> Some (key, stamp |> Stamp.mapTo item)
                | None -> None)

    [<CompiledName("ManyPresent")>]
    let manyPresent entitySet = entitySet |> mapApplyFuncAsSeq manyPresentSeq




    let private withManyFunc func stamp entitySet =
        stamp.Item
        |> Map.fold
            (fun entitySet key item -> entitySet |> func key (stamp |> Stamp.mapTo item))
            entitySet


    [<CompiledName("WithOne")>]
    let withOne key stamp (EntitySet map) =
        match map |> Map.tryFind key with
        | Some entityInfo -> entityInfo |> EntityInfo.withValue stamp
        | None -> EntityInfo.ofValue stamp
        |> fun entityInfo -> map |> Map.add key entityInfo
        |> EntitySet

    [<CompiledName("WithMany")>]
    let withMany stamp entitySet = entitySet |> withManyFunc withOne stamp


    [<CompiledName("WithOnePending")>]
    let withOnePending key stamp (EntitySet map) =
        match map |> Map.tryFind key with
        | Some entityInfo -> entityInfo |> EntityInfo.withPending stamp
        | None -> EntityInfo.ofPending stamp
        |> fun entityInfo -> map |> Map.add key entityInfo
        |> EntitySet

    [<CompiledName("WithManyPending")>]
    let withManyPending stamp entitySet = entitySet |> withManyFunc withOnePending stamp


    [<CompiledName("WithOneCompleted")>]
    let withOneCompleted key stamp (EntitySet map) =
        match map |> Map.tryFind key with
        | Some entityInfo -> entityInfo |> EntityInfo.withCompleted stamp
        | None -> EntityInfo.ofCompleted stamp
        |> fun entityInfo -> map |> Map.add key entityInfo
        |> EntitySet

    [<CompiledName("WithManyCompleted")>]
    let withManyCompleted stamp entitySet = entitySet |> withManyFunc withOneCompleted stamp


    [<CompiledName("WithOneFailed")>]
    let withOneFailed key stamp (EntitySet map) =
        match map |> Map.tryFind key with
        | Some entityInfo -> entityInfo |> EntityInfo.withFailed stamp
        | None -> EntityInfo.ofFailed stamp
        |> fun entityInfo -> map |> Map.add key entityInfo
        |> EntitySet

    [<CompiledName("WithManyFailed")>]
    let withManyFailed stamp entitySet = entitySet |> withManyFunc withOneFailed stamp


    [<CompiledName("WithOneSuccessful")>]
    let withOneSuccessful key stamp (EntitySet map) =
        match map |> Map.tryFind key with
        | Some entityInfo -> entityInfo |> EntityInfo.withSuccessful stamp
        | None -> EntityInfo.ofSuccessful stamp
        |> fun entityInfo -> map |> Map.add key entityInfo
        |> EntitySet

    [<CompiledName("WithManySuccessful")>]
    let withManySuccessful stamp entitySet = entitySet |> withManyFunc withOneSuccessful stamp


    [<CompiledName("WithOneAbsent")>]
    let withOneAbsent key stamp entitySet =
        entitySet |> withOneSuccessful key (stamp |> Stamp.mapTo None)

    [<CompiledName("WithManyAbsent")>]
    let withManyAbsent stamp entitySet = entitySet |> withManyFunc withOneAbsent stamp


    [<CompiledName("WithOnePresent")>]
    let withOnePresent key stamp entitySet =
        entitySet |> withOneSuccessful key (stamp |> Stamp.map Some)

    [<CompiledName("WithManyPresent")>]
    let withManyPresent stamp entitySet = entitySet |> withManyFunc withOnePresent stamp
