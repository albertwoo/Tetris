module Client.Playground.Views

open Fable.React
open Fable.React.Props
open Tetris.Core
open Client.Controls


let private playButtn attrs =
    button </> [
        Classes [ 
            Tw.``px-02``; Tw.``m-02``; Tw.``text-white``
            Tw.``hover:bg-brand``; Tw.``focus:outline-none``
            Tw.``rounded-full``; Tw.``w-10``; Tw.``h-10``
        ]
        yield! attrs
    ]


let render state dispatch =
    FunctionComponent.Of(
        fun (state, dispatch) ->
            let move =  Event.NewOperation >> NewEvent >> dispatch
            Hooks.useDeviceInput state dispatch
            div [] [
                TetrisView.render state

                div </> [
                    Classes [
                        Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                        Tw.``mt-04``; Tw.``opacity-50``
                    ]
                    Children [
                        if not state.IsViewMode then
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_left``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveLeft |> move)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_down``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveDown |> move)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-rotate-right``; Tw.``text-sm`` ]
                                OnClick (fun _ -> Operation.RotateClockWise |> move)
                            ]
                            playButtn [
                                Classes [ Icons.``icon-keyboard_arrow_right``; Tw.``text-2xl`` ]
                                OnClick (fun _ -> Operation.MoveRight |> move)
                            ]
                    ]
                ]
            ]

    ) (state, dispatch)
