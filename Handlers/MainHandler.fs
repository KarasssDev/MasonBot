namespace Handlers

open Funogram.Telegram.Bot

open Logging

module MainHandler =

    let updateArrived (ctx: UpdateContext) =
        match Basic.processHandlers [
            StartHandler.handleStart
            StartHandler.handleHowToMason
            StartHandler.handleWhatIsMason
        ] ctx with
        | Basic.Success -> ()
        | Basic.Fail ->
            match ctx.Update.Message with
            | Some m -> Logging.logError $"Unhandled message\n {m}"
            | None -> ()
            match ctx.Update.CallbackQuery with
            | Some c -> Logging.logError $"Unhandled callback\n {c.Data}"
            | None -> ()

