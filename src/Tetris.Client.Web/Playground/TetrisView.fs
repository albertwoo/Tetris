module Tetris.Client.Web.Playground.TetrisView

open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


let private scalePx x = sprintf "%dpx" (x * 18)


let private square =
    FunctionComponent.Of 
        (fun (_, s: Square, attrs) ->
            div </> [
                Style [
                    Position PositionOptions.Absolute
                    Left (s.X |> scalePx)
                    Top (s.Y |> scalePx)
                    BorderWidth "1px"
                    Width (scalePx 1)
                    Height (scalePx 1)
                ]
                yield! attrs
            ]
        ,memoizeWith = (fun (k1, s1, _) (k2, s2, _) -> s1 = s2 && k1 = k2)
        ,withKey = (fun (k, s, _) -> sprintf "tetris-square-%s-%d-%d" k s.X s.Y))


let render state = 
    div </> [
        Style [
            Width (state.Playground.Border.Width |> scalePx)
            Height (state.Playground.Border.Height |> scalePx)
            Position PositionOptions.Relative
        ]
        Children [
            for s in state.Playground.RemainSquares do
                square ("remain", s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-25`` ]
                ])

            for s in state.Playground.MovingBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("moving", s, [
                    Classes [ Tw.``bg-brand`` ]
                ])

            for s in state.Playground.PredictionBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("prediction", s, [
                    Classes [ Tw.``bg-brand``; Tw.``opacity-25`` ]
                ])

            for column in 0..state.Playground.Border.Width-1 do
                let s = { X = column; Y = state.Playground.Border.Height }
                square ("border", s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-50`` ]
                ])
        ]
    ]
