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
        [<Key>] TelegramId: int
        Wallet: string option
    }

    [<CLIMutable>]
    type Voting = {
        [<Key>] Id: int
        Creator: User
        Description: string
        StartDate: DateTime
        Duration: DateTime
    }

    [<CLIMutable>]
    type Variant = {
        [<Key>] Id: int
        Description: string
        Voting: Voting
    }

    [<CLIMutable>]
    type Vote = {
        [<Key>] Id: int
        Variant: Variant
        NftAddress: string
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

        override _.OnConfiguring(options: DbContextOptionsBuilder) : unit =
            #if DEBUG
            Paths.configureDataPath ""
            #endif
            options.UseSqlite($"Data Source={Paths.databasePath()}").UseFSharpTypes() |> ignore



