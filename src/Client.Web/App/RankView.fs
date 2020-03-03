module Client.App.RankView

open Fable.React
open Fable.React.Props
open Client.Controls


[<RequireQualifiedAccess>]
type RankInfoProp =
    | CellProps of (IHTMLProp list) list
    | ContainerProps of IHTMLProp list

let rankInfoRow =
    FunctionComponent.Of(
        fun props ->
            div </> [
                yield! props |> UnionProps.concat (function RankInfoProp.ContainerProps x -> Some x | _ -> None)
                Classes [ 
                    Tw.flex; Tw.``flex-row``; Tw.``items-center``
                    Tw.``text-gray-light``
                ]
                Children [
                    yield!
                        props
                        |> UnionProps.concat (function RankInfoProp.CellProps x -> Some x | _ -> None)
                        |> List.map (fun ps ->
                            span </> [
                                yield! ps
                                Classes [ Tw.``flex-1``; Tw.``text-center`` ]
                            ] 
                        )
                ]
            ]
    )


let render state dispatch =
    div </> [
        Classes [ Tw.``pt-20`` ]
        Children [
            h2 </> [
                Text "#排行榜（TOP 10）"
                Classes [ Tw.``text-center``; Tw.``text-2xl``; Tw.``font-bold``; Tw.``text-gray-lighter``; Tw.``opacity-25`` ]
            ]
            div </> [
                Classes [
                    Tw.``text-gray-light``; Tw.``mt-02``
                ]
                Children [
                    rankInfoRow [
                        RankInfoProp.ContainerProps [
                            Classes [ Tw.``pt-02``; Tw.``pb-01``; Tw.``opacity-75``; Tw.``text-xs`` ]
                        ]
                        RankInfoProp.CellProps [
                            []
                            [ Text "分数" ]
                            [ Text "⏱时间" ]
                            [ Text "昵称" ]
                        ]
                    ]
                    for info in state.RankInfos do
                        let maxScore = state.RankInfos.Head.Score
                        let relativeV = info.Score * 100 / maxScore
                        rankInfoRow [
                            RankInfoProp.ContainerProps [
                                Classes [ 
                                    Tw.``my-02``; Tw.``py-01``; Tw.relative; Tw.``text-gray-lighter``
                                    Tw.``hover:bg-brand-dark``; Tw.``cursor-pointer``
                                ]
                                OnClick (fun _ -> Some info |> SelectRankInfo |> dispatch)
                            ]
                            RankInfoProp.CellProps [
                                [
                                    Classes [ Tw.``bg-brand``; Tw.absolute; Tw.``left-0``; Tw.``top-0``; Tw.``bottom-0`` ]
                                    Style [
                                        Width (sprintf "%d%%" relativeV)
                                        Opacity (sprintf "%d%%" relativeV)
                                        ZIndex -1
                                    ]
                                ]
                                [
                                    Classes [
                                        Tw.``text-xl``; Tw.``text-gray-lighter``; Tw.``hover:text-gray-lightest``
                                        if state.SelectedRankInfo = Some info then
                                            Icons.``icon-play-circle``
                                    ]
                                    OnClick (fun _ -> StartReply |> dispatch)
                                ]
                                [ Text (sprintf "#%d" info.Score) ]
                                [ Text (sprintf "%d<S>" info.TimeCost) ]
                                [ Text info.Name ]
                            ]
                        ]
                ]
            ]
        ]
    ]
