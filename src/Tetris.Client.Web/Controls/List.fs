namespace Tetris.Client.Web.Controls

open Feliz
open Tetris.Client.Web.Controls


type FlexScale = int

[<RequireQualifiedAccess>]
type ListRowProp =
    | Cell of (FlexScale option * ReactElement) list
    | OnClick of (unit -> unit)
    | ContainerClasses of string list


[<RequireQualifiedAccess>]
module List =
    let row =
        React.functionComponent(
            fun props ->
                Html.div [
                    prop.onClick (fun _ ->
                        props 
                        |> UnionProps.tryLast (function ListRowProp.OnClick x -> Some x | _ -> None)
                        |> Option.iter (fun f -> f())
                    )
                    prop.classes [ 
                        yield! props |> UnionProps.concat (function ListRowProp.ContainerClasses x -> Some x | _ -> None)
                        Tw.flex; Tw.``flex-row``; Tw.``items-center``
                        Tw.``text-gray-light``; Tw.``overflow-hidden``
                    ]
                    prop.children [
                        yield!
                            props
                            |> UnionProps.concat (function ListRowProp.Cell x -> Some x | _ -> None)
                            |> List.map (fun (s, child) ->
                                if s.IsSome then
                                    Html.span [
                                        prop.classes [ Tw.``text-center`` ]
                                        prop.style [ 
                                            style.flexGrow s.Value
                                            style.flexBasis 1
                                            style.flexShrink 0
                                        ]
                                        prop.children [ child ]
                                    ]
                                else
                                    child
                            )
                    ]
                ]
        )
