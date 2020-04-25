module Client.App.PlaygroundView

open Fable.React
open Fable.React.Props
open Client
open Client.Controls


let render state dispatch =
    div </> [
        Classes [ Tw.``h-full`` ]
        Children [
            div </> [
                Classes [ Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                Children [
                    match state.Plaground with
                    | PlaygroundState.Replaying (DeferredValue s)
                    | PlaygroundState.Playing s ->
                        Playground.Views.render s (PlaygroundMsg >> dispatch)
                    | _ -> ()

                    div </> [
                        Classes [ 
                            Tw.``fixed``; Tw.``bottom-0``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``
                        ]
                        Children [
                            match state.Plaground with
                            | PlaygroundState.Replaying _ ->
                                Button.danger [
                                    Text "关闭"
                                    OnClick (fun _ -> StopReplay |> dispatch)
                                    Classes [ Tw.``my-10`` ]
                                ]
                                Button.primary [
                                    Text "重播"
                                    OnClick (fun _ -> Playground.ReplayEvent 0 |> PlaygroundMsg |> dispatch)
                                    Classes [ Tw.``my-10`` ]
                                ]
                            | PlaygroundState.Playing _ ->
                                Button.danger [
                                    Text "结束"
                                    OnClick (fun _ -> StopPlay |> dispatch)
                                    Classes [ Tw.``my-10`` ]
                                ]
                            | _ ->
                                ()
                        ]
                    ]
                ]
            ]
        ]
    ]
