namespace RxStore


type EntityInfo<'TItem> = EntityInfo<'TItem, EntityError>

and EntityInfo<'TItem, 'TError> =
    { Status: EntityInfoStatus;
      LastOk: 'TItem Stamp option;
      LastError: 'TError Stamp option }

and EntityInfoStatus = Offline | Online

and EntityError = string


module EntityInfo =

    [<CompiledName "Map">]
    let map mapping entityInfo =
        { Status = entityInfo.Status;
          LastOk = entityInfo.LastOk |> Option.map (Stamp.map mapping);
          LastError = entityInfo.LastError }

    [<CompiledName "MapTo">]
    let mapTo item entityInfo =
        entityInfo |> map (fun _ -> item)


    [<CompiledName "Result">]
    let result entityInfo =
        entityInfo.LastOk
        |> Option.map (Stamp.map Ok)
        |> Option.orElseWith (fun () -> entityInfo.LastError |> Option.map (Stamp.map Error))
        
            


    // TODO ZipWith


    [<CompiledName "Empty">]
    let empty =
        { Status = Offline;
          LastOk = None;
          LastError = None }


    [<CompiledName "WithStatus">]
    let withStatus status entityInfo =
        { entityInfo with Status = status }


    [<CompiledName "WithOfflineStatus">]
    let withOfflineStatus entityInfo =
        { entityInfo with Status = Offline }


    [<CompiledName "WithOnlineStatus">]
    let withOnlineStatus entityInfo =
        { entityInfo with Status = Online }


    [<CompiledName "WithError">]
    let withError stamp entityInfo =
        { entityInfo with LastError = Some stamp }


    [<CompiledName "WithCompletingError">]
    let withCompletingError stamp entityInfo =
        { entityInfo with
            Status = Offline;
            LastError = Some stamp }


    [<CompiledName "WithOk">]
    let withOk stamp entityInfo =
        { entityInfo with LastOk = Some stamp }


    [<CompiledName "WithCompletingOk">]
    let withCompletingOk stamp entityInfo =
        { entityInfo with
            Status = Offline;
            LastOk = Some stamp }


    [<CompiledName "WithResult">]
    let withResult stamp entityInfo =
        match stamp.Item with
        | Ok ok -> entityInfo |> withOk stamp
        | Error error -> entityInfo |> withError stamp


    [<CompiledName "WithCompletingResult">]
    let withCompletingResult stamp entityInfo =
        match stamp.Item with
        | Ok ok -> entityInfo |> withCompletingOk stamp
        | Error error -> entityInfo |> withCompletingError stamp
