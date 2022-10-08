namespace MasonCore

module UserTypes =

    type UserId = int64

    type User =
        | Master of UserId * BlockchainTypes.WalletAddress
        | Holder of UserId * BlockchainTypes.WalletAddress
        | WithoutNft of UserId * BlockchainTypes.WalletAddress
        | Unverified of UserId

    let mkUser (userId: UserId) (nftCnt: int) (wallet: BlockchainTypes.WalletAddress option) =
        match wallet with
        | Some wallet when nftCnt >= 23 -> Master (userId, wallet)
        | Some wallet when (0 < nftCnt && nftCnt <= 23) -> Holder (userId, wallet)
        | Some wallet -> WithoutNft (userId, wallet)
        | None -> Unverified userId
