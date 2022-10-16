namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content

module StaticContentHandlers =

    let welcomeKeyboard = createInlineKeyboard [|
        [| ($"{infoEmoji} Как стать масоном?", HowToMason) |]
        [| ($"{infoEmoji} Что такое TON MASON?", WhatIsMason) |]
        [| ($"{lockKeyEmoji} Авторизация", AuthorizationStart) |]
        [| ($"{eyeEmoji} Для посвященных", ForMason) |]
    |]

    let howToMasonKeyboard = createInlineKeyboard [|
        [| $"{infoEmoji} Как купить TON?", BuyTon |]
        [| $"{infoEmoji} Всё об NFT", AboutNFT |]
        [| $"{infoEmoji} Как купить NFT TON MASON?", BuyMasonNFT |]
        [| $"{leftArrowEmoji} В главное меню", Start |]
    |]

    let handleStart (ctx: UpdateContext) =

        let handlingText = "/start"
        let handlingCallback = Start
        let handlerName = "Start handler"

        match (matchTextMessage handlingText ctx) with
        | Some chat ->
            logMessage handlerName chat.Id handlingText
            sendMessage chat.Id
            <| (welcomeMessage, ParseMode.Markdown)
            <| Some (welcomePhoto())
            <| Some welcomeKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None ->
            match (matchSimpleCallbackMessage handlingCallback ctx) with
            | Some from ->
                logCallback handlerName from.Id handlingCallback
                sendMessage from.Id
                <| (welcomeMessage, ParseMode.Markdown)
                <| Some (welcomePhoto())
                <| Some welcomeKeyboard
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
            <| Some howToMasonKeyboard
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
            <| Some welcomeKeyboard
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
            <| Some howToMasonKeyboard
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
            <| Some howToMasonKeyboard
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
            <| Some howToMasonKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
