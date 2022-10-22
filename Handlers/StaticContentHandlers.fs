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

    let private staticContentHandler handlerName handlingCallback message keyboard (ctx: UpdateContext) =
        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| (message, ParseMode.Markdown)
            <| None
            <| Some keyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleHowToMason = staticContentHandler "How to Mason" HowToMason howToMasonMessage Keyboard.howToMasonKeyboard
    let handleWhatIsMason = staticContentHandler "What is Mason" WhatIsMason whatIsMasonMessage Keyboard.welcomeKeyboard
    let handleBuyTon = staticContentHandler "Buy Ton" BuyTon buyTonMessage Keyboard.howToMasonKeyboard
    let handleAboutNFT = staticContentHandler "About NFT" AboutNFT aboutNftMessage Keyboard.howToMasonKeyboard
    let handleBuyMasonNFT = staticContentHandler "Buy Mason NFT" BuyMasonNFT buyMasonNftMessage Keyboard.howToMasonKeyboard
