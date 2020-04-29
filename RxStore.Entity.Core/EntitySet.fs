namespace RxStore.Entity


type EntitySet<'Value> = EntitySet<Key, 'Value>

and EntitySet<'Key, 'Value> when 'Key : comparison =
    EntitySet of Map<'Key, 'Value EntityInfo>


and Key = System.Guid


module EntitySet =

    [<CompiledName "Empty">]
    let empty<'Key, 'Value when 'Key : comparison> : EntitySet<'Key, 'Value> =
        EntitySet (Map.empty)


    let private mapApplyFuncAsSeq seqFunc map =
        map
        |> Map.toSeq
        |> seqFunc
        |> Map.ofSeq


    [<CompiledName "One">]
    let one key (EntitySet map) = map |> Map.tryFind key

    [<CompiledName "Many">]
    let many (EntitySet map) = map


    [<CompiledName "OneCompleted">]
    let oneCompleted key entitySet =
        entitySet
        |> one key
        |> Option.bind (fun entityInfo -> entityInfo.LatestCompletedOption)

    let private manyCompletedSeq seq =
        seq
        |> Seq.choose
            (fun (key, entityInfo) ->
                entityInfo.LatestCompletedOption
                |> Option.map (fun completedEntityInfo -> key, completedEntityInfo))

    [<CompiledName "ManyCompleted">]
    let manyCompleted entitySet = entitySet |> mapApplyFuncAsSeq manyCompletedSeq


    [<CompiledName "OneSuccessful">]
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
                |> Option.map (fun stamp -> key, stamp))

    [<CompiledName "ManySuccessful">]
    let manySuccessful entitySet = entitySet|> mapApplyFuncAsSeq manySuccessfulSeq


    [<CompiledName "OnePresent">]
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

    [<CompiledName "ManyPresent">]
    let manyPresent entitySet = entitySet |> mapApplyFuncAsSeq manyPresentSeq




    let private withManyFunc func stamp (EntitySet map) =
        stamp.Item
        |> Map.fold
            (fun map key item -> map |> func key (stamp |> Stamp.mapTo item))
            map


    let private mapWithOne key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithValue stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOne">]
    let withOne key stamp (EntitySet map) =
        map
        |> mapWithOne key stamp
        |> EntitySet

    [<CompiledName "WithMany">]
    let withMany stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOne


    let private mapWithoutOne key map = map |> Map.remove key

    [<CompiledName "WithoutOne">]
    let withoutOne key (EntitySet map) =
        map
        |> mapWithoutOne key
        |> EntitySet

    let withoutMany keys (EntitySet map) =
        keys |> Seq.fold (fun map key -> map |> mapWithoutOne key) map


    let private mapWithOnePending key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithPending stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOnePending">]
    let withOnePending key stamp (EntitySet map) =
        map
        |> mapWithOnePending key stamp
        |> EntitySet

    [<CompiledName "WithManyPending">]
    let withManyPending stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOnePending


    let private mapWithOneCompleted key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithCompleted stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOneCompleted">]
    let withOneCompleted key stamp (EntitySet map) =
        map
        |> mapWithOneCompleted key stamp
        |> EntitySet

    [<CompiledName "WithManyCompleted">]
    let withManyCompleted stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOneCompleted


    let private mapWithOneFailed key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithFailed stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOneFailed">]
    let withOneFailed key stamp (EntitySet map) =
        map
        |> mapWithOneFailed key stamp
        |> EntitySet

    [<CompiledName "WithManyFailed">]
    let withManyFailed stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOneFailed


    let private mapWithOneSuccessful key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithSuccessful stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOneSuccessful">]
    let withOneSuccessful key stamp (EntitySet map) =
        map
        |> mapWithOneSuccessful key stamp
        |> EntitySet

    [<CompiledName "WithManySuccessful">]
    let withManySuccessful stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOneSuccessful


    let private mapWithOneAbsent key stamp map =
        map |> mapWithOneSuccessful key (stamp |> Stamp.mapTo None)

    [<CompiledName "WithOneAbsent">]
    let withOneAbsent key stamp (EntitySet map) =
        map
        |> mapWithOneAbsent key stamp
        |> EntitySet

    [<CompiledName "WithManyAbsent">]
    let withManyAbsent stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOneAbsent


    let private mapWithOnePresent key stamp map =
        map |> mapWithOneSuccessful key (stamp |> Stamp.map Some)

    [<CompiledName "WithOnePresent">]
    let withOnePresent key stamp (EntitySet map) =
        map
        |> mapWithOnePresent key stamp
        |> EntitySet

    [<CompiledName "WithManyPresent">]
    let withManyPresent stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOnePresent
