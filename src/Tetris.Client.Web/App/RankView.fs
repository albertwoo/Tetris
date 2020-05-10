module Tetris.Client.Web.App.RankView

open Fable.React
open Fable.React.Props
open Feliz
open Fun.ReactSpring
open Tetris.Server.WebApi.Dtos.Game
open Tetris.Client.Web.Controls


let private rankView (gameboard: GameBoard) state dispatch =
    Html.div [
        for (i, info) in gameboard.TopRanks |> List.indexed do
            let maxScore = gameboard.TopRanks.Head.Score
            let relativeV = info.Score * 100 / maxScore
            List.row [
                ListRowProp.ContainerClasses [
                    Tw.``my-02``; Tw.``py-01``; Tw.relative; Tw.``text-gray-lighter``
                    Tw.``hover:bg-brand-dark``; Tw.``cursor-pointer``
                ]
                ListRowProp.OnClick (fun _ -> 
                    Some info |> SelectRankInfo |> dispatch
                    PlayMsg.StartReplay |> ControlPlayground |> dispatch
                )
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
                        Some 2,
                        Html.div [
                            prop.classes [
                                Tw.``text-xl``; Tw.``text-gray-lighter``; Tw.``hover:text-gray-lightest``; Tw.``pl-03``
                                Icons.``icon-play-circle``
                                if state.SelectedRankInfo = Some info then
                                    Tw.``opacity-75``
                                else
                                    Tw.``opacity-25``
                            ]
                            prop.onClick (fun e ->
                                e.stopPropagation()
                                Some info |> SelectRankInfo |> dispatch
                                PlayMsg.StartReplay |> ControlPlayground |> dispatch
                            )
                        ]
                    )
                    (
                        Some 10, 
                        Html.span [
                            prop.text (sprintf "#%d" info.Score)
                            prop.classes [ Tw.``font-bold`` ]
                        ]
                    )
                    (
                        Some 10, 
                        Html.span [
                            Html.span [
                                prop.text (sprintf "%d" (info.TimeCostInMs / 1000))
                                prop.classes [ Tw.``font-semibold`` ]
                            ]
                            Html.span [
                                prop.text " s"
                                prop.classes [ Tw.``opacity-75`` ]
                            ]
                        ]
                    )
                    (
                        Some 10,
                        Html.span [
                            prop.text (if info.PlayerName.Length > 10 then info.PlayerName.Substring(0, 10) else info.PlayerName)
                            prop.classes [ Tw.``text-xs``; Tw.``opacity-75`` ]
                        ]
                    )
                ]
            ]
    ]


let render state dispatch =
    let tranStr = state.Context.Translate >> Html.text
    Html.div [
        prop.classes [ Tw.``py-04`` ]
        prop.children [
            Html.h2 [
                prop.text (state.Context.Translate "App.Rank.Title")
                prop.classes [ 
                    Tw.``text-center``; Tw.``text-2xl``; Tw.``sm:text-xl``;
                    Tw.``font-bold``; Tw.``text-gray-lighter``; Tw.``opacity-25``
                ]
            ]
            Html.div [
                prop.classes [
                    Tw.``text-gray-light``; Tw.``mt-02``
                ]
                prop.children [
                    List.row [
                        ListRowProp.ContainerClasses [
                            Tw.``pt-02``; Tw.``pb-01``; Tw.``opacity-75``; Tw.``text-xs``
                        ]
                        ListRowProp.Cell [
                            (None, Html.none)
                            (Some 2, Html.none)
                            (Some 10, tranStr "App.Rank.Score")
                            (Some 10, tranStr "App.Rank.Time")
                            (Some 10, tranStr "App.Rank.Name")
                        ]
                    ]
                    match state.GameBoard with
                    | Deferred.Loading | Deferred.Reloading _ -> Loader.line()
                    | _ -> ()
                    match state.GameBoard with
                    | DeferredValue gameboard -> rankView gameboard state dispatch
                    | _ -> ()
                ]
            ]
        ]
    ]
