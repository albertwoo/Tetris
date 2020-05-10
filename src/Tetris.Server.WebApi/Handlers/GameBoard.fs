module Tetris.Server.Handlers.GameBoard

open Giraffe
open FSharp.Control.Tasks
open Orleans

open Tetris.Server.WebApi.Common
open Tetris.Server.WebApi.Grain.Interfaces
open Tetris.Server.WebApi.Dtos
open Tetris.Server.WebApi.Dtos.Game


let updateOnline: HttpHandler =
    fun nxt ctx ->
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


let getGameBoardInfo: HttpHandler =
    fun nxt ctx ->
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

