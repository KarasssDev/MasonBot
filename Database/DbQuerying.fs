namespace Database

open System.Linq
open Database.Connection
open Microsoft.EntityFrameworkCore

module DbQuerying =

    let getUser (ctx: MasonDbContext) telegramId =
        query {
            for user in ctx.Users do
            where (user.TelegramId = telegramId)
            select user
        } |> Seq.toList

    let getAllVotings (ctx: MasonDbContext) =
        query {
            for voting in ctx.Votings do
            select voting
        } |> Seq.toList
