module Client.App.SubmitRecord

open Fable.React
open Fable.React.Props
open Fun.LightForm
open Fun.LightForm.Validators
open Fun.LightForm.FormView
open Server.Dtos.Game
open Client.Controls


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
        fun (state, dispatch) ->
            let form = 
                Hooks.useStateLazy (fun () -> 
                    { Name = ""; Password = "" }
                    |> generateFormByValue 
                    |> updateFormWithValidators validators
                )

            div </> [
                Classes [ 
                    Tw.``h-full``; Tw.flex; Tw.``flex-col``; Tw.``justify-center``; Tw.``items-center``
                ]
                Children [
                    div </> [
                        Styles [ Width 400 ]
                        Classes [ Tw.``bg-gray-darkest``; Tw.rounded; Tw.``shadow-lg`` ]
                        Children [
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
                                            match state.PlagroundState, tryGenerateValueByForm<PlayerInfo> form.current with
                                            | PlayState.Submiting p, Ok value ->
                                                { PlayerName = value.Name
                                                  PlayerPassword = value.Password
                                                  GameEvents = []
                                                  Score = p.Playground.Score
                                                  TimeCostInMs = 0 }
                                                |> UploadRecord
                                                |> dispatch
                                            | _, Error e -> ClientError.DtoParseError (string e) |> Some |> Msg.OnError |> dispatch
                                            | _ ->
                                                ()
                                        )
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
    )
