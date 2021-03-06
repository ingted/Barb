﻿module Barb.Tests.PredicateLanguageGenericInteropTests

open System

open Barb
open Barb.Compiler
open Barb.Representation

open System.Collections.Generic

open Xunit

let returnit (v: 'a) = v

[<Fact>]
let ``Barb should be able to call simple generic functions`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "returnit 10"

    let func = new BarbFunc<unit,int>(pred, settings)
    let res = func.Execute()
    Assert.Equal(10, res)

let returnsecond (a: 'a) (b: 'b) = b
    
[<Fact>]
let ``Barb should be able to call simple generic functions with two parameters one generic`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "returnsecond 10 \"Hello\""

    let func = new BarbFunc<unit,string>(pred, settings)
    let res = func.Execute()
    Assert.Equal<string>("Hello", res)

open System.Collections.Generic

let isCountEqualTo (l: IEnumerable<'T>) (len: int) =
    (l |> Seq.length) = len

[<Fact>]
let ``Barb should be able to call a function with a multi parameter nested generic`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "isCountEqualTo [|10; 20; 30; 40|] 4"

    let func = new BarbFunc<unit,bool>(pred, settings)
    let res = func.Execute()
    Assert.Equal<bool>(true, res)

let seqHead (l: IEnumerable<'T>) = Seq.head l

[<Fact>]
let ``Barb should be able to call a function with a single parameter nested generic`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "seqHead [|10; 20; 30; 40|]"

    let func = new BarbFunc<unit,int>(pred, settings)
    let res = func.Execute()
    Assert.Equal<int>(10, res)

let seqLength (l: IEnumerable<'T>) = Seq.length l

[<Fact>]
let ``Barb should be able to call a function with a single parameter nested generic, twice empty`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "seqLength [| |] + seqLength [|10; 20; 30; 40|] + seqLength [| |]"

    let func = new BarbFunc<unit,int>(pred, settings)
    let res = func.Execute()
    Assert.Equal<int>(4, res)

type slPrm<'a> = { Value: 'a [] }

[<Fact>]
let ``Barb should be able to call a function with an empty single parameter nested generic (typed by return value)`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "seqLength Value"

    let func = new BarbFunc<slPrm<int>,int>(pred, settings)
    let res = func.Execute({ Value = Array.empty<int> })
    Assert.Equal<int>(0, res)

[<Fact>]
let ``Barb should be able to call a function with an empty single parameter nested generic (typed by return value) via seq`` () =
    let namespaces = BarbSettings.Default.Namespaces |> Set.add "Microsoft.FSharp.Collections"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "Seq.length Value"

    let func = new BarbFunc<slPrm<int>,int>(pred, settings)
    let res = func.Execute({ Value = Array.empty<int> })
    Assert.Equal<int>(0, res)

[<Fact>]
let ``Barb should be able to call a constructor with two generic args`` () =
    let namespaces = BarbSettings.Default.Namespaces 
                     |> Set.add "System.Collections.Generic"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 

    let pred = "new KeyValuePair ('Hello', 'World')" in
        let func = new BarbFunc<unit,KeyValuePair<string, string>>(pred, settings) in 
        let res = func.Execute()
        Assert.Equal<KeyValuePair<string, string>>(new KeyValuePair<_,_>("Hello", "World"), res)  

let KeyValuePair (k: 'k) (v: 'v) = new KeyValuePair<'k,'v>(k,v)

[<Fact>]
let ``Barb should be able to call a constructing function with two generic args`` () =
    let namespaces = BarbSettings.Default.Namespaces 
                     |> Set.add "System.Collections.Generic"
                     |> Set.add "Barb.Tests.PredicateLanguageGenericInteropTests"
    let settings = { BarbSettings.Default with Namespaces = namespaces } 
    let pred = "KeyValuePair 'Hello' 'World'" in
        let func = new BarbFunc<unit,KeyValuePair<string, string>>(pred, settings) 
        let res = func.Execute()
        Assert.Equal<KeyValuePair<string, string>>(new KeyValuePair<_,_>("Hello", "World"), res)  
