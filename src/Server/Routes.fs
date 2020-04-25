module Server.Routes

open System
open Giraffe
open FSharp.Control.Tasks
open Orleans

open Server.Common
open Server.Grains.Interfaces
open Server.Dtos.Game


let all: HttpHandler =
    choose [
        GET >=> routeCi "/health"    >=> text "Health"

        subRouteCi "/api" (choose [
            POST    >=> routeCi "/game/ping"     
                    >=> fun nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let gamebord = factory.GetGrain<IGameBoardGrain>(int64 Constants.GameZone1)
                            let ip = ctx.Request.HttpContext.Connection.RemoteIpAddress.ToString()
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
                                  TopRanks = state.Ranks |> List.map (fun x -> { Id = x.Id; PlayerName = x.PlayerName; Score = x.Score; RecordDate = x.RecordDate }) }
                            return! json board  nxt ctx
                        }
                            
            POST    >=> routeCi "/player/record"
                    >=> fun nxt ctx ->
                        task {
                            let! payload = ctx.BindJsonAsync<NewRecord>()
                            let factory = ctx.GetService<IGrainFactory>()
                            let player = factory.GetGrain<IPlayerGrain>(payload.PlayerName)
                            let! id = player.AddRecord(payload.PlayerPassword, { Id = 0; PlayerName = payload.PlayerName; GameEvents = payload.GameEvents; Score = payload.Score; RecordDate = DateTime.Now })
                            return! HttpStatusCodeHandlers.Successful.CREATED id nxt ctx
                        }

            GET     >=> routeCif "/player/%s/record/%i"
                        (fun (playerName, recordId) nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let player = factory.GetGrain<IPlayerGrain>(playerName)
                            let! record = player.GetRecord(recordId)
                            return! json record nxt ctx
                        })
        ])
    ]
