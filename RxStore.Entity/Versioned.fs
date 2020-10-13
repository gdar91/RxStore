namespace RxStore.Entity


type Versioned<'Version, 'Item> =
    { Version: 'Version;
      Item: 'Item;}

type Versioned<'Item> = Versioned<Version, 'Item>

and Version = int64


module Versioned =

    [<CompiledName("OfValues")>]
    let ofValues version item =
        { Version = version;
          Item = item }

    [<CompiledName("Map")>]
    let map mapping versioned =
        { Version = versioned.Version;
          Item = mapping versioned.Item }

    [<CompiledName("MapTo")>]
    let mapTo item versioned =
        { Version = versioned.Version;
          Item = item }

    [<CompiledName("ZipWith")>]
    let zipWith func versionedA versionedB =
        { Version = versionedB.Version;
          Item = func versionedA.Item versionedB.Item }
