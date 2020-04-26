[<AutoOpen>]
module Tetris.Client.Web.Playground.Hooks

open System
open Fable.React
open Tetris.Core


type IHooks with
    member _.useDeviceInput state dispatch  =
        let touchStart: IStateHook<(float * float) option> = Hooks.useState None

        let move = NewOperation >> Msg.NewEvent >> dispatch

        let moving (x, y) =
            match touchStart.current with
            | None -> ()
            | Some (x0, y0) ->
                let dx, dy = x - x0, y - y0
                let threshhold = 30.
                if Math.Abs dx > Math.Abs dy && Math.Abs dx > threshhold  then
                    if dx > 0. 
                    then Some Operation.MoveLeft
                    else Some Operation.MoveRight
                elif Math.Abs dx < Math.Abs dy && dy > threshhold then
                    Some Operation.MoveDown
                else None
                |> Option.iter move

        Hooks.useEffectDisposable (fun () ->
            let onTouchStart (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.TouchEvent
                touchStart.update(fun _ ->
                    Some (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)
                )
            let onTouchMove (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.TouchEvent
                moving (e.changedTouches.[0].clientX, e.changedTouches.[0].clientY)

            let onTouchEnd (e: Browser.Types.Event) =
                touchStart.update (fun _ -> None)

            let onKeyDown (e: Browser.Types.Event) =
                let e = e :?> Browser.Types.KeyboardEvent
                if e.keyCode = 37.   then Some Operation.MoveLeft
                elif e.keyCode = 38. then Some Operation.RotateClockWise
                elif e.keyCode = 39. then Some Operation.MoveRight
                elif e.keyCode = 40. then Some Operation.MoveDown
                else None
                |> Option.iter move

            if not state.IsViewMode then
                Browser.Dom.window.addEventListener("touchstart", onTouchStart)
                Browser.Dom.window.addEventListener("touchmove", onTouchMove)
                Browser.Dom.window.addEventListener("touchend", onTouchEnd)
                Browser.Dom.window.addEventListener("keydown", onKeyDown)

            { new IDisposable with
                member _.Dispose() =
                    Browser.Dom.window.removeEventListener("touchstart", onTouchStart)
                    Browser.Dom.window.removeEventListener("touchmove", onTouchMove)
                    Browser.Dom.window.removeEventListener("touchend", onTouchEnd)
                    Browser.Dom.window.removeEventListener("keydown", onKeyDown)
            }
        )
