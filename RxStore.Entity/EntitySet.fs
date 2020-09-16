namespace RxStore.Entity


type EntitySet<'TEntity> = EntitySet<Key, 'TEntity, EntityError>

and EntitySet<'TKey, 'TEntity, 'TEntityError> when 'TKey : comparison =
    EntitySet of Map<'TKey, EntityInfo<'TEntity, 'TEntityError>>


and Key = System.Guid




module EntitySet =

    [<CompiledName "Empty">]
    let empty<'TKey, 'TValue, 'TError when 'TKey : comparison> : EntitySet<'TKey, 'TValue, 'TError> =
        EntitySet (Map.empty)








    let private mapApplyFuncAsSeq seqFunc map =
        map
        |> Map.toSeq
        |> seqFunc
        |> Map.ofSeq

    let private entitySetApplyFuncAsSeq seqFunc (EntitySet map) =
        map
        |> mapApplyFuncAsSeq
            (seqFunc << Seq.map (fun (key, entityInfo) -> key, entityInfo.Stamp))




    [<CompiledName "One">]
    let one key (EntitySet map) = map |> Map.tryFind key

    [<CompiledName "Many">]
    let many (EntitySet map) = map




    [<CompiledName "OnePending">]
    let onePending key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.pendingOption

    let private manyPendingSeq seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.pendingOptionOfValue
                |> Option.map (fun stamp -> key, stamp))

    [<CompiledName "ManyPendingOfValues">]
    let manyPendingOfValues map = map |> mapApplyFuncAsSeq manyPendingSeq

    [<CompiledName "ManyPending">]
    let manyPending entitySet = entitySet |> entitySetApplyFuncAsSeq manyPendingSeq




    [<CompiledName "OneCompleted">]
    let oneCompleted key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.completedOption

    let private manyCompletedSeq seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.completedOptionOfValue
                |> Option.map (fun stamp -> key, stamp))

    [<CompiledName "ManyCompletedOfValues">]
    let manyCompletedOfValues map = map |> mapApplyFuncAsSeq manyCompletedSeq

    [<CompiledName "ManyCompleted">]
    let manyCompleted entitySet = entitySet |> entitySetApplyFuncAsSeq manyCompletedSeq

    [<CompiledName "OneLatestCompleted">]
    let oneLatestCompleted key entitySet =
        entitySet
        |> one key
        |> Option.bind (fun entityInfo -> entityInfo.LatestCompletedOption)

    [<CompiledName "ManyLatestCompleted">]
    let manyLatestCompleted (EntitySet map) =
        map
        |> mapApplyFuncAsSeq
            (Seq.choose
                (fun (key, entityInfo) ->
                    entityInfo.LatestCompletedOption
                    |> Option.map (fun value -> key, value)))




    [<CompiledName "OneFailed">]
    let oneFailed key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.failedOption

    let private manyFailedSeqOfCompleted seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.failedOptionOfCompletedValue
                |> Option.map (fun stamp -> key, stamp))

    let private manyFailedSeq seq =
        seq
        |> manyCompletedSeq
        |> manyFailedSeqOfCompleted

    [<CompiledName "ManyFailedOfCompletedValues">]
    let manyFailedOfCompletedValues map = map |> mapApplyFuncAsSeq manyFailedSeqOfCompleted

    [<CompiledName "ManyFailed">]
    let manyFailed entitySet = entitySet |> entitySetApplyFuncAsSeq manyFailedSeq




    [<CompiledName "OneSuccessful">]
    let oneSuccessful key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.successfulOption

    let private manySuccessfulSeqOfCompleted seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.successfulOptionOfCompletedValue
                |> Option.map (fun stamp -> key, stamp))

    let private manySuccessfulSeq seq =
        seq
        |> manyCompletedSeq
        |> manySuccessfulSeqOfCompleted

    [<CompiledName "ManySuccessfulOfCompletedValues">]
    let manySuccessfulOfCompletedValues map = map |> mapApplyFuncAsSeq manySuccessfulSeqOfCompleted

    [<CompiledName "ManySuccessful">]
    let manySuccessful entitySet = entitySet |> entitySetApplyFuncAsSeq manySuccessfulSeq

    [<CompiledName "OneLatestSuccessful">]
    let oneLatestSuccessful key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.latestSuccessfulOption

    [<CompiledName "ManyLatestSuccessfulOfCompletedValues">]
    let manyLatestSuccessfulOfCompletedValues map =
        map
        |> mapApplyFuncAsSeq
            (Seq.choose
                (fun (key, completedEntityInfo) ->
                    completedEntityInfo.LatestSuccessfulOption
                    |> Option.map (fun value -> key, value)))

    [<CompiledName "ManyLatestSuccessful">]
    let manyLatestSuccessful (EntitySet map) =
        map
        |> mapApplyFuncAsSeq
            (Seq.choose
                (fun (key, entityInfo) ->
                    entityInfo
                    |> EntityInfo.latestSuccessfulOption
                    |> Option.map (fun value -> key, value)))




    [<CompiledName "OneAbsent">]
    let oneAbsent key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.absentOption

    let private manyAbsentSeqOfSuccessful seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.absentOptionOfSuccessfulValue
                |> Option.map (fun stamp -> key, stamp))

    let private manyAbsentSeq seq =
        seq
        |> manySuccessfulSeq
        |> manyAbsentSeqOfSuccessful

    [<CompiledName "ManyAbsentOfSuccessfulValues">]
    let manyAbsentOfSuccessfulValues map = map |> mapApplyFuncAsSeq manyAbsentSeqOfSuccessful

    [<CompiledName "ManyAbsent">]
    let manyAbsent entitySet = entitySet |> entitySetApplyFuncAsSeq manyAbsentSeq




    [<CompiledName "OnePresent">]
    let onePresent key entitySet =
        entitySet
        |> one key
        |> Option.bind EntityInfo.presentOption

    let private manyPresentSeqOfSuccessful seq =
        seq
        |> Seq.choose
            (fun (key, stamp) ->
                stamp
                |> EntityInfo.presentOptionOfSuccessfulValue
                |> Option.map (fun stamp -> key, stamp))

    let private manyPresentSeq seq =
        seq
        |> manySuccessfulSeq
        |> manyPresentSeqOfSuccessful

    [<CompiledName "ManyPresentOfSuccessfulValues">]
    let manyPresentOfSuccessfulValues map = map |> mapApplyFuncAsSeq manyPresentSeqOfSuccessful

    [<CompiledName "ManyPresent">]
    let manyPresent entitySet = entitySet |> entitySetApplyFuncAsSeq manyPresentSeq




    



    let private withManyFunc func stamp (EntitySet map) =
        stamp.Item
        |> Map.fold
            (fun map key item -> map |> func key (stamp |> Stamp.mapTo item))
            map
        |> EntitySet




    let withEntityInfo key entityInfo (EntitySet map) =
        map
        |> Map.add key entityInfo
        |> EntitySet

    let withoutEntityInfo key (EntitySet map) =
        map
        |> Map.remove key
        |> EntitySet

    let withEntityInfoOption key entityInfoOption entitySet =
        match entityInfoOption with
        | Some entityInfo -> withEntityInfo key entityInfo
        | None -> withoutEntityInfo key
        <| entitySet




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

    [<CompiledName "WithoutMany">]
    let withoutMany keys (EntitySet map) =
        keys
        |> Seq.fold (fun map key -> map |> mapWithoutOne key) map
        |> EntitySet


    

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
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithAbsent stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOneAbsent">]
    let withOneAbsent key stamp (EntitySet map) =
        map
        |> mapWithOneAbsent key stamp
        |> EntitySet

    [<CompiledName "WithManyAbsent">]
    let withManyAbsent stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOneAbsent


    

    let private mapWithOnePresent key stamp map =
        map
        |> Map.tryFind key
        |> EntityInfo.optionWithPresent stamp
        |> fun entityInfo -> map |> Map.add key entityInfo

    [<CompiledName "WithOnePresent">]
    let withOnePresent key stamp (EntitySet map) =
        map
        |> mapWithOnePresent key stamp
        |> EntitySet

    [<CompiledName "WithManyPresent">]
    let withManyPresent stamp entitySet =
        (stamp, entitySet) ||> withManyFunc mapWithOnePresent
