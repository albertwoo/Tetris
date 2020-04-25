namespace Server.Dtos

open System


[<AutoOpen>]
module Constants =
    let [<Literal>] RobotCheckerIdKey = "X-Robot-Checker-Id"
    let [<Literal>] RobotCheckerValueKey = "X-Robot-Checker-Value"


type RobotChecker =
    { Id: Guid
      Base64ImageSource: string }
