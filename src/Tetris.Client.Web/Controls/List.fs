namespace Tetris.Client.Web.Controls

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


type FlexScale = float

[<RequireQualifiedAccess>]
type ListRowProp =
    | Cell of (FlexScale option * ReactElement) list
    | ContainerProps of IHTMLProp list


[<RequireQualifiedAccess>]
module List =
    let row =
        FunctionComponent.Of(
            fun props ->
                div </> [
                    yield! props |> UnionProps.concat (function ListRowProp.ContainerProps x -> Some x | _ -> None)
                    Classes [ 
                        Tw.flex; Tw.``flex-row``; Tw.``items-center``
                        Tw.``text-gray-light``; Tw.``overflow-hidden``
                    ]
                    Children [
                        yield!
                            props
                            |> UnionProps.concat (function ListRowProp.Cell x -> Some x | _ -> None)
                            |> List.map (fun (s, child) ->
                                if s.IsSome then
                                    span </> [
                                        Classes [ Tw.``text-center`` ]
                                        Style [ Flex s.Value ]
                                        Children [
                                            child
                                        ]
                                    ] 
                                else
                                    child
                            )
                    ]
                ]
        )
