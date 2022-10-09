namespace MasonCore

module Utils =
    let (>>=) (m: Result<'a, 'e>) (f: 'a -> Result<'b, 'e>) = Result.bind f m
    let (>=>) (f1: 'a -> Result<'b, 'e>) (f2: 'b -> Result<'c, 'e>) = fun x -> f1 x >>= f2

    type ResultBuilder() =
      member this.Return(x) = Ok x
      member this.ReturnFrom(x) = x
      member this.Bind(m, f) = m >>= f