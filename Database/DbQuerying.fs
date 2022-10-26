namespace Database

open System
open System.Collections.Concurrent
open Database.Connection
open Logging

module DbQuerying =

    // Database cache
    // Cache should be refreshed after modifying the database
    let private dbCache = {
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
            failwith $"Key [{key}] not found in cache [{typeof<'b>.Name}]"

    let private allFromCache (cache: ConcurrentDictionary<'a, 'b>) =
        Logging.logDebug $"Getting all from cache [{typeof<'b>.Name}]"
        cache.Values |> Seq.toList



    // Database logging
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

    // Access to cached data
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

    let getAllVotings () =
        let requestName = "Get all votings"
        logDbRequest requestName None
        allFromCache dbCache.Votings

    let getVoting id =
        let requestName = "Get voting"
        logDbRequest requestName <| Some [$"id={id}"]
        fromCache dbCache.Votings id

    let getVariant id  =
        let requestName = "Get variant"
        logDbRequest requestName <| Some [$"id={id}"]
        fromCache dbCache.Variants id

    let getVotes (variantId: Guid) =
        let requestName = "Get votes"
        logDbRequest requestName <| Some [$"variantId={variantId}"]
        allFromCache dbCache.Votes |> List.filter (fun v -> v.Variant.Id = variantId)

    // Access to database data
    // You should use this for getting data with same context
    let getUserFromCtx (ctx: MasonDbContext) (telegramId: int64)  =
        let requestName = "Get user from ctx"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"]
        query {
            for user in ctx.Users do
            find (user.TelegramId = telegramId)
        } |> logDbGet requestName

    let getVotingFromCtxByVariantId (ctx: MasonDbContext) id =
        let requestName = "Get voting from ctx"
        logDbRequest requestName <| Some [$"id={id}"]
        ctx.Votings |> Seq.find (fun v -> v.Variants |> Seq.exists (fun va -> va.Id = id)) |> logDbGet requestName

    let getVotesFromCtxByVotingId (ctx: MasonDbContext) (votingId: Guid) =
        let requestName = "Get votes from ctx"
        logDbRequest requestName <| Some [$"variantId={votingId}"]
        query {
            for vote in ctx.Votes do
            where (vote.Voting.Id = votingId)
            select vote
        } |> Seq.toList |> logDbGet requestName

    let getVariantFromCtx (ctx: MasonDbContext) (id: Guid)  =
        let requestName = "Get variant from ctx"
        logDbRequest requestName <| Some [$"id={id}"]
        query {
            for variant in ctx.Variants do
            find (variant.Id = id)
        } |> logDbGet requestName

    // Modify database data
    // Don't use functions which access to cache here
    let createUser (ctx: MasonDbContext) (telegramId: int64) (name: string option) (wallet: string option) =
        let requestName = "Get user"
        logDbRequest requestName <| Some [$"telegramId={telegramId}"; $"wallet={wallet}"]
        let user: User = {
            TelegramId = telegramId
            Wallet = wallet
            Name = name
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

    let private deleteVotesByNftsUnsafe (ctx: MasonDbContext) (voting: Voting) (nfts: string seq) =
        let requestName = "Delete votes"
        logDbRequest requestName <| Some [$"nfts={Seq.toList nfts}"]
        let votes = getVotesFromCtxByVotingId ctx voting.Id
        List.iter (fun vote ->
            if nfts |> Seq.contains vote.NftAddress then
                ctx.Votes.Remove vote |> logDbDelete requestName
        ) votes

    let private createVotesUnsafe (ctx: MasonDbContext) (user: User) (voting: Voting) (variant: Variant) (nfts: string seq) =
        let requestName = "Create votes"
        logDbRequest requestName <| Some [$"variant={variant}"; $"nfts={Seq.toList nfts}"]
        Seq.iter (fun nft ->
            ctx.Votes.Add {
                Id = Guid.NewGuid()
                User = user
                Variant = variant
                Voting = voting
                NftAddress = nft
            } |> logDbCreate requestName
        ) nfts

    let makeVote (ctx: MasonDbContext) (user: User) (variant: Variant) (nfts: string seq) =
        let requestName = "Make vote"
        logDbRequest requestName <| Some [$"user={user}"; $"variant={variant}"; $"nft={nfts}"]
        let voting = getVotingFromCtxByVariantId ctx variant.Id
        mkTransaction ctx requestName (fun () ->
            deleteVotesByNftsUnsafe ctx voting nfts
            createVotesUnsafe ctx user voting variant nfts
            ctx.SaveChanges() |> ignore
            refreshCache dbCache.Votes (fun () -> ctx.Votes |> Seq.map (fun v -> v.Id, v) |> List.ofSeq)
        )
