namespace Handlers

open System
open System.IO
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open System.Security.Cryptography
open System.Text
open QRCoder

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open Logging
open MasonCore
open TonApi

module AuthorizationHandlers = // TODO unhardcodig + пееписать на State

    let private AUTH_SUM = 10000000UL
    let private authorizationStartMessage =
        """
Для авторизации необходимо перевести 0,01 TON на специальный кошелек
Вы можете перейти по ссылке или отсканировать QR-код:
        """

    let private generateMessage (from: User) =
        let id = from.Id.ToString ()
        let data = Encoding.UTF8.GetBytes id
        use md5 = MD5.Create ()
        (StringBuilder(), md5.ComputeHash(data))
        ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
        |> string

    let private generateLink (mes: string) =
        let domain = "https://app.tonkeeper.com"
        let path = $"transfer/{TonApiQuerying.COLLECTION_WALLET}"
        let query = [ $"amount={AUTH_SUM}"; $"text={mes}" ]
        let sep = "&"
        $"{domain}/{path}?{query |> String.concat sep}"

    let private generateQR (mes: string) =
        use qrGen = new QRCodeGenerator()
        let qrData = qrGen.CreateQrCode(mes, QRCodeGenerator.ECCLevel.Q)
        use qrCode = new PngByteQRCode(qrData)
        let bytes = qrCode.GetGraphic(10)
        Funogram.Telegram.Types.File ("Authorization QR-code", new MemoryStream(bytes))

    let private updateRuntime (from: User) mes =
        let id = from.Id
        let authInfo = {
            Runtime.AuthorizationInfo.message = mes
        }
        Runtime.update id <| fun i -> {i with authorizationInfo = Some authInfo}


    let handleAuthorizationStart(ctx: UpdateContext) =

        let handlingCallback = AuthorizationStart
        let handlerName = "AuthorizationStart"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            Logging.logDebug $"[{from.Id}] started authorization"
            match Querying.getUser from.Id with
            | Ok (UserTypes.Unverified _) ->
                let mes = generateMessage from
                updateRuntime from mes
                logCallback handlerName from.Id handlingCallback

                sendMessage from.Id
                <| ($"{authorizationStartMessage}", ParseMode.Markdown)
                <| None
                <| None
                <| ctx.Config

                let link = generateLink mes
                let qr = generateQR link
                sendMessage from.Id
                <| ($"{link}", ParseMode.Markdown)
                <| Some qr
                <| None
                <| ctx.Config

                sendMessage from.Id
                <| ("Отправьте идентификатор транзакции после завершения ее обработки. Если вы уже совершали транзакцию и что-то пошло не так, то можете отправить ее идентификатор",
                    ParseMode.Markdown)
                <| None
                <| None
                <| ctx.Config

                Logging.logDebug $"[{from.Id}] got authorization message: [{mes}]"
            | Error err ->
                let mes = defaultHandleQueryingError err
                sendMessage from.Id
                <| ($"{mes}", ParseMode.Markdown)
                <| None
                <| Some Keyboard.authKeyboard
                <| ctx.Config
                Logging.logDebug $"[{from.Id}]: authorization start error [{mes}]"
            | _ ->
                sendMessage from.Id
                <| ("Вы уже авторизованы!", ParseMode.Markdown)
                <| None
                <| Some Keyboard.authKeyboard
                <| ctx.Config
                Logging.logDebug $"[{from.Id}] already authorized"
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleAuthorizationVerification(ctx: UpdateContext) = // TODO: нормальные логи
        let handlingCallback = AuthorizationVerification // мы не хендлим тут callback, а просто проверяем, что это сообщение
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
                logCallback handlerName from.Id handlingCallback // Тут, соответственно, мы логируем, что это сообщение, а не callback
                let info = Runtime.getOrDefault from.Id
                let mes = info.authorizationInfo.Value.message.Trim()

                let ansMes =
                    match Querying.verifyTransaction mes AUTH_SUM hash with
                    | Ok (true, wallet) ->
                        Querying.updateUserWallet from.Id wallet |> ignore
                        "Вы успешно авторизовались!"

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
                <| Some Keyboard.authKeyboard
                <| ctx.Config

                Runtime.disableAuthorization from.Id
                HandlingResult.Success
        | Some from, None ->
            Runtime.disableAuthorization from.Id
            HandlingResult.Fail
        | _ ->
            HandlingResult.Fail

