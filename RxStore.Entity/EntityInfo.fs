namespace RxStore


type EntityInfo<'TItem> = EntityInfo<'TItem, EntityError>

and EntityInfo<'TItem, 'TError> =
    { Status: EntityInfoStatus;
      LatestResult: EntityInfoResult<'TItem, 'TError> option; }


and EntityInfoStatus = Offline | Online


and EntityInfoResult<'T, 'TError> =
    { Stamp: Result<'T, 'TError> Stamp;
      LatestItem: 'T Stamp option }


and EntityError = string




module EntityInfo =

    [<CompiledName "Map">]
    let map mapping entityInfo =
        entityInfo.LatestResult
        |> Option.map
            (fun latestResult ->
                match latestResult.Stamp, latestResult.LatestItem with
                | { Time = time1; Item = Ok item1 }, Some { Time = time2; Item = item2 }
                        when time1 = time2 && item1 = item2 ->
                    let time = time1
                    let item = (mapping item1)
                    { Stamp = Stamp.ofValues time (Ok item);
                      LatestItem = Some (Stamp.ofValues time item) }
                | _ ->
                    { Stamp = latestResult.Stamp |> Stamp.map (Result.map mapping);
                      LatestItem = latestResult.LatestItem |> Option.map (Stamp.map mapping) })


    [<CompiledName "MapTo">]
    let mapTo item entityInfo =
        entityInfo |> map (fun _ -> item)


    // TODO ZipWith


    [<CompiledName "LatestItem">]
    let latestItem entityInfo =
        entityInfo.LatestResult
        |> Option.bind (fun latestResult -> latestResult.LatestItem)


    [<CompiledName "Item">]
    let item entityInfo =
        entityInfo.LatestResult
        |> Option.bind
            (fun latestResult ->
                match latestResult.LatestItem with
                | Some stamp -> Some stamp
                | None ->
                    match latestResult.Stamp.Item with
                    | Ok item -> Some (latestResult.Stamp |> Stamp.mapTo item)
                    | Error _ -> None)


    [<CompiledName "Empty">]
    let empty =
        { Status = Offline;
          LatestResult = None }


    [<CompiledName "WithStatus">]
    let withStatus stamp entityInfo =
        { entityInfo with Status = stamp }


    [<CompiledName "WithOfflineStatus">]
    let withOfflineStatus stamp entityInfo =
        { entityInfo with Status = Offline }


    [<CompiledName "WithOnlineStatus">]
    let withOnlineStatus stamp entityInfo =
        { entityInfo with Status = Online }


    [<CompiledName "WithResult">]
    let withResult stamp entityInfo =
        { entityInfo with
            LatestResult =
                { Stamp = stamp;
                  LatestItem =
                    match stamp.Item with
                    | Ok item ->
                        stamp
                        |> Stamp.mapTo item
                        |> Some
                    | Error _ ->
                        entityInfo.LatestResult
                        |> Option.bind (fun latestResult -> latestResult.LatestItem) }
                |> Some }


    [<CompiledName "WithCompletingResult">]
    let withCompletingResult stamp entityInfo =
        { entityInfo with
            Status = Offline;
            LatestResult =
                { Stamp = stamp;
                  LatestItem =
                    match stamp.Item with
                    | Ok item ->
                        stamp
                        |> Stamp.mapTo item
                        |> Some
                    | Error _ ->
                        entityInfo.LatestResult
                        |> Option.bind (fun latestResult -> latestResult.LatestItem) }
                |> Some }


    [<CompiledName "WithError">]
    let withError stamp entityInfo =
        { entityInfo with
            LatestResult =
                { Stamp = stamp |> Stamp.map Error;
                  LatestItem =
                    entityInfo.LatestResult
                    |> Option.bind (fun latestResult -> latestResult.LatestItem) }
                |> Some }


    [<CompiledName "WithCompletingError">]
    let withCompletingError stamp entityInfo =
        { entityInfo with
            Status = Offline;
            LatestResult =
                { Stamp = stamp |> Stamp.map Error;
                  LatestItem =
                    entityInfo.LatestResult
                    |> Option.bind (fun latestResult -> latestResult.LatestItem) }
                |> Some }


    [<CompiledName "WithOk">]
    let withOk stamp entityInfo =
        { entityInfo with
            LatestResult =
                { Stamp = stamp |> Stamp.map Ok;
                  LatestItem = Some stamp }
                |> Some }


    [<CompiledName "WithCompletingOk">]
    let withCompletingOk stamp entityInfo =
        { entityInfo with
            Status = Offline;
            LatestResult =
                { Stamp = stamp |> Stamp.map Ok;
                  LatestItem = Some stamp }
                |> Some }
