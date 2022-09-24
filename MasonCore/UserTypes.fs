namespace MasonCore

module UserTypes =

    type UserId = int64

    type User =
        | Master of UserId * BlockchainTypes.WalletAddress
        | Holder of UserId * BlockchainTypes.WalletAddress
        | Unverified of UserId
