namespace Handlers

open System.IO
open MasonCore
open Logging

module Content =

    let mutable token = ""
    let mutable logFileWriter = Unchecked.defaultof<StreamWriter>

    let mutable welcomeMessage = ""
    let mutable welcomePhoto = fun () -> Funogram.Telegram.Types.File ("welcome", File.OpenRead($"{Paths.staticContentPath()}welcomePicture.png"))
    let mutable howToMasonMessage = ""
    let mutable whatIsMasonMessage = ""
    let mutable buyTonMessage = ""
    let mutable buyMasonNftMessage = ""
    let mutable aboutNftMessage = ""

    let uploadStaticContent () =
        try
            token <- File.ReadAllText($"{Paths.telegramToken()}").Trim()
            logFileWriter <- new StreamWriter(Paths.logFilePath())
            welcomeMessage <- File.ReadAllText($"{Paths.staticContentPath()}welcomeMessage.txt")
            howToMasonMessage <- File.ReadAllText($"{Paths.staticContentPath()}howToMasonMessage.txt")
            whatIsMasonMessage <- File.ReadAllText($"{Paths.staticContentPath()}whatIsMasonMessage.txt")
            buyTonMessage <- File.ReadAllText($"{Paths.staticContentPath()}buyTonMessage.txt")
            buyMasonNftMessage <- File.ReadAllText($"{Paths.staticContentPath()}buyMasonNftMessage.txt")
            aboutNftMessage <- File.ReadAllText($"{Paths.staticContentPath()}aboutNftMessage.txt")
            true
        with
            | :? System.Exception as ex ->
                Logging.logError $"Cannot upload static content: {ex.Message}"
                false

    let checkDynamicContentExist () =
        File.Exists($"{Paths.staticContentPath()}welcomePicture.png")
