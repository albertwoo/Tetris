module TetrisDomain

open System

type Square = { Location: int * int; Color: int * int * int * float }
type BlockType = T | L | J | I | O | X
type Block = { Type: BlockType; Squares: Square list }
type Action = Rotate | Left | Right | Down

let generateRandomColor () =
    let rand = new Random()
    rand.Next(0, 188), rand.Next(0, 188), rand.Next(0, 188), 1.
let getRandomBlockType () =
    let rand = (new Random()).Next(1, 100)
    if rand < 20 then T
    elif rand >= 2 && rand < 40 then L
    elif rand >= 40 && rand < 60 then J
    elif rand >= 60 && rand < 80 then I
    elif rand >= 80 && rand < 99 then O
    else X
let generateBlock blockType color =
    let usedColor = if Option.isSome color then color.Value else generateRandomColor()
    match blockType with 
    | T -> 
        {
            Type = T; 
            Squares = [
                { Location = (0, 1); Color = usedColor }
                { Location = (0, 0); Color = usedColor }
                { Location = (0, 2); Color = usedColor }
                { Location = (1, 1); Color = usedColor }
            ]
        }
    | L -> 
        { 
            Type = L; 
            Squares = [
                { Location = (2, 0); Color = usedColor }
                { Location = (0, 0); Color = usedColor }
                { Location = (1, 0); Color = usedColor }
                { Location = (2, 1); Color = usedColor }
            ]
        }
    | J -> 
        { 
            Type = J; 
            Squares = [
                { Location = (2, 1); Color = usedColor }
                { Location = (0, 1); Color = usedColor }
                { Location = (1, 1); Color = usedColor }
                { Location = (2, 0); Color = usedColor }
            ]
        }
    | I -> 
        { 
            Type = I; 
            Squares = [
                { Location = (0, 0); Color = usedColor }
                { Location = (1, 0); Color = usedColor }
                { Location = (2, 0); Color = usedColor }
                { Location = (3, 0); Color = usedColor }
            ]
        }
    | O -> 
        { 
            Type = O; 
            Squares = [
                { Location = (0, 0); Color = usedColor }
                { Location = (0, 1); Color = usedColor }
                { Location = (1, 0); Color = usedColor }
                { Location = (1, 1); Color = usedColor }
            ]
        }
    | X -> 
        { 
            Type = X; 
            Squares = [
                { Location = (1, 1); Color = usedColor }
                { Location = (0, 1); Color = usedColor }
                { Location = (1, 0); Color = usedColor }
                { Location = (1, 2); Color = usedColor }
                { Location = (2, 1); Color = usedColor }
            ]
        }
let setBlockToMid columns movingBlock =
    { 
        movingBlock with 
            Squares = 
                movingBlock.Squares 
                |> List.map (fun x -> 
                    {
                        Location = (fst x.Location, snd x.Location + columns / 2 - 1)
                        Color = x.Color
                    })
    }
let transformBlock action movingBlock amount =
    match action with
    | Rotate -> 
        if movingBlock.Type = O then movingBlock
        else
            let centerSquare = movingBlock.Squares |> List.head
            let r, c = centerSquare |> fun x -> x.Location
            { movingBlock with
                Squares = 
                    centerSquare
                    :: movingBlock.Squares |> List.skip 1 |> List.map (fun x ->
                        let r1, c1 = x.Location
                        { x with Location = r + (c1 - c), c - (r1 - r) }) }
    | _ ->
        { movingBlock with
            Squares =
                movingBlock.Squares 
                |> List.map (fun x -> 
                    let (r, c) = x.Location
                    { Location = (if action = Down then r + amount else r),
                                 (if action = Left then c - amount elif action = Right then c + amount else c);
                      Color = x.Color })}
let isOutOfBoundry block boundry = block.Squares |> List.exists (fun x -> snd x.Location < 0 || snd x.Location > snd boundry)
let isBlocked block boundry squares =
    if squares |> List.exists (fun x -> block.Squares |> List.exists (fun y -> x.Location = y.Location)) then true
    else block.Squares |> List.exists (fun x -> fst x.Location > fst boundry)
let isNewBornBlock block = block.Squares |> List.exists (fun x -> fst x.Location = 0)
let getScore boundry squares =
    let rowSquares = 
        let row, column = boundry
        [for r in 0..row do
            let blocks =
                [for c in 0..column do
                    let block = squares |> List.tryFind (fun x -> x.Location = (r, c))
                    if block.IsSome then yield block.Value ]
            if List.length blocks = column + 1 then yield (r, blocks)]
    let removeRow row targetSquare sourceSquare =
        sourceSquare 
        |> List.filter (fun x -> not (targetSquare |> List.exists (fun y -> y = x)))
        |> List.map (fun x -> 
            if fst x.Location < row then { Location = (fst x.Location + 1, snd x.Location); Color = x.Color }
            else x)       
    let mutable remainSquares = squares
    rowSquares
    |> List.iter (fun (r, targetBlocks) -> remainSquares <- removeRow r targetBlocks remainSquares)
    let score = if rowSquares.Length = 0 then 0 else int (Math.Pow(2., float (rowSquares.Length - 1)))
    remainSquares, score
let getPredictionBlock movingBlock boundry squares =
    let getMinRow sqs = if List.isEmpty sqs then 0 
                        else sqs |> List.minBy (fun x -> fst x.Location) |> fun x -> fst x.Location
    let mbMin = getMinRow movingBlock.Squares
    [mbMin..(fst boundry + 1)]
    |> List.map (fun x -> transformBlock Down movingBlock (x - mbMin))
    |> List.find (fun x -> isBlocked x boundry squares)
    |> fun x -> transformBlock Down x -1
    |> fun x -> { x with Squares = x.Squares |> List.map (fun y ->
                        let r, g, b, a = y.Color
                        { y with Color = r, g, b, 0.3 }) }