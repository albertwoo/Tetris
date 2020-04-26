namespace Tetris.Client.Web.Controls

open System
open Fable.React
open Fable.React.Props
open Tetris.Client.Web
open Tetris.Client.Web.Controls
open Tetris.Server.WebApi.Dtos


type RobotCheckerValue =
    { Id: Guid
      Value: float }


[<RequireQualifiedAccess>]
module RobotChecker =
    let render =
        FunctionComponent.Of (
            fun (props: {| onCheck: RobotCheckerValue -> unit |}) ->
                let robotCheckerWidth = 20
                let robotChecker: IStateHook<RobotChecker option> = Hooks.useState None
                let robotCherkerX: IStateHook<float option> = Hooks.useState None
                let robotCheckerContainer: IRefHook<Browser.Types.Element option> = Hooks.useRef None

                let timeoutRef = Hooks.useRef 0.

                let check x =
                    robotCherkerX.update(Some x)
                    match robotChecker.current, robotCheckerContainer.current with
                    | Some checker, Some ref ->
                        props.onCheck
                            { Id = checker.Id
                              Value = x / ref.clientWidth }
                    | _ ->
                        ()

                let rec getChecker () =
                    Http.get "/api/robot/checker"
                    |> Http.handleJsonAsync
                        (fun data ->
                            robotChecker.update(Some data)
                            let timeoutInMs = (data.ExpireDate - DateTime.Now).TotalMilliseconds
                            timeoutRef.current <-
                                Browser.Dom.window.setTimeout
                                    (fun () -> getChecker()
                                    ,int timeoutInMs)
                        )
                        (fun x -> Browser.Dom.console.error x)
                    |> Async.StartImmediate

                Hooks.useEffectDisposable 
                    (fun () ->
                        getChecker()
                        { new IDisposable with
                            member _.Dispose() =
                                Browser.Dom.window.clearTimeout timeoutRef.current
                        }
                    ,[||])

                div [] [
                    div </> [
                        Classes [ Tw.relative ]
                        OnClick (fun e ->
                            let t = e.target :?> Browser.Types.Element
                            let rect = t.getBoundingClientRect()
                            check(e.clientX - rect.left - (float robotCheckerWidth / 2.))
                        )
                        Ref (fun x -> robotCheckerContainer.current <- Some x)
                        Children [
                            match robotChecker.current with
                            | None -> ()
                            | Some checker ->
                                img [
                                    Src checker.Base64ImageSource
                                ]
                                match robotCherkerX.current with
                                | None -> ()
                                | Some x ->
                                    div </> [
                                        Classes [ Tw.``bg-brand``; Tw.``h-full``; Tw.``opacity-25``; Tw.``pointer-events-none`` ]
                                        Style [
                                            Position PositionOptions.Absolute
                                            Left x
                                            Top 0
                                            Width robotCheckerWidth
                                        ]
                                    ]
                        ]
                    ]
                    p </> [
                        Classes [ Tw.``text-center``; Tw.``text-xs``; Tw.``opacity-50``; Tw.``text-warning`` ]
                        Text "请在相应位置点击方块"
                    ]
                ]
        )
