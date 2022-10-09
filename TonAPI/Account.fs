namespace TonApi

module Account =
    type Addresses = {
        bounceable: string
        non_bounceable: string
        raw: string
    }
    
    type Account = {
        address: Addresses
    }