module Tetris.Client.Web.App.SubmitRecordView

open System
open Fable.React
open Fable.React.Props
open Fun.LightForm
open Fun.LightForm.Validators
open Fun.LightForm.FormView
open Tetris.Server.WebApi.Dtos.Game
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
        fun (state, playground: Tetris.Client.Web.Playground.State, dispatch) ->
            let form = 
                Hooks.useStateLazy (fun () -> 
                    { Name = ""; Password = "" }
                    |> generateFormByValue 
                    |> updateFormWithValidators validators
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
                div [] [
                    match state.UploadingState with
                    | Deferred.NotStartYet ->
                        lightForm [
                            LightFormProp.InitForm form.current
                            LightFormProp.OnFormChanged form.update
                            LightFormProp.Validators validators
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
                    | Deferred.Loading | Deferred.Reloading _ ->
                        Loader.line()
                    | Deferred.LoadFailed e | Deferred.ReloadFailed (_, e) ->
                        errorView e ignore
                    | Deferred.Loaded _ ->
                        ()
                    match state.UploadingState with
                    | Deferred.NotStartYet | Deferred.LoadFailed _ | Deferred.ReloadFailed _ ->
                        RobotChecker.render {| onCheck = Some >> robotCheckerValue.update |}
                    | _ ->
                        ()
                ]

            let buttons =
                div </> [
                    Classes [ 
                        Tw.flex; Tw.``justify-center``; Tw.``items-center``
                        Tw.``my-04``
                    ]
                    Children [
                        Button.danger [
                            Text "不保存"
                            OnClick (fun _ -> ClosePlay |> dispatch)
                        ]
                        match getFormErrors form.current, robotCheckerValue.current, state.UploadingState with
                        | [], Some checkerValue, (Deferred.NotStartYet | Deferred.LoadFailed _) ->
                            Button.primary [
                                Text "提交"
                                Classes [ Tw.``ml-04`` ]
                                OnClick (fun _ ->
                                    match tryGenerateValueByForm<PlayerInfo> form.current with
                                    | Ok value ->
                                        match playground.StartTime, playground.Events with
                                        | Some startTime, _::_ ->
                                            (
                                                checkerValue,
                                                { PlayerName = value.Name
                                                  PlayerPassword = value.Password
                                                  GameEvents = playground.Events
                                                  Score = playground.Playground.Score
                                                  TimeCostInMs = (DateTime.Now - startTime).TotalMilliseconds |> int },
                                                AsyncOperation.Start
                                            )
                                            |> UploadRecord
                                            |> dispatch
                                        | _ ->
                                            dispatch ClosePlay
                                    | Error e -> 
                                        ClientError.DtoParseError (string e) |> Some |> Msg.OnError |> dispatch
                                )
                            ]
                        | _ ->
                            ()
                    ]
                ]

            div </> [
                Classes [ 
                    Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center``
                ]
                Children [
                    div </> [
                        Styles [ Width 400 ]
                        Classes [ Tw.``bg-gray-darkest``; Tw.rounded; Tw.``shadow-lg`` ]
                        Children [
                            score
                            submitForm
                            buttons
                        ]
                    ]
                ]
            ]
    )
