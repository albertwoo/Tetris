module Tetris.Client.Web.Playground.PlayButton

open Feliz
open Tetris.Core
open Tetris.Client.Web.Controls


let private btn =
    React.functionComponent
        (fun (operation, dispatch) ->
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
            ])


let render =
    React.memo
        (fun (isViewMode, dispatch) ->
            Html.div [
                prop.classes [
                    Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``
                    Tw.``mb-02``; Tw.``opacity-50``
                ]
                prop.children [
                    if not isViewMode then
                        btn (Operation.MoveLeft, dispatch)
                        btn (Operation.MoveDown, dispatch)
                        btn (Operation.RotateClockWise, dispatch)
                        btn (Operation.MoveRight, dispatch)
                ]
            ]
        ,areEqual = fun (p1, _) (p2, _) -> p1 = p2)
