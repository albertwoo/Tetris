namespace Tetris.Client.Web.Controls

open System
open Feliz
open Tetris.Client.Web
open Tetris.Client.Web.Controls
open Tetris.Server.WebApi.Dtos


type RobotCheckerValue =
    { Id: Guid
      Value: float }


[<RequireQualifiedAccess>]
module RobotChecker =
    let render =
        React.functionComponent (
            fun (props: {| label: string; onCheck: RobotCheckerValue option -> unit |}) ->
                let robotCheckerWidth = 20
                let robotChecker, setRobotChecker = React.useState<RobotChecker option> None
                let robotCherkerX, setRobotCherkerX = React.useState<float option> None
                let robotCheckerContainer = React.useRef<Browser.Types.Element option> None

                let timeoutRef = React.useRef 0.
                let isLoadingChecker, setIsLoadingChecker = React.useState false

                let check x =
                    setRobotCherkerX(Some x)
                    match robotChecker, robotCheckerContainer.current with
                    | Some checker, Some ref ->
                        { Id = checker.Id
                          Value = x / ref.clientWidth }
                        |> Some
                        |> props.onCheck
                    | _ ->
                        ()

                let rec getChecker () =
                    setIsLoadingChecker true
                    Http.get "/api/robot/checker"
                    |> Http.handleJsonAsync
                        (fun data ->
                            setIsLoadingChecker false
                            setRobotChecker(Some data)
                            props.onCheck None
                            let timeoutInMs = (data.ExpireDate - DateTime.Now).TotalMilliseconds
                            timeoutRef.current <-
                                Browser.Dom.window.setTimeout
                                    (fun () -> getChecker()
                                    ,int timeoutInMs)
                        )
                        (fun x ->
                            timeoutRef.current <-
                                Browser.Dom.window.setTimeout
                                    (fun () -> getChecker()
                                    ,int 2000)
                        )
                    |> Async.StartImmediate

                React.useEffectOnce (fun () ->
                    getChecker()
                    { new IDisposable with
                        member _.Dispose() =
                            Browser.Dom.window.clearTimeout timeoutRef.current
                    }
                )

                Html.div [
                    Html.div [
                        prop.classes [ Tw.relative ]
                        prop.onClick (fun e ->
                            let t = e.target :?> Browser.Types.Element
                            let rect = t.getBoundingClientRect()
                            check(e.clientX - rect.left - (float robotCheckerWidth / 2.))
                        )
                        prop.ref (fun x -> robotCheckerContainer.current <- Some x)
                        prop.children [
                            match robotChecker with
                            | None -> ()
                            | Some checker ->
                                Html.img [
                                    prop.src checker.Base64ImageSource
                                ]
                                match robotCherkerX with
                                | None -> ()
                                | Some x ->
                                    Html.div [
                                        prop.classes [ Tw.``bg-brand``; Tw.``h-full``; Tw.``opacity-25``; Tw.``pointer-events-none`` ]
                                        prop.style [
                                            style.position.absolute
                                            style.left (int x)
                                            style.top 0
                                            style.width robotCheckerWidth
                                        ]
                                    ]
                        ]
                    ]
                    if isLoadingChecker then
                        Loader.line()
                    Html.p [
                        prop.classes [ Tw.``text-center``; Tw.``text-xs``; Tw.``opacity-50``; Tw.``text-warning`` ]
                        prop.text props.label
                    ]
                ]
        )
