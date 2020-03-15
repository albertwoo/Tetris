module Client.App.Views

open Fable.React
open Fable.React.Props
open Client.Controls


let onlineInfo (state: State) =
    div </> [
        Classes [ 
            Tw.``bg-brand-dark``; Tw.``text-xs``; Tw.``py-01``; Tw.``text-center``
            Tw.``text-white``; Tw.``opacity-75``
            Tw.``fixed``; Tw.``top-0``; Tw.``right-0``; Tw.``left-0``
        ]
        Text (
            match state.OnlineInfo with
            | Some info -> sprintf "%d正在玩/最高分%d" info.PlayerCount info.HightestScore
            | None -> "..."
        )
    ]


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
            onlineInfo state
            githubBrand

            match state.IsPlaying, state.IsReplying with
            | true, _ ->
                PlaygroundView.render state dispatch
            | _, true ->
                ReplyingGround.render state dispatch
            | false, false ->
                heading
                playButton state dispatch
                RankView.render state dispatch
        ]
    ]
