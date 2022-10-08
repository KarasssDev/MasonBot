namespace Handlers

module Callback =

    type CallbackContent =
        | HowToMason
        | WhatIsMason
        | BuyTon
        | BuyMasonNFT
        | AboutNFT
        | Start

    let string2SimpleCallbackContent str =
        match str with
        | "HowToMason" -> Some HowToMason
        | "WhatIsMason" -> Some WhatIsMason
        | "BuyTon" -> Some BuyTon
        | "BuyMasonNFT" -> Some BuyMasonNFT
        | "AboutNFT" -> Some AboutNFT
        | "Start" -> Some Start
        | _ -> None

    let callbackContent2String content =
        match content with
        | HowToMason -> "HowToMason"
        | WhatIsMason -> "WhatIsMason"
        | BuyTon -> "BuyTon"
        | BuyMasonNFT -> "BuyMasonNFT"
        | AboutNFT -> "AboutNFT"
        | Start -> "Start"
