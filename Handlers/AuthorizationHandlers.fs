namespace Handlers

open System
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Logging
open TonApi.TonApiQuerying
open MasonCore

module AuthorizationHandlers = // TODO

    let private AUTH_SUM = 10000000UL
    let private authorizationStartMessage (mes: string) =
        $"""
Для авторизации необходимо перевести 0,01 TON на кошелек:
{COLLECTION_WALLET}

При переводе необходимо указать следующее сообщение:
{mes}

Дождитесь окончания перевода, после чего введите идентификатор транзакции
        """
    
    let authKeyboard = createInlineKeyboard [|
        [| ("В главное меню", Start) |]
    |]
    
    let private generateMessage (from: User) =
        let date = DateTime.Now.ToLongDateString ()
        let time = DateTime.Now.ToLongTimeString ()
        let id = from.Id.ToString ()
        Text.Encoding.UTF8.GetBytes $"{id}{date}{time}"
        |> Convert.ToBase64String
    
    let private updateRuntime (from: User) mes =
        let id = from.Id
        let info = Runtime.getOrDefault id
        let authInfo = {
            Runtime.AuthorizationInfo.message = mes
        }
        {info with authorizationInfo = Some authInfo}
        |> Runtime.update id
            
    
    let handleAuthorizationStart(ctx: UpdateContext) =

        let handlingCallback = AuthorizationStart
        let handlerName = "AuthorizationStart"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            Logging.logDebug $"[{from.Id}] started authorization"
            match Querying.getUser from.Id with
            | Ok (UserTypes.Unverified _) ->
                let mes = "OTcyOTU5ODYyVHVlc2RheSwgMTEgT2N0b2JlciAyMDIyMDI6Mzk6Mzc="//generateMessage from
                updateRuntime from mes
                logCallback handlerName from.Id handlingCallback
                sendMessage from.Id
                <| ($"{authorizationStartMessage mes}", ParseMode.Markdown)
                <| None
                <| None
                <| ctx.Config
                Logging.logDebug $"[{from.Id}] got authorization message: [{mes}]"
            | Error err ->
                let mes = defaultHandleQueryingError err
                sendMessage from.Id
                <| ($"{mes}", ParseMode.Markdown)
                <| None
                <| Some authKeyboard
                <| ctx.Config
                Logging.logDebug $"[{from.Id}]: authorization start error [{mes}]"
            | _ ->
                sendMessage from.Id
                <| ("Вы уже авторизованы!", ParseMode.Markdown)
                <| None
                <| Some authKeyboard
                <| ctx.Config
                Logging.logDebug $"[{from.Id}] already authorized"
            HandlingResult.Success
        | None -> HandlingResult.Fail
    
    let handleAuthorizationVerification(ctx: UpdateContext) =
        let handlingCallback = AuthorizationVerification
        let handlerName = "AuthorizationVerification"
        match (matchAnyMessage ctx) with
        | Some from, Some hash ->
            if (from.Id
                |> Runtime.getOrDefault
                |> fun x -> x.authorizationInfo.IsNone
                ) then
                HandlingResult.Fail
            else
                Logging.logDebug $"[{from.Id}] is verifying wallet with transaction hash [{hash}]"
                logCallback handlerName from.Id handlingCallback
                let info = Runtime.getOrDefault from.Id
                let mes = info.authorizationInfo.Value.message.Trim()
                
                let ansMes =
                    match Querying.verifyTransaction mes AUTH_SUM hash with
                    | Ok (true, wallet) ->
                        match Querying.updateUserWallet from.Id wallet with
                        | Ok _ ->
                            Logging.logInfo $"[{from.Id}] is verified with wallet [{wallet}]"
                            "Вы успешно авторизовались!"
                        | Error err ->
                            let mes = defaultHandleQueryingError err
                            Logging.logError $"Verification error: on update [{from.Id}] wallet to [{wallet}] with [{mes}]"
                            mes
                    | Ok (false, _) ->
                        Logging.logDebug $"[{from.Id}] typed invalid transaction with hash [{hash}]"
                        "Указанная транзакция не соответствует запрашиваемой, проверьте адресата или указанное сообщение"
                    | Error err ->
                        let mes = defaultHandleQueryingError err
                        Logging.logError $"Verification API error: [{from.Id}] with [{mes}]"
                        "Ошибка сервера авторизации. Возможно, вы ввели неверный идентификатор транзакции"
                    
                sendMessage from.Id
                <| (ansMes, ParseMode.Markdown)
                <| None
                <| Some authKeyboard
                <| ctx.Config
                
                Runtime.disableAuthorization from.Id
                HandlingResult.Success
        | Some from, None ->
            Runtime.disableAuthorization from.Id
            HandlingResult.Fail
        | _ ->
            HandlingResult.Fail

