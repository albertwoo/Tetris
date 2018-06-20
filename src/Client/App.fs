module Client
open System
open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Import
open Fulma
open Fulma.FontAwesome
open TetrisDomain

type Model = {
    Tetris: Tetris.Model
    TimeCost: int
    TouchStartPoint: (float * float) option
    TouchMovingPoint: (float * float) option
    TouchTime: DateTime option
    IsPaused: bool
    IsRestarting: bool
    HideDetail: bool }

type Msg =
    | TetrisMsg of Tetris.Msg
    | BeginRestart | CancelRestart | Restart
    | Tick
    | TouchStart of float * float | TouchMove of float * float | TouchEnd of float * float
    | Pause | Continue
    | HideDetail

let init () =
    {
        Tetris = Tetris.init()
        TimeCost = 0
        TouchStartPoint = None
        TouchMovingPoint = None
        TouchTime = None
        IsPaused = false
        IsRestarting = false
        HideDetail = true
    }, Cmd.none

type ModelOption<'a> = M of Model | P of 'a

let update msg model: Model * Cmd<Msg> =
    match msg with
    | BeginRestart -> { model with IsRestarting = true }, Cmd.none
    | CancelRestart -> { model with IsRestarting = false }, Cmd.none
    | Restart -> init ()
    | TetrisMsg msg' ->
        if model.Tetris.IsOver || model.IsPaused || model.IsRestarting then
            model, Cmd.none
        else
            let tetrisModel, cmd = Tetris.update msg' model.Tetris
            { model with Tetris = tetrisModel }, cmd
    | Tick ->
        if model.Tetris.IsOver || model.IsPaused || model.IsRestarting then
            model, Cmd.none
        elif model.Tetris.Speed <= model.Tetris.SpeedCount then
            { model with TimeCost = model.TimeCost + 1
                         Tetris = { model.Tetris with SpeedCount = 0 } }, Cmd.ofMsg (Down |> Tetris.Action |> Msg.TetrisMsg)
        else
            { model with TimeCost = model.TimeCost + 1
                         Tetris = { model.Tetris with SpeedCount = model.Tetris.SpeedCount + 1 } }, Cmd.none
    | TouchStart (x, y) ->
        { model with
            TouchStartPoint = Some(x, y)
            TouchMovingPoint = Some(x, y)
            TouchTime = Some(DateTime.Now) }, Cmd.none
    | TouchMove (x, y) ->
        match model.TouchMovingPoint with
        | Some (x0, y0) ->
            let model1 = { model with TouchMovingPoint = Some(x ,y) }
            let dx, dy = x - x0, y - y0
            let threshhold = 30.
            if Math.Abs dx > Math.Abs dy && Math.Abs dx > threshhold  then
                if dx > 0. then
                    model1, Cmd.ofMsg (Right |> Tetris.Action |> TetrisMsg)
                else
                    model1, Cmd.ofMsg (Left |> Tetris.Action |> TetrisMsg)
            elif Math.Abs dx < Math.Abs dy && dy > threshhold then
                model1, Cmd.ofMsg (Down |> Tetris.Action |> TetrisMsg)
            else model, Cmd.none
        | _ -> model, Cmd.none
    | TouchEnd (x, y) ->
        match model.TouchStartPoint with
        | Some (x0, y0) -> 
            let model' = { model with
                            TouchStartPoint = None
                            TouchMovingPoint = None
                            TouchTime = None }
            let dx, dy = x - x0, y - y0
            let threshhold = 80.
            let isQuickTouch = (DateTime.Now -  model.TouchTime.Value).Ticks < TimeSpan.FromMilliseconds(200.).Ticks
            // if model.Tetris.IsOver then model', Cmd.ofMsg Restart
            if isQuickTouch && Math.Abs dx < 10. && Math.Abs dy < 10. then
                model', Cmd.ofMsg (Rotate |> Tetris.Action |> TetrisMsg)
            elif isQuickTouch && Math.Abs dx > Math.Abs dy && Math.Abs dx > threshhold then
                if dx > 0. then model', Cmd.ofMsg (Tetris.ReachRight |> TetrisMsg)
                else model', Cmd.ofMsg (Tetris.ReachLeft |> TetrisMsg)
            elif isQuickTouch && Math.Abs dx < Math.Abs dy && Math.Abs dy > threshhold then
                model', Cmd.ofMsg (Tetris.ReachBottom |> TetrisMsg) 
            else model', Cmd.none
        | _ -> model, Cmd.none
    | Pause -> { model with IsPaused = true; }, Cmd.none
    | Continue -> { model with IsPaused = false }, Cmd.none
    | HideDetail -> { model with HideDetail = not model.HideDetail }, Cmd.none
   
let view (model : Model) (dispatch : Msg -> unit) =
    let iconButton onClick icon text =
        Button.button
            [
                Button.Color IsDanger; Button.Size IsSmall
                Button.OnClick onClick
            ]
            [
                Icon.faIcon [ Icon.Size IsLarge ] [ Fa.icon icon ]
                span [] [str text]
            ]
        
    div [ Class "root"; Id "root"
          TabIndex 0.
          OnClick (fun e -> (e.target :?> Browser.HTMLElement).focus())
          OnTouchStart (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchStart |> dispatch)
          OnTouchMove (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchMove |> dispatch)
          OnTouchEnd (fun e -> (e.changedTouches.[0.].clientX, e.changedTouches.[0.].clientY) |> TouchEnd |> dispatch)
          OnKeyDown (fun e ->
            if e.keyCode = 37. then Left |> Tetris.Action |> Msg.TetrisMsg |> dispatch
            elif e.keyCode = 38. then Rotate |> Tetris.Action |> Msg.TetrisMsg |> dispatch
            elif e.keyCode = 39. then Right |> Tetris.Action |> Msg.TetrisMsg |> dispatch
            elif e.keyCode = 40. then Down |> Tetris.Action |> Msg.TetrisMsg |> dispatch )
        ]
        [
            if not model.IsRestarting then
                yield div [ Class "container" ]
                    [
                        yield div [ Class "playground"] [ Tetris.view model.Tetris ]
                        yield div [ Class "center-h info score"] 
                            [
                                yield div [] [str (sprintf "分数：%d ❤ 等级：%d" (model.Tetris.Score) (Tetris.getTetrisLevel (model.Tetris)))]
                                yield div [] [str (sprintf " 🙏 %s 秒" (string (model.TimeCost / 10)))]
                                if model.Tetris.IsOver then yield div [] [str "😝游戏结束"]
                            ]
                        yield div [ Class "controls" ]
                            [
                                iconButton (fun e -> e.stopPropagation(); HideDetail |> dispatch)
                                    Fa.I.Expand
                                    (if model.HideDetail then "更多" else "隐藏")
                                iconButton (fun e -> e.stopPropagation(); BeginRestart |> dispatch)
                                    Fa.I.Refresh "重新开始"
                                iconButton (fun e -> e.stopPropagation(); (if model.IsPaused then Continue else Pause) |> dispatch)
                                    (if model.IsPaused then Fa.I.Forward else Fa.I.Pause)
                                    (if model.IsPaused then "继续" else "暂停")
                            ]
                        if not model.HideDetail then 
                            yield div [ Class "center-v info" ]
                                [
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
                                iconButton (fun e -> e.stopPropagation(); Restart |> dispatch)
                                    Fa.I.Refresh "是"
                                iconButton (fun e -> e.stopPropagation(); CancelRestart |> dispatch)
                                    Fa.I.Pause "否"
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