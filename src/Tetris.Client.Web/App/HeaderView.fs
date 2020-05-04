module Tetris.Client.Web.App.HeaderView

open Fable.React
open Fable.React.Props
open Tetris.Client.Web.Controls


let private langButton (lang, isActive, dispatch) =
    button </> [
        match lang with
        | Lang.CN -> 
            Text "中文"
            OnClick (fun _ -> GetTranslations(Lang.CN, AsyncOperation.Start) |> dispatch)
        | Lang.EN -> 
            Text "English"
            OnClick (fun _ -> GetTranslations(Lang.EN, AsyncOperation.Start) |> dispatch)
        Classes [ 
            Tw.``ml-02``; Tw.``hover:bg-brand``; Tw.``px-02``; Tw.``rounded-full``
            Tw.``outline-none``; Tw.``focus:outline-none``; Tw.``text-xs``
            if isActive then Tw.``bg-brand-dark``
        ]
    ]


let render (context: ClientContext, dispatch)=
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
                        Text (context.Translate "App.Title")
                        Classes [ 
                            Tw.``text-2xl``; Tw.``text-gray-lightest``; Tw.``opacity-75``; Tw.``font-bold`` 
                            Tw.``ml-04``
                        ]
                    ]
                ]
            ]
            div </> [
                Classes [ Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``; Tw.``text-white``; Tw.``opacity-75``; Tw.``mb-03`` ]
                Children [
                    span </> [
                        Classes [ Icons.``icon-language``; Tw.``mr-02`` ]
                    ]
                    langButton (Lang.CN, (match context.Lang with Lang.CN -> true | _ -> false), dispatch)
                    langButton (Lang.EN, (match context.Lang with Lang.EN -> true | _ -> false), dispatch)
                ]
            ]
            p </> [
                Text (context.Translate "App.Guide")
                Classes [
                    Tw.``text-center``; Tw.``text-warning``; Tw.``text-xs``; Tw.``opacity-75``; Tw.``px-02``; Tw.``mx-auto``
                    Tw.``sm:w-02/03``; Tw.``md:w-03/05``; Tw.``lg:w-03/04``; Tw.``xl:w-01/02``
                ]
            ]
        ]
    ]
