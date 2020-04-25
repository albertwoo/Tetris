module Client.App.Views

open Fable.React
open Fable.React.Props
open Client.Controls


let heading =
    div </> [
        Children [
            h1 </> [
                Classes [
                    Tw.flex; Tw.``flex-row``; Tw.``items-center``; Tw.``justify-center``
                    Tw.``py-10``
                ]
                Children [
                    span </> [
                        Text "slaveoftime@"
                        Classes [ Tw.``text-xl``; Tw.``text-gray-lightest``; Tw.``opacity-50`` ]
                    ]
                    span </> [
                        Text "俄罗斯方块"
                        Classes [ 
                            Tw.``text-2xl``; Tw.``text-gray-lightest``; Tw.``opacity-75``; Tw.``font-bold`` 
                            Tw.``ml-04``
                        ]
                    ]
                ]
            ]    
        ]
    ]


let playButton state dispatch =
    div </> [
        Classes [ Tw.flex; Tw.``flex-col``; Tw.``items-center``; Tw.``py-04`` ]
        Children [
            Button.primary [
                Text "开始对局"
                OnClick (fun _ -> StartPlay |> dispatch)
            ]
        ]
    ]


let render state dispatch =
    div </> [
        Classes [ Tw.``h-full`` ]
        Children [
            OnlineInfo.render state
            githubBrand

            match state.Plaground with
            | PlaygroundState.Replaying _
            | PlaygroundState.Playing _ ->
                PlaygroundView.render state dispatch
            | PlaygroundState.Submiting _ ->
                SubmitRecord.render (state, dispatch)
            | _ ->
                heading
                playButton state dispatch
                RankView.render state dispatch

            match state.ErrorInfo with
            | Some e -> errorView e (Msg.OnError >> dispatch)
            | None -> ()
        ]
    ]
