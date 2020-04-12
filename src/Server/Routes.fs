module Server.Routes

open Giraffe
open FSharp.Control.Tasks
open Orleans
open Server.Grains.Interfaces


let all: HttpHandler =
    choose [
        GET     >=> routeCi "/hello"    >=> text "Hi"

        GET     >=> routeCi "/api/game/onlineusers/count" 
                >=> fun nxt ctx ->
                        task {
                            let factory = ctx.GetService<IGrainFactory>()
                            let gamebord = factory.GetGrain<IGameBoardGrain>(0L)
                            do! gamebord.UpdateGame GameEvent.IncreaseOnlineCount
                            let! count = gamebord.GetOnlineCount()
                            return! json count nxt ctx
                        }
    ]