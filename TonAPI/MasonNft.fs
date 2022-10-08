namespace TonApi

module internal MasonNft =
    
    type Collection = {
        address: string
        name: string
    }

    type Attribute = {
        trait_type: string
        value: string
    }

    type Metadata = {
        attributes: Attribute array
        description: string
        image: string
        name: string
    }

    type Owner = {
        address: string
        is_scam: bool
    }

    type Preview = {
        resolution: string
        url: string
    }

    type MasonNft = {
        address: string
        collection: Collection
        collectionAddress: string
        index: int
        metadata: Metadata
        owner: Owner
        previews: Preview array
        verified: bool
    }
    
    type MasonNfts = {
        nft_items: MasonNft array
    }