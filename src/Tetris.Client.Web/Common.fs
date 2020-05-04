namespace global


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
type Deferred<'T> =
    | NotStartYet
    | Loading
    | Loaded of 'T
    | LoadFailed of ClientError
    | Reloading of 'T
    | ReloadFailed of 'T * ClientError

[<RequireQualifiedAccess>]
type AsyncOperation<'T> =
    | Start
    | Finished of 'T
    | Failed of ClientError


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
