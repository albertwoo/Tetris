module Tetris.Core.Utils

open System

let rotate block =
    match block.CenterSquare with
    | None -> block
    | Some center ->
        { block with
            Squares =
                block.Squares
                |> List.map (fun s ->
                    { s with
                        X = center.X - (s.Y - center.Y)
                        Y = center.Y + (s.X - center.X) }
                ) }


let moveL square = { square with X = square.X - 1 }
let moveR square = { square with X = square.X + 1 }
let moveD square = { square with Y = square.Y + 1 }

let move moveSquare block =
    { block with
        Squares = block.Squares |> List.map moveSquare
        CenterSquare = block.CenterSquare |> Option.map moveSquare }


let getBlockSquares block =
    match block.CenterSquare with
    | None -> block.Squares
    | Some s -> s::block.Squares

let isCollidedWith block squares2 =
    block
    |> getBlockSquares
    |> Seq.exists (fun s -> squares2 |> Seq.contains s)

let boolToOption = function true -> Some() | false -> None


let createLeftBorder size = [ for y in 0..size.Height -> { X = -1; Y = y } ]
let createRightBorder size = [ for y in 0..size.Height -> { X = size.Width; Y = y } ]
let createBottomBorder size = [ for x in 0..size.Width-1 -> { X = x; Y = size.Height } ]
    

// FSharp active pattern
let (|CollidedWithBlocks|_|) blocks block = blocks |> List.collect getBlockSquares |> isCollidedWith block |> boolToOption
let (|CollidedWithSquares|_|) squares block = squares |> isCollidedWith block |> boolToOption

let (|CollidedWithBorderLeft|_|) size block =
    createLeftBorder size
    |> isCollidedWith block
    |> boolToOption

let (|CollidedWithBorderRight|_|) size block =
    createRightBorder size
    |> isCollidedWith block
    |> boolToOption

let (|CollidedWithBorderBottom|_|) size block =
    createBottomBorder size
    |> isCollidedWith block
    |> boolToOption


let createDefaultPlayground (width, height) =
    let size = { Width = width; Height = height }
    {
        IsGameOver = false
        Score = 0
        Size = size
        LeftBorder = createLeftBorder size
        RightBorder = createRightBorder size
        BottomBorder = createBottomBorder size
        MovingBlock = None
        PredictionBlock = None
        RemainSquares = []
    }


type RelativeSquare =
    | NormalSquare of Square
    | CenterSquare of Square


let predefinedBlocks =
    [
        """
        cooo
        """

        """
        oo
        oo
        """

        """
        xox
        oco
        """

        """
        ox
        ox
        co
        """

        """
        ocx
        xoo
        """
    ]
    |> List.map (fun str ->
        let squares =
            str.Split '\n'
            |> Seq.map (fun x -> x.Trim())
            |> Seq.mapi (fun row line ->
                line
                |> Seq.mapi (fun column ch ->
                    match ch with
                    | 'c' -> CenterSquare { X = column; Y = row } |> Some
                    | 'o' -> NormalSquare { X = column; Y = row } |> Some
                    | _ -> None 
                )
                |> Seq.choose (fun x -> x)
            )
            |> Seq.concat
        {
            Squares = squares |> Seq.choose (function NormalSquare x -> Some x | CenterSquare _ -> None) |> Seq.toList
            CenterSquare = squares |> Seq.tryPick (function CenterSquare x -> Some x | NormalSquare x -> None)
        }
    )

let generateRamdomBlock moveRight =
    let move block =
        [1..moveRight]
        |> List.fold (fun s _ -> move moveR s) block

    predefinedBlocks
    |> List.item (
        System.Random().Next(0, predefinedBlocks.Length - 1)
    )
    |> move
