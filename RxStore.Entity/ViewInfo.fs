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

    [<CompiledName "AddEntityTargets">]
    let addEntityTargets entityTargets =
        { View = ();
          EntityTargets = Set.ofSeq entityTargets }

    [<CompiledName "AddEntityTarget">]
    let addEntityTarget entityTarget =
        { View = ();
          EntityTargets = Set.singleton entityTarget }

    [<CompiledName "Map">]
    let map mapping viewInfo =
        { View = mapping viewInfo.View;
          EntityTargets = viewInfo.EntityTargets }

    [<CompiledName "Apply">]
    let apply applying viewInfo =
        { View = applying.View viewInfo.View;
          EntityTargets = Set.union applying.EntityTargets viewInfo.EntityTargets }

    [<CompiledName "Lift">]
    let lift2 viewInfo1 viewInfo2 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2

    [<CompiledName "Lift">]
    let lift3 viewInfo1 viewInfo2 viewInfo3 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2
        |> fun viewInfo -> apply viewInfo viewInfo3

    [<CompiledName "Lift">]
    let lift4 viewInfo1 viewInfo2 viewInfo3 viewInfo4 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2
        |> fun viewInfo -> apply viewInfo viewInfo3
        |> fun viewInfo -> apply viewInfo viewInfo4

    [<CompiledName "Lift">]
    let lift5 viewInfo1 viewInfo2 viewInfo3 viewInfo4 viewInfo5 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2
        |> fun viewInfo -> apply viewInfo viewInfo3
        |> fun viewInfo -> apply viewInfo viewInfo4
        |> fun viewInfo -> apply viewInfo viewInfo5

    [<CompiledName "Lift">]
    let lift6 viewInfo1 viewInfo2 viewInfo3 viewInfo4 viewInfo5 viewInfo6 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2
        |> fun viewInfo -> apply viewInfo viewInfo3
        |> fun viewInfo -> apply viewInfo viewInfo4
        |> fun viewInfo -> apply viewInfo viewInfo5
        |> fun viewInfo -> apply viewInfo viewInfo6

    [<CompiledName "Lift">]
    let lift7 viewInfo1 viewInfo2 viewInfo3 viewInfo4 viewInfo5 viewInfo6 viewInfo7 lifting =
        viewInfo1
        |> map lifting
        |> fun viewInfo -> apply viewInfo viewInfo2
        |> fun viewInfo -> apply viewInfo viewInfo3
        |> fun viewInfo -> apply viewInfo viewInfo4
        |> fun viewInfo -> apply viewInfo viewInfo5
        |> fun viewInfo -> apply viewInfo viewInfo6
        |> fun viewInfo -> apply viewInfo viewInfo7

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
        let innerViewInfo = binding viewInfo.View
        { View = innerViewInfo.View;
          EntityTargets = Set.union viewInfo.EntityTargets innerViewInfo.EntityTargets }

    [<CompiledName "Sequence">]
    let sequence viewInfos =
        (result Seq.empty, viewInfos)
        ||> Seq.fold
            (fun state element ->
                element
                |> map (fun item sequence -> seq { yield! sequence; yield item })
                |> fun applying -> state |> apply applying)

    [<CompiledName "SequenceList">]
    let sequenceList viewInfos =
        viewInfos
        |> sequence
        |> map List.ofSeq

    [<CompiledName "SequenceOption">]
    let sequenceOption viewInfoOption =
        match viewInfoOption with
        | Some viewInfo -> viewInfo |> map Some
        | None -> result None


    type Builder() =
        member x.Bind(comp, func) = bind func comp
        member x.Return(value) = result value
        member x.ReturnFrom(value) = value
        member x.Zero() = result ()




[<AutoOpen>]
module ViewInfoBuilder =

    let viewInfo = ViewInfo.Builder()
