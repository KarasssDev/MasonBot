namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open MasonCore

module VotingHandlers = // TODO

    let votingKeyboard = createInlineKeyboard [|
        [| ($"{leftArrowEmoji} В главное меню", Start) |]
    |]

    let handleVoting(ctx: UpdateContext) =

        let handlingCallback = Voting
        let handlerName = "Voting"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                sendMessage from.Id
                <| ("TODO message", ParseMode.Markdown)
                <| None
                <| Some ForMasonHandlers.forMasonKeyboard
                <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| ("TODO Access denied", ParseMode.Markdown)
                <| None
                <| Some ForMasonHandlers.forMasonKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| ("TODO match error", ParseMode.Markdown)
                <| None
                <| Some ForMasonHandlers.forMasonKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
