module Tetris.Client.Web.Playground.TetrisView

open System
open Fable.Core.JsInterop
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


let render playground =
    div </> [
        Style [
            Width (playground.Size.Width |> scalePx)
            Height (playground.Size.Height |> scalePx)
            Position PositionOptions.Relative
        ]
        Classes [ Tw.``mb-02`` ]
        Children [
            for s in playground.RemainSquares do
                square ("remain", s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-25`` ]
                ])

            for s in playground.MovingBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("moving", s, [
                    Classes [ Tw.``bg-brand`` ]
                ])

            for s in playground.PredictionBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("prediction", s, [
                    Classes [ Tw.``bg-brand``; Tw.``opacity-25`` ]
                ])

            for s in playground.BottomBorder do
                square ("border", s, [
                    Classes [ Tw.``bg-gray``; Tw.``opacity-50`` ]
                ])
        ]
    ]


let private squareScale = 18
let private squareBorder = 1
                
let private drawSquare color (context: Browser.Types.CanvasRenderingContext2D) square =
    let x, y = squareScale * square.X, squareScale * square.Y
    context.fillStyle <- !^"rgba(0, 0, 0, 0)"
    context.fillRect(float x, float y, float squareScale, float squareScale)
    context.fillStyle <- !^color
    context.fillRect
        (float (x + squareBorder)
        ,float (y + squareBorder)
        ,float (squareScale - squareBorder * 2)
        ,float (squareScale - squareBorder * 2))

let private drawBlock block color targetContext =
    match block with
    | Some mb ->
        mb.Squares |> List.iter (drawSquare color targetContext)
        mb.CenterSquare |> Option.iter (drawSquare color targetContext)
    | _ -> 
        ()


let renderCanvas =
    FunctionComponent.Of (fun (playground: Playground) ->
        let context: IRefHook<Browser.Types.CanvasRenderingContext2D option> = Hooks.useRef None
        let canvasId = Hooks.useState (Random().Next(0, 10000))
        
        let draw () =
            match context.current with
            | None -> ()
            | Some context ->
                let rows, columns = playground.Size.Height + 1, playground.Size.Width + 1
                let screenWidth, screenHeight = columns * squareScale, rows * squareScale
                context.clearRect(0., 0., float screenWidth, float screenHeight)
            
                drawBlock playground.PredictionBlock "rgba(18,123,25, 0.4)" context
                drawBlock playground.MovingBlock "#127b19" context
                playground.RemainSquares |> List.iter (drawSquare "rgba(250,250,250,0.2)" context)
            
                playground.BottomBorder |> Seq.iter (drawSquare "rgba(250,250,250,0.1)" context)

        Hooks.useEffect(draw, [||])

        draw()

        canvas </> [
            Key (sprintf "tetris-playground-canvas-%d" canvasId.current)
            Ref (fun x -> 
                let canva = x :?> Browser.Types.HTMLCanvasElement
                if canva |> isNull |> not then 
                    context.current <- Some(canva.getContext_2d())
            )
            HTMLAttr.Width (sprintf "%d" (playground.Size.Width * squareScale) |> box)
            HTMLAttr.Height (sprintf "%d" ((playground.Size.Height + 1) * squareScale) |> box)
        ]
    )
