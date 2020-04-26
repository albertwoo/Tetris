module Tetris.Client.Web.App.SubmitRecordView

open System
open Fable.React
open Fable.React.Props
open Fun.LightForm
open Fun.LightForm.Validators
open Fun.LightForm.FormView
open Tetris.Server.WebApi.Dtos
open Tetris.Server.WebApi.Dtos.Game
open Tetris.Client.Web
open Tetris.Client.Web.Controls


let private validators =
    Map.empty
    |> addValidators "Name" [ required "Required" ]
    |> addValidators "Password" [ required "Required" ]

let private inputClasses =
    [ 
        Tw.``bg-gray-darker``; Tw.``w-full``; Tw.``p-01``
        Tw.``focus:bg-gray-dark``; Tw.``hover:bg-gray-dark``; Tw.``outline-none``
    ]


type PlayerInfo = 
    { Name: string
      Password: string }

let render =
    FunctionComponent.Of(
        fun (playground: Tetris.Client.Web.Playground.State, dispatch) ->
            let form = 
                Hooks.useStateLazy (fun () -> 
                    { Name = ""; Password = "" }
                    |> generateFormByValue 
                    |> updateFormWithValidators validators
                )

            let robotCheckerWidth = 20
            let robotChecker: IStateHook<RobotChecker option> = Hooks.useState None
            let robotCherkerX: IStateHook<float option> = Hooks.useState None
            let robotCheckerContainer: IRefHook<Browser.Types.Element option> = Hooks.useRef None

            Hooks.useEffect 
                (fun () ->
                    Http.get "/api/robot/checker"
                    |> Http.handleJsonAsync
                        (Some >> robotChecker.update)
                        (Some >> Msg.OnError >> dispatch)
                    |> Async.Start
                ,[||])

            div </> [
                Classes [ 
                    Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center``
                ]
                Children [
                    div </> [
                        Styles [ Width 400 ]
                        Classes [ Tw.``bg-gray-darkest``; Tw.rounded; Tw.``shadow-lg`` ]
                        Children [
                            div </> [
                                Classes [ Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                                Children [
                                    p </> [
                                        Text (sprintf "#%d"  playground.Playground.Score)
                                        Classes [ 
                                            Tw.``text-2xl``; Tw.``opacity-75``; Tw.``mt-04``; Tw.``mb-02``
                                            Tw.``text-white``; Tw.``font-bold``
                                        ]
                                    ]
                                ]
                            ]
                            lightForm [
                                LightFormProp.InitForm form.current
                                LightFormProp.OnFormChanged form.update
                                LightFormProp.CreateFields (fun field ->
                                    [
                                        field "Name" (Form.input [
                                            InputProp.Label "Nick Name"
                                            InputProp.InputAttrs [
                                                Classes inputClasses
                                            ]
                                        ])
                                        field "Password" (Form.input [
                                            InputProp.Label "Password"
                                            InputProp.ConvertTo InputValue.Password
                                            InputProp.InputAttrs [
                                                Classes inputClasses
                                            ]
                                        ])
                                    ]
                                )
                            ]

                            div </> [
                                Classes [ Tw.relative ]
                                OnClick (fun e ->
                                    let t = e.target :?> Browser.Types.Element
                                    let rect = t.getBoundingClientRect()
                                    robotCherkerX.update(Some(e.clientX - rect.left - (float robotCheckerWidth / 2.)))
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
                                Text "请再相应位置点击方块"
                            ]

                            div </> [
                                Classes [ 
                                    Tw.flex; Tw.``justify-center``; Tw.``items-center``
                                    Tw.``my-04``
                                ]
                                Children [
                                    Button.danger [
                                        Text "取消"
                                        OnClick (fun _ -> ClosePlay |> dispatch)
                                    ]
                                    Button.primary [
                                        Text "提交"
                                        Classes [ Tw.``ml-04`` ]
                                        OnClick (fun _ ->
                                            match robotChecker.current, robotCherkerX.current, robotCheckerContainer.current, tryGenerateValueByForm<PlayerInfo> form.current with
                                            | Some checker, Some x, Some ref, Ok value ->
                                                match playground.StartTime, playground.Events with
                                                | Some startTime, _::_ ->
                                                    (
                                                        { Id = checker.Id
                                                          Value = x / ref.clientWidth },
                                                        { PlayerName = value.Name
                                                          PlayerPassword = value.Password
                                                          GameEvents = playground.Events
                                                          Score = playground.Playground.Score
                                                          TimeCostInMs = (DateTime.Now - startTime).TotalMilliseconds |> int }
                                                    )
                                                    |> UploadRecord
                                                    |> dispatch
                                                | _ ->
                                                    dispatch ClosePlay
                                            | _, _, _, Error e -> ClientError.DtoParseError (string e) |> Some |> Msg.OnError |> dispatch
                                            | _ -> ()
                                        )
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
    )
