﻿[<AutoOpen>]
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
                let onTouchEnd (e: Browser.Types.Event) =
                    let e = e :?> Browser.Types.TouchEvent
                    tryFinallyMove(e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
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
