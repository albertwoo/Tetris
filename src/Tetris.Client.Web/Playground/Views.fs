module Tetris.Client.Web.Playground.Views

open Feliz
open Tetris.Core
open Tetris.Client.Web.Controls
open Tetris.Client.Web.Playground


let render =
    React.functionComponent(
        fun (props: {| state: State; dispatch: Msg -> unit |}) ->
            let containerId, _ = React.useState (sprintf "tetris-playground-%d" (System.Random().Next()))
            React.useDeviceInput props.state props.dispatch containerId
            Html.div [
                prop.classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center`` ]
                prop.children [
                    Html.div [
                        prop.id containerId
                        //prop.children [ TetrisView.render state.Playground ]
                        prop.children [ TetrisView.renderCanvas props.state.Playground ]
                    ]
                    Html.div [
                        prop.classes [
                            Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                            Tw.``mb-02``; Tw.``opacity-50``
                        ]
                        prop.children [
                            if not props.state.IsViewMode then
                                PlayButton.render {| dispatch = props.dispatch; operation = Operation.MoveLeft |}
                                PlayButton.render {| dispatch = props.dispatch; operation = Operation.MoveDown |}
                                PlayButton.render {| dispatch = props.dispatch; operation = Operation.RotateClockWise |}
                                PlayButton.render {| dispatch = props.dispatch; operation = Operation.MoveRight |}
                        ]
                    ]
                ]
            ]
    )
