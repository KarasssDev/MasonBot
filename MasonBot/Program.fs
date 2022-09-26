open Handlers
open Database
open MasonCore
open Logging
open BotLauncher

module Program =

    [<EntryPoint>]
    let main _ =

        #if DEBUG
        Paths.configureDataPath "/Users/chez/sources/proj/MasonBot/data/"
        Paths.configureLogPath "/Users/chez/sources/proj/MasonBot/logs/"
        Paths.configureSecretsPath "/Users/chez/sources/proj/MasonBot/secrets/"
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
