namespace TonApi

open System.Threading.Tasks
open Flurl.Http

open Logging

module internal Core =
    let private TIMEOUT = 5
    let private TONAPI_IO = "https://tonapi.io/v1/"
    let private TONCENTER_COM = "https://toncenter.com/api/v2/"
    
    type OptionBuilder() =
        member x.Bind(v,f) = Option.bind f v
        member x.Return v = Some v
        member x.ReturnFrom o = o
        member x.Zero () = None

    let opt = OptionBuilder()
    
    let private add name value ps =
        $"{name}={value}" :: ps
    let private addOption name value ps =
        match value with
        | Some value -> add name value ps
        | None -> ps
        
    let searchItems 
        (owner: string option)
        (collection: string option)
        (includeOnSale: bool option)
        (limit: int)
        (offset: int) =
        
        let query =
               addOption "owner" owner []
            |> addOption "collection" collection
            |> addOption "include_on_sale" includeOnSale
            |> add "limit" limit
            |> add "offset" offset
            
        task {
            Logging.logInfo $"Requesting nft/searchItems with parameters: {query}"
            let urlBase = TONAPI_IO + "nft/searchItems"
            let request = urlBase.AllowAnyHttpStatus().WithTimeout(TIMEOUT)
            return! (query |> request.SetQueryParams).GetAsync()
        }
    
    let private messageOf (response: IFlurlResponse) =
        response.GetStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
    
    let private processErrorFail response =
        Logging.logError $"{messageOf response}"
        None
    
    let processResponseAsync f (response: Task<IFlurlResponse>) =
        task {
            let! response = response
            let retcode = response.StatusCode
            Logging.logInfo $"Response with code {retcode}"
            match retcode with
            | 200 -> 
                let! result = f response
                return Some result
            | 401 ->
                Logging.logError "Access token is missing or invalid"
                return None
            | 400 | 404 -> return processErrorFail response
            | 500 ->
                Logging.logInfo $"{messageOf response}"
                return None
            | _ ->
                Logging.logError "Unexpected response code returned"
                return processErrorFail response
        } |> Async.AwaitTask |> Async.RunSynchronously