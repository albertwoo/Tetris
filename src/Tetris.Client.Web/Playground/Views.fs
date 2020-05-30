module Tetris.Client.Web.Playground.Views

open Feliz
open Tetris.Client.Web.Controls
open Tetris.Client.Web.Playground


let render =
    React.functionComponent(
        fun (state: State, dispatch) ->
            let containerId, _ = React.useState (sprintf "tetris-playground-%d" (System.Random().Next()))
            React.useDeviceInput state dispatch containerId
            Html.div [
                prop.classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center`` ]
                prop.children [
                    Html.div [
                        prop.id containerId
                        //prop.children [ TetrisView.render state.Playground ]
                        prop.children [ TetrisView.renderCanvas state.Playground ]
                    ]
                    PlayButton.render (state.IsViewMode, dispatch)
                ]
            ]
    )
