namespace Tetris.Server.WebApi.Grain

open System
open FSharp.Control.Tasks
open Orleans
open SixLabors.ImageSharp
open SixLabors.Primitives
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Tetris.Server.WebApi.Grain.Interfaces


type RobotCheckerGrain() =
    inherit Grain()

    let width = 560.f
    let height = 100.f
    let scale = 15.f
    let expireDate = DateTime.Now.AddSeconds(15.)
    let expected = System.Random().Next(0, int(width - (3.f * scale))) |> float32

    interface IRobotCheckerGrain with
        member _.Check value =
            task {
                return
                    if DateTime.Now > expireDate then ValidateResult.Expired
                    elif (width * value - expected) > 5.f then ValidateResult.InvalidValue
                    else ValidateResult.Valid
            }

        member _.GetCheckerImage () =
            task {
                use image = new Image<Rgba32>(int width, int height)
                image.Mutate (fun ctx ->
                    let pen = Pens.Solid(Color.Green, 2.f)
                    let h = height / 2.f - 2.f * scale
                    ctx
                        .Draw(pen, RectangleF(expected, h, scale, scale))
                        .Draw(pen, RectangleF(expected, h + scale, scale, scale))
                        .Draw(pen, RectangleF(expected, h + 2.f * scale, scale, scale))
                        .Draw(pen, RectangleF(expected + scale, h + 2.f * scale, scale, scale))
                    |> ignore
                )
                return image.ToBase64String(Formats.Png.PngFormat.Instance)
            }

        member _.GetExpireDate () =
            task {
                return expireDate
            }
