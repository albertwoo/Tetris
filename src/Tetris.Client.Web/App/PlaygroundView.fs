module Tetris.Client.Web.App.PlaygroundView

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
                            | PlaygroundState.Playing p | PlaygroundState.Replaying (DeferredValue p) ->
                                div </> [
                                    Text (sprintf "#%d" p.Playground.Score)
                                    Classes [ Tw.``text-3xl``; Tw.``font-bold``; Tw.``opacity-50``; Tw.``text-white``; Tw.``mr-04`` ]
                                ]
                            | _ ->
                                ()
                            match state.Plaground with
                            | PlaygroundState.Playing _ ->
                                button </> [
                                    OnClick (fun e -> e.preventDefault(); StopPlay |> dispatch)
                                    Classes [ 
                                        Icons.``icon-close``; Tw.``text-white``; Tw.``opacity-50``; Tw.``w-10``; Tw.``h-10``
                                        Tw.``rounded-full``; Tw.border; Tw.``outline-none``
                                        Tw.``hover:bg-red-600``; Tw.``hover:opacity-100``; Tw.``focus:bg-red-600``; Tw.``focus:opacity-100``
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
                    | PlaygroundState.Playing s ->
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
                                    Text "关闭"
                                    OnClick (fun _ -> StopReplay |> dispatch)
                                ]
                                Button.primary [
                                    Text "重播"
                                    OnClick (fun _ -> Playground.ReplayEvent 0 |> PlaygroundMsg |> dispatch)
                                    Classes [ Tw.``ml-04`` ]
                                ]
                            | _ ->
                                ()
                        ]
                    ]
                ]
            ]
        ]
    ]
