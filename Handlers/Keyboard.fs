namespace Handlers

open Content
open Callback

module Keyboard =
    
    module Button =
        
        let start = ($"{leftArrowEmoji} В главное меню", Start)
        let voting = ($"{folderEmoji} Голосования", Voting)
        let statistics = ($"{statisticEmoji} Статистика", Statistics)
        let howToMason = ($"{infoEmoji} Как стать масоном?", HowToMason)
        let whatIsMason = ($"{infoEmoji} Что такое TON MASON?", WhatIsMason)
        let authorization = ($"{lockKeyEmoji} Авторизация", AuthorizationStart)
        let forMason = ($"{eyeEmoji} Для посвященных", ForMason)
        let buyTon = $"{infoEmoji} Как купить TON?", BuyTon
        let aboutNft = $"{infoEmoji} Всё об NFT", AboutNFT
        let buyMasonNft = $"{infoEmoji} Как купить NFT TON MASON?", BuyMasonNFT
        