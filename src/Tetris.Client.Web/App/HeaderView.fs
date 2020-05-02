module Tetris.Client.Web.App.HeaderView

open Fable.React
open Tetris.Client.Web.Controls


let view =
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
                        Text "俄罗斯方块"
                        Classes [ 
                            Tw.``text-2xl``; Tw.``text-gray-lightest``; Tw.``opacity-75``; Tw.``font-bold`` 
                            Tw.``ml-04``
                        ]
                    ]
                ]
            ]
            p </> [
                Text "本游戏支持:手势滑动，键盘，按钮来操作，以及长按或快速滑动"
                Classes [ Tw.``text-center``; Tw.``text-warning``; Tw.``text-xs``; Tw.``opacity-75`` ]
            ]
        ]
    ]
