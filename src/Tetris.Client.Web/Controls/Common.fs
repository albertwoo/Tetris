[<AutoOpen>]
module Tetris.Client.Web.Controls.Common

open Feliz
open Zanaptak.TypedCssClasses


let [<Literal>] TailwindCssPath = __SOURCE_DIRECTORY__ + "/../public/css/tailwind-generated.css"
let [<Literal>] IconsCssPath = __SOURCE_DIRECTORY__ + "/../public/icomoon/style.css"

type Tw = CssClasses<TailwindCssPath, Naming.Verbatim>
type Icons = CssClasses<IconsCssPath, Naming.Verbatim>


let errorView (e: ClientError) close =
    Html.div [
        prop.classes [ 
            Tw.``bg-red-600``; Tw.``text-xs``; Tw.``py-01``; Tw.``text-center``
            Tw.``text-white``; Tw.``opacity-75``
            Tw.``fixed``; Tw.``bottom-0``; Tw.``right-0``; Tw.``left-0``
        ]
        prop.text (string e)
        prop.onClick (fun _ -> close None)
    ]
