namespace Handlers

open System.Collections.Generic
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open MasonCore
open Microsoft.FSharp.Core

module ForMasonHandlers = // TODO

    let forMasonKeyboard = createInlineKeyboard [|
        [| ($"{statisticEmoji} Статистика", Statistics) |]
        [| ($"{folderEmoji} Голосования", Voting) |]
        [| ($"{leftArrowEmoji} В главное меню", Start) |]
    |]

    let handleForMason(ctx: UpdateContext) =

        let handlingCallback = ForMason
        let handlerName = "For Mason"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                sendMessage from.Id
                <| ("TODO message", ParseMode.Markdown)
                <| None
                <| Some forMasonKeyboard
                <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| ("TODO Access denied", ParseMode.Markdown)
                <| None
                <| Some StaticContentHandlers.welcomeKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| ("TODO match error", ParseMode.Markdown)
                <| None
                <| Some StaticContentHandlers.welcomeKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleStatistics(ctx: UpdateContext) =

        let statisticFormatter (stat: Dictionary<BlockchainTypes.WalletAddress, _>) =
            let users = Querying.getAllUsers () |> Set.filter (fun u -> Option.isSome u.Wallet)
            let usersDict = users |> Set.toList |> List.map (fun u -> KeyValuePair(u.Wallet.Value, u.TelegramId)) |> Dictionary
            let usersWallet = users |> Set.map (fun x -> x.Wallet) |> Set.filter Option.isSome
            stat
            |> Seq.sortBy (fun (KeyValue(k, _)) -> k)
            |> Seq.filter (fun (KeyValue(k, _)) -> usersWallet.Contains (Some k))
            |> Seq.map (fun (KeyValue(k, v)) -> $"{usersDict[k]}: {v}")
            |> String.concat ""

        let handlingCallback = Statistics
        let handlerName = "Statistics"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                match (Querying.getStatistic statisticFormatter) with
                | Ok statistic ->
                    sendMessage from.Id
                    <| (statistic, ParseMode.Markdown)
                    <| None
                    <| Some forMasonKeyboard
                    <| ctx.Config
                | Error err -> ()
            | Ok _ ->
                sendMessage from.Id
                <| ("TODO Access denied", ParseMode.Markdown)
                <| None
                <| Some StaticContentHandlers.welcomeKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| ("TODO match error", ParseMode.Markdown)
                <| None
                <| Some StaticContentHandlers.welcomeKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
