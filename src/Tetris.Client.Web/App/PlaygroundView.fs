﻿module Tetris.Client.Web.App.PlaygroundView

open Fable.React
open Fable.React.Props
open Tetris.Client.Web
open Tetris.Client.Web.Controls


let render state dispatch =
    div </> [
        Classes [ Tw.``py-08``; Tw.``overflow-hidden`` ]
        Children [
            div </> [
                Classes [ Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                Children [
                    div </> [
                        Classes [ 
                            Tw.``mt-02``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``; Tw.``mb-02``
                        ]
                        Children [
                            match state.Plaground with
                            | PlaygroundState.Playing p | PlaygroundState.Replaying (DeferredValue p) | PlaygroundState.Paused p ->
                                div </> [
                                    Text (sprintf "#%d" p.Playground.Score)
                                    Classes [ Tw.``text-3xl``; Tw.``font-bold``; Tw.``opacity-50``; Tw.``text-white``; Tw.``mr-04`` ]
                                ]
                            | _ ->
                                ()
                            match state.Plaground with
                            | PlaygroundState.Playing _ ->
                                button </> [
                                    OnClick (fun e -> e.preventDefault(); PausePlay |> dispatch)
                                    Classes [ 
                                        Icons.``icon-lock``; Tw.``text-white``; Tw.``opacity-50``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.border; Tw.``border-brand``; Tw.``outline-none``; Tw.``mx-01``; Tw.``focus:outline-none``
                                        Tw.``hover:bg-brand``; Tw.``hover:opacity-100``; Tw.``focus:bg-brand``; Tw.``focus:opacity-100``
                                    ]
                                ]
                                button </> [
                                    OnClick (fun e -> e.preventDefault(); StopPlay |> dispatch)
                                    Classes [ 
                                        Icons.``icon-close``; Tw.``text-white``; Tw.``opacity-50``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.border; Tw.``border-red-600``; Tw.``outline-none``; Tw.``focus:outline-none``
                                        Tw.``hover:bg-red-600``; Tw.``hover:opacity-100``; Tw.``focus:bg-red-600``; Tw.``focus:opacity-100``
                                    ]
                                ]
                            | PlaygroundState.Paused _ ->
                                button </> [
                                    OnClick (fun e -> e.preventDefault(); ReStartPlay |> dispatch)
                                    Classes [ 
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
                        Playground.Views.render (s, PlaygroundMsg >> dispatch)
                    | _ -> ()

                    div </> [
                        Classes [ 
                            Tw.``mt-02``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``
                        ]
                        Children [
                            match state.Plaground with
                            | PlaygroundState.Replaying _ ->
                                Button.danger [
                                    Text (state.Context.Translate "App.Close")
                                    OnClick (fun _ -> StopReplay |> dispatch)
                                ]
                                match state.Plaground with
                                | PlaygroundState.Replaying (DeferredValue p) when not p.IsReplaying ->
                                    Button.primary [
                                        Text (state.Context.Translate "App.Replay")
                                        OnClick (fun _ -> Playground.ReplayEvent 0 |> PlaygroundMsg |> dispatch)
                                        Classes [ Tw.``ml-04`` ]
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
