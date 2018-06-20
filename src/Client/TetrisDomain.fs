module TetrisDomain
open System

type Square = { Location: int * int; Color: int * int * int * float }
type BlockType = T | L | J | I | O | Z | RZ | X
type Block = { Type: BlockType; Squares: Square list }
type Action = Rotate | Left | Right | Down

let generateRandomColor () =
    let rand = new Random()
    rand.Next(0, 188), rand.Next(0, 188), rand.Next(0, 188), 1.
let generateRandomBlockType () =
    let rand = (new Random()).Next(1, 100)
    let rate = 100 / 7
    if rand < rate then T
    elif rand >= rate && rand < rate * 2 then L
    elif rand >= rate * 2 && rand < rate * 3 then J
    elif rand >= rate * 3 && rand < rate * 4 then I
    elif rand >= rate * 4 && rand < rate * 5 then O
    elif rand >= rate * 5 && rand < rate * 6 then Z
    elif rand >= rate * 6 && rand < (rate * 7 - 1) then RZ
    else X
let generateBlock color blockType =
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
    | Z -> 
        { 
            Type = Z; 
            Squares = [
                { Location = (1, 1); Color = usedColor }
                { Location = (0, 0); Color = usedColor }
                { Location = (0, 1); Color = usedColor }
                { Location = (1, 2); Color = usedColor }
            ]
        }
    | RZ -> 
        { 
            Type = RZ; 
            Squares = [
                { Location = (0, 1); Color = usedColor }
                { Location = (0, 2); Color = usedColor }
                { Location = (1, 1); Color = usedColor }
                { Location = (1, 0); Color = usedColor }
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

let isSquaresCollided squares1 squares2 =
    squares1 |> List.exists (fun x -> squares2 |> List.exists (fun y -> x.Location = y.Location))
let isBlocked boundry squares block =
    if isSquaresCollided squares block.Squares then true
    else block.Squares |> List.exists (fun x ->
        fst x.Location > fst boundry || snd x.Location < 0 || snd x.Location > snd boundry)

let transformBlock action block =
    match action with
    | Rotate -> 
        if block.Type = O then block
        else
            let centerSquare = block.Squares |> List.head
            let r, c = centerSquare |> fun x -> x.Location
            { block with
                Squares = centerSquare :: block.Squares
                                          |> List.skip 1 |> List.map (fun x ->
                                            let r1, c1 = x.Location
                                            { x with Location = r + (c1 - c), c - (r1 - r) }) }
    | _ ->
        { block with
            Squares =
                block.Squares 
                |> List.map (fun x -> 
                    let (r, c) = x.Location
                    { x with Location =
                                 (if action = Down then r + 1 else r),
                                 (if action = Left then c - 1 elif action = Right then c + 1 else c)})}
let rec moveUntilBlocked boundry squares action mb =
    let pb = transformBlock action mb 
    if isBlocked boundry squares pb then mb
    else moveUntilBlocked boundry squares action pb

let cleanSquares boundry squares =
    let matchedRowSquares = 
        let row, column = boundry
        [for r in 0..row do
            let matchedSquares =
                [for c in 0..column do
                    let square = squares |> List.tryFind (fun x -> x.Location = (r, c))
                    if square.IsSome then yield square.Value ]
            if matchedSquares.Length = column + 1 then yield (r, matchedSquares)]
    let removeRow squares row =
        squares 
        |> List.filter (fun x -> fst x.Location <> row)
        |> List.map (fun x -> 
            if fst x.Location < row then { x with Location = (fst x.Location + 1, snd x.Location) }
            else x)
    let rec clean squares rowSquares =
        if List.isEmpty rowSquares then squares
        else clean
                (removeRow squares (rowSquares |> List.head |> fst))
                (List.skip 1 rowSquares)
    let score =
        if matchedRowSquares.Length = 0 then 0
        else int (Math.Pow(2., float (matchedRowSquares.Length - 1)))
    clean squares matchedRowSquares, score
    
let calculateSpeed defaultSpeed score = 
    let s = defaultSpeed - score / 10
    if s < 1 then 1 else s