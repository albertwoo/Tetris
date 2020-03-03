module Server.Routes

open Giraffe


let all: HttpHandler =
    choose [
        GET     >=> routeCi "/hello"    >=> text "Hi"
        GET     >=> text "Home"
    ]