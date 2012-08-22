﻿module Barb.Representation

open System
open System.Collections.Generic

type BarbSettings = 
    {
        BindGlobalsWhenReducing: bool
        Namespaces: string Set
        AdditionalBindings: IDictionary<string,obj>
    }
    with static member Default = 
            { 
                BindGlobalsWhenReducing = true
                AdditionalBindings = [] |> dict
                Namespaces = [null; ""; "System"; "Microsoft.FSharp"; "Microsoft.FSharp.Collections"; "Barb.Lib"] |> Set.ofList
            }

type MethodSig = ((obj array -> obj) * Type array) list

// Mutable so we can update the bindings with itself for easy recursion.
type LambdaRecord = { Params: string list; mutable Bindings: Bindings; Contents: ExprRep }

and ExprTypes = 
    | Unit
    | Invoke
    | New
    | Method of MethodSig
    | IndexedProperty of MethodSig
    | Obj of obj
    | Returned of obj
    | Prefix of (obj -> obj)    
    | Postfix of (obj -> obj)
    | Infix of int * (obj -> obj -> obj) 
    | SubExpression of ExprRep list
    | Tuple of ExprRep array
    | IndexArgs of ExprRep
    | AppliedInvoke of string
    | Unknown of string
    | Binding of string * ExprRep
    // Lambda: Parameters, Bindings, Contents
    | Lambda of LambdaRecord
    | IfThenElse of ExprRep list * ExprRep list * ExprRep list
    | Generator of ExprRep * ExprRep * ExprRep
    // Has no Unknowns
    | Resolved of ExprRep
    // Has Unknowns
    | Unresolved of ExprTypes

and ExprRep =
    {
        Offset: int
        Length: int
        Expr: ExprTypes
    }

and Bindings = (String, ExprTypes Lazy) Map 

type BarbData = 
    {
        InputType: Type
        OutputType: Type
        Contents: ExprRep list
        Settings: BarbSettings
    }
    with static member Default = { InputType = typeof<unit>; OutputType = typeof<unit>; Contents = []; Settings = BarbSettings.Default }

let exprRepListOffsetLength (exprs: ExprRep seq) =
    let offsets = exprs |> Seq.map (fun e -> e.Offset)
    let max = offsets |> Seq.max 
    let min = offsets |> Seq.min
    min, max - min

let listToSubExpression (exprs: ExprRep list) =
    let offset, length = exprRepListOffsetLength exprs
    { Offset = offset; Length = length; Expr = SubExpression exprs }

let rec exprExistsInRep (pred: ExprTypes -> bool)  (rep: ExprRep) =
    exprExists pred rep.Expr
and exprExists (pred: ExprTypes -> bool) (expr: ExprTypes) =
    match expr with
    | _ when pred expr -> true 
    | SubExpression (repList) -> repList |> List.exists (exprExistsInRep pred)
    | Tuple (repArray) -> repArray |> Array.exists (exprExistsInRep pred)
    | IndexArgs (rep) -> exprExistsInRep pred rep
    | Binding (name, rep) -> exprExistsInRep pred rep
    | Lambda (lambda) -> exprExistsInRep pred (lambda.Contents)
    | IfThenElse (ifexpr, thenexpr, elseexpr) ->
        if ifexpr |> List.exists (exprExistsInRep pred) then true
        elif thenexpr |> List.exists (exprExistsInRep pred) then true
        elif elseexpr |> List.exists (exprExistsInRep pred) then true
        else false
    | Generator (fromRep, incRep, toRep) -> [fromRep; incRep; toRep] |> List.exists (exprExistsInRep pred) 
    // The two tagged cases
    | Resolved (rep) -> exprExistsInRep pred rep
    | Unresolved (expr) -> exprExists pred expr
    // Nothing found
    | _ -> false

let wrapResolved (rep: ExprRep) = { rep with Expr = Resolved rep }
let wrapUnresolved (rep: ExprRep) = { rep with Expr = Unresolved rep.Expr }