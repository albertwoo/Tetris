[<AutoOpen>]
module Client.Controls.Common

open Fable.React
open Fable.React.Props
open Zanaptak.TypedCssClasses


let [<Literal>] TailwindCssPath = __SOURCE_DIRECTORY__ + "/../public/css/tailwind-generated.css"
let [<Literal>] IconsCssPath = __SOURCE_DIRECTORY__ + "/../public/icomoon/style.css"

type Tw = CssClasses<TailwindCssPath, Naming.Verbatim>
type Icons = CssClasses<IconsCssPath, Naming.Verbatim>


[<RequireQualifiedAccess>]
module Button =
    let danger attrs =
        button </> [
            yield! attrs
            Classes [
                Tw.``text-2xl``; Tw.``text-center``; Tw.``py-02``; Tw.``px-08``
                Tw.``rounded-full``; Tw.``shadow-lg``; Tw.``bg-red-600``; Tw.``text-white``
                Tw.``border-red-600``; Tw.``border-2``; Tw.``font-bold``
                Tw.``hover:shadow-xl``; Tw.``hover:border-white``
                Tw.``outline-none``; Tw.``focus:outline-none``
            ]
        ]

    let primary attrs =
        button </> [
            yield! attrs
            Classes [
                Tw.``text-2xl``; Tw.``text-center``; Tw.``py-02``; Tw.``px-08``
                Tw.``rounded-full``; Tw.``shadow-lg``; Tw.``bg-brand``; Tw.``text-white``
                Tw.``border-brand``; Tw.``border-2``; Tw.``font-bold``
                Tw.``hover:shadow-xl``; Tw.``hover:border-white``
                Tw.``outline-none``; Tw.``focus:outline-none``
            ]
        ]
