module Tetris.Client.Web.App.SubmitRecordView

open System
open Fable.React
open Feliz
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
    React.functionComponent(
        fun (state, playground: Tetris.Client.Web.Playground.State, dispatch) ->
            let tran = state.Context.Translate
            let form, updateForm = 
                React.useState (
                    { Name = ""; Password = "" }
                    |> generateFormByValue 
                    |> updateFormWithValidators (validators tran)
                )

            let robotCheckerValue, setRobotCheckerValue = React.useState None
            
            let canSave =
                match getFormErrors form, robotCheckerValue, state.UploadingState with
                | [], Some _, (Deferred.NotStartYet | Deferred.LoadFailed _) -> true
                | _ -> false

            let score =
                Html.div [
                    prop.classes [ Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center`` ]
                    prop.children [
                        Html.p [
                            prop.text (sprintf "#%d"  playground.Playground.Score)
                            prop.classes [ 
                                Tw.``text-2xl``; Tw.``opacity-75``; Tw.``mt-04``; Tw.``mb-02``
                                Tw.``text-white``; Tw.``font-bold``
                            ]
                        ]
                    ]
                ]

            let submitForm =
                lightForm [
                    LightFormProp.InitForm form
                    LightFormProp.OnFormChanged updateForm
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
                Html.div [
                    prop.classes [ 
                        Tw.flex; Tw.``justify-center``; Tw.``items-center``
                        Tw.``my-04``
                    ]
                    prop.children [
                        Button.render [
                            ButtonProp.Text (tran "App.SubmitRecord.Discard")
                            ButtonProp.OnClick (fun _ -> ClosePlay |> dispatch)
                            ButtonProp.Variant ButtonVariant.Danger
                        ]
                        
                        Button.render [
                            ButtonProp.Text (tran "App.SubmitRecord.Submit")
                            ButtonProp.Classes [ Tw.``ml-04`` ]
                            ButtonProp.ButtonAttrs [
                                prop.style [
                                    if not canSave then style.cursor.notAllowed; style.opacity 0.25
                                ]
                            ]
                            ButtonProp.OnClick (fun _ ->
                                match canSave, robotCheckerValue, tryGenerateValueByForm<PlayerInfo> form with
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

            Html.div [
                prop.classes [ 
                    Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center``
                ]
                prop.children [
                    Html.div [
                        prop.style [ style.width 300 ]
                        prop.classes [ Tw.``bg-gray-darkest``; Tw.``rounded-lg``; Tw.``shadow-lg`` ]
                        prop.children [
                            score

                            Html.div [
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
                                           onCheck = setRobotCheckerValue |}
                                | _ ->
                                    ()
                            ]

                            buttons
                        ]
                    ]
                ]
            ]
    )
