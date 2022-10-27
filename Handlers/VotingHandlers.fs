namespace Handlers

open Database
open Funogram.Telegram.Bot
open Funogram.Telegram.Types

open Handlers.Basic
open Handlers.Callback
open Handlers.Content
open MasonCore

module VotingHandlers =

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
                <| ("TODO: Описание, что такое голосование", ParseMode.Markdown)
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

    // Voting create
    let handleCreateVoting(ctx: UpdateContext) = // TODO: add check last created voting

        let handlingCallback = CreateVoting
        let handlerName = "Create voting"

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) ->
                Runtime.enableVotingCreating from.Id
                Runtime.setState from.Id Runtime.WaitChooseVotingType
                sendMessage from.Id
                <| ("TODO: описание типов голосований", ParseMode.Markdown)
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

        match (matchStateWithSimpleCallback Runtime.WaitChooseVotingType handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback
            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) ->
                Runtime.setVotingType from.Id votingType
                Runtime.setState from.Id Runtime.WaitVotingDescription
                sendMessage from.Id
                <| ("TODO: просьба отправить описание голосования", ParseMode.Markdown)
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
                    <| ("TODO: Отправь описание первого варианта", ParseMode.Markdown)
                    <| None
                    <| None
                    <| ctx.Config
                else
                    sendMessage chat.Id
                    <| ("TODO: Описание голосования некорректно", ParseMode.Markdown)
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
                    |> List.mapi (fun i d -> $"[{i + 1}] {d}")
                    |> String.concat "\n"
                $"Тип голосования: {votingTypeText}\nОписание: {description}\nВарианты:\n{variantsText}"
            | r -> failwith $"Inconsistent runtime {r}"

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
                        <| ("TODO: Описание варианта некорректно, попробуй еще раз", ParseMode.Markdown)
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
            | r -> failwith $"Inconsistent runtime {r}"

        match (matchStateWithSimpleCallback Runtime.WaitAcceptVotingCreation handlingCallback ctx) with
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

    // Voting watching
    let handleShowVotings(ctx: UpdateContext) =

        let handlingCallback = ShowVotings
        let handlerName = "Show votings"

        let renderKeyboard () =
            let votings = Querying.getVotings ()
            votings
            |> List.map (fun v -> [|v.Description, ShowVoting v.Id|])
            |> (fun x -> List.append x [[|"Назад", Voting|]])
            |> List.toArray
            |> createInlineKeyboard

        match (matchSimpleCallbackMessage handlingCallback ctx) with
        | Some from ->
            logCallback handlerName from.Id handlingCallback

            let user = Querying.getUser from.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                sendMessage from.Id
                <| ("TODO: Описание, что такое голосование", ParseMode.Markdown)
                <| None
                <| Some (renderKeyboard ())
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

    let handleShowVoting(ctx: UpdateContext) =

        let handlerName = "Show voting"

        let renderMessage id =
            let voting, results = Querying.getVotingResults id
            results
            |> Seq.toList
            |> List.mapi (
                fun i (variant, votes) ->
                    $"[{i + 1}] Суммарно за: "
                    + $"{votes |> Seq.sumBy (fun (_, x) -> Seq.length x)}\n"
                    + $"Вариант: {variant.Description}\n"
                    + "Проголосовали:\n"
                    + (votes |> Seq.fold (fun st (holder, votes) -> st + $"""{if holder.Name.IsSome then $"@{holder.Name.Value}" else $"{holder.TelegramId}"} - {Seq.length votes}\n""") "")
            )
            |> String.concat "\n\n"
            |> (fun x ->
                $"Описание голосования: {voting.Description}"
                + $"Дата окончания: {voting.StartDate + voting.Duration}"
                + $"\n\n{x}"
            )

        let renderKeyboard id =
            createInlineKeyboard [|
                [| "Проголосовать", MakeVoteVoting id |]
                [| "Назад", ShowVotings |]
            |]

        match ctx with
        | ShowVotingCallback (us, guid) ->
            logCallback handlerName us.Id ShowVotings

            let user = Querying.getUser us.Id
            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                sendMessage us.Id
                <| (renderMessage guid, ParseMode.Markdown)
                <| None
                <| Some (renderKeyboard guid)
                <| ctx.Config
            | Ok _ ->
                sendMessage us.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage us.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            HandlingResult.Success
        | _ -> HandlingResult.Fail

    let handleMakeVote(ctx: UpdateContext) =

        let handlerName = "Make vote"

        let renderMessage id = "TODO: описание"

        let renderKeyboard id =
            let voting = Querying.getVoting id
            voting.Variants
            |> Seq.map (fun x -> [|x.Description, MakeVoteVariant x.Id|])
            |> Seq.toArray
            |> (fun x -> Array.append x [|[|("Назад", ShowVoting id)|]|])
            |> createInlineKeyboard

        match ctx with
        | MakeVoteVotingCallback (us, guid) ->
            logCallback handlerName us.Id ShowVotings

            let user = Querying.getUser us.Id
            let voting = Querying.getVoting guid

            let votingFinished = voting.StartDate + voting.Duration > System.DateTime.Now

            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                if votingFinished then
                    sendMessage us.Id
                    <| ("Голосование завершено", ParseMode.Markdown)
                    <| None
                    <| Some Keyboard.votingKeyboard
                    <| ctx.Config
                else
                    sendMessage us.Id
                    <| (renderMessage guid, ParseMode.Markdown)
                    <| None
                    <| Some (renderKeyboard guid)
                    <| ctx.Config
            | Ok _ ->
                sendMessage us.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage us.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            HandlingResult.Success
        | _ -> HandlingResult.Fail

    let handleMakeVoteVariant(ctx: UpdateContext) =

        let handlerName = "Make vote variant"

        let renderMessage id = "TODO: вы проголосвали"

        match ctx with
        | MakeVoteVariantCallback (us, guid) ->
            logCallback handlerName us.Id ShowVotings

            let user = Querying.getUser us.Id

            match user with
            | Ok (UserTypes.Master _) | Ok (UserTypes.Holder _) ->
                match Querying.vote us.Id guid with
                | Ok _ ->
                    sendMessage us.Id
                    <| (renderMessage guid, ParseMode.Markdown)
                    <| None
                    <| None
                    <| ctx.Config
                | Error err ->
                    sendMessage us.Id
                    <| (defaultHandleQueryingError err, ParseMode.Markdown)
                    <| None
                    <| Some Keyboard.forMasonKeyboard
                    <| ctx.Config
            | Ok _ ->
                sendMessage us.Id
                <| (Text.accessDeniedHolder, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            | Error err ->
                sendMessage us.Id
                <| (defaultHandleQueryingError err, ParseMode.Markdown)
                <| None
                <| Some Keyboard.forMasonKeyboard
                <| ctx.Config
            HandlingResult.Success
        | _ -> HandlingResult.Fail
