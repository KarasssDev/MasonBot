namespace TonApi

open System
open System.Collections.Generic

open Core
open Logging
open MasonNft
open Transaction
open Account
open MasonCore.BlockchainTypes


module TonApiQuerying =
    
    let AUTHORIZATION_WALLET = "0:c9e4f14038278d577054678357ae9b901a49ea971df28028def99c5f6e862f47"
    let COLLECTION_WALLET = "0:b89707a5c01c48ac1c56da15ae19aa313205037dcc12ac181de30c4a4426c5ac"
    let private COLLECTION = "EQCFttZHA0tsLteL-w8ymLH3Wa7YeB74CDyVBUB1UtTTKAwG"
    let private NFT_COUNT = 3333
    
    let private tryWithLog f =
        try
            f ()
        with | :? Exception as ex ->
                 Logging.logError $"Unhandled exception from Ton API: {ex.Message}"
                 None
    
    let private searchMasonItems wallet limit =
        searchItems
        <| Some wallet
        <| Some COLLECTION
        <| Some true
        <| limit
        <| 0
    
    let private getMasonItems () =
        searchItems
        <| None
        <| Some COLLECTION
        <| None
        <| NFT_COUNT
        <| 0
    
    let private parseMasonNfts = processResponseAsync (fun x -> x.GetJsonAsync<MasonNfts>())
    let private getNfts wallet =
        searchMasonItems wallet NFT_COUNT
        |> parseMasonNfts
    
    let private getMasonNfts () =
        getMasonItems () |> parseMasonNfts
        
    let private owner2NftCount () = opt {
        let! nfts = getMasonNfts ()
        let owners = Array.map (fun x -> x.owner.address) nfts.nft_items
        let ownersCounter = 
            Array.fold
            <| fun (dict: Dictionary<WalletAddress, int>) owner ->
                if dict.ContainsKey owner then
                    dict[owner] <- dict[owner] + 1
                else
                    dict.Add(owner, 1)
                dict
            <| Dictionary()
            <| owners
        return ownersCounter
    }
    
    let getNftsCount (wallet: WalletAddress) =
        fun () -> opt {
            let! nfts = getNfts wallet
            return nfts.nft_items.Length
        } |> tryWithLog
    
    let getStatistics () =
        fun () -> owner2NftCount ()
        |> tryWithLog
    
    let getTransaction (hash: TransactionHash) =
        let hash = hash.Replace('+', '-').Replace('/', '_')
        getTransaction hash
        |> processResponseAsync (fun x -> x.GetJsonAsync<Transaction>())
    
    let verifyTransaction message amount (hash: TransactionHash) =
        fun () -> opt {
            let! transaction = getTransaction hash
            let msg = transaction.out_msgs[0]
            let decodedBase64 =
                msg.msg_data
                |> Convert.FromBase64String
                |> System.Text.Encoding.UTF8.GetString
            let mes = decodedBase64[4..]
            return
                msg.destination.address = COLLECTION_WALLET &&
                msg.value = amount &&
                mes = message
                , msg.source.address
        } |> tryWithLog
    
    let private getInfo (wallet: WalletAddress) =
        getInfo wallet
        |> processResponseAsync (fun x -> x.GetJsonAsync<Account>())
    
    let getAddresses (wallet: WalletAddress) =
        fun () -> opt {
            let! info = getInfo wallet
            return info.address
        } |> tryWithLog
        
    