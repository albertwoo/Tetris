namespace Tetris.Client.Web.Playground

open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


module PlayButton =
    let render =
        FunctionComponent.Of(
            fun (dispatch, operation) ->
                let move =  Event.NewOperation >> NewEvent >> dispatch
                let movedByLongPress = Hooks.useRef false
                let longPressTimeRef = Hooks.useRef None

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
                    OnTouchStart (fun e -> e.preventDefault(); start())
                    OnTouchEnd (fun e -> e.preventDefault(); endClick())
                    OnMouseDown (fun e -> e.preventDefault(); start())
                    OnMouseUp (fun e -> e.preventDefault(); endClick())
                ]
        )
