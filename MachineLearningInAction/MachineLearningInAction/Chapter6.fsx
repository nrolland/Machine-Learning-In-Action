﻿// Support Vector Machine

open System

let rng = new Random()
let testData = [ for i in 1 .. 50 -> [ rng.NextDouble(); rng.NextDouble() ] ]
let testLabels = testData |> List.map (fun el -> if (el |> List.sum >= 0.5) then 1.0 else -1.0)

let clip min max x =
    if (x > max)
    then max
    elif (x < min)
    then min
    else x
    
let dot (vec1: float list) 
        (vec2: float list) =
    List.zip vec1 vec2
    |> List.map (fun e -> fst e * snd e)
    |> List.sum

let elementProduct (alphas: float list) labels =
    List.zip alphas labels |> List.map (fun (a, l) -> a * l)

let predict (data: list<float list>) labels alphas b i =
    let row = data.[i] 
    data 
    |> List.map (fun obs -> dot obs row)
    |> dot (elementProduct alphas labels)
    |> (+) b

// pick an index other than i in [0..(count-1)]
let pickAnother (rng: System.Random) i count = 
    let j = rng.Next(0, count - 1)
    if j >= i then j + 1 else j

let findLowHigh low high (label1, alpha1) (label2, alpha2) = 
    if label1 = label2
    then max low (alpha1 + alpha2 - high), min high (alpha2 - alpha1)
    else max low (alpha2 - alpha1),        min high (high + alpha2 - alpha1) 

         
let simpleSmo dataset (labels: float list) C tolerance iterations =
    
    let size = dataset |> List.length
    
    let b = 0.0
    let alphas = [ for i in 1 .. size -> 0.0 ]

    let rng = new Random()
    let lohi = findLowHigh 0.0 C

    let update i = 

        printfn "%i" i
        let iClass = labels.[i]
        let iError = (predict dataset labels alphas b i) - iClass
        let iAlpha = alphas.[i]

        if (iError * iClass < - tolerance && iAlpha < C) || (iError * iClass > tolerance && iAlpha > 0.0)
        then
            let j = pickAnother rng i size
            printfn "%i" j
            let jClass = labels.[j]
            let jError = (predict dataset labels alphas b j) - jClass
            let jAlpha = alphas.[j]

            let lo, hi = lohi (labels.[i], iAlpha) (labels.[j], jAlpha)
                
            if lo = hi 
            then 
                printfn "Low = High"
                b, alphas
            else
                let iObs, jObs = dataset.[i], dataset.[j]
                let eta = 2.0 * dot iObs jObs - dot iObs iObs - dot jObs jObs

                if eta >= 0.0 
                then 
                    printfn "ETA >= 0"
                    b, alphas
                else
                    let jAlphaNew = clip (jAlpha - (jClass * (iError - jError) / eta)) lo hi

                    if abs (jAlpha - jAlphaNew) < 0.00001 
                    then
                        printfn "j not moving enough"
                        b, alphas
                    else
                        let iAlphaNew = iAlpha + (iClass * jClass * (jAlpha - jAlphaNew))

                        let b1 = b - iError - iClass * (iAlphaNew - iAlpha) * (dot iObs iObs) - jClass * (jAlphaNew - jAlpha) * (dot iObs jObs)
                        let b2 = b - jError - iClass * (iAlphaNew - iAlpha) * (dot iObs jObs) - jClass * (jAlphaNew - jAlpha) * (dot jObs jObs)

                        let bNew =
                            if (iAlphaNew > 0.0 && iAlphaNew < C)
                            then b1
                            elif (jAlphaNew > 0.0 && jAlphaNew < C)
                            then b2
                            else (b1 + b2) / 2.0

                        printfn "Changed %i and %i" i j
                        b, alphas
        else b, alphas

    for i in 0 .. (size - 1) do
        let b, a = update i
        a |> List.iter (fun v -> printf "%f, " v)
        printf "%f" b
        printfn "Updated"

simpleSmo testData testLabels 5.0 0.1 100