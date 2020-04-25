namespace Server.Grains

open System
open Server.Grains.Interfaces
open FSharp.Control.Tasks
open Orleans
open SixLabors.ImageSharp
open SixLabors.Primitives
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing


type RobotCheckerGrain() =
    inherit Grain()

    let width = 560.f
    let height = 100.f
    let scale = 10.f
    let expiredDate = DateTime.Now.AddMinutes(2.)
    let expected = System.Random().Next(0, int(width - (3.f * scale))) |> float32

    interface IRobotCheckerGrain with
        member _.Check value =
            task {
                let x = expected
                if DateTime.Now > expiredDate then return false
                else return (width * value - expected) < 10.f
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
                        .Draw(pen, RectangleF(expected, h + 3.f * scale, scale, scale))
                        .Draw(pen, RectangleF(expected + scale, h + 3.f * scale, scale, scale))
                    |> ignore
                )
                return image.ToBase64String(Formats.Png.PngFormat.Instance)
            }
