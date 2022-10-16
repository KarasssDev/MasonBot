namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content


module StaticContentHandlers =

    let handleStart (ctx: UpdateContext) =

        let handlingText = "/start"
        let handlingCallback = Start
        let handlerName = "Start handler"

        match (matchTextMessage handlingText ctx) with
        | Some chat ->
            if Querying.userExist chat.Id |> not then Querying.createUser chat.Id None |> ignore
            logMessage handlerName chat.Id handlingText
            let user = Querying.getUser chat.Id
            match user with
            | Error Querying.UserNotFound -> Querying.createUser chat.Id None |> ignore
            | _ -> ()

            sendMessage chat.Id
            <| (welcomeMessage, ParseMode.Markdown)
            <| Some (welcomePhoto())
            <| Some Keyboard.welcomeKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None ->
            match (matchSimpleCallbackMessage handlingCallback ctx) with
            | Some from ->
                logCallback handlerName from.Id handlingCallback
                sendMessage from.Id
                <| (welcomeMessage, ParseMode.Markdown)
                <| Some (welcomePhoto())
                <| Some Keyboard.welcomeKeyboard
                <| ctx.Config
                HandlingResult.Success
            | None -> HandlingResult.Fail

    let handleHowToMason(ctx: UpdateContext) =

        let handlingCallback = HowToMason
        let handlerName = "How to Mason"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (howToMasonMessage, ParseMode.Markdown)
            <| None
            <| Some Keyboard.howToMasonKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleWhatIsMason(ctx: UpdateContext)  =

        let handlingCallback = WhatIsMason
        let handlerName = "What is Mason"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (whatIsMasonMessage, ParseMode.Markdown)
            <| None
            <| Some Keyboard.welcomeKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleBuyTon(ctx: UpdateContext)  =

        let handlingCallback = BuyTon
        let handlerName = "Buy Ton"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (buyTonMessage, ParseMode.Markdown)
            <| None
            <| Some Keyboard.howToMasonKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleAboutNFT(ctx: UpdateContext)  =

        let handlingCallback = AboutNFT
        let handlerName = "About NFT"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (aboutNftMessage, ParseMode.Markdown)
            <| None
            <| Some Keyboard.howToMasonKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleBuyMasonNFT(ctx: UpdateContext)  =

        let handlingCallback = BuyMasonNFT
        let handlerName = "Buy Mason NFT"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (buyMasonNftMessage, ParseMode.Markdown)
            <| None
            <| Some Keyboard.howToMasonKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
