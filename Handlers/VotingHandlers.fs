namespace Handlers

open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open MasonCore

module VotingHandlers = // TODO

    let handleVoting(ctx: UpdateContext) =

        let handlingCallback = Voting
        let handlerName = "Voting"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                sendMessage from.Id
                <| ("TODO message", ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleCreateVoting(ctx: UpdateContext) = // TODO: add check last created voting1

        let handlingCallback = CreateVoting
        let handlerName = "Create voting"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) ->
                Runtime.enableVotingCreating from.Id
                sendMessage from.Id
                <| ("TODO message", ParseMode.Markdown)
                <| None
                <| Some Keyboard.chooseVotingTypeKeyboard
                <| ctx.Config
            | Ok _ ->
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let private handleChoseVotingType (ctx: UpdateContext) handlingCallback =

        let handlerName = "Chose default voting type"

        let votingType =
           match handlingCallback with
           | ChooseDefaultVotingType -> Runtime.Default
           | ChooseImportantVotingType -> Runtime.Important
           | _ -> failwith $"Unexpected callback {handlingCallback}"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) ->
                Runtime.setVotingType from.Id votingType
                Runtime.setState from.Id Runtime.WaitVotingDescription
                sendMessage from.Id
                <| ("TODO message", ParseMode.Markdown)
                <| None
                <| None
                <| ctx.Config
            | Ok _ ->
                Runtime.disableVotingCreating from.Id
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Error err ->
                Runtime.disableVotingCreating from.Id
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleChoseDefaultVotingType(ctx: UpdateContext) =
        handleChoseVotingType ctx ChooseDefaultVotingType

    let handleChoseImportantVotingType(ctx: UpdateContext) =
        handleChoseVotingType ctx ChooseImportantVotingType

    let handleChoseVotingDescription(ctx: UpdateContext) =

        let handlerName = "Chose voting description"

        let validateDescription (description: string) = description.Length > 0 && description.Length < 50

        match (matchStateWithTextMessage Runtime.WaitVotingDescription ctx) with
        | Some (chat, description) ->
            logMessage handlerName chat.Id description

            let user = Querying.getUser chat.Id
            match user with
            | Ok (UserTypes.Master _) ->
                if validateDescription description then
                    Runtime.setVotingDescription chat.Id description
                    Runtime.setState chat.Id Runtime.WaitVotingVariant
                    sendMessage chat.Id
                    <| ("Отправь описание первого варианта", ParseMode.Markdown)
                    <| None
                    <| None
                    <| ctx.Config
                else
                    sendMessage chat.Id
                    <| ("Описание голосования говно", ParseMode.Markdown)
                    <| None
                    <| None
                    <| ctx.Config
            | Ok _ ->
                Runtime.disableVotingCreating chat.Id
                sendMessage chat.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Error err ->
                Runtime.disableVotingCreating chat.Id
                sendMessage chat.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleCreateVariant(ctx: UpdateContext) =

        let handlerName = "Chose voting type"

        let validateDescription (description: string) = description.Length > 0 && description.Length < 20
        let getVotingAsText userId =
            match Runtime.getVotingCreatingInfo userId with
            | Some { Runtime.votingType = Some votingType; Runtime.description = Some description; Runtime.variants = variants } ->
                let votingTypeText =
                    match votingType with
                    | Runtime.Default -> "Обычное"
                    | Runtime.Important -> "Важное"
                let variantsText =
                    variants
                    |> List.map (fun d -> $"[] {d}")
                    |> String.concat "\n"
                $"Тип голосования: {votingTypeText}\nОписание: {description}\nВарианты:\n{variantsText}"
            | _ -> "Что-то пошло не так..." // TODO

        match (matchStateWithTextMessage Runtime.WaitVotingVariant ctx) with
        | Some (chat, description) ->
            logMessage handlerName chat.Id description

            let user = Querying.getUser chat.Id
            match user with
            | Ok (UserTypes.Master _) ->
                match description with
                | "Готово" ->
                    Runtime.setState chat.Id Runtime.WaitAcceptVotingCreation
                    sendMessage chat.Id
                    <| ($"Норм создал?\n\n{getVotingAsText chat.Id}", ParseMode.Markdown)
                    <| None
                    <| Some Keyboard.acceptVoteCreatingKeyboard
                    <| ctx.Config
                | _ ->
                    if validateDescription description then
                        Runtime.addVotingVariant chat.Id description
                        sendMessage chat.Id
                        <| ("Отправь описание следующего варианта или напиши 'Готово'", ParseMode.Markdown)
                        <| None
                        <| None
                        <| ctx.Config
                    else
                        sendMessage chat.Id
                        <| ("Описание варианта говно, попробуй еще раз", ParseMode.Markdown)
                        <| None
                        <| None
                        <| ctx.Config
            | Ok _ ->
                Runtime.disableVotingCreating chat.Id
                sendMessage chat.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Error err ->
                Runtime.disableVotingCreating chat.Id
                sendMessage chat.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let private handleAcceptVotingCreating(ctx: UpdateContext) handlingCallback =

        let handlerName = "Accept voting creating"

        let isAccepted =
           match handlingCallback with
           | AcceptCreateVoting -> true
           | DiscardCreateVoting -> false
           | _ -> failwith $"Unexpected callback {handlingCallback}"

        let getInfo userId =
            match Runtime.getVotingCreatingInfo userId with
            | Some { Runtime.votingType = Some votingType; Runtime.description = Some description; Runtime.variants = variants } ->
                match votingType with
                | Runtime.Default -> (false, description, variants)
                | Runtime.Important -> (true, description, variants)
            | _ -> failwith "TODO: error message" // TODO

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) ->
                Runtime.discardState from.Id

                if isAccepted then
                    let isImportant, description, variants = getInfo from.Id
                    let result = Querying.createVoting from.Id description variants isImportant
                    match result with
                    | Ok () ->
                        sendMessage from.Id
                        <| ("Голосование создано", ParseMode.Markdown)
                        <| None
                        <| Some Keyboard.votingKeyboard
                        <| ctx.Config
                    | Error err ->
                        sendMessage from.Id
                        <| (defaultHandleQueryingError err, ParseMode.Markdown)
                        <| None
                        <| Some Keyboard.votingKeyboard
                        <| ctx.Config
                else
                    sendMessage from.Id
                    <| ("Создание сброшено", ParseMode.Markdown)
                    <| None
                    <| None
                    <| ctx.Config
            | Ok _ ->
                Runtime.disableVotingCreating from.Id
                Runtime.discardState from.Id
                sendMessage from.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            | Error err ->
                Runtime.disableVotingCreating from.Id
                Runtime.discardState from.Id
                sendMessage from.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.votingKeyboard
                <| ctx.Config
            HandlingResult.Success
        | None -> HandlingResult.Fail

    let handleAccept(ctx: UpdateContext) =
        handleAcceptVotingCreating ctx AcceptCreateVoting

    let handleDiscard(ctx: UpdateContext) =
        handleAcceptVotingCreating ctx DiscardCreateVoting
