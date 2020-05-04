namespace Tetris.Client.Web.Playground

open Feliz
open Tetris.Core
open Tetris.Client.Web.Controls


module PlayButton =
    let render =
        React.functionComponent(
            fun (dispatch, operation) ->
                let move =  Event.NewOperation >> NewEvent >> dispatch
                let movedByLongPress = React.useRef false
                let longPressTimeRef = React.useRef None

                let start () =
                    if longPressTimeRef.current.IsNone then
                        longPressTimeRef.current <- 
                            Browser.Dom.window.setTimeout
                                (fun _ ->
                                    Msg.MoveToEnd operation |> dispatch
                                    movedByLongPress.current <- true
                                ,200)
                            |> Some

                let endClick () =
                    match longPressTimeRef.current with
                    | Some ref -> Browser.Dom.window.clearTimeout ref
                    | None -> ()
                    
                    if not movedByLongPress.current then move operation

                    longPressTimeRef.current <- None
                    movedByLongPress.current <- false

                Html.button [
                    prop.classes [ 
                        Tw.``px-02``; Tw.``py-02``; Tw.``m-04``; Tw.``text-white``
                        Tw.``hover:bg-brand``; Tw.``focus:outline-none``
                        Tw.``rounded-full``; Tw.``w-12``; Tw.``h-12``
                        match operation with
                        | Operation.MoveLeft -> Icons.``icon-keyboard_arrow_left``; Tw.``text-2xl``
                        | Operation.MoveDown -> Icons.``icon-keyboard_arrow_down``; Tw.``text-2xl``
                        | Operation.RotateClockWise -> Icons.``icon-rotate-right``
                        | Operation.MoveRight -> Icons.``icon-keyboard_arrow_right``; Tw.``text-2xl``
                    ]
                    prop.onTouchStart (fun e -> e.preventDefault(); start())
                    prop.onTouchEnd (fun e -> e.preventDefault(); endClick())
                    prop.onMouseDown (fun e -> e.preventDefault(); start())
                    prop.onMouseUp (fun e -> e.preventDefault(); endClick())
                ]
        )
