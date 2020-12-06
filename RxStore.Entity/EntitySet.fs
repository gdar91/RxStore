namespace RxStore


type EntitySet<'TEntity> = EntitySet<Key, 'TEntity, EntityError>

and EntitySet<'TKey, 'TEntity, 'TEntityError> when 'TKey : comparison =
    EntitySet of Map<'TKey, EntityInfo<'TEntity, 'TEntityError>>


and Key = System.Guid




module EntitySet =

    [<CompiledName "Empty">]
    let empty<'TKey, 'TValue, 'TError when 'TKey : comparison> : EntitySet<'TKey, 'TValue, 'TError> =
        EntitySet (Map.empty)




    [<CompiledName "One">]
    let one key (EntitySet map) =
        map
        |> Map.tryFind key
        |> Option.defaultValue EntityInfo.empty


    [<CompiledName "Many">]
    let many (EntitySet map) = map


    [<CompiledName "OneResult">]
    let oneResult key entitySet =
        entitySet
        |> one key
        |> fun entityInfo -> entityInfo.LatestResult


    [<CompiledName "ManyResults">]
    let manyResults (EntitySet map) =
        map
        |> Map.toSeq
        |> Seq.choose
            (fun (key, value) ->
                value.LatestResult
                |> Option.map (fun value -> key, value))
        |> Map.ofSeq


    [<CompiledName "OneItem">]
    let oneItem key entitySet =
        entitySet
        |> oneResult key
        |> Option.bind (fun entityInfoResult -> entityInfoResult.LatestItem)


    [<CompiledName "ManyItems">]
    let manyItems (EntitySet map) =
        map
        |> Map.toSeq
        |> Seq.choose
            (fun (key, value) ->
                value.LatestResult
                |> Option.bind (fun latestResult -> latestResult.LatestItem)
                |> Option.map (fun value -> key, value))
        |> Map.ofSeq




    [<CompiledName "WithOne">]
    let withOne key entityInfo (EntitySet map) =
        map
        |> Map.add key entityInfo
        |> EntitySet


    [<CompiledName "WithoutOne">]
    let withoutOne key entitySet =
        entitySet |> withOne key EntityInfo.empty


    [<CompiledName "MapOne">]
    let mapOne key mapping entitySet =
        entitySet
        |> one key
        |> mapping
        |> fun entityInfo -> entitySet |> withOne key entityInfo


    [<CompiledName "WithOneStatus">]
    let withOneStatus key status entitySet =
        entitySet
        |> mapOne key (EntityInfo.withStatus status)


    [<CompiledName "WithOneOfflineStatus">]
    let withOneOfflineStatus key entitySet =
        entitySet
        |> mapOne key EntityInfo.withOfflineStatus


    [<CompiledName "WithOneOnlineStatus">]
    let withOneOnlineStatus key entitySet =
        entitySet
        |> mapOne key EntityInfo.withOnlineStatus


    [<CompiledName "WithOneResult">]
    let withOneResult key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withResult stamp)


    [<CompiledName "WithOneCompletingResult">]
    let withOneCompletingResult key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withCompletingResult stamp)


    [<CompiledName "WithOneError">]
    let withOneError key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withError stamp)


    [<CompiledName "WithOneCompletingError">]
    let withOneCompletingError key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withCompletingError stamp)


    [<CompiledName "WithOneOk">]
    let withOneOk key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withOk stamp)


    [<CompiledName "WithOneCompletingOk">]
    let withOneCompletingOk key stamp entitySet =
        entitySet
        |> mapOne key (EntityInfo.withCompletingOk stamp)
