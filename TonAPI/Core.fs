namespace TonApi

open System.Threading.Tasks
open Flurl.Http

open Logging
open MasonCore.Utils
open MasonNft
open TonApi.MasonNft

module internal Core =
    let private TIMEOUT = 5
    let private TONAPI_IO = "https://tonapi.io/v1/"
    
    type OptionBuilder() =
        member x.Bind(v,f) = Option.bind f v
        member x.Return v = Some v
        member x.ReturnFrom o = o
        member x.Zero () = None

    let opt = OptionBuilder()
    let res = ResultBuilder()
    type private ApiMethod = SearchItems | GetTransaction | GetInfo
    
    let private apiMethod2url = function
        | SearchItems -> "nft/searchItems"
        | GetTransaction -> "blockchain/getTransaction"
        | GetInfo -> "account/getInfo"
    
    let private asyncRequest apiMethod (query: string list) (timeout: int) =
        task {
            let method = apiMethod2url apiMethod
            Logging.logInfo $"Requesting {method} with parameters: {query}"
            let urlBase = $"{TONAPI_IO}{method}"
            let request = urlBase.AllowAnyHttpStatus().WithTimeout(timeout)
            return! (query |> request.SetQueryParams).GetAsync()
        }
    
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
            
        asyncRequest SearchItems query TIMEOUT
    
    let getTransaction (hash: string) =
        let query = add "hash" hash []
        asyncRequest GetTransaction query TIMEOUT
    
    
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