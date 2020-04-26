module Tetris.Client.Web.App.RankView

open Fable.React
open Fable.React.Props
open Fun.ReactSpring
open Tetris.Server.WebApi.Dtos.Game
open Tetris.Client.Web.Controls


let private rankView (gameboard: GameBoard) state dispatch =
    div [] [
        for (i, info) in gameboard.TopRanks |> List.indexed do
            let maxScore = gameboard.TopRanks.Head.Score
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
                            OnClick (fun e ->
                                e.stopPropagation()
                                Some info |> SelectRankInfo |> dispatch
                                StartReplay |> dispatch
                            )
                        ]
                    )
                    (
                        Some 1.0, 
                        span </> [
                            Text (sprintf "#%d" info.Score)
                            Classes [ Tw.``font-bold`` ]
                        ]
                    )
                    (
                        Some 1.0, 
                        span [] [
                            span </> [
                                Text (sprintf "%d" (info.TimeCostInMs / 1000))
                                Classes [ Tw.``font-semibold`` ]
                            ]
                            span </> [
                                Text " s"
                                Classes [ Tw.``opacity-75`` ]
                            ]
                        ]
                    )
                    (
                        Some 1.0,
                        span </> [
                            Text (if info.PlayerName.Length > 10 then info.PlayerName.Substring(0, 10) else info.PlayerName)
                            Classes [ Tw.``text-xs``; Tw.``opacity-75`` ]
                        ]
                    )
                ]
            ]
    ]


let render state dispatch =
    div </> [
        Classes [ Tw.``py-04`` ]
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
                    match state.GameBoard with
                    | DeferredValue gameboard -> rankView gameboard state dispatch
                    | _ -> ()
                ]
            ]
        ]
    ]
