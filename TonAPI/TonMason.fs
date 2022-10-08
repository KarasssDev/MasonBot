namespace TonApi

open System.Collections.Generic

open MasonNft
open Core
open MasonCore.BlockchainTypes

module TonMason =
    
    let private COLLECTION_WALLET = "EQDJ5PFAOCeNV3BUZ4NXrpuQGknqlx3ygCje-ZxfboYvRzNE"
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
    
    let getNftsCount wallet = opt {
        let! nfts = getNfts wallet
        return nfts.nft_items.Length
    }
    
    let getStatistics = owner2NftCount
    