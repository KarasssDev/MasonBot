namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback

module AuthorizationHandlers = // TODO

    let authorizationKeyboard = createInlineKeyboard [|
        [| ("В главное меню", Start) |]
    |]

    let handleAuthorization(ctx: UpdateContext) =

        let handlingCallback = Authorization
        let handlerName = "Authorization"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            sendMessage from.Id
            <| ("TODO", ParseMode.Markdown)
            <| None
            <| Some authorizationKeyboard
            <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

