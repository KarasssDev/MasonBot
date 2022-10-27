namespace Handlers


open System.IO
open MasonCore
open Logging
open Callback
open Basic

module Content =

    module Text =
        let eyeEmoji = "👁"
        let boomEmoji = "💥"
        let infoEmoji = "ℹ️"
        let lockKeyEmoji = "🔐 "
        let leftArrowEmoji = "⬅️"
        let statisticEmoji = "📊"
        let folderEmoji = "📂"

        let accessDeniedHolder = "Ты не холдер"
        let accessDeniedMaster = "Ты не мастер"


    module private Button =

        let start = ($"{Text.leftArrowEmoji} В главное меню", Start)
        let voting = ($"{Text.folderEmoji} Голосования", Voting)
        let statistics = ($"{Text.statisticEmoji} Статистика", Statistics)
        let howToMason = ($"{Text.infoEmoji} Как стать масоном?", HowToMason)
        let whatIsMason = ($"{Text.infoEmoji} Что такое TON MASON?", WhatIsMason)
        let authorization = ($"{Text.lockKeyEmoji} Авторизация", AuthorizationStart)
        let forMason = ($"{Text.eyeEmoji} Для посвященных", ForMason)
        let buyTon = $"{Text.infoEmoji} Как купить TON?", BuyTon
        let aboutNft = $"{Text.infoEmoji} Всё об NFT", AboutNFT
        let buyMasonNft = $"{Text.infoEmoji} Как купить NFT TON MASON?", BuyMasonNFT
        let createVoting = "Создать голосование", CreateVoting
        let showVotings = "Посмотреть голосования", ShowVotings
        let chooseImportant = "Важное", ChooseImportantVotingType
        let chooseDefault = "Обычное", ChooseDefaultVotingType
        let acceptCreateVoting = "Создать", AcceptCreateVoting
        let discardCreateVoting = "Отмена", DiscardCreateVoting

    module Keyboard =
            let forMasonKeyboard = createInlineKeyboard [|
                [| Button.statistics |]
                [| Button.voting |]
                [| Button.start |]
            |]
            let welcomeKeyboard = createInlineKeyboard [|
                [| Button.howToMason |]
                [| Button.whatIsMason |]
                [| Button.authorization |]
                [| Button.forMason |]
            |]
            let howToMasonKeyboard = createInlineKeyboard [|
                [| Button.buyTon |]
                [| Button.aboutNft |]
                [| Button.buyMasonNft |]
                [| Button.start |]
            |]
            let authKeyboard = createInlineKeyboard [|
                [| Button.start |]
            |]
            let votingKeyboard = createInlineKeyboard [|
                [| Button.showVotings |]
                [| Button.createVoting |]
                [| Button.start |]
            |]
            let chooseVotingTypeKeyboard = createInlineKeyboard [|
                [| Button.chooseImportant |]
                [| Button.chooseDefault |]
                [| Button.start |]
            |]
            let acceptVoteCreatingKeyboard = createInlineKeyboard [|
                [| Button.acceptCreateVoting |]
                [| Button.discardCreateVoting |]
            |]

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
