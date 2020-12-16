namespace RxStore


type Versioned<'Version, 'Item> =
    { Version: 'Version;
      Item: 'Item;}

type Versioned<'Item> = Versioned<Version, 'Item>

and Version = int64

and DynamicVersion =
    { Lifeline: System.DateTimeOffset;
      Value: Version }


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


type Transition<'State, 'Event> =
    { State: 'State;
      Event: 'Event }

type Update<'Version, 'State, 'Event> =
| DoNotModify
| Replace of 'State
| Update of 'Event
