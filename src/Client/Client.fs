module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Fable
open Fable.Import
open Fable.Import.JS
open System
open Elmish
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Browser

open Fulma
open Fulma.FontAwesome

open Shared
open TetrisDomain
open Fable.Import.React

let defaultSpeed = 10
let calculatSpeed score = 
    let s = defaultSpeed - score / 6
    if s < 1 then 2 else s

type Model = {
    Score: int
    AllSquares: Square list
    Boundry: int * int
    IsOver: bool
    PreviewBlock: Block option
    MovingBlock: Block option
    PrectionBlock: Block option
    TimeCost: int
    Speed: int
    SpeedCount: int
    TouchStartPoint: (float * float) option
    TouchMovingPoint: (float * float) option
    TouchTime: DateTime option
    IsPaused: bool
    IsRestarting: bool
    HideDetail: bool }

type Msg =
    | Action of Action | GoBottom
    | BeginRestart | CancelRestart | Restart
    | Tick
    | TouchStart of float * float | TouchMove of float * float | TouchEnd of float * float
    | Pause | Continue
    | HideDetail

let init () =
    let rows, columns = 25, 15
    { 
        Score = 0
        Boundry = rows - 1, columns - 1
        AllSquares = []
        IsOver = false
        PreviewBlock = Some(generateBlock (getRandomBlockType()) None |> setBlockToMid columns)
        MovingBlock = Some(generateBlock (getRandomBlockType()) None |> setBlockToMid columns)
        PrectionBlock = None
        TimeCost = 0
        Speed = defaultSpeed
        SpeedCount = 0
        TouchStartPoint = None
        TouchMovingPoint = None
        TouchTime = None
        IsPaused = false
        IsRestarting = false
        HideDetail = true
    }, Cmd.none

let update msg model =
    match msg with
    | BeginRestart -> { model with IsRestarting = true }, Cmd.none
    | CancelRestart -> { model with IsRestarting = false }, Cmd.none
    | Restart -> init ()
    | Action action ->
        match model.MovingBlock with
        | Some mb ->
            if model.IsPaused || model.IsOver then model, Cmd.none
            elif action = Down && isNewBornBlock mb && isBlocked mb model.Boundry model.AllSquares then
                { model with IsOver = true }, Cmd.none
            else
                let processedMB = transformBlock action mb 1
                if isOutOfBoundry processedMB model.Boundry then model, Cmd.none
                else
                    let blocked = isBlocked processedMB model.Boundry model.AllSquares
                    if action = Down && blocked then
                        let squares, lines = getScore model.Boundry (model.AllSquares @ mb.Squares)
                        let score = model.Score + lines
                        { model with PreviewBlock = Some(generateBlock (getRandomBlockType()) None |> setBlockToMid (snd model.Boundry + 1))
                                     MovingBlock = model.PreviewBlock
                                     PrectionBlock = Some(getPredictionBlock model.PreviewBlock.Value model.Boundry squares)
                                     AllSquares = squares
                                     Score = score 
                                     Speed = calculatSpeed score
                                     SpeedCount = 0
                                     TouchMovingPoint = None }, Cmd.none
                    elif not blocked then { model with MovingBlock = Some(processedMB)
                                                       PrectionBlock = 
                                                            if model.PrectionBlock.IsSome && action = Down then model.PrectionBlock
                                                            else Some(getPredictionBlock processedMB model.Boundry model.AllSquares) }, Cmd.none
                    else model, Cmd.none
        | _ -> model, Cmd.none
    | GoBottom ->
        match model.PrectionBlock with
        | Some mb ->
            let squares, lines = getScore model.Boundry (model.AllSquares @ mb.Squares)
            let score = model.Score + lines
            { model with PreviewBlock = Some(generateBlock (getRandomBlockType()) None |> setBlockToMid (snd model.Boundry + 1))
                         MovingBlock = model.PreviewBlock
                         PrectionBlock = Some(getPredictionBlock model.PreviewBlock.Value model.Boundry squares)
                         AllSquares = squares
                         Score = score 
                         Speed = calculatSpeed score
                         SpeedCount = 0
                         TouchMovingPoint = None }, Cmd.none
        | _ -> model, Cmd.none
    | Tick ->
        if model.IsOver || model.IsPaused || model.IsRestarting then
            model, Cmd.none
        elif model.Speed <= model.SpeedCount then
            { model with TimeCost = model.TimeCost + 1
                         SpeedCount = 0 }, Cmd.ofMsg (Down |> Msg.Action)
        else
            { model with TimeCost = model.TimeCost + 1
                         SpeedCount = model.SpeedCount + 1 }, Cmd.none
    | TouchStart (x, y) -> { model with TouchStartPoint = Some(x, y)
                                        TouchMovingPoint = Some(x, y)
                                        TouchTime = Some(DateTime.Now) }, Cmd.none
    | TouchMove (x, y) ->
        match model.TouchMovingPoint with
        | Some (x0, y0) -> 
            let dx, dy = x - x0, y - y0
            let threshhold = 20.
            if Math.Abs dx > Math.Abs dy && Math.Abs dx > threshhold then
                if dx > 0. then { model with TouchMovingPoint = Some(x ,y) }, Cmd.ofMsg (Right |> Msg.Action)
                else { model with TouchMovingPoint = Some(x ,y) },  Cmd.ofMsg (Left |> Msg.Action)
            elif Math.Abs dx < Math.Abs dy then
                if dy > threshhold && dy < threshhold * 2. then
                    { model with TouchMovingPoint = Some(x ,y) }, Cmd.ofMsg (Down |> Msg.Action)
                elif dy >= threshhold * 2. then
                    { model with TouchMovingPoint = None }, Cmd.ofMsg GoBottom
                else model, Cmd.none
            else model, Cmd.none
        | _ -> model, Cmd.none
    | TouchEnd (x, y) ->
        match model.TouchStartPoint with
        | Some (x0, y0) -> 
            let dx, dy = x - x0, y - y0
            if Math.Abs dx < 10. && Math.Abs dy < 10. && 
               (DateTime.Now - model.TouchTime.Value).Ticks < TimeSpan.FromMilliseconds(500.).Ticks then
                if model.IsOver then model, Cmd.ofMsg Restart
                else { model with TouchStartPoint = None
                                  TouchMovingPoint = None
                                  TouchTime = None }, Cmd.ofMsg (Rotate |> Msg.Action)
            else { model with TouchStartPoint = None
                              TouchMovingPoint = None
                              TouchTime = None }, Cmd.none
        | _ -> { model with TouchMovingPoint = None
                            TouchTime = None }, Cmd.none
    | Pause -> { model with IsPaused = true; }, Cmd.none
    | Continue -> { model with IsPaused = false }, Cmd.none
    | HideDetail -> { model with HideDetail = not model.HideDetail }, Cmd.none

let updateCanvas squareSize model =
    let squareBorder = 1
    let backgroundColor = "#efe9dc"
    let borderColor = "rgba(0, 0, 0, 0.3)"
    let generateColorString color =
        let r, g, b, a = color
        sprintf "rgba(%d, %d, %d, %f)" r g b a
    let canvases = Browser.document.getElementsByTagName_canvas()
    if canvases.length > 0. then
        let canvas = canvases.[0]
        let rows, columns = fst model.Boundry + 1, snd model.Boundry + 1
        let screenWidth, screenHeight = columns * squareSize, rows * squareSize

        let context = canvas.getContext_2d()
        context.fillStyle <- !^backgroundColor
        context.fillRect(0., 0., float screenWidth, float screenHeight)

        let drawSquare location color (context: CanvasRenderingContext2D) =
            let row, column = location
            let x, y = squareSize * column, squareSize * row
            context.fillStyle <- !^borderColor
            context.fillRect(float x, float y, float squareSize, float squareSize)
            context.fillStyle <- !^color
            context.fillRect(float (x + squareBorder), float (y + squareBorder), float (squareSize - squareBorder * 2), float (squareSize - squareBorder * 2))
        let drawGrid targetContext =
            for r in 0..rows do
                for c in 0..columns do
                    drawSquare (r, c) backgroundColor targetContext
            targetContext
        let drawBlock block targetContext =
            match block with
            | Some mb ->
                for square in mb.Squares do drawSquare square.Location (generateColorString square.Color) targetContext
                targetContext
            | _ -> targetContext
    
        context
        |> drawGrid
        |> drawBlock model.PreviewBlock
        |> drawBlock model.MovingBlock
        |> drawBlock model.PrectionBlock
        |> fun x ->
            model.AllSquares |> List.iter (fun s -> drawSquare s.Location "rgba(0,0,0,0.8)" x)
            
let view (model : Model) (dispatch : Msg -> unit) =
    let generateColorString color =
        let r, g, b, a = color
        sprintf "rgba(%d, %d, %d, %f)" r g b a
    let squareSize = 18
    let pixleLength length = sprintf "%dpx" length
    Browser.window.location.href <- "#root" // Focus root on init
    updateCanvas squareSize model
    div [ Class "root"; Id "root"
          TabIndex 0.
          OnClick (fun e ->
            (e.target :?> Browser.HTMLElement).focus()
            //Rotate |> Msg.Action |> dispatch
            )
          OnTouchStart (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchStart |> dispatch)
          OnTouchMove (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchMove |> dispatch)
          OnTouchEnd (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchEnd |> dispatch)
          OnKeyDown (fun e ->
            if e.keyCode = 37. then Left |> Msg.Action |> dispatch
            elif e.keyCode = 38. then Rotate |> Msg.Action |> dispatch
            elif e.keyCode = 39. then Right |> Msg.Action |> dispatch
            elif e.keyCode = 40. then Down |> Msg.Action |> dispatch )
        ]
        [
            if not model.IsRestarting then
                yield div [ Class "container" ]
                    [
                        yield canvas [ Class "playground"
                                       HTMLAttr.Width ((snd model.Boundry + 1) * squareSize)
                                       HTMLAttr.Height ((fst model.Boundry + 1) * squareSize) ]
                            []
                        yield div [ Class "center-h info score"] 
                            [
                                div [] [str (sprintf "分数：%d" (model.Score))]
                                div [] [str (sprintf " 🙏 %s 秒" (string (model.TimeCost / 10)))]
                            ]
                        yield div [ Class "controls" ]
                            [
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); HideDetail |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Expand ]
                                      span [] [str (if model.HideDetail then "更多" else "隐藏")] ]
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); BeginRestart |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Refresh ]
                                      span [] [str "重新开始"] ]
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); Pause |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Pause ]
                                      span [] [str "暂停"] ]
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); Continue |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Forward ]
                                      span [] [str "继续"] ]
                            ]
                        if not model.HideDetail then 
                            yield div [ Class "center-v info" ]
                                [
                                    if model.IsOver then yield div [] [str "游戏结束"]
                                    yield div [] [str "支持手势和键盘(*上键为旋转)"]
                                    yield div [] [str "@slaveoftime 🖖"]
                                ]
                    ]
            if model.IsRestarting then
                yield div [ Class "container" ]
                    [
                        div [ Class "info" ] [ str "你确定要重新开始吗?" ]
                        div [ Class "controls" ]
                            [
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); Restart |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Refresh ]
                                      span [] [str "是"] ]
                                Button.button [
                                    Button.Color IsDanger; Button.Size IsSmall
                                    Button.OnClick (fun e -> e.stopPropagation(); CancelRestart |> dispatch) ]
                                    [ Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon Fa.I.Pause ]
                                      span [] [str "否"] ]
                            ]
                    ]
        ]

let timerTick dispatch = 
    let timer = new System.Timers.Timer(100.0)
    timer.Elapsed.Subscribe (fun _ -> dispatch Tick) |> ignore
    timer.Enabled <- true
    timer.Start()

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
|> Program.withSubscription (fun _ -> Cmd.ofSub timerTick)
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
//|> Program.withDebugger
#endif
|> Program.run