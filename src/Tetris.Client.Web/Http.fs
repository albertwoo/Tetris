module Tetris.Client.Web.Http

open Fable.SimpleHttp
open Thoth.Json
open Fun.Result


let inline fromJson<'T> str = Decode.Auto.fromString<'T>(str)
let inline toJson obj = Encode.Auto.toString(4, obj)


let inline requestJson url method data =
    Http.request url
    |> Http.method method
    |> Http.content (toJson data |> BodyContent.Text)
    |> Http.header (Headers.contentType "application/json")


let inline get url = Http.request url |> Http.method GET
let inline delete url = Http.request url |> Http.method DELETE
let inline postJson url data = requestJson url POST data
let inline putJson url data = requestJson url PUT data


let handle map (resp: HttpResponse) =
    match resp.statusCode with
    | x when x = 401 ->
        // Redirect to somewhere
        ClientError.ServerError "Not authenticated" |> Error
    | x when x > 399 && x < 500 ->
        ClientError.RequestError resp.responseText |> Error
    | x when x > 500 ->
        ClientError.ServerError resp.responseText |> Error
    | _ ->
        map resp |> Result.mapError ClientError.DtoParseError

let inline handleAsync (onOk: HttpResponse -> 'T2) (onError: ClientError -> 'T2) request =
    request
    |> Http.send
    |> Async.map (handle Ok)
    |> Async.map (function
      | Ok x    -> onOk x
      | Error e -> onError e)

let inline handleJson<'T> = handle (fun x -> fromJson<'T> x.responseText)

let inline handleJsonAsync (onOk: 'T -> 'T2) (onError: ClientError -> 'T2) request =
    request
    |> Http.send
    |> Async.map handleJson<'T>
    |> Async.map (function
        | Ok x    -> onOk x
        | Error e -> onError e)


let inline handleAsyncOperation operation = 
    handleJsonAsync 
        (AsyncOperation.Finished >> operation) 
        (AsyncOperation.Failed >> operation)
