namespace TonApi

open System.Collections.Generic

open Core
open MasonNft
open Transaction
open MasonCore.BlockchainTypes


module TonApiQuerying =
    
    let private COLLECTION_WALLET = "0:c9e4f14038278d577054678357ae9b901a49ea971df28028def99c5f6e862f47"
    let private COLLECTION = "EQCFttZHA0tsLteL-w8ymLH3Wa7YeB74CDyVBUB1UtTTKAwG"
    let private NFT_COUNT = 3333
    
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
        <| Some true
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
    
    let getNftsCount (wallet: WalletAddress) = opt {
        let! nfts = getNfts wallet
        return nfts.nft_items.Length
    }
    
    let getStatistics = owner2NftCount
    
    let private getTransaction (hash: TransactionHash) =
        getTransaction hash
        |> processResponseAsync (fun x -> x.GetJsonAsync<Transaction>())
    
    let verifyTransaction (from: WalletAddress) (amount: uint64) (hash: TransactionHash) = opt {
        let! transaction = getTransaction hash
        let msg = transaction.in_msg
        return
            msg.source.address = from &&
            msg.destination.address = COLLECTION_WALLET &&
            msg.value = amount
    }
    
    