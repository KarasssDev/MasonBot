namespace Database

open System
open Database.Connection
open Logging
open MasonCore

module DbQuerying =

    // DB logging
    let logDbRequest requestName (args: string list option) =
        Logging.logDebug $"Database process request[{requestName}] with args[{args}]"
    let logDbGet requestName result =
        Logging.logDebug $"Database processed request[{requestName}] with result[{result}]"
        result
    let logDbCreate requestName result =
        Logging.logDebug $"Database processed request[{requestName}] with create value[{result}]"
    let logDbUpdate requestName result =
        Logging.logDebug $"Database processed request[{requestName}] with new value[{result}]"
    let logDbDelete requestName result =
        Logging.logDebug $"Database processed request[{requestName}] with delete value[{result}]"

    let mkTransaction (ctx: MasonDbContext) (transactionName: string) (f: unit -> unit) =
        Logging.logDebug $"Start transaction[{transactionName}]"
        use transaction = ctx.Database.BeginTransaction()
        try
            transaction.CreateSavepoint("Savepoint")
            f()
            transaction.Commit()
        with
            | :? Exception as ex ->
                transaction.RollbackToSavepoint("Savepoint")
                Logging.logDebug $"Exception[{ex.Message}] while proceed transaction[{transactionName}]"
        Logging.logDebug $"Finish transaction[{transactionName}]"

    // Users
    let getAllUsers (ctx: MasonDbContext) =
        let requestName = "Get all users"
        logDbRequest requestName None
        query {
            for user in ctx.Users do
            select user
        } |> Seq.toList |> logDbGet requestName

    let getUser (ctx: MasonDbContext) (telegramId: int64) =
        let requestName = "Get user"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"]
        query {
            for user in ctx.Users do
            where (user.TelegramId = telegramId)
            select user
        } |> Seq.toList |> logDbGet requestName

    let createUser (ctx: MasonDbContext) (telegramId: int64) (wallet: string option) =
        let requestName = "Get user"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"; $"wallet={wallet}"]
        let user: User = {
            TelegramId = telegramId
            Wallet = wallet
        }
        ctx.Users.Add user |> logDbCreate requestName
        ctx.SaveChanges()

    let updateUserWallet (ctx: MasonDbContext) (user: User) (wallet: string) =
        let requestName = "Update user wallet"
        logDbRequest requestName <| Some [$"user.Id={user.TelegramId}"; $"wallet={wallet}"]
        let newUser = {user with Wallet = Some wallet}
        logDbUpdate requestName newUser
        ctx.Entry(user).CurrentValues.SetValues(newUser)
        ctx.SaveChanges()

    // Votings
    let getAllVotings (ctx: MasonDbContext) =
        let requestName = "Get all votings"
        logDbRequest requestName None
        query {
            for voting in ctx.Votings do
            select voting
        } |> Seq.toList |> logDbGet requestName

    let createVoting
        (ctx: MasonDbContext)
        (creator: User)
        (description: string)
        (startDate: DateTime)
        (duration: TimeSpan) =
        let requestName = "Create voting"
        logDbRequest requestName <| Some [
            $"creator.telegramId={creator.TelegramId}"
            $"description={description}"
            $"{startDate}"
            $"{duration}"
        ]
        let voting = {
            Id = Guid.NewGuid()
            Creator = creator
            Description = description
            StartDate = startDate
            Duration = duration
        }
        ctx.Votings.Add voting |> logDbCreate requestName
        ctx.SaveChanges()

    // Variants
    let getVariants (ctx: MasonDbContext) (voting: Voting) =
        let requestName = "Get variants"
        logDbRequest requestName None
        query {
            for variant in ctx.Variants do
            where (variant.Voting = voting)
            select variant
        } |> Seq.toList |> logDbGet requestName

    let createVariants (ctx: MasonDbContext) (voting: Voting) (descriptions: string seq) =
        let requestName = "Create variants"
        logDbRequest requestName <| Some [$"voting={voting}"; $"descriptions={Seq.toList descriptions}"]
        Seq.map (fun d ->
            ctx.Variants.Add {
                Id = Guid.NewGuid()
                Description = d
                Voting = voting
            } |> logDbCreate requestName
        ) descriptions |> ignore
        ctx.SaveChanges()

    // Votes
    let getVotes (ctx: MasonDbContext) (variant: Variant) =
        let requestName = "Get votes"
        logDbRequest requestName <| Some [$"variant={variant}"]
        query {
            for vote in ctx.Votes do
            where (vote.Variant = variant)
            select vote
        } |> Seq.toList |> logDbGet requestName

    let createVotes (ctx: MasonDbContext) (variant: Variant) (nfts: string seq) =
        let requestName = "Create votes"
        logDbRequest requestName <| Some [$"variant={variant}"; $"nfts={Seq.toList nfts}"]
        Seq.map (fun nft ->
            ctx.Votes.Add {
                Id = Guid.NewGuid()
                Variant = variant
                NftAddress = nft
            } |> logDbCreate requestName
        ) nfts |> ignore
        ctx.SaveChanges()

    let deleteVotes (ctx: MasonDbContext) (variant: Variant) =
        let requestName = "Delete votes"
        logDbRequest requestName <| Some [$"variant={variant}"]
        let votes = getVotes ctx variant
        List.map (fun vote ->
            ctx.Votes.Remove vote |> logDbDelete requestName
        ) votes |> ignore
        ctx.SaveChanges()

    // Results
    let getResult (ctx: MasonDbContext) (variant: Variant) =
        let requestName = "Get result"
        logDbRequest requestName <| Some [$"variant={variant}"]
        query {
            for result in ctx.Results do
            where (result.Variant = variant)
            select result
        } |> Seq.toList |> logDbGet requestName

    let createResult (ctx: MasonDbContext) (variant: Variant) (cnt: int) =
        let requestName = "Create result"
        logDbRequest requestName <| Some [$"variant={variant}"; $"cnt={cnt}"]
        {
            Id = Guid.NewGuid()
            Variant = variant
            Count = cnt
        } |> ctx.Results.Add |> logDbCreate requestName
        ctx.SaveChanges ()
