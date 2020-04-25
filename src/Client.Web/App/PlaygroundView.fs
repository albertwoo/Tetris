module Client.App.PlaygroundView

open Fable.React
open Fable.React.Props
open Client
open Client.Controls
open Tetris.Core


let playButtn attrs =
    button </> [
        Classes [ 
            Tw.``px-02``; Tw.``m-02``; Tw.``text-white``
            Tw.``hover:bg-brand``; Tw.``focus:outline-none``
            Tw.``rounded-full``; Tw.``w-10``; Tw.``h-10``
        ]
        yield! attrs
    ]

let render state dispatch =
    div </> [
        Classes [ Tw.``h-full`` ]
        Children [
            div </> [
                Classes [ Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                Children [
                    match state.PlagroundState with
                    | PlayState.Playing s ->
                        Playground.Views.render s (PlaygroundMsg >> dispatch)
                    | _ -> ()

                    div </> [
                        Classes [
                            Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                            Tw.``mt-04``; Tw.``opacity-50``
                        ]
                        Children [
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_left``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveLeft |> Event.NewOperation |> Playground.NewEvent |> PlaygroundMsg |> dispatch)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_down``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveDown |> Event.NewOperation |> Playground.NewEvent |> PlaygroundMsg |> dispatch)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-rotate-right``; Tw.``text-sm`` ]
                                OnClick (fun _ -> Operation.RotateClockWise |> Event.NewOperation |> Playground.NewEvent |> PlaygroundMsg |> dispatch)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_right``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveRight |> Event.NewOperation |> Playground.NewEvent |> PlaygroundMsg |> dispatch)
                            ]
                        ]
                    ]

                    div </> [
                        Classes [ 
                            Tw.``fixed``; Tw.``bottom-0``; Tw.flex; Tw.``flex-row``;
                            Tw.``items-center``; Tw.``justify-center``
                        ]
                        Children [
                            Button.danger [
                                Text "结束"
                                OnClick (fun _ -> StopPlay |> dispatch)
                                Classes [ Tw.``my-10`` ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
