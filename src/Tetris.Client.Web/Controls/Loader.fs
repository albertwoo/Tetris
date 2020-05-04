[<RequireQualifiedAccess>]
module Tetris.Client.Web.Controls.Loader

open System
open Fable.React
open Fable.React.Props
open Feliz
open Tetris.Client.Web.Controls
open Fun.ReactSpring


let line =
    React.functionComponent(
        fun () ->
            let key, _ = React.useState (Random().Next())
            let reverse, setReverse = React.useState false

            Html.div [
                prop.classes [ Tw.flex; Tw.``flex-row``; Tw.``items-center``; Tw.``justify-center`` ]
                prop.style [ style.minWidth 100 ]
                prop.children [
                    for i in [1..3] do
                        spring [
                            SpringRenderProp.From {| radians = 0. |}
                            SpringRenderProp.To {| radians = if reverse then 0. else 100. |}
                            SpringRenderProp.Delay (float i * 100.)
                            SpringRenderProp.Reset true
                            SpringRenderProp.OnRest (fun _ -> setReverse (not reverse))
                            SpringRenderProp.ChildrenByFn (fun op -> [
                                Animated.div </> [
                                    Id (sprintf "loader-box-%d-%d" key i)
                                    Key (sprintf "loader-box-%d-%d" key i)
                                    Classes [ Tw.``bg-brand``; Tw.``h-02`` ]
                                    Style [ 
                                        Width (sprintf "%f%%" op.radians)
                                        WillChange "width"
                                        Opacity (0.2 * float i)
                                    ]
                                ]
                            ])
                        ]
                ]
            ]
    )
