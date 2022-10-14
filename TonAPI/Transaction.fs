namespace TonApi

open MasonNft

module Transaction =
    
    type Message = {
        destination: Owner
        source: Owner
        value: uint64
        msg_data: string
    }
    
    type Transaction = {
        out_msgs: Message array
    }

