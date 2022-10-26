namespace Handlers

open System.Collections.Concurrent

open MasonCore

module Runtime =

    // State
    type State =
        // Voting states
        | WaitChooseVotingType
        | WaitVotingDescription
        | WaitVotingVariant
        | WaitAcceptVotingCreation

    // Authorization
    type AuthorizationInfo = {
        message: string
    }

    // Voting
    type VotingType =
        | Important
        | Default

    type VoteCreatingInfo = {
        votingType: VotingType option
        description: string option
        variants: string list
    }

    // Runtime
    type UserInfo = {
        authorizationInfo: AuthorizationInfo option
        voteCreatingInfo: VoteCreatingInfo option
        state: State option
        lastMessage: int64 option
    }

    let private runtime = ConcurrentDictionary<int64, UserInfo>()

    let private defaultUserInfo = {
        authorizationInfo = None
        voteCreatingInfo = None
        state = None
        lastMessage = None
    }

    let getOrDefault id =
        if runtime.ContainsKey id then
            runtime[id]
        else
            defaultUserInfo

    let update id updateInfo =
        let current = getOrDefault id
        if runtime.ContainsKey id then
            runtime[id] <- updateInfo current
        else
            runtime.TryAdd (id, updateInfo current) |> ignore

    // Message helpers
    let setLastMessage id messageId  =
        update id (fun x -> { x with lastMessage = Some messageId })
    let getLastMessage id =
        getOrDefault id |> fun x -> x.lastMessage

    // State helpers
    let getState id = getOrDefault id |> fun x -> x.state
    let setState id state = update id (fun x -> { x with state = Some state })
    let discardState id = update id (fun x -> { x with state = None })

    // Authorization helpers
    let disableAuthorization id = update id <| fun i -> {i with authorizationInfo = None}

    // Voting helpers
    let enableVotingCreating id = update id <| fun i -> { i with voteCreatingInfo = Some { votingType = None; description = None; variants = [] }}
    let disableVotingCreating id =
        discardState id
        update id <| fun i -> {i with voteCreatingInfo = None}
    let setVotingType id votingType = update id <| fun i -> { i with voteCreatingInfo = Some { i.voteCreatingInfo.Value with votingType = Some votingType }}
    let setVotingDescription id description = update id <| fun i -> { i with voteCreatingInfo = Some { i.voteCreatingInfo.Value with description = Some description }}
    let addVotingVariant id variant = update id <| fun i -> { i with voteCreatingInfo = Some { i.voteCreatingInfo.Value with variants = i.voteCreatingInfo.Value.variants @ [variant] }}
    let getVotingCreatingInfo id = getOrDefault id |> fun x -> x.voteCreatingInfo
