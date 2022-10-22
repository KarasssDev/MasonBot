namespace MasonCore

module UserTypes =

    type UserId = int64

    type User =
        | Master of UserId * BlockchainTypes.WalletAddress
        | Holder of UserId * BlockchainTypes.WalletAddress
        | WithoutNft of UserId * BlockchainTypes.WalletAddress
        | Unverified of UserId

    let masterNftCount = 4 // TODO: only for debug
    let holderNftCount = 1

    let mkUser (userId: UserId) (nftCnt: int option) (wallet: BlockchainTypes.WalletAddress option) =
        match nftCnt with
        | Some cnt ->
            match wallet with
            | Some wallet when cnt >= masterNftCount -> Master (userId, wallet)
            | Some wallet when (holderNftCount <= cnt && cnt <= masterNftCount) -> Holder (userId, wallet)
            | Some wallet -> WithoutNft (userId, wallet)
            | None -> Unverified userId
        | None -> Unverified userId
