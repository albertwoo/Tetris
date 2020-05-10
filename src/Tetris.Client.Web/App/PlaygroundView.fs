module Tetris.Client.Web.App.PlaygroundView

open Feliz
open Tetris.Client.Web
open Tetris.Client.Web.Controls


let render state dispatch =
    Html.div [
        prop.classes [ Tw.``py-08``; Tw.``overflow-hidden`` ]
        prop.children [
            Html.div [
                prop.classes [ Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                prop.children [
                    Html.div [
                        prop.classes [ 
                            Tw.``mt-02``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``; Tw.``mb-02``
                        ]
                        prop.children [
                            match state.Plaground with
                            | PlaygroundState.Playing p | PlaygroundState.Replaying (DeferredValue p) | PlaygroundState.Paused p ->
                                Html.div [
                                    prop.text (sprintf "#%d" p.Playground.Score)
                                    prop.classes [ Tw.``text-3xl``; Tw.``font-bold``; Tw.``opacity-50``; Tw.``text-white``; Tw.``mr-04`` ]
                                ]
                            | _ ->
                                ()
                            match state.Plaground with
                            | PlaygroundState.Playing _ ->
                                Html.button [
                                    prop.onClick (fun e -> e.preventDefault(); PlayMsg.PausePlay |> ControlPlayground |> dispatch)
                                    prop.classes [ 
                                        Icons.``icon-lock``; Tw.``text-white``; Tw.``opacity-50``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.border; Tw.``border-brand``; Tw.``outline-none``; Tw.``mx-01``; Tw.``focus:outline-none``
                                        Tw.``hover:bg-brand``; Tw.``hover:opacity-100``; Tw.``focus:bg-brand``; Tw.``focus:opacity-100``
                                    ]
                                ]
                                Html.button [
                                    prop.onClick (fun e -> e.preventDefault(); PlayMsg.StopPlay |> ControlPlayground |> dispatch)
                                    prop.classes [ 
                                        Icons.``icon-close``; Tw.``text-white``; Tw.``opacity-50``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.border; Tw.``border-red-600``; Tw.``outline-none``; Tw.``focus:outline-none``
                                        Tw.``hover:bg-red-600``; Tw.``hover:opacity-100``; Tw.``focus:bg-red-600``; Tw.``focus:opacity-100``
                                    ]
                                ]
                            | PlaygroundState.Paused _ ->
                                Html.button [
                                    prop.onClick (fun e -> e.preventDefault(); PlayMsg.ReStartPlay |> ControlPlayground |> dispatch)
                                    prop.classes [ 
                                        Icons.``icon-play-circle``; Tw.``text-white``; Tw.``opacity-75``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.``outline-none``; Tw.``bg-brand``
                                        Tw.``hover:opacity-100``; Tw.``focus:opacity-100``; Tw.``focus:outline-none``
                                    ]
                                ]
                            | _ ->
                                ()
                        ]
                    ]

                    match state.Plaground with
                    | PlaygroundState.Replaying (Deferred.Loading | Deferred.Reloading _) ->
                        Loader.line()
                    | _ ->
                        ()

                    match state.Plaground with
                    | PlaygroundState.Replaying (DeferredValue s)
                    | PlaygroundState.Playing s
                    | PlaygroundState.Paused s ->
                        Playground.Views.render {| state = s; dispatch = PlaygroundMsg >> dispatch |}
                    | _ -> ()

                    Html.div [
                        prop.classes [ 
                            Tw.``mt-02``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``
                        ]
                        prop.children [
                            match state.Plaground with
                            | PlaygroundState.Replaying _ ->
                                Button.render [
                                    ButtonProp.Text (state.Context.Translate "App.Close")
                                    ButtonProp.OnClick (fun _ -> PlayMsg.StopReplay |> ControlPlayground |> dispatch)
                                    ButtonProp.Variant ButtonVariant.Danger
                                ]
                                match state.Plaground with
                                | PlaygroundState.Replaying (DeferredValue p) when not p.IsReplaying ->
                                    Button.render [
                                        ButtonProp.Text (state.Context.Translate "App.Replay")
                                        ButtonProp.OnClick (fun _ -> Playground.ReplayEvent 0 |> PlaygroundMsg |> dispatch)
                                        ButtonProp.Classes [ Tw.``ml-04`` ]
                                    ]
                                | _ ->
                                    ()
                            | _ ->
                                ()
                        ]
                    ]
                ]
            ]
        ]
    ]
