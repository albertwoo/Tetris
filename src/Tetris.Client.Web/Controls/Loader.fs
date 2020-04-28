[<RequireQualifiedAccess>]
module Tetris.Client.Web.Controls.Loader

open System
open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls
open Fun.ReactSpring


let line =
    FunctionComponent.Of(
        fun () ->
            let key = Hooks.useState (Random().Next())
            let reverse = Hooks.useState false

            div </> [
                Classes [ Tw.flex; Tw.``flex-row``; Tw.``items-center``; Tw.``justify-center`` ]
                Children [
                    for i in [1..3] do
                        spring [
                            SpringRenderProp.From {| radians = 0. |}
                            SpringRenderProp.To {| radians = if reverse.current then 0. else 100. |}
                            SpringRenderProp.Delay (float i * 100.)
                            SpringRenderProp.Reset true
                            SpringRenderProp.OnRest (fun _ -> reverse.update not)
                            SpringRenderProp.ChildrenByFn (fun op -> [
                                Animated.div </> [
                                    Id (sprintf "loader-box-%d-%d" key.current i)
                                    Key (sprintf "loader-box-%d-%d" key.current i)
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
