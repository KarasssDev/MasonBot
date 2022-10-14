namespace Handlers

open System.Collections.Generic

open MasonCore

module Runtime =

    type AuthorizationInfo = {
        message: string
    }
    
    type VoteCreatingInfo = {
        x: int // TODO
    }

    type UserInfo = {
        authorizationInfo: AuthorizationInfo option
    }

    let private runtime = Dictionary<int64, UserInfo>()
    
    let private defaultUserInfo = {
        authorizationInfo = None
    }
    
    let getOrDefault id =
        if runtime.ContainsKey id then
            runtime[id]
        else
            defaultUserInfo
    
    let update id info =
        if runtime.ContainsKey id then
            runtime[id] <- info
        else
            runtime.Add (id, info)
    
    let disableAuthorization id =
        let info = getOrDefault id
        update id {info with authorizationInfo = None}