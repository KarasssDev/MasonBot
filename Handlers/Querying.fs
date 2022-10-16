namespace Handlers

open Database
open Funogram.Telegram.Types
open MasonCore
open TonApi

module Querying =

    let (>>=) (m: Result<'a, 'e>) (f: 'a -> Result<'b, 'e>) = Result.bind f m
    let (>=>) (f1: 'a -> Result<'b, 'e>) (f2: 'b -> Result<'c, 'e>) = fun x -> f1 x >>= f2

    type ResultBuilder() =
      member this.Return(x) = Ok x
      member this.ReturnFrom(x) = x
      member this.Bind(m, f) = m >>= f

    let result = ResultBuilder()

    type Error =
        | ApiError
        | UnexpectedError
        | UserNotFound

    let toApiError v =
        match v with
        | Some v -> Ok v
        | None -> Error ApiError

    let oneOf errorEmpty errorMany lst =
        match lst with
        | [x] -> Ok x
        | [] -> Error errorEmpty
        | _ -> Error errorMany

    let nonEmpty errorEmpty lst =
        match lst with
        | [] -> Error errorEmpty
        | _ -> Ok lst

    // Users
    let getAllUsers () =
        use ctx = new Connection.MasonDbContext()
        DbQuerying.getAllUsers ctx |> Set

    let getUser (telegramId: int64) =
        use ctx = new Connection.MasonDbContext()
        result {
            let! user = DbQuerying.getUser ctx telegramId |> oneOf UserNotFound UnexpectedError
            match user.Wallet with
            | Some w ->
                let! nftCount = TonApiQuerying.getNftsCount w |> toApiError
                return UserTypes.mkUser user.TelegramId (Some nftCount) user.Wallet
            | None ->
                return UserTypes.mkUser user.TelegramId None user.Wallet
        }

    let userExist (telegramId: int64) =
        use ctx = new Connection.MasonDbContext()
        let user = DbQuerying.getUser ctx telegramId
        match user with
        | [_] -> true
        | _ -> false

    let createUser telegramId wallet =
        use ctx = new Connection.MasonDbContext()
        DbQuerying.createUser ctx telegramId wallet

    let updateUserWallet telegramId wallet =
        use ctx = new Connection.MasonDbContext()
        result {
            let! user = DbQuerying.getUser ctx telegramId |> oneOf UserNotFound UnexpectedError
            return DbQuerying.updateUserWallet ctx user wallet
        }

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
