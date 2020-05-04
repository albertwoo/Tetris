module Server.Routes

open System
open System.IO
open Giraffe
open FSharp.Control.Tasks
open Orleans
open Fun.Result

open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces
open Tetris.Server.WebApi.Dtos
open Tetris.Server.WebApi.Dtos.Game


let robotCheckHeader: HttpHandler =
    fun nxt ctx ->
        task {
            let FobitErrorMsg = "Robot check failed"
            if ctx.Request.Headers.ContainsKey RobotCheckerIdKey &&
               ctx.Request.Headers.ContainsKey RobotCheckerValueKey
            then
                match Guid.TryParse(ctx.Request.Headers.Item(RobotCheckerIdKey).ToString()),
                      System.Single.TryParse(ctx.Request.Headers.Item(RobotCheckerValueKey).ToString())
                    with
                    | (true, id), (true, value)->
                        let factory = ctx.GetService<IGrainFactory>()
                        let robotChecker = factory.GetGrain<IRobotCheckerGrain>(id)
                        let! result = robotChecker.Check value
                        if result then return! nxt ctx
                        else return! HttpStatusCodeHandlers.RequestErrors.FORBIDDEN FobitErrorMsg nxt ctx
                    | _ ->
                        return! HttpStatusCodeHandlers.RequestErrors.FORBIDDEN FobitErrorMsg nxt ctx
            else
                return! HttpStatusCodeHandlers.RequestErrors.FORBIDDEN FobitErrorMsg nxt ctx
        }


let translationHanlder lang: HttpHandler =
    fun nxt ctx ->
        task {
            let lines = File.ReadAllLines("Translation.lang")
            let index = 
                lines
                |> Seq.tryHead
                |> Option.bind (fun head ->
                    head.Split ','
                    |> Seq.tryFindIndex (fun x -> x.Equals(lang, StringComparison.OrdinalIgnoreCase))
                )
            match index with
            | None -> return! RequestErrors.NOT_FOUND "" nxt ctx
            | Some index ->
                let translations =
                    lines
                    |> Seq.skip 1
                    |> Seq.choose (fun line ->
                        let columns = line.Split ','
                        option {
                            let! key = columns |> Seq.tryItem 0
                            let! text = columns |> Seq.tryItem index
                            return key, text
                        }
                    )
                    |> Map.ofSeq
                return! json translations nxt ctx
        }


let all: HttpHandler =
    choose [
        GET >=> routeCi "/health"    >=> text "Health"

        subRouteCi "/api" (choose [
            POST    >=> routeCi "/game/ping"     
                    >=> fun nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let gamebord = factory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
                            let ip = 
                                if ctx.Request.Headers.ContainsKey("X-Forwarded-For") 
                                then ctx.Request.Headers.["X-Forwarded-For"].Item(0)
                                else string ctx.Request.HttpContext.Connection.RemoteIpAddress
                            do! gamebord.Ping ip
                            return! text "pong" nxt ctx
                        }

            GET     >=> routeCi "/game/board" 
                    >=> fun nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let gamebord = factory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
                            let! state = gamebord.GetState()
                            let board: GameBoard =
                                { OnlineCount = state.OnlineIPs.Count
                                  TopRanks = 
                                    state.Ranks 
                                    |> List.map (fun x -> 
                                        { Id = x.Id
                                          PlayerName = x.PlayerName
                                          Score = x.Score
                                          RecordDate = x.RecordDate; 
                                          TimeCostInMs = x.TimeCostInMs }) }
                            return! json board  nxt ctx
                        }
                            
            POST    >=> routeCi "/player/record"
                    >=> robotCheckHeader
                    >=> fun nxt ctx ->
                        task {
                            let! payload = ctx.BindJsonAsync<NewRecord>()

                            if String.IsNullOrEmpty payload.PlayerName then
                                return! HttpStatusCodeHandlers.RequestErrors.BAD_REQUEST "Player name cannot be empty" nxt ctx
                            else
                                let factory = ctx.GetService<IGrainFactory>()
                                let player = factory.GetGrain<IPlayerGrain>(payload.PlayerName)
                                let newRecord: Record =
                                    { Id = 0
                                      PlayerName = payload.PlayerName
                                      Score = payload.Score
                                      RecordDate = DateTime.Now
                                      TimeCostInMs = payload.TimeCostInMs }
                                let! id = player.AddRecord(payload.PlayerPassword, newRecord, payload.GameEvents)
                                match id with
                                | Ok id   -> return! HttpStatusCodeHandlers.Successful.CREATED id nxt ctx
                                | Error e -> return! HttpStatusCodeHandlers.RequestErrors.BAD_REQUEST e nxt ctx
                        }

            GET     >=> routeCif "/player/%s/record/%i"
                        (fun (playerName, recordId) nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let player = factory.GetGrain<IPlayerGrain>(playerName)
                            let! record = player.GetRecord(recordId)
                            match record with
                            | Some x -> return! json record nxt ctx
                            | None   -> return! HttpStatusCodeHandlers.RequestErrors.NOT_FOUND "" nxt ctx
                        })

            GET     >=> routeCif "/player/%s/record/%i/events"
                        (fun (playerName, recordId) nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let player = factory.GetGrain<IPlayerGrain>(playerName)
                            let! record = player.GetRecordEvents(recordId)
                            match record with
                            | Some x -> return! text x nxt ctx
                            | None   -> return! HttpStatusCodeHandlers.RequestErrors.NOT_FOUND "" nxt ctx
                        })

            GET     >=> routeCi "/robot/checker"
                    >=> fun nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let id = Guid.NewGuid()
                            let robotChecker = factory.GetGrain<IRobotCheckerGrain>(id)
                            let! base64 = robotChecker.GetCheckerImage()
                            let! expireDate = robotChecker.GetExpireDate()
                            let data = { Id = id; Base64ImageSource = base64; ExpireDate = expireDate }
                            return! json data nxt ctx
                        }

            GET     >=> routeCif "/translations/%s" (fun lang -> publicResponseCaching (60 * 60 * 24 * 30) None >=> translationHanlder lang)
        ])
    ]
