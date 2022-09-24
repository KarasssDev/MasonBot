namespace Database

open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open EntityFrameworkCore.FSharp.DbContextHelpers

module Connection = // WIP

    [<CLIMutable>]
    type Blog = {
        [<Key>] Id: int
        Url: string
    }

    type BloggingContext() =
        inherit DbContext()

        [<DefaultValue>] val mutable blogs : DbSet<Blog>
        member this.Blogs with get() = this.blogs and set v = this.blogs <- v

        override _.OnModelCreating builder =
            builder.RegisterOptionTypes()

        override _.OnConfiguring(options: DbContextOptionsBuilder) : unit =
            options.UseSqlite("Data Source=/home/viktor/RiderProjects/MasonBot/data/data.db") |> ignore

    let lol (ctx: BloggingContext) =
        query {
            for blog in ctx.Blogs do
            where (blog.Id = 11)
            select blog
        }

    let lmao (ctx: BloggingContext) =
        {
            Id = 11
            Url = "123"
        } |> addEntity ctx
        saveChanges ctx


