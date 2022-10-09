namespace TonApi

open MasonNft

module Transaction =
    
    type Message = {
        destination: Owner
        source: Owner
        value: uint64
    }
    
    type Transaction = {
        in_msg: Message
    }

