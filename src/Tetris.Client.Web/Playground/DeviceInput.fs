[<AutoOpen>]
module Tetris.Client.Web.Playground.Hooks

open System
open Fable.React
open Tetris.Core


type IHooks with
    member _.useDeviceInput state dispatch containerId =
        let touchStart: IRefHook<(float * float) option> = Hooks.useRef None
        let touchStartTime: IRefHook<DateTime option> = Hooks.useRef None
        let touchMove: IRefHook<(float * float) option> = Hooks.useRef None

        let move = NewOperation >> Msg.NewEvent >> dispatch

        let tryMoving (x, y) =
            match touchMove.current with
            | Some (x0, y0) ->
                touchMove.current <- (Some (x, y))
                let dx, dy = x - x0, y - y0
                let threshholdH = 25.
                let threshholdV = 5.
                if Math.Abs dx > Math.Abs dy && Math.Abs dx > threshholdH then
                    if dx < 0. 
                    then Some Operation.MoveLeft
                    else Some Operation.MoveRight
                elif Math.Abs dx < Math.Abs dy && dy > threshholdV then
                    Some Operation.MoveDown
                else None
                |> Option.iter move
            | _ -> ()

        let tryFinallyMove () =
            match touchStart.current, touchMove.current with
            | Some (x0, y0), Some(x1, y1) ->
                let threshhold = 3.
                if Math.Abs(x0 - x1) < threshhold &&
                   Math.Abs(y0 - y1) < threshhold
                then
                    move Operation.RotateClockWise
            | _ ->
                ()

        Hooks.useEffectDisposable 
            (fun () ->
                let onTouchStart (e: Browser.Types.Event) =
                    let e = e :?> Browser.Types.TouchEvent
                    touchStart.current <- Some (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                    touchStartTime.current <- Some DateTime.Now
                    touchMove.current <- Some (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                let onTouchMove (e: Browser.Types.Event) =
                    let e = e :?> Browser.Types.TouchEvent
                    tryMoving (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                let onTouchEnd (_: Browser.Types.Event) =
                    tryFinallyMove()
                    touchStart.current <- None
                    touchStartTime.current <- None
                    touchMove.current <- None

                let onKeyDown (e: Browser.Types.Event) =
                    let e = e :?> Browser.Types.KeyboardEvent
                    if e.keyCode = 37.   then Some Operation.MoveLeft
                    elif e.keyCode = 38. then Some Operation.RotateClockWise
                    elif e.keyCode = 39. then Some Operation.MoveRight
                    elif e.keyCode = 40. then Some Operation.MoveDown
                    else None
                    |> Option.iter move

                let container = Browser.Dom.document.getElementById containerId
                if not state.IsViewMode then
                    container.addEventListener("touchstart", onTouchStart)
                    container.addEventListener("touchmove", onTouchMove)
                    container.addEventListener("touchend", onTouchEnd)
                    Browser.Dom.window.addEventListener("keydown", onKeyDown)

                { new IDisposable with
                    member _.Dispose() =
                        container.removeEventListener("touchstart", onTouchStart)
                        container.removeEventListener("touchmove", onTouchMove)
                        container.removeEventListener("touchend", onTouchEnd)
                        Browser.Dom.window.removeEventListener("keydown", onKeyDown)
                }
            ,[||])
