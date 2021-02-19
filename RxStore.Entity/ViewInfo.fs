namespace RxStore


type ViewInfo<'View, 'EntityTarget when 'EntityTarget : comparison> =
    { View: 'View;
      EntityTargets: 'EntityTarget Set }

module ViewInfo =

    [<CompiledName "Result">]
    let result view =
        { View = view;
          EntityTargets = Set.empty }

    [<CompiledName "WithEntityTargets">]
    let withEntityTargets entityTargets viewInfo =
        viewInfo.EntityTargets
        |> Set.union (Set.ofSeq entityTargets)
        |> function
        | resultEntityTargets when resultEntityTargets <> viewInfo.EntityTargets ->
            { View = viewInfo.View;
              EntityTargets = resultEntityTargets }
        | _ -> viewInfo
    
    [<CompiledName "WithEntityTarget">]
    let withEntityTarget entityTarget viewInfo =
        viewInfo
        |> withEntityTargets [ entityTarget ]

    [<CompiledName "Map">]
    let map mapping viewInfo =
        { View = mapping viewInfo.View;
          EntityTargets = viewInfo.EntityTargets }

    [<CompiledName "Apply">]
    let apply applying viewInfo =
        { View = applying.View viewInfo.View;
          EntityTargets = Set.union applying.EntityTargets viewInfo.EntityTargets }

    [<CompiledName "Combine">]
    let combine viewInfo1 viewInfo2 =
        viewInfo1
        |> map (fun view1 view2 -> view1, view2)
        |> fun viewInfo -> apply viewInfo viewInfo2

    [<CompiledName "CombineWith">]
    let combineWith viewInfo2 viewInfo =
        viewInfo
        |> map (fun view1 view2 -> view1, view2)
        |> fun viewInfo -> apply viewInfo viewInfo2

    [<CompiledName "Join">]
    let join viewInfo =
        { View = viewInfo.View.View;
          EntityTargets = Set.union viewInfo.EntityTargets viewInfo.View.EntityTargets }

    [<CompiledName "Bind">]
    let bind binding viewInfo =
        viewInfo
        |> map binding
        |> join

    [<CompiledName "Sequence">]
    let sequence viewInfos =
        (result Seq.empty, viewInfos)
        ||> Seq.fold
            (fun state element ->
                element
                |> map (fun item sequence -> seq { yield! sequence; yield item })
                |> fun applying -> state |> apply applying)

    [<CompiledName "SequenceOption">]
    let sequenceOption viewInfoOption =
        //(result None, viewInfoOption)
        //||> Option.fold
        //    (fun state element ->
        //        combineWith
        //            state
        //            element
        //            (fun option item -> Some item))
        match viewInfoOption with
        | Some viewInfo -> viewInfo |> map Some
        | None -> result None
        


    type Builder() =
        member x.Bind(comp, func) = bind func comp
        member x.Return(value) = result value
        member x.ReturnFrom(value) = value
        member x.Zero() = result ()

    let builder = Builder()
