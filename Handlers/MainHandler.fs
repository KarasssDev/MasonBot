namespace Handlers

open Funogram.Telegram.Bot

open Logging

module MainHandler =

    let updateArrived (ctx: UpdateContext) =
        match Basic.processHandlers [
            StaticContentHandlers.handleStart
            StaticContentHandlers.handleHowToMason
            StaticContentHandlers.handleWhatIsMason
            StaticContentHandlers.handleBuyTon
            StaticContentHandlers.handleAboutNFT
            StaticContentHandlers.handleBuyMasonNFT
        ] ctx with
        | Basic.Success -> ()
        | Basic.Fail ->
            match ctx.Update.Message with
            | Some m -> Logging.logError $"Unhandled message\n {m}"
            | None -> ()
            match ctx.Update.CallbackQuery with
            | Some c -> Logging.logError $"Unhandled callback\n {c.Data}"
            | None -> ()

