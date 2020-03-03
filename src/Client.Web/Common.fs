module Client.Common


type NeedDefine = unit

type ErrorCode = string
type ErrorMessage = string

type ServerError = NeedDefine

type ClientError =
    | GeneralError of ErrorCode * ErrorMessage
    | ServerError of ServerError
    | UnknowExn of exn
