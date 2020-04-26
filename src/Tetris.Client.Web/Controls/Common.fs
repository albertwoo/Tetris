[<AutoOpen>]
module Tetris.Client.Web.Controls.Common

open Fable.React
open Fable.React.Props
open Zanaptak.TypedCssClasses


let [<Literal>] TailwindCssPath = __SOURCE_DIRECTORY__ + "/../public/css/tailwind-generated.css"
let [<Literal>] IconsCssPath = __SOURCE_DIRECTORY__ + "/../public/icomoon/style.css"

type Tw = CssClasses<TailwindCssPath, Naming.Verbatim>
type Icons = CssClasses<IconsCssPath, Naming.Verbatim>


let errorView (e: ClientError) close =
    div </> [
        Classes [ 
            Tw.``bg-red-600``; Tw.``text-xs``; Tw.``py-01``; Tw.``text-center``
            Tw.``text-white``; Tw.``opacity-75``
            Tw.``fixed``; Tw.``bottom-0``; Tw.``right-0``; Tw.``left-0``
        ]
        Text (string e)
        OnClick (fun _ -> close None)
    ]
