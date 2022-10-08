namespace Handlers

open Database
open MasonCore

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

    let getUser telegramId =
        use ctx = new Connection.MasonDbContext()
        result {
            let! user = DbQuerying.getUser ctx telegramId |> oneOf UserNotFound UnexpectedError
            let! nftCount = Ok 10 // Api call here
            return UserTypes.mkUser user.TelegramId nftCount user.Wallet
        }




