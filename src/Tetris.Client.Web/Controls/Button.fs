namespace rec Tetris.Client.Web.Controls

open Feliz


[<RequireQualifiedAccess>]
type ButtonProp =
    | Variant of ButtonVariant
    | Text of string
    | Classes of string list
    | OnClick of (unit -> unit)
    | ButtonAttrs of IReactProperty list

[<RequireQualifiedAccess>]
type ButtonVariant =
    | Primary
    | Danger


[<RequireQualifiedAccess>]
module Button =
    let render props =
        let variant = props |> UnionProps.tryLast (function ButtonProp.Variant x -> Some x | _ -> None) |> Option.defaultValue ButtonVariant.Primary
        let text    = props |> UnionProps.tryLast (function ButtonProp.Text x -> Some x | _ -> None) |> Option.defaultValue ""
        let classes = props |> UnionProps.concat (function ButtonProp.Classes x -> Some x | _ -> None)
        let onClick = props |> UnionProps.tryLast (function ButtonProp.OnClick x -> Some x | _ -> None) |> Option.defaultValue ignore

        Html.button [
            prop.onClick (fun _ -> onClick())
            prop.text text
            prop.classes [
                yield! classes
                Tw.``text-center``; Tw.``py-01``; Tw.``px-06``
                Tw.``rounded-full``; Tw.``shadow-lg``;  Tw.``text-white``; Tw.``border-2``; Tw.``font-bold``
                Tw.``hover:shadow-xl``; Tw.``hover:border-white``
                Tw.``outline-none``; Tw.``focus:outline-none``
                Tw.``opacity-75``; Tw.``hover:opacity-100``
                match variant with
                | ButtonVariant.Primary ->
                    Tw.``bg-brand``
                    Tw.``border-brand``
                | ButtonVariant.Danger ->
                    Tw.``bg-red-600``;
                    Tw.``border-red-600``
            ]
        ]
