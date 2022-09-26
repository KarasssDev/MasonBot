namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open Logging.Logging

module StaticContentHandlers =

    let keyboard = createInlineKeyboard [|
        [| ("Как стать масоном?", HowToMason) |]
        [| ("Что такое TON MASON?", WhatIsMason) |]
    |]

    let handleStart (ctx: UpdateContext) =

        let handlingText = "/start"
        let handlerName = "Start handler"

        match (matchTextMessage handlingText ctx) with
        | Some chat ->
            logMessage handlerName chat.Id handlingText
            sendMessage chat.Id
            <| (welcomeMessage, ParseMode.Markdown)
            <| Some (welcomePhoto())
            <| Some keyboard
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
            <| Some keyboard
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
            <| Some keyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
