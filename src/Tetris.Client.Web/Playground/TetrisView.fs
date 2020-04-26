module Tetris.Client.Web.Playground.TetrisView

open Fable.React
open Fable.React.Props
open Tetris.Core
open Tetris.Client.Web.Controls


let private scalePx x = sprintf "%dpx" (x * 17)


let private square =
    FunctionComponent.Of 
        (fun (s: Square, attrs) ->
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
        ,memoizeWith = (fun (s1, _) (s2, _) -> s1 = s2)
    )


let render state = 
    div </> [
        Style [
            Width (state.Playground.Border.Width |> scalePx)
            Height (state.Playground.Border.Height |> scalePx)
            Position PositionOptions.Relative
        ]
        Children [
            for s in state.Playground.RemainSquares do
                square (s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-25`` ]
                ])

            for s in state.Playground.MovingBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square (s, [
                    Classes [ Tw.``bg-brand`` ]
                ])

            for column in 0..state.Playground.Border.Width-1 do
                let s = { X = column; Y = state.Playground.Border.Height }
                square (s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-50`` ]
                ])
        ]
    ]
