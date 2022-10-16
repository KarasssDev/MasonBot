open System
open Handlers
open Database
open MasonCore
open Logging
open BotLauncher
open TonApi

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
            printfn $"{DateTime.Now + TimeSpan(7, 0, 0, 0)}"
            //TonApiQuerying.getNftsCount "EQDCs0SXI6Zuo7FXNfTAu8qvzz_svk5tFhDcnUY71fr4A_RC" |> printfn "%O"
            BotLauncher.startBot ()
        else 1
