[<AutoOpen>]
module Tetris.Client.Web.Playground.Hooks

open System
open Feliz
open Tetris.Core


type React with
    static member useDeviceInput state dispatch containerId =
        let touchStart = React.useRef<(float * float) option> None
        let touchStartTime = React.useRef<DateTime option> None
        let touchMove = React.useRef<(float * float) option> None

        let move = NewOperation >> Msg.NewEvent >> dispatch
        let keyDownTime = React.useRef None
        let movedByLongPressKey = React.useRef false

        let tryMoving (x, y) =
            match touchMove.current with
            | Some (x0, y0) ->
                let dx, dy = x - x0, y - y0
                let threshhold = 30.
                if Math.Abs dx > Math.Abs dy && Math.Abs dx > threshhold then
                    touchMove.current <- (Some (x, y))
                    if dx < 0. 
                    then move Operation.MoveLeft
                    else move Operation.MoveRight
                elif Math.Abs dx < Math.Abs dy && dy > threshhold then
                    touchMove.current <- (Some (x, y))
                    move Operation.MoveDown
                else 
                    ()
            | _ -> 
                ()

        let tryFinallyMove (x1, y1) =
            match touchStart.current, touchStartTime.current with
            | Some (x0, y0), Some startTime ->
                let threshhold = 5.
                let isQuickMove = (DateTime.Now - startTime).TotalMilliseconds < 200.
                let dx, dy = Math.Abs(x1 - x0), Math.Abs(y1 - y0)
                if dx < threshhold && dy < threshhold then 
                    move Operation.RotateClockWise
                elif isQuickMove && dy > 80. && dy > dx then 
                    MoveToEnd Operation.MoveDown |> dispatch
                elif isQuickMove && x1 - x0 > 80. && dx > dy then 
                    MoveToEnd Operation.MoveRight |> dispatch
                elif isQuickMove && x1 - x0 < -80. && dx > dy then 
                    MoveToEnd Operation.MoveLeft |> dispatch
            | _ ->
                ()

        React.useEffectOnce (fun () ->
            let onTouchStart (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.TouchEvent
                touchStart.current <- Some (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                touchStartTime.current <- Some DateTime.Now
                touchMove.current <- Some (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
            let onTouchMove (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.TouchEvent
                tryMoving (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
            let onTouchEnd (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.TouchEvent
                tryFinallyMove(e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                touchStart.current <- None
                touchStartTime.current <- None
                touchMove.current <- None

            let onKeyDown (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.KeyboardEvent

                if keyDownTime.current.IsNone then
                    keyDownTime.current <- (Some DateTime.Now)

                let move op =
                    match movedByLongPressKey.current, keyDownTime.current with
                    | false, Some time ->
                        let diff = (DateTime.Now - time).TotalMilliseconds
                        if diff > 50. then 
                            Msg.MoveToEnd op |> dispatch
                            movedByLongPressKey.current <- true
                        else 
                            move op
                    | _ ->
                        ()

                if e.key = "ArrowLeft"    then move Operation.MoveLeft
                elif e.key = "ArrowUp"    then move Operation.RotateClockWise
                elif e.key = "ArrowRight" then move Operation.MoveRight
                elif e.key = "ArrowDown"  then move Operation.MoveDown
                else ()

            let onKeyUp (_: Browser.Types.Event) =
                keyDownTime.current <- None
                movedByLongPressKey.current <- false

            let container = Browser.Dom.document.getElementById containerId
            if not state.IsViewMode then
                container.addEventListener("touchstart", onTouchStart)
                container.addEventListener("touchmove", onTouchMove)
                container.addEventListener("touchend", onTouchEnd)
                Browser.Dom.window.addEventListener("keydown", onKeyDown)
                Browser.Dom.window.addEventListener("keyup", onKeyUp)

            { new IDisposable with
                member _.Dispose() =
                    container.removeEventListener("touchstart", onTouchStart)
                    container.removeEventListener("touchmove", onTouchMove)
                    container.removeEventListener("touchend", onTouchEnd)
                    Browser.Dom.window.removeEventListener("keydown", onKeyDown)
                    Browser.Dom.window.removeEventListener("keyup", onKeyUp)
            }
        )
