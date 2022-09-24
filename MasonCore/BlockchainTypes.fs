namespace MasonCore

module BlockchainTypes =

    type NftAddress = string

    type WalletAddress = string

    type Wallet = {
        address: WalletAddress
        nftsCnt: int
        nfts: NftAddress list
    }
