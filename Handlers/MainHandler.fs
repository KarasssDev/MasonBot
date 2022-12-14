namespace Handlers

open System
open Funogram.Telegram.Bot

open Logging

module MainHandler =

    let updateArrived (ctx: UpdateContext) =
        try
            match Basic.processHandlers [
                AuthorizationHandlers.handleAuthorizationStart
                AuthorizationHandlers.handleAuthorizationVerification
                VotingHandlers.handleVoting
                VotingHandlers.handleCreateVoting
                VotingHandlers.handleChoseDefaultVotingType
                VotingHandlers.handleChoseImportantVotingType
                VotingHandlers.handleChoseVotingDescription
                VotingHandlers.handleCreateVariant
                VotingHandlers.handleAccept
                VotingHandlers.handleDiscard
                VotingHandlers.handleShowVotings
                VotingHandlers.handleShowVoting
                VotingHandlers.handleMakeVote
                VotingHandlers.handleMakeVoteVariant
                StaticContentHandlers.handleStart
                StaticContentHandlers.handleHowToMason
                StaticContentHandlers.handleWhatIsMason
                StaticContentHandlers.handleBuyTon
                StaticContentHandlers.handleAboutNFT
                StaticContentHandlers.handleBuyMasonNFT
                ForMasonHandlers.handleForMason
                ForMasonHandlers.handleStatistics
                VotingHandlers.handleVoting
            ] ctx with
            | Basic.Success -> ()
            | Basic.Fail ->
                match ctx.Update.Message with
                | Some m -> Logging.logWarning $"Unhandled message\n {m}"
                | None -> ()
                match ctx.Update.CallbackQuery with
                | Some c -> Logging.logWarning $"Unhandled callback\n {c.Data}"
                | None -> ()
        with ex -> Logging.logError $"Unhandled exception {ex.Message}\n {ex.StackTrace}"
