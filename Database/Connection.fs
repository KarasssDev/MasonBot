namespace Database

open System
open System.ComponentModel.DataAnnotations
open MasonCore
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open Microsoft.FSharp.Core

module Connection =

    [<CLIMutable>]
    type User = {
        [<Key>] TelegramId: int64
        Wallet: string option
    }

    and [<CLIMutable>] Voting = {
        [<Key>] Id: Guid
        Creator: User
        Description: string
        StartDate: DateTime
        Duration: TimeSpan
        Variants: Variant seq
    }

    and [<CLIMutable>] Variant = {
        [<Key>] Id: Guid
        Description: string
    }

    [<CLIMutable>]
    type Vote = {
        [<Key>] Id: Guid
        User: User
        Variant: Variant
        NftAddress: string
    }

    type DbCache = {
        Users: Collections.Concurrent.ConcurrentDictionary<int64, User>
        Votings: Collections.Concurrent.ConcurrentDictionary<Guid, Voting>
        Variants: Collections.Concurrent.ConcurrentDictionary<Guid, Variant>
        Votes: Collections.Concurrent.ConcurrentDictionary<Guid, Vote>
    }

    type MasonDbContext() =
        inherit DbContext()

        [<DefaultValue>] val mutable users : DbSet<User>
        member this.Users with get() = this.users and set v = this.users <- v

        [<DefaultValue>] val mutable votings: DbSet<Voting>
        member this.Votings with get() = this.votings and set v = this.votings <- v

        [<DefaultValue>] val mutable variants : DbSet<Variant>
        member this.Variants with get() = this.variants and set v = this.variants <- v

        [<DefaultValue>] val mutable votes : DbSet<Vote>
        member this.Votes with get() = this.votes and set v = this.votes <- v


        override _.OnModelCreating builder =
            builder.RegisterOptionTypes()
            builder.Entity<Voting>().Navigation<Variant seq>(fun x -> x.Variants).AutoInclude() |> ignore
            builder.Entity<Voting>().Navigation(fun x -> x.Creator).AutoInclude() |> ignore
            builder.Entity<Vote>().Navigation(fun x -> x.User).AutoInclude() |> ignore
            builder.Entity<Vote>().Navigation(fun x -> x.Variant).AutoInclude() |> ignore

        override _.OnConfiguring(options: DbContextOptionsBuilder) : unit =
            #if DEBUG
            Paths.configureDataPath ""
            #endif
            Logging.Logging.logDebug $"Database path: {Paths.databasePath()}"
            options
                //.LogTo(Action<string> (fun x -> printfn $"{x}"))
                .UseSqlite($"Data Source={Paths.databasePath()}")
                .UseFSharpTypes() |> ignore



