namespace Handlers

open System.Collections.Generic

open MasonCore

module Runtime =

    type VoteCreatingInfo = {
        x: int // TODO
    }

    type UserInfo = {
        voteCratingInfo: VoteCreatingInfo
    }

    let runtime = Dictionary<int64, UserInfo>()
