open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot

open Handlers
open Database
open MasonCore
open Logging

module Program =

    let startBot () =
        try
            async {
                let config = {Config.defaultConfig with Token = Content.token}
                let! _ = Api.deleteWebhookBase () |> api config
                return! startBot config MainHandler.updateArrived None
            } |> Async.RunSynchronously
            0
        with
            | :? System.Exception as _ ->
                Logging.logError "Invalid telegram API token"
                1
    
    [<EntryPoint>]
    let main _ =

        #if DEBUG
        Paths.configureDataPath ""
        Paths.configureLogPath ""
        Paths.configureSecretsPath ""
        Logging.configureWriter System.Console.Out
        Logging.configureLogLevel Logging.Debug
        #endif

        let contentUploaded = Content.uploadStaticContent()
        let contentExist = Content.checkDynamicContentExist()
        let readyToStart = contentUploaded && contentExist

        if readyToStart then
            Logging.logInfo "Bot started"
            startBot ()
        else 1
