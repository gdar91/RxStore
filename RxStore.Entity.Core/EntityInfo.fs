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

    [<CompiledName("LatestSuccessfulOption")>]
    let latestSuccessfulOption entityInfo =
        entityInfo.LatestCompletedOption
        |> Option.bind (fun latestCompleted -> latestCompleted.LatestSuccessfulOption)




    [<CompiledName("OfValue")>]
    let ofValue stamp =
        match stamp.Item with
        | Pending -> ofPending (stamp |> Stamp.mapTo ())
        | Completed item -> ofCompleted (stamp |> Stamp.mapTo item)


    [<CompiledName("OfPending")>]
    let ofPending stamp =
        { Stamp = stamp |> Stamp.mapTo Pending;
          LatestCompletedOption = None }


    [<CompiledName("OfCompleted")>]
    let ofCompleted stamp =
        match stamp.Item with
        | Ok item -> ofSuccessful (stamp |> Stamp.mapTo item)
        | Error item -> ofFailed (stamp |> Stamp.mapTo item)
    
    [<CompiledName("OfFailed")>]
    let ofFailed stamp =
        { Stamp = stamp |> Stamp.map (Completed << Error);
          LatestCompletedOption =
            Some
                { Stamp = stamp |> Stamp.map Error;
                  LatestSuccessfulOption = None } }

    [<CompiledName("OfSuccessful")>]
    let ofSuccessful stamp =
        { Stamp = stamp |> Stamp.map (Completed << Ok);
          LatestCompletedOption =
            Some
                { Stamp = stamp |> Stamp.map Ok;
                  LatestSuccessfulOption = Some stamp } }




    [<CompiledName("WithValue")>]
    let withValue stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else
            match stamp.Item with
            | Pending -> entityInfo |> withPending (stamp |> Stamp.mapTo ())
            | Completed item -> entityInfo |> withCompleted (stamp |> Stamp.mapTo item)


    [<CompiledName("WithPending")>]
    let withPending stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else { entityInfo with Stamp = stamp |> Stamp.mapTo Pending }

       
    [<CompiledName("WithCompleted")>]
    let withCompleted stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else
            match stamp.Item with
            | Ok item -> entityInfo |> withSuccessful (stamp |> Stamp.mapTo item)
            | Error item -> entityInfo |> withFailed (stamp |> Stamp.mapTo item)


    [<CompiledName("WithFailed")>]
    let withFailed stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else
            { entityInfo with
                Stamp = stamp |> Stamp.map (Completed << Error);
                LatestCompletedOption =
                    Some
                        { Stamp = stamp |> Stamp.map Error;
                          LatestSuccessfulOption = entityInfo |> latestSuccessfulOption } }


    [<CompiledName("WithSuccessful")>]
    let withSuccessful stamp entityInfo =
        if entityInfo.Stamp.Time >= stamp.Time
        then entityInfo
        else
            { entityInfo with
                Stamp = stamp |> Stamp.map (Completed << Ok);
                LatestCompletedOption =
                    Some
                        { Stamp = stamp |> Stamp.map Ok;
                          LatestSuccessfulOption = Some stamp } }
    