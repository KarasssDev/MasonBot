namespace Handlers

open System.Collections.Generic
open Database
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open MasonCore
open Microsoft.FSharp.Core

module ForMasonHandlers =



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
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.welcomeKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.welcomeKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleStatistics(ctx: UpdateContext) =

        let statisticFormatter (stat: Dictionary<BlockchainTypes.WalletAddress, int>) =

            let wallets = stat.Keys |> Set

            let formatUser (user: Connection.User) nftCnt =
                let displayName =
                    match user.Name with
                    | Some n -> $"@{n}"
                    | None -> $"{user.TelegramId}"
                let emoji = if nftCnt < 23 then Text.eyeEmoji else Text.boomEmoji
                $"{emoji} {displayName}"

            Querying.getAllUsers ()
            |> List.filter (fun u -> Option.isSome u.Wallet && Set.contains u.Wallet.Value wallets)
            |> List.map (fun u -> (u, stat[u.Wallet.Value]))
            |> List.sortBy snd
            |> List.map (fun (u, count) -> formatUser u count)
            |> String.concat "\n"

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
                    <| Some Keyboard.forMasonKeyboard
                    <| ctx.Config
                | Error err ->
                    sendMessage from.Id
                    <| (defaultHandleQueryingError err, ParseMode.Markdown)
                    <| None
                    <| Some Keyboard.forMasonKeyboard
                    <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.welcomeKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.welcomeKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail
