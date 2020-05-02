module Tetris.Client.Web.Playground.Views

open System
open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


let render =
    FunctionComponent.Of(
        fun (state, dispatch) ->
            let containerId = Hooks.useState (sprintf "tetris-playground-%d" (System.Random().Next()))
            Hooks.useDeviceInput state dispatch containerId.current
            div </> [
                Classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center`` ]
                Children [
                    div </> [
                        Id containerId.current
                        //Children [ TetrisView.render state.Playground ]
                        Children [ TetrisView.renderCanvas state.Playground ]
                    ]
                    div </> [
                        Classes [
                            Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                            Tw.``mb-02``; Tw.``opacity-50``
                        ]
                        Children [
                            if not state.IsViewMode then
                                PlayButton.render (dispatch, Operation.MoveLeft)
                                PlayButton.render (dispatch, Operation.MoveDown)
                                PlayButton.render (dispatch, Operation.RotateClockWise)
                                PlayButton.render (dispatch, Operation.MoveRight)
                        ]
                    ]
                ]
            ]
    )
