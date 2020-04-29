namespace RxStore.Entity


type CompletedEntityInfo<'Value> =
    { Stamp: 'Value Result Stamp;
      LatestSuccessfulOption: 'Value Stamp option }

and EntityInfo<'Value> =
    { Stamp: 'Value Result Completion Stamp;
      LatestCompletedOption: 'Value CompletedEntityInfo option }


and Result<'Item> = Result<'Item, ErrorValue>

and ErrorValue = string


module rec EntityInfo =

    [<CompiledName "LatestSuccessfulOption">]
    let latestSuccessfulOption entityInfo =
        entityInfo.LatestCompletedOption
        |> Option.bind (fun latestCompleted -> latestCompleted.LatestSuccessfulOption)




    [<CompiledName "Map">]
    let map mapping entityInfo =
        { Stamp = entityInfo.Stamp |> Stamp.map (Completion.map (Result.map mapping));
          LatestCompletedOption =
            entityInfo.LatestCompletedOption |> Option.map (mapCompletedEntityInfo mapping) }


    [<CompiledName "MapCompletedEntityInfo">]
    let mapCompletedEntityInfo mapping completedEntityInfo =
        { Stamp = completedEntityInfo.Stamp |> Stamp.map (Result.map mapping);
          LatestSuccessfulOption =
            completedEntityInfo.LatestSuccessfulOption |> Option.map (Stamp.map mapping) }




    let private withNext f stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else f ()

    let private optionWithNext ofStamp withStamp stamp entityInfoOption =
        match entityInfoOption with
        | Some entityInfo -> entityInfo |> withStamp stamp
        | None -> ofStamp stamp




    [<CompiledName "OfValue">]
    let ofValue stamp =
        match stamp.Item with
        | Pending -> ofPending (stamp |> Stamp.mapTo ())
        | Completed item -> ofCompleted (stamp |> Stamp.mapTo item)


    [<CompiledName "WithValue">]
    let withValue stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                match stamp.Item with
                | Pending -> entityInfo |> withPending (stamp |> Stamp.mapTo ())
                | Completed item -> entityInfo |> withCompleted (stamp |> Stamp.mapTo item))


    [<CompiledName "OptionWithValue">]
    let optionWithValue stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofValue withValue




    [<CompiledName "OfPending">]
    let ofPending stamp =
        { Stamp = stamp |> Stamp.mapTo Pending;
          LatestCompletedOption = None }


    [<CompiledName "WithPending">]
    let withPending stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext (fun () -> { entityInfo with Stamp = stamp |> Stamp.mapTo Pending })


    [<CompiledName "OptionWithPending">]
    let optionWithPending stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofPending withPending




    [<CompiledName "OfCompleted">]
    let ofCompleted stamp =
        match stamp.Item with
        | Ok item -> ofSuccessful (stamp |> Stamp.mapTo item)
        | Error item -> ofFailed (stamp |> Stamp.mapTo item)


    [<CompiledName "WithCompleted">]
    let withCompleted stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                match stamp.Item with
                | Ok item -> entityInfo |> withSuccessful (stamp |> Stamp.mapTo item)
                | Error item -> entityInfo |> withFailed (stamp |> Stamp.mapTo item))


    [<CompiledName "OptionWithCompleted">]
    let optionWithCompleted stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofCompleted withCompleted



    
    [<CompiledName "OfFailed">]
    let ofFailed stamp =
        { Stamp = stamp |> Stamp.map (Completed << Error);
          LatestCompletedOption =
            Some
                { Stamp = stamp |> Stamp.map Error;
                  LatestSuccessfulOption = None } }


    [<CompiledName "WithFailed">]
    let withFailed stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                { entityInfo with
                    Stamp = stamp |> Stamp.map (Completed << Error);
                    LatestCompletedOption =
                        Some
                            { Stamp = stamp |> Stamp.map Error;
                              LatestSuccessfulOption = entityInfo |> latestSuccessfulOption } })


    [<CompiledName "OptionWithFailed">]
    let optionWithFailed stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofFailed withFailed




    [<CompiledName "OfSuccessful">]
    let ofSuccessful stamp =
        { Stamp = stamp |> Stamp.map (Completed << Ok);
          LatestCompletedOption =
            Some
                { Stamp = stamp |> Stamp.map Ok;
                  LatestSuccessfulOption = Some stamp } }


    [<CompiledName "WithSuccessful">]
    let withSuccessful stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                { entityInfo with
                    Stamp = stamp |> Stamp.map (Completed << Ok);
                    LatestCompletedOption =
                        Some
                            { Stamp = stamp |> Stamp.map Ok;
                              LatestSuccessfulOption = Some stamp } })


    [<CompiledName "OptionWithSuccessful">]
    let optionWithSuccessful stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofSuccessful withSuccessful
    