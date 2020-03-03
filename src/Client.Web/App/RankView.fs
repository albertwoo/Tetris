module Client.App.RankView

open Fable.React
open Fable.React.Props
open Fun.ReactSpring
open Client.Controls


let render state dispatch =
    div </> [
        Classes [ Tw.``pt-20`` ]
        Children [
            h2 </> [
                Text "#排行榜（TOP 10）"
                Classes [ Tw.``text-center``; Tw.``text-2xl``; Tw.``font-bold``; Tw.``text-gray-lighter``; Tw.``opacity-25`` ]
            ]
            div </> [
                Classes [
                    Tw.``text-gray-light``; Tw.``mt-02``
                ]
                Children [
                    List.row [
                        ListRowProp.ContainerProps [
                            Classes [ Tw.``pt-02``; Tw.``pb-01``; Tw.``opacity-75``; Tw.``text-xs`` ]
                        ]
                        ListRowProp.Cell [
                            (None, emptyView)
                            (Some 0.2, emptyView)
                            (Some 1.0, str "分数")
                            (Some 1.0, str "⏱时间")
                            (Some 1.0, str "昵称")
                        ]
                    ]
                    for (i, info) in state.RankInfos |> List.indexed do
                        let maxScore = state.RankInfos.Head.Score
                        let relativeV = info.Score * 100 / maxScore
                        List.row [
                            ListRowProp.ContainerProps [
                                Classes [ 
                                    Tw.``my-02``; Tw.``py-01``; Tw.relative; Tw.``text-gray-lighter``
                                    Tw.``hover:bg-brand-dark``; Tw.``cursor-pointer``
                                ]
                                OnClick (fun _ -> Some info |> SelectRankInfo |> dispatch)
                            ]
                            ListRowProp.Cell [
                                (
                                    None, 
                                    spring [
                                        SpringRenderProp.From {| width = 0.0 |}
                                        SpringRenderProp.To {| width = 1.0 |}
                                        SpringRenderProp.Delay (float i * 100.)
                                        SpringRenderProp.ChildrenByFn (fun op -> [
                                            div </> [
                                                Classes [ Tw.``bg-brand``; Tw.absolute; Tw.``left-0``; Tw.``top-0``; Tw.``bottom-0`` ]
                                                Style [
                                                    Width (sprintf "%f%%" (float relativeV * op.width))
                                                    Opacity (sprintf "%d%%" relativeV)
                                                    ZIndex -1
                                                ]
                                            ]
                                        ])
                                    ]
                                )
                                (
                                    Some 0.2,
                                    div </> [
                                        Classes [
                                            Tw.``text-xl``; Tw.``text-gray-lighter``; Tw.``hover:text-gray-lightest``; Tw.``pl-03``
                                            Icons.``icon-play-circle``
                                            if state.SelectedRankInfo = Some info then
                                                Tw.``opacity-75``
                                            else
                                                Tw.``opacity-25``
                                        ]
                                        OnClick (fun _ -> StartReply |> dispatch)
                                    ]
                                )
                                (Some 1.0, str (sprintf "#%d" info.Score))
                                (Some 1.0, str (sprintf "%d<S>" info.TimeCost))
                                (Some 1.0, str info.Name)
                            ]
                        ]
                ]
            ]
        ]
    ]
