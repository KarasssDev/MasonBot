namespace Tests

open System
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open Handlers
open Handlers.Basic
open NUnit.Framework

module Helpers =

    let createTextMessageWithId chatId text =
        let chat = Chat.Create(chatId, ChatType.Unknown)
        let msg = Message.Create(chatId, DateTime.Now, chat, text = text)
        let createUpdate = Update.Create(Random.Shared.NextInt64(), message = msg)
        let ctx = {
            Update = createUpdate
            Config = Config.defaultConfig
            Me = Unchecked.defaultof<User>
        }
        ctx

    let createTextMessage = createTextMessageWithId (Random.Shared.NextInt64())

    let createCallbackMessageWithId chatId callbackContent =
        let chat = User.Create(chatId, false, "Mocked")
        let callbackData = Callback.callbackContent2String callbackContent
        let callback =
            CallbackQuery.Create(Random.Shared.NextInt64().ToString(), chat, "Mocked", data = callbackData)
        let createUpdate = Update.Create(Random.Shared.NextInt64(), callbackQuery = callback)
        let ctx = {
            Update = createUpdate
            Config = Config.defaultConfig
            Me = Unchecked.defaultof<User>
        }
        ctx

    let createCallbackMessage = createCallbackMessageWithId (Random.Shared.NextInt64())
    let expectSuccess result =
        match result with
        | HandlingResult.Success -> true
        | HandlingResult.Fail -> false
        |> Assert.IsTrue

    let expectFail result =
        match result with
        | HandlingResult.Success -> false
        | HandlingResult.Fail -> true
        |> Assert.IsTrue

    let mockContent () =
        Content.welcomePhoto <- fun () -> Unchecked.defaultof<InputFile>
        Content.welcomeMessage <- "welcomeMessage"
        Content.howToMasonMessage <- "howToMasonMessage"
        Content.whatIsMasonMessage <- "whatIsMasonMessage"
        Content.aboutNftMessage <- "aboutNftMessage"
        Content.buyTonMessage <- "buyTonMessage"
        Content.buyMasonNftMessage <- "buyMasonNftMessage"
