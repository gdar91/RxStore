namespace RxStore.Entity


type CompletedEntityInfo<'Value> =
    { Stamp: 'Value Result Stamp;
      LatestSuccessfulOption: 'Value Stamp option }

and EntityInfo<'Value> =
    { Stamp: 'Value Result Completion Stamp;
      LatestCompletedOption: 'Value CompletedEntityInfo option }


and Result<'Item> = Result<'Item, ErrorValue>

and ErrorValue = string


module EntityInfo =
    
    [<CompiledName "MapCompletedEntityInfo">]
    let mapCompletedEntityInfo mapping completedEntityInfo =
        let forTypeInference = completedEntityInfo.LatestSuccessfulOption
        { Stamp = completedEntityInfo.Stamp |> Stamp.map (Result.map mapping);
          LatestSuccessfulOption =
            completedEntityInfo.LatestSuccessfulOption |> Option.map (Stamp.map mapping) }

    [<CompiledName "Map">]
    let map mapping entityInfo =
        { Stamp = entityInfo.Stamp |> Stamp.map (Completion.map (Result.map mapping));
          LatestCompletedOption =
            entityInfo.LatestCompletedOption |> Option.map (mapCompletedEntityInfo mapping) }

    [<CompiledName "MapOption">]
    let mapOption mapping entityInfoOption =
        entityInfoOption |> Option.map (map mapping)




    [<CompiledName "LatestSuccessfulOption">]
    let latestSuccessfulOption entityInfo =
        entityInfo.LatestCompletedOption
        |> Option.bind (fun latestCompleted -> latestCompleted.LatestSuccessfulOption)








    [<CompiledName "PendingOptionOfValue">]
    let pendingOptionOfValue<'a> (stamp: 'a Result Completion Stamp) =
        match stamp.Item with
        | Pending -> Some (stamp |> Stamp.mapTo ())
        | Completed _ -> None

    [<CompiledName "PendingOption">]
    let pendingOption entityInfo =
        entityInfo.Stamp
        |> pendingOptionOfValue




    [<CompiledName "CompletedOptionOfValue">]
    let completedOptionOfValue<'a> (stamp: 'a Result Completion Stamp) =
        match stamp.Item with
        | Pending -> None
        | Completed result -> Some (stamp |> Stamp.mapTo result)

    [<CompiledName "CompletedOption">]
    let completedOption entityInfo =
        entityInfo.Stamp
        |> completedOptionOfValue




    [<CompiledName "FailedOptionOfCompletedValue">]
    let failedOptionOfCompletedValue<'a> (stamp: 'a Result Stamp) =
        match stamp.Item with
        | Ok _ -> None
        | Error value -> Some (stamp |> Stamp.mapTo value)

    [<CompiledName "FailedOption">]
    let failedOption entityInfo =
        entityInfo
        |> completedOption
        |> Option.bind failedOptionOfCompletedValue




    [<CompiledName "SuccessfulOptionOfCompletedValue">]
    let successfulOptionOfCompletedValue<'a> (stamp: 'a Result Stamp) =
        match stamp.Item with
        | Ok value -> Some (stamp |> Stamp.mapTo value)
        | Error _ -> None

    [<CompiledName "SuccessfulOption">]
    let successfulOption entityInfo =
        entityInfo
        |> completedOption
        |> Option.bind successfulOptionOfCompletedValue




    [<CompiledName "AbsentOptionOfSuccessfulValue">]
    let absentOptionOfSuccessfulValue stamp =
        match stamp.Item with
        | Some _ -> None
        | None -> Some (stamp |> Stamp.mapTo ())

    [<CompiledName "AbsentOption">]
    let absentOption entityInfo =
        entityInfo
        |> successfulOption
        |> Option.bind absentOptionOfSuccessfulValue




    [<CompiledName "PresentOptionOfSuccessfulValue">]
    let presentOptionOfSuccessfulValue stamp =
        match stamp.Item with
        | Some value -> Some (stamp |> Stamp.mapTo value)
        | None -> None

    [<CompiledName "PresentOption">]
    let presentOption entityInfo =
        entityInfo
        |> successfulOption
        |> Option.bind presentOptionOfSuccessfulValue








    let private withNext f stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else f ()

    let private optionWithNext ofStamp withStamp stamp entityInfoOption =
        match entityInfoOption with
        | Some entityInfo -> entityInfo |> withStamp stamp
        | None -> ofStamp stamp




    [<CompiledName "OfPresent">]
    let ofPresent stamp =
        let optionStamp = stamp |> Stamp.map Some
        { Stamp = optionStamp |> Stamp.map (Completed << Ok);
          LatestCompletedOption =
            Some
                { Stamp = optionStamp |> Stamp.map Ok;
                  LatestSuccessfulOption = Some optionStamp } }

    [<CompiledName "WithPresent">]
    let withPresent stamp entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                let optionStamp = stamp |> Stamp.map Some
                { entityInfo with
                    Stamp = optionStamp |> Stamp.map (Completed << Ok);
                    LatestCompletedOption =
                        Some
                            { Stamp = optionStamp |> Stamp.map Ok;
                              LatestSuccessfulOption = Some optionStamp } })

    [<CompiledName "OptionWithPresent">]
    let optionWithPresent stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofPresent withPresent




    [<CompiledName "OfAbsent">]
    let ofAbsent (stamp: unit Stamp) =
        let optionStamp = stamp |> Stamp.mapTo None
        { Stamp = optionStamp |> Stamp.map (Completed << Ok);
          LatestCompletedOption =
            Some
                { Stamp = optionStamp |> Stamp.map Ok;
                  LatestSuccessfulOption = Some optionStamp } }

    [<CompiledName "WithAbsent">]
    let withAbsent (stamp: unit Stamp) entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                let optionStamp = stamp |> Stamp.mapTo None
                { entityInfo with
                    Stamp = optionStamp |> Stamp.map (Completed << Ok);
                    LatestCompletedOption =
                        Some
                            { Stamp = optionStamp |> Stamp.map Ok;
                              LatestSuccessfulOption = Some optionStamp } })

    [<CompiledName "OptionWithAbsent">]
    let optionWithAbsent stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofAbsent withAbsent




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




    [<CompiledName "OfCompleted">]
    let ofCompleted<'a> (stamp: 'a Result Stamp) =
        match stamp.Item with
        | Ok item -> ofSuccessful (stamp |> Stamp.mapTo item)
        | Error item -> ofFailed (stamp |> Stamp.mapTo item)

    [<CompiledName "WithCompleted">]
    let withCompleted<'a> (stamp: 'a Result Stamp) entityInfo =
        (stamp, entityInfo)
        ||> withNext
            (fun () ->
                match stamp.Item with
                | Ok item -> entityInfo |> withSuccessful (stamp |> Stamp.mapTo item)
                | Error item -> entityInfo |> withFailed (stamp |> Stamp.mapTo item))

    [<CompiledName "OptionWithCompleted">]
    let optionWithCompleted stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofCompleted withCompleted




    [<CompiledName "OfPending">]
    let ofPending stamp =
        { Stamp = stamp |> Stamp.mapTo Pending;
          LatestCompletedOption = None }

    [<CompiledName "WithPending">]
    let withPending (stamp: unit Stamp) entityInfo =
        (stamp, entityInfo)
        ||> withNext (fun () -> { entityInfo with Stamp = stamp |> Stamp.mapTo Pending })

    [<CompiledName "OptionWithPending">]
    let optionWithPending stamp entityInfoOption =
        (stamp, entityInfoOption) ||> optionWithNext ofPending withPending




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
