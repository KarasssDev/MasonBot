namespace MasonCore

open System

module VotingTypes =


    type VariantID = int64

    type VotingID = int64

    type Description = string

    type Vote = VariantID * BlockchainTypes.NftAddress

    type Variant = VariantID * Description

    type Voting =
        | Important of VotingID * Description * Variant list * DateTime
        | Default of VotingID * Description * Variant list * DateTime

    type VotingResult =
        | Accepted of VotingID * (VariantID * int64) list
        | Declined of VotingID * (VariantID * int64) list
        | InProgress of VotingID * (VariantID * int64) list
