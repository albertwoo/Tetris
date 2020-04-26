module Tetris.Client.Web.Playground.Views

open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


let private playButtn attrs =
    button </> [
        Classes [ 
            Tw.``px-02``; Tw.``py-02``; Tw.``m-04``; Tw.``text-white``
            Tw.``hover:bg-brand``; Tw.``focus:outline-none``
            Tw.``rounded-full``; Tw.``w-10``; Tw.``h-10``
        ]
        yield! attrs
    ]


let render =
    FunctionComponent.Of(
        fun (state, dispatch) ->
            let move =  Event.NewOperation >> NewEvent >> dispatch
            Hooks.useDeviceInput state dispatch
            div </> [
                Children [
                    TetrisView.render state

                    div </> [
                        Classes [
                            Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                            Tw.``mt-04``; Tw.``mb-02``; Tw.``opacity-50``
                        ]
                        Children [
                            if not state.IsViewMode then
                                playButtn [
                                    Classes [ Icons.``icon-keyboard_arrow_left``; Tw.``text-2xl`` ]
                                    OnClick (fun e -> e.preventDefault(); Operation.MoveLeft |> move)
                                ]
                                playButtn [
                                    Classes [ Icons.``icon-keyboard_arrow_down``; Tw.``text-2xl`` ]
                                    OnClick (fun e -> e.preventDefault(); Operation.MoveDown |> move)
                                ]
                                playButtn [
                                    Classes [ Icons.``icon-rotate-right``; Tw.``text-sm`` ]
                                    OnClick (fun e -> e.preventDefault(); Operation.RotateClockWise |> move)
                                ]
                                playButtn [
                                    Classes [ Icons.``icon-keyboard_arrow_right``; Tw.``text-2xl`` ]
                                    OnClick (fun e -> e.preventDefault(); Operation.MoveRight |> move)
                                ]
                        ]
                    ]
                ]
            ]
    )
