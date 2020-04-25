module Client.Http

open Fable.SimpleHttp
open Fable.SimpleJson
open Fun.Result


let inline fromJson<'T> str = Json.tryParseAs<'T> str

let toJson obj = Json.stringify obj

let requestJson url method data =
    Http.request url
    |> Http.method method
    |> Http.content (toJson data |> BodyContent.Text)
    |> Http.header (Headers.contentType "application/json")


let get url = Http.request url |> Http.method GET
let delete url = Http.request url |> Http.method DELETE
let postJson url data = requestJson url POST data
let putJson url data = requestJson url PUT data


let handle map (resp: HttpResponse) =
    match resp.statusCode with
    | x when x = 401 ->
        // Redirect to somewhere
        ClientError.ServerError "Not authenticated" |> Error
    | x when x > 399 ->
        ClientError.ServerError resp.responseText |> Error
    | _ ->
        map resp
        |> Result.mapError ClientError.DtoParseError

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
