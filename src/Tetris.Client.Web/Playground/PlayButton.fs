namespace Tetris.Client.Web.Playground

open System
open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


module PlayButton =
    let render =
        FunctionComponent.Of(
            fun (dispatch, operation) ->
                let move =  Event.NewOperation >> NewEvent >> dispatch
                let clickStart = Hooks.useRef None

                button </> [
                    Classes [ 
                        Tw.``px-02``; Tw.``py-02``; Tw.``m-04``; Tw.``text-white``
                        Tw.``hover:bg-brand``; Tw.``focus:outline-none``
                        Tw.``rounded-full``; Tw.``w-12``; Tw.``h-12``
                        match operation with
                        | Operation.MoveLeft -> Icons.``icon-keyboard_arrow_left``; Tw.``text-2xl``
                        | Operation.MoveDown -> Icons.``icon-keyboard_arrow_down``; Tw.``text-2xl``
                        | Operation.RotateClockWise -> Icons.``icon-rotate-right``
                        | Operation.MoveRight -> Icons.``icon-keyboard_arrow_right``; Tw.``text-2xl``
                    ]
                    OnMouseDown (fun e ->
                        e.preventDefault()
                        if clickStart.current.IsNone then
                            clickStart.current <- (Some DateTime.Now)
                    )
                    OnMouseUp (fun e ->
                        e.preventDefault()
                        match clickStart.current with
                        | Some time ->
                            let diff = (DateTime.Now - time).TotalMilliseconds
                            if diff > 150. then
                                Msg.MoveToEnd operation |> dispatch
                                clickStart.current <- None
                            else 
                                move operation
                        | _ ->
                            ()
                        clickStart.current <- None
                    )
                ]
        )
