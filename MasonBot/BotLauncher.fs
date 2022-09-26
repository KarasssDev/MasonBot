namespace BotLauncher

open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot

open Handlers
open Logging

module BotLauncher =
    
    let private configureMessage (ex: System.Exception) =
        let mes = ex.Message
        if "Not Found".Equals mes then
            "Invalid telegram API token"
        else
            $"Unknown error occured: {mes}"
    
    let startBot () =
        try
            async {
                let config = {Config.defaultConfig with Token = Content.token}
                let! _ = Api.deleteWebhookBase () |> api config
                return! startBot config MainHandler.updateArrived None
            } |> Async.RunSynchronously
            0
        with
            | :? System.Exception as ex ->
                let mes = configureMessage ex
                Logging.logError $"{mes}"
                1