namespace Handlers

module Callback =

    type CallbackContent =
        | HowToMason
        | WhatIsMason
        | BuyTon
        | BuyMasonNFT
        | AboutNFT
        | Start
        | AuthorizationStart
        | AuthorizationVerification
        | ForMason
        | Statistics
        | Voting
        | CreateVoting
        | ShowVotings
        | ChooseImportantVotingType
        | ChooseDefaultVotingType
        | AcceptCreateVoting
        | DiscardCreateVoting

    let string2SimpleCallbackContent str =
        match str with
        | "HowToMason" -> Some HowToMason
        | "WhatIsMason" -> Some WhatIsMason
        | "BuyTon" -> Some BuyTon
        | "BuyMasonNFT" -> Some BuyMasonNFT
        | "AboutNFT" -> Some AboutNFT
        | "Start" -> Some Start
        | "AuthorizationStart" -> Some AuthorizationStart
        | "AuthorizationVerification" -> Some AuthorizationVerification
        | "ForMason" -> Some ForMason
        | "Statistics" -> Some Statistics
        | "Voting" -> Some Voting
        | "CreateVoting" -> Some CreateVoting
        | "ShowVotings" -> Some ShowVotings
        | "ChooseImportantVotingType" -> Some ChooseImportantVotingType
        | "ChooseDefaultVotingType" -> Some ChooseDefaultVotingType
        | "AcceptCreateVoting" -> Some AcceptCreateVoting
        | "DiscardCreateVoting" -> Some DiscardCreateVoting
        | _ -> None

    let callbackContent2String content =
        match content with
        | HowToMason -> "HowToMason"
        | WhatIsMason -> "WhatIsMason"
        | BuyTon -> "BuyTon"
        | BuyMasonNFT -> "BuyMasonNFT"
        | AboutNFT -> "AboutNFT"
        | Start -> "Start"
        | AuthorizationStart -> "AuthorizationStart"
        | AuthorizationVerification -> "AuthorizationVerification"
        | ForMason -> "ForMason"
        | Statistics -> "Statistics"
        | Voting -> "Voting"
        | CreateVoting -> "CreateVoting"
        | ShowVotings -> "ShowVotings"
        | ChooseImportantVotingType -> "ChooseImportantVotingType"
        | ChooseDefaultVotingType -> "ChooseDefaultVotingType"
        | AcceptCreateVoting -> "AcceptCreateVoting"
        | DiscardCreateVoting -> "DiscardCreateVoting"
