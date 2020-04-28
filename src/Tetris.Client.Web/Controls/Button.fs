namespace rec Tetris.Client.Web.Controls

open Fable.React
open Fable.React.Props


[<RequireQualifiedAccess>]
type ButtonProp =
    | Variant of ButtonVariant
    | Text of string
    | Classes of string list
    | OnClick of (unit -> unit)
    | ButtonAttrs of IHTMLProp list

[<RequireQualifiedAccess>]
type ButtonVariant =
    | Primary
    | Danger


[<RequireQualifiedAccess>]
module Button =
    let danger attrs =
        button </> [
            yield! attrs
            Classes [
                Tw.``text-base``; Tw.``text-center``; Tw.``py-02``; Tw.``px-08``
                Tw.``rounded-full``; Tw.``shadow-lg``; Tw.``bg-red-600``; Tw.``text-white``
                Tw.``border-red-600``; Tw.``border-2``; Tw.``font-bold``
                Tw.``hover:shadow-xl``; Tw.``hover:border-white``
                Tw.``outline-none``; Tw.``focus:outline-none``
                Tw.``opacity-75``; Tw.``hover:opacity-100``
            ]
        ]

    let primary attrs =
        button </> [
            yield! attrs
            Classes [
                Tw.``text-base``; Tw.``text-center``; Tw.``py-02``; Tw.``px-08``
                Tw.``rounded-full``; Tw.``shadow-lg``; Tw.``bg-brand``; Tw.``text-white``
                Tw.``border-brand``; Tw.``border-2``; Tw.``font-bold``
                Tw.``hover:shadow-xl``; Tw.``hover:border-white``
                Tw.``outline-none``; Tw.``focus:outline-none``
                Tw.``opacity-75``; Tw.``hover:opacity-100``
            ]
        ]
