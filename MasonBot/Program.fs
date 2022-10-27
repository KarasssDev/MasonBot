open Handlers
open MasonCore
open Logging
open BotLauncher


module Program =

    [<EntryPoint>]
    let main _ =

        #if DEBUG
        Paths.configureDataPath ""
        Paths.configureLogPath ""
        Paths.configureSecretsPath ""
        Logging.configureWriter System.Console.Out
        Logging.configureLogLevel Logging.Debug
        #endif

        #if RELEASE
        Logging.configureWriter Content.logFileWriter
        Logging.configureLogLevel Logging.Debug
        #endif

        let contentUploaded = Content.uploadStaticContent()
        let contentExist = Content.checkDynamicContentExist()
        let readyToStart = contentUploaded && contentExist

        if readyToStart then
            Database.DbQuerying.initCaches ()
            Logging.logInfo "Bot started"
            BotLauncher.startBot ()
        else 1
