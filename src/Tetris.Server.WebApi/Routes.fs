module Tetris.Server.Routes

open Giraffe
open Tetris.Server.HttpHandlers


let cacheOneMonth = publicResponseCaching (60 * 60 * 24 * 30) None

let (>>=>) handler1 handler2 x = handler1 >=> handler2 x


let all: HttpHandler =
    choose [
        GET >=> routeCi "/health" >=> text "Health"

        subRouteCi "/api" (choose [
            GET     >=> routeCif "/translations/%s" (cacheOneMonth >>=> Translations.getTranslations)
                                 
            GET     >=> routeCi  "/robot/checker" >=> RobotChecker.getChecker
                                 
            POST    >=> routeCif "/game/ping/%s" GameBoard.updateOnline
            GET     >=> routeCi  "/game/board" >=> GameBoard.getGameBoardInfo
                                 
            POST    >=> routeCi  "/player/record" >=> RobotChecker.checkHeader >=> Player.uploadRecord
            GET     >=> routeCif "/player/%s/record/%i" Player.getRecord
            GET     >=> routeCif "/player/%s/record/%i/events" Player.getRecordEvents
        ])
    ]
