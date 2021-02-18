module Tetris.Client.Web.Playground.TetrisView

open System
open Fable.Core.JsInterop
open Feliz
open Fun.Result
open Tetris.Core
open Tetris.Client.Web.Controls


let private squareBorder = 1


let private square =
    React.memo
        (fun (_, scale: int, s: Square, attrs) ->
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (s.X * scale)
                    style.top (s.Y * scale)
                    style.borderWidth 1
                    style.width scale
                    style.height scale
                ]
                yield! attrs
            ]
        ,withKey = fun (k, scale, s, _) -> sprintf "tetris-square-%s-%d-%d-%d" k scale s.X s.Y)


let render scale playground =
    Html.div [
        prop.style [
            style.width (playground.Size.Width * scale)
            style.height (playground.Size.Height * scale)
            style.position.relative
        ]
        prop.classes [ Tw.``mb-02`` ]
        prop.children [
            let grid = playground.SquaresGrid.value
            for y in [0..Seq.length grid - 1] do
                for x in [0..(grid |> Seq.item y |> Seq.length) - 1] do
                    match grid |> Seq.item y |> Seq.item x with
                    | Used -> 
                        square ("remain", scale, { X = x; Y = y }, [
                            prop.classes [ Tw.``bg-gray``; Tw.``opacity-25`` ]
                        ])
                    | NotUsed ->
                        ()

            for s in playground.MovingBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("moving", scale, s, [
                    prop.classes [ Tw.``bg-brand`` ]
                ])

            for s in playground.PredictionBlock |> Option.map Utils.getBlockSquares |> Option.defaultValue [] do
                square ("prediction", scale, s, [
                    prop.classes [ Tw.``bg-brand``; Tw.``opacity-25`` ]
                ])

            for s in playground.BottomBorder do
                square ("border", scale, s, [
                    prop.classes [ Tw.``bg-gray``; Tw.``opacity-50`` ]
                ])
        ]
    ]

                
let private drawSquare scale color (context: Browser.Types.CanvasRenderingContext2D) x y =
    let x, y = scale * x, scale * y
    context.fillStyle <- !^"rgba(0, 0, 0, 0)"
    context.fillRect(float x, float y, float scale, float scale)
    context.fillStyle <- !^color
    context.fillRect
        (float (x + squareBorder)
        ,float (y + squareBorder)
        ,float (scale - squareBorder * 2)
        ,float (scale - squareBorder * 2))

let private drawBlock scale block color targetContext =
    match block with
    | Some mb ->
        mb.Squares |> List.iter (fun s -> drawSquare scale color targetContext s.X s.Y)
        mb.CenterSquare |> Option.iter (fun s -> drawSquare scale color targetContext s.X s.Y)
    | _ -> 
        ()


let renderCanvas =
    React.functionComponent (fun (playground: Playground) ->
        let context = React.useRef<Browser.Types.CanvasRenderingContext2D option> None
        let canvasId, _ = React.useState (Random().Next(0, 10000))
        let scale, setScale = React.useState 10

        React.useEffectOnce
            (fun () ->
                let calculateScale (_: Browser.Types.Event) =
                    let scaleWidth = (int Browser.Dom.window.innerWidth - 20) / playground.Size.Width
                    let scaleHeight = (int Browser.Dom.window.innerHeight - 180) / playground.Size.Height
                    setScale(Math.Min(30, Math.Min(scaleWidth, scaleHeight)))
                calculateScale(null)
                Browser.Dom.window.addEventListener("resize", calculateScale)
                React.createDisposable(fun () -> Browser.Dom.window.removeEventListener("resize", calculateScale)))
        
        let draw () =
            match context.current with
            | None -> ()
            | Some context ->
                let rows, columns = playground.Size.Height + 1, playground.Size.Width + 1
                let screenWidth, screenHeight = columns * scale, rows * scale
                context.clearRect(0., 0., float screenWidth, float screenHeight)
            
                drawBlock scale playground.PredictionBlock "rgba(18,123,25, 0.25)" context
                drawBlock scale playground.MovingBlock "#127b19" context

                let grid = playground.SquaresGrid.value
                for y in [0..Seq.length grid - 1] do
                    for x in [0..(grid |> Seq.item y |> Seq.length) - 1] do
                        match grid |> Seq.item y |> Seq.item x with
                        | Used -> drawSquare scale "rgba(250,250,250,0.2)" context x y
                        | NotUsed -> ()
            
                playground.BottomBorder 
                |> Seq.iter (fun s -> drawSquare scale "rgba(250,250,250,0.1)" context s.X s.Y)

        React.useEffectOnce draw

        draw()

        Html.canvas [
            prop.key (sprintf "tetris-playground-canvas-%d" canvasId)
            prop.ref (fun x -> 
                let canva = x :?> Browser.Types.HTMLCanvasElement
                if canva |> isNull |> not then 
                    context.current <- Some(canva.getContext_2d())
            )
            prop.width (playground.Size.Width * scale)
            prop.height ((playground.Size.Height + 1) * scale)
        ]
    )
