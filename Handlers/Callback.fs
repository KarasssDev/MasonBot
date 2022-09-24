namespace Handlers

module Callback =

    type CallbackContent =
        | HowToMason
        | WhatIsMason

    let string2CallbackContent str =
        match str with
        | "HowToMason" -> Some HowToMason
        | "WhatIsMason" -> Some WhatIsMason
        | _ -> None

    let callbackContent2String content =
        match content with
        | HowToMason -> "HowToMason"
        | WhatIsMason -> "WhatIsMason"
