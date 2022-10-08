open Handlers
open Database
open MasonCore
open Logging
open BotLauncher

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
            BotLauncher.startBot ()
        else 1
