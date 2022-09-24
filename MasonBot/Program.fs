open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot

open Handlers
open Database
open MasonCore
open Logging

module Program =

    [<EntryPoint>]
    let main _ =

        #if DEBUG
        Paths.configureDataPath "/home/viktor/RiderProjects/MasonBot/data/"
        Paths.configureLogPath "/home/viktor/RiderProjects/MasonBot/logs/"
        Paths.configureSecretsPath "/home/viktor/RiderProjects/MasonBot/secrets/"
        Logging.configureWriter System.Console.Out
        Logging.configureLogLevel Logging.Debug
        #endif

        let contentUploaded = Content.uploadStaticContent()
        let contentExist = Content.checkDynamicContentExist()
        let readyToStart = contentUploaded && contentExist

        if readyToStart then
            Logging.logInfo "Bot started"
            async {
              let config = {Config.defaultConfig with Token = "5474863531:AAEPxvze9nq9efNfwPbWu9DHKP9043VFtTw"}
              let! _ = Api.deleteWebhookBase () |> api config
              return! startBot config MainHandler.updateArrived None
            } |> Async.RunSynchronously
            0
        else 1




