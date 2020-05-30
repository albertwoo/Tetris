module Tetris.Client.Web.Playground.TetrisView

open System
open Fable.Core.JsInterop
open Feliz
open Fun.Result
open Tetris.Core
open Tetris.Client.Web.Controls


let private squareScale = 
    match Browser.Dom.window.innerWidth with
    | LessEqual 320. -> 12
    | BetweenEqual 320. 375. -> 15
    | BetweenEqual 375. 410. -> 16
    | BetweenEqual 410. 450. -> 17
    | _ -> 18
    
let private squareBorder = 1

let private scalePx x = x * squareScale


let private square =
    React.memo
        (fun (_, s: Square, attrs) ->
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (s.X |> scalePx)
                    style.top (s.Y |> scalePx)
                    style.borderWidth 1
                    style.width (scalePx 1)
                    style.height (scalePx 1)
                ]
                yield! attrs
            ]
        ,withKey = fun (k, s, _) -> sprintf "tetris-square-%s-%d-%d" k s.X s.Y)


let render playground =
    Html.div [
        prop.style [
            style.width (playground.Size.Width |> scalePx)
            style.height (playground.Size.Height |> scalePx)
            style.position.relative
        ]
        prop.classes [ Tw.``mb-02`` ]
        prop.children [
            let grid = playground.SquaresGrid.value
            for y in [0..Seq.length grid - 1] do
                for x in [0..(grid |> Seq.item y |> Seq.length) - 1] do
                    match grid |> Seq.item y |> Seq.item x with
                    | Used -> 
                        square ("remain", { X = x; Y = y }, [
                            prop.classes [ Tw.``bg-gray``; Tw.``opacity-25`` ]
                        ])
                    | NotUsed ->
                        ()

            for s in playground.MovingBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("moving", s, [
                    prop.classes [ Tw.``bg-brand`` ]
                ])

            for s in playground.PredictionBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("prediction", s, [
                    prop.classes [ Tw.``bg-brand``; Tw.``opacity-25`` ]
                ])

            for s in playground.BottomBorder do
                square ("border", s, [
                    prop.classes [ Tw.``bg-gray``; Tw.``opacity-50`` ]
                ])
        ]
    ]

                
let private drawSquare color (context: Browser.Types.CanvasRenderingContext2D) x y =
    let x, y = squareScale * x, squareScale * y
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
        mb.Squares |> List.iter (fun s -> drawSquare color targetContext s.X s.Y)
        mb.CenterSquare |> Option.iter (fun s -> drawSquare color targetContext s.X s.Y)
    | _ -> 
        ()


let renderCanvas =
    React.functionComponent (fun (playground: Playground) ->
        let context = React.useRef<Browser.Types.CanvasRenderingContext2D option> None
        let canvasId, _ = React.useState (Random().Next(0, 10000))
        
        let draw () =
            match context.current with
            | None -> ()
            | Some context ->
                let rows, columns = playground.Size.Height + 1, playground.Size.Width + 1
                let screenWidth, screenHeight = columns * squareScale, rows * squareScale
                context.clearRect(0., 0., float screenWidth, float screenHeight)
            
                drawBlock playground.PredictionBlock "rgba(18,123,25, 0.25)" context
                drawBlock playground.MovingBlock "#127b19" context

                let grid = playground.SquaresGrid.value
                for y in [0..Seq.length grid - 1] do
                    for x in [0..(grid |> Seq.item y |> Seq.length) - 1] do
                        match grid |> Seq.item y |> Seq.item x with
                        | Used -> drawSquare "rgba(250,250,250,0.2)" context x y
                        | NotUsed -> ()
            
                playground.BottomBorder 
                |> Seq.iter (fun s -> drawSquare "rgba(250,250,250,0.1)" context s.X s.Y)

        React.useEffectOnce draw

        draw()

        Html.canvas [
            prop.key (sprintf "tetris-playground-canvas-%d" canvasId)
            prop.ref (fun x -> 
                let canva = x :?> Browser.Types.HTMLCanvasElement
                if canva |> isNull |> not then 
                    context.current <- Some(canva.getContext_2d())
            )
            prop.width (playground.Size.Width * squareScale)
            prop.height ((playground.Size.Height + 1) * squareScale)
        ]
    )
