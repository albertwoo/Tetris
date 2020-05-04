module Tetris.Client.Web.App.SubmitRecordView

open System
open Fable.React
open Fable.React.Props
open Fun.LightForm
open Fun.LightForm.Validators
open Fun.LightForm.FormView
open Tetris.Server.WebApi.Dtos.Game
open Tetris.Client.Web.Controls
open Tetris.Client.Web.Http


let private validators tran =
    Map.empty
    |> addValidators "Name" [ required (tran "App.SubmitRecord.Required") ]
    |> addValidators "Password" [ required (tran "App.SubmitRecord.Required") ]

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
        fun (state, playground: Tetris.Client.Web.Playground.State, dispatch) ->
            let tran = state.Context.Translate
            let form = 
                Hooks.useStateLazy (fun () -> 
                    { Name = ""; Password = "" }
                    |> generateFormByValue 
                    |> updateFormWithValidators (validators tran)
                )

            let robotCheckerValue = Hooks.useState None

            let score =
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

            let submitForm =
                lightForm [
                    LightFormProp.InitForm form.current
                    LightFormProp.OnFormChanged form.update
                    LightFormProp.Validators (validators tran)
                    LightFormProp.CreateFields (fun field ->
                        [
                            field "Name" (Form.input [
                                InputProp.Label (tran "App.SubmitRecord.Name")
                                InputProp.InputAttrs [
                                    Classes inputClasses
                                ]
                            ])
                            field "Password" (Form.input [
                                InputProp.Label  (tran "App.SubmitRecord.Password")
                                InputProp.ConvertTo InputValue.Password
                                InputProp.InputAttrs [
                                    Classes inputClasses
                                ]
                            ])
                        ]
                    )
                ]

            let buttons =
                let canSave =
                    match getFormErrors form.current, robotCheckerValue.current, state.UploadingState with
                    | [], Some _, (Deferred.NotStartYet | Deferred.LoadFailed _) -> true
                    | _ -> false
                div </> [
                    Classes [ 
                        Tw.flex; Tw.``justify-center``; Tw.``items-center``
                        Tw.``my-04``
                    ]
                    Children [
                        Button.danger [
                            Text (tran "App.SubmitRecord.Discard")
                            OnClick (fun _ -> ClosePlay |> dispatch)
                        ]
                        
                        Button.primary [
                            Text (tran "App.SubmitRecord.Submit")
                            Classes [ Tw.``ml-04`` ]
                            Styles [
                                if not canSave then Cursor "not-allowed"; Opacity "0.25"
                            ]
                            OnClick (fun _ ->
                                match canSave, robotCheckerValue.current, tryGenerateValueByForm<PlayerInfo> form.current with
                                | true, Some checkerValue, Ok value ->
                                    match playground.StartTime, playground.Events with
                                    | Some startTime, _::_ ->
                                        (
                                            checkerValue,
                                            { PlayerName = value.Name
                                              PlayerPassword = value.Password
                                              GameEvents = toJson playground.Events
                                              Score = playground.Playground.Score
                                              TimeCostInMs = (DateTime.Now - startTime).TotalMilliseconds |> int },
                                            AsyncOperation.Start
                                        )
                                        |> UploadRecord
                                        |> dispatch
                                    | _ ->
                                        dispatch ClosePlay
                                | _, _, Error e -> 
                                    ClientError.DtoParseError (string e) |> Some |> Msg.OnError |> dispatch
                                | _ ->
                                    ()
                            )
                        ]
                    ]
                ]

            div </> [
                Classes [ 
                    Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center``
                ]
                Children [
                    div </> [
                        Styles [ Width 400 ]
                        Classes [ Tw.``bg-gray-darkest``; Tw.``rounded-lg``; Tw.``shadow-lg`` ]
                        Children [
                            score

                            div [] [
                                match state.UploadingState with
                                | Deferred.NotStartYet | Deferred.LoadFailed _ | Deferred.ReloadFailed _ ->
                                    submitForm
                                | Deferred.Loading | Deferred.Reloading _ ->
                                    Loader.line()
                                | Deferred.Loaded _ ->
                                    ()
                                match state.UploadingState with
                                | Deferred.NotStartYet | Deferred.LoadFailed _ | Deferred.ReloadFailed _ ->
                                    RobotChecker.render 
                                        {| label = tran "App.SubmitRecord.RobotChecker"
                                           onCheck = robotCheckerValue.update |}
                                | _ ->
                                    ()
                            ]

                            buttons
                        ]
                    ]
                ]
            ]
    )
