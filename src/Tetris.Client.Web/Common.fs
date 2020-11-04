namespace global

open Fun.Result


type NeedDefine = unit

type ErrorCode = string
type ErrorMessage = string

type ServerError = string

type ClientError =
    | GeneralError of ErrorCode * ErrorMessage
    | ServerError of ServerError
    | RequestError of ErrorCode
    | UnknowExn of exn
    | DtoParseError of string


type ClientContext =
    { Lang: Lang
      Translations: Map<string, string> }


and [<RequireQualifiedAccess>] Lang =
    | EN
    | CN


[<RequireQualifiedAccess>]
type Deferred<'T> = DeferredState<'T, ClientError>

[<RequireQualifiedAccess>]
type AsyncOperation<'T> = DeferredOperation<'T, ClientError>


[<AutoOpen>]
module Helpers =
    let (|DeferredValue|_|) = function
        | Deferred.Loaded x 
        | Deferred.Reloading x
        | Deferred.ReloadFailed (x, _) -> Some x
        | _ -> None

    type ClientContext with
        member this.Translate key = 
            this.Translations 
            |> Map.tryFind key
            |> Option.defaultValue key
        
        static member defaultValue =
            { Lang = Lang.CN
              Translations = Map.empty }
