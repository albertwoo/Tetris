module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Fable
open Fable.Import
open Fable.Import.JS

open Fulma
open Fulma.FontAwesome

open Shared
open TetrisDomain

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
    TouchStartPos: (float * float) option
    IsPaused: bool
    IsRestarting: bool
    HideDetail: bool
}

type Msg =
    | Action of Action
    | BeginRestart | CancelRestart | Restart
    | Tick
    | TouchStart of float * float | TouchEnd
    | Pause | Continue
    | HideDetail

let init () =
    let rows, columns = 28, 18
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
        TouchStartPos = None
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
                                     TouchStartPos = None }, Cmd.none
                    elif not blocked then { model with MovingBlock = Some(processedMB)
                                                       PrectionBlock = 
                                                            if model.PrectionBlock.IsSome && action = Down then model.PrectionBlock
                                                            else Some(getPredictionBlock processedMB model.Boundry model.AllSquares) }, Cmd.none
                    else model, Cmd.none
        | _ -> model, Cmd.none
    | Tick ->
        if model.Speed <= model.SpeedCount && not model.IsPaused && not model.IsOver then
            { model with TimeCost = model.TimeCost + 1
                         SpeedCount = 0 }, Cmd.ofMsg (Down |> Msg.Action)
        else
            { model with TimeCost = model.TimeCost + 1
                         SpeedCount = model.SpeedCount + 1 }, Cmd.none
    | TouchStart (p1, p2) -> { model with TouchStartPos = Some(p1, p2) }, Cmd.none
    | TouchEnd -> { model with TouchStartPos = None }, Cmd.none
    | Pause -> { model with IsPaused = true }, Cmd.none
    | Continue -> { model with IsPaused = false }, Cmd.none
    | HideDetail -> { model with HideDetail = not model.HideDetail }, Cmd.none

let view (model : Model) (dispatch : Msg -> unit) =
    let generateColorString color =
        let r, g, b, a = color
        sprintf "rgba(%d, %d, %d, %f)" r g b a
    let squareSize = 15
    let pixleLength length = sprintf "%dpx" length
    let createSquare row column color =
        div [ Class "square"
              Style [
                BackgroundColor (generateColorString color)
                Width (pixleLength squareSize); Height (pixleLength squareSize)
                Position "absolute"
                Top (pixleLength (15 * row)); CSSProp.Left (pixleLength (15 * column))
            ]] []
    Browser.window.location.href <- "#root" // Focus root on init
    div [ Class "root"; Id "root"
          TabIndex 0.
          OnClick (fun e ->
            (e.target :?> Browser.HTMLElement).focus()
            Rotate |> Msg.Action |> dispatch)
          OnTouchStart (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchStart |> dispatch)
          OnTouchEnd (fun e ->
            match model.TouchStartPos with
            | Some (x0, y0) -> 
                let x, y = e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY
                let dx, dy = x - x0, y - y0
                let threshhold = 40.
                let extraSteps d =
                    (Math.abs d) - threshhold * 3.
                    |> fun x -> if x > 0. then int (x / 6.) else 0
                    |> fun x -> if x > 5 then 5 else x
                if Math.abs dx > Math.abs dy && Math.abs dx > threshhold then
                    if dx > 0. then for _ in 0..(extraSteps dx) do Right |> Msg.Action |> dispatch
                    else for _ in 0..(extraSteps dx) do Left |> Msg.Action |> dispatch
                if Math.abs dx < Math.abs dy && dy > threshhold then
                    for _ in 0..(extraSteps dy) do Down |> Msg.Action |> dispatch
            | _ -> ()
            TouchEnd |> dispatch) 
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
                        yield div [ Class "playground";
                              Style [ Width (pixleLength ((snd model.Boundry + 1) * squareSize))
                                      Height (pixleLength ((fst model.Boundry + 1) * squareSize)) ] ]
                            [
                                for row in 0..(fst model.Boundry) do
                                    for col in 0..(snd model.Boundry) ->
                                        createSquare row col (255, 255, 255, 0.)
                                for block in model.AllSquares -> 
                                    createSquare (fst block.Location) (snd block.Location) (0, 0, 0, 1.)
                                if model.PreviewBlock.IsSome && model.MovingBlock.IsSome &&
                                   not (model.MovingBlock.Value.Squares |> List.exists (fun x -> fst x.Location < 5)) then
                                    for block in model.PreviewBlock.Value.Squares ->
                                        let r, g, b, a = block.Color
                                        createSquare (fst block.Location) (snd block.Location) (r, g, b, 0.2)
                                if model.MovingBlock.IsSome then
                                    for block in model.MovingBlock.Value.Squares ->
                                        createSquare (fst block.Location) (snd block.Location) block.Color
                                if model.PrectionBlock.IsSome then
                                    for block in model.PrectionBlock.Value.Squares ->
                                        createSquare (fst block.Location) (snd block.Location) block.Color
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
                        yield div [ Class "center-h info score"] 
                            [
                                div [] [str (sprintf "分数：%d" (model.Score))]
                                div [] [str (sprintf " 🙏 %s 秒" (string (model.TimeCost / 10)))]
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
|> Program.withDebugger
#endif
|> Program.run