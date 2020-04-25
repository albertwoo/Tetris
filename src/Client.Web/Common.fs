namespace global


type NeedDefine = unit

type ErrorCode = string
type ErrorMessage = string

type ServerError = string

type ClientError =
    | GeneralError of ErrorCode * ErrorMessage
    | ServerError of ServerError
    | UnknowExn of exn
    | DtoParseError of string


[<RequireQualifiedAccess>]
type Deferred<'T> =
    | NotStartYet
    | Loading
    | Loaded of 'T
    | LoadFailed of ClientError
    | Reloading of 'T
    | ReloadingFailed of 'T * ClientError

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
        | Deferred.ReloadingFailed (x, _) -> Some x
        | _ -> None
