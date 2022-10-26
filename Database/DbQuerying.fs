namespace Database

open System
open System.Collections.Concurrent
open Database.Connection
open Logging

module DbQuerying =

    // DB caches
    let private dbCache = { // Migrate to Redis?
        Users = ConcurrentDictionary()
        Votings = ConcurrentDictionary()
        Votes = ConcurrentDictionary()
        Variants = ConcurrentDictionary()
    }

    let private refreshCache (cache: ConcurrentDictionary<'a, 'b>) (update: unit -> ('a *'b) list)=
        cache.Clear()
        let items = update()
        items |> List.iter (fun (key, item) -> cache.TryAdd(key, item) |> ignore)
        Logging.logDebug $"Cache [{typeof<'b>.Name}] refreshed"

    let initCaches () =
        use ctx = new MasonDbContext()
        Logging.logDebug "Initializing caches"
        refreshCache dbCache.Users (fun () -> ctx.Users |> Seq.map (fun u -> u.TelegramId, u) |> List.ofSeq)
        refreshCache dbCache.Votings (fun () -> ctx.Votings |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)
        refreshCache dbCache.Votes (fun () -> ctx.Votes |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)
        refreshCache dbCache.Variants (fun () -> ctx.Variants |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)

    let private existInCache (cache: ConcurrentDictionary<'a, 'b>) (key: 'a) =
        cache.ContainsKey(key)

    let private fromCache (cache: ConcurrentDictionary<'a, 'b>) (key: 'a) =
        match cache.TryGetValue key with
        | true, value ->
            Logging.logDebug $"Getting [{key}] from cache [{typeof<'b>.Name}]"
            value
        | false, _ ->
            Logging.logError$"Key [{key}] not found in cache"
            failwith $"Key [{key}] not found in cache"

    let private allFromCache (cache: ConcurrentDictionary<'a, 'b>) =
        Logging.logDebug $"Getting all from cache [{cache.GetType().Name}]"
        cache.Values |> Seq.toList



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
    let getAllUsers () =
        let requestName = "Get all users"
        logDbRequest requestName None
        allFromCache dbCache.Users

    let userExist (userId: int64) =
        let requestName = "Check user exist"
        logDbRequest requestName <| Some []
        existInCache dbCache.Users userId

    let getUser (telegramId: int64)  =
        let requestName = "Get user"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"]
        fromCache dbCache.Users telegramId

    let createUser (ctx: MasonDbContext) (telegramId: int64) (wallet: string option) =
        let requestName = "Get user"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"; $"wallet={wallet}"]
        let user: User = {
            TelegramId = telegramId
            Wallet = wallet
        }
        ctx.Users.Add user |> logDbCreate requestName
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Users (fun () -> ctx.Users |> Seq.map (fun u -> u.TelegramId, u) |> List.ofSeq)

    let updateUserWallet (ctx: MasonDbContext) (user: User) (wallet: string) =
        let requestName = "Update user wallet"
        logDbRequest requestName <| Some [$"user.Id={user.TelegramId}"; $"wallet={wallet}"]
        let newUser = {user with Wallet = Some wallet}
        logDbUpdate requestName newUser
        ctx.Entry(user).CurrentValues.SetValues(newUser)
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Users (fun () -> ctx.Users |> Seq.map (fun u -> u.TelegramId, u) |> List.ofSeq)

    // Votings
    let getAllVotings () =
        let requestName = "Get all votings"
        logDbRequest requestName None
        allFromCache dbCache.Votings

    let getVoting id =
        let requestName = "Get voting"
        logDbRequest requestName <| Some [$"id={id}"]
        fromCache dbCache.Votings id

    let createVoting
        (ctx: MasonDbContext)
        (creator: User)
        (description: string)
        (variants: Variant seq)
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
            Variants = variants |> Seq.toArray :> System.Collections.Generic.ICollection<Variant>
        }
        ctx.Votings.Add voting |> logDbCreate requestName
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Votings (fun () -> ctx.Votings |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)

    // Variants
    let createVariants (ctx: MasonDbContext) (descriptions: string seq) =
        let requestName = "Create variants"
        logDbRequest requestName <| Some [$"descriptions={Seq.toList descriptions}"]
        let variants = descriptions |> Seq.map (fun description -> {
            Id = Guid.NewGuid()
            Description = description
        })
        Seq.iter (fun d ->
            ctx.Variants.Add d |> logDbCreate requestName
        ) variants
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Variants (fun () -> ctx.Variants |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)
        variants

    let getVariant id  =
        let requestName = "Get variant"
        logDbRequest requestName <| Some [$"id={id}"]
        fromCache dbCache.Variants id

    // Votes
    let getVotes (variant: Variant) =
        let requestName = "Get votes"
        logDbRequest requestName <| Some [$"variant={variant}"]
        allFromCache dbCache.Votes |> List.filter (fun v -> v.Variant.Id = variant.Id)

    let createVotes (ctx: MasonDbContext) (user: User) (variant: Variant) (nfts: string seq) =
        let requestName = "Create votes"
        logDbRequest requestName <| Some [$"variant={variant}"; $"nfts={Seq.toList nfts}"]
        Seq.iter (fun nft ->
            ctx.Votes.Add {
                Id = Guid.NewGuid()
                User = user
                Variant = variant
                NftAddress = nft
            } |> logDbCreate requestName
        ) nfts
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Votes (fun () -> ctx.Votes |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)

    let deleteAllVotes (ctx: MasonDbContext) (variant: Variant) =
        let requestName = "Delete all votes"
        logDbRequest requestName <| Some [$"variant={variant}"]
        let votes = getVotes variant
        List.iter (fun vote ->
            ctx.Votes.Remove vote |> logDbDelete requestName
        ) votes
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Votes (fun () -> ctx.Votes |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)

    let deleteVotesByNfts (ctx: MasonDbContext) (variant: Variant) (nfts: string seq) =
        let requestName = "Delete votes"
        logDbRequest requestName <| Some [$"nfts={Seq.toList nfts}"]
        let variant = getVariant variant.Id
        let votes = getVotes variant
        List.iter (fun vote ->
            if nfts |> Seq.contains vote.NftAddress then
                ctx.Votes.Remove vote |> logDbDelete requestName
        ) votes
        ctx.SaveChanges() |> ignore
        refreshCache dbCache.Votes (fun () -> ctx.Votes |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)

