namespace Handlers

open System
open Database
open Funogram.Telegram.Types
open MasonCore
open TonApi

module Querying =

    // Result monad
    let (>>=) (m: Result<'a, 'e>) (f: 'a -> Result<'b, 'e>) = Result.bind f m
    let (>=>) (f1: 'a -> Result<'b, 'e>) (f2: 'b -> Result<'c, 'e>) = fun x -> f1 x >>= f2

    type ResultBuilder() =
      member this.Return(x) = Ok x
      member this.ReturnFrom(x) = x
      member this.Bind(m, f) = m >>= f

    let result = ResultBuilder()

    // Error helpers
    type Error =
        | ApiError
        | UnexpectedError
        | UserNotFound

    let private toApiError v =
        match v with
        | Some v -> Ok v
        | None -> Error ApiError

    let private oneOf errorEmpty errorMany lst =
        match lst with
        | [x] -> Ok x
        | [] -> Error errorEmpty
        | _ -> Error errorMany

    let private nonEmpty errorEmpty lst =
        match lst with
        | [] -> Error errorEmpty
        | _ -> Ok lst

    // Users
    let getAllUsers = DbQuerying.getAllUsers

    let userExist (telegramId: int64) = DbQuerying.userExist telegramId

    let getUser (telegramId: int64) =
        result {
            let user = DbQuerying.getUser telegramId
            match user.Wallet with
            | Some w ->
                let! nftCount = TonApiQuerying.getNftsCount w |> toApiError
                return UserTypes.mkUser user.TelegramId (Some nftCount) user.Wallet
            | None ->
                return UserTypes.mkUser user.TelegramId None user.Wallet
        }

    let createUser (telegramId: int64) name wallet =
        use ctx = new Connection.MasonDbContext()
        if DbQuerying.userExist telegramId then ()
        else DbQuerying.createUser ctx telegramId name wallet

    let updateUserWallet telegramId wallet =
        use ctx = new Connection.MasonDbContext()
        let user = DbQuerying.getUserFromCtx ctx telegramId
        DbQuerying.updateUserWallet ctx user wallet


    // Authorization
    let verifyTransaction message amount hash =
        result {
            let! res = TonApiQuerying.verifyTransaction message amount hash |> toApiError
            return res
        }

    // Statistic
    let getStatistic formatter =
        result {
            let! rawStatistic = TonApiQuerying.getStatistics () |> toApiError
            return formatter rawStatistic
        }

    // Votings
    let createVoting (userId: int64) (description: string) (variantDescriptions: string list) (isImportant: bool) =
        use ctx = new Connection.MasonDbContext()

        let duration = if isImportant then TimeSpan(3,0,0,0) else TimeSpan(7,0,0,0) // TODO: move it
        let currentDate = DateTime.Now

        result {
            let user = DbQuerying.getUserFromCtx ctx userId
            let variants = DbQuerying.createVariants ctx variantDescriptions
            return DbQuerying.createVoting ctx user description variants currentDate duration
        }

    let getVotings = DbQuerying.getAllVotings
    let getVoting id = DbQuerying.getVoting id

    let vote userId variantId =
        use ctx = new Connection.MasonDbContext()
        result {
            let user = DbQuerying.getUserFromCtx ctx userId
            let variant = DbQuerying.getVariantFromCtx ctx variantId
            let! wallet = user.Wallet |> toApiError
            let! nfts = TonApiQuerying.getNftsAddresses wallet |> toApiError
            DbQuerying.makeVote ctx user variant nfts
            return ()
        }

    let getVotingResults id =
        let voting = DbQuerying.getVoting id
        let results =
            voting.Variants
            |> Seq.map (fun v -> v, DbQuerying.getVotes v.Id)
            |> Seq.map (fun (v, votes) -> v, votes |> Seq.groupBy (fun v -> v.User))
        (voting, results)
