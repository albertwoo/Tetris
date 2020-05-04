module Tetris.Client.Web.App.HeaderView

open Feliz
open Tetris.Client.Web.Controls


let private langButton (lang, isActive, dispatch) =
    Html.button [
        match lang with
        | Lang.CN -> 
            prop.text "中文"
            prop.onClick (fun _ -> GetTranslations(Lang.CN, AsyncOperation.Start) |> dispatch)
        | Lang.EN -> 
            prop.text "English"
            prop.onClick (fun _ -> GetTranslations(Lang.EN, AsyncOperation.Start) |> dispatch)
        prop.classes [ 
            Tw.``ml-02``; Tw.``hover:bg-brand``; Tw.``px-02``; Tw.``rounded-full``
            Tw.``outline-none``; Tw.``focus:outline-none``; Tw.``text-xs``
            if isActive then Tw.``bg-brand-dark``
        ]
    ]


let render (context: ClientContext, dispatch)=
    Html.div [
        prop.children [
            Html.h1 [
                prop.classes [
                    Tw.flex; Tw.``flex-row``; Tw.``items-center``; Tw.``justify-center``
                    Tw.``py-10``; Tw.``sm:py-04``
                ]
                prop.children [
                    Html.span [
                        prop.text "slaveoftime@"
                        prop.classes [ Tw.``text-xl``; Tw.``text-gray-lightest``; Tw.``opacity-50`` ]
                    ]
                    Html.span [
                        prop.text (context.Translate "App.Title")
                        prop.classes [ 
                            Tw.``text-2xl``; Tw.``text-gray-lightest``; Tw.``opacity-75``; Tw.``font-bold`` 
                            Tw.``ml-04``
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.classes [ Tw.flex; Tw.``flex-row``; Tw.``justify-center``; Tw.``items-center``; Tw.``text-white``; Tw.``opacity-75``; Tw.``mb-03`` ]
                prop.children [
                    Html.span [
                        prop.classes [ Icons.``icon-language``; Tw.``mr-02`` ]
                    ]
                    langButton (Lang.CN, (match context.Lang with Lang.CN -> true | _ -> false), dispatch)
                    langButton (Lang.EN, (match context.Lang with Lang.EN -> true | _ -> false), dispatch)
                ]
            ]
            Html.p [
                prop.text (context.Translate "App.Guide")
                prop.classes [
                    Tw.``text-center``; Tw.``text-warning``; Tw.``text-xs``; Tw.``opacity-50``; Tw.``px-02``; Tw.``mx-auto``
                    Tw.``sm:w-04/05``; Tw.``md:w-03/05``; Tw.``lg:w-03/04``; Tw.``xl:w-01/02``
                ]
            ]
        ]
    ]
