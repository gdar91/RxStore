namespace RxStore.Entity


type Stamp<'Item> =
    { Time: Time;
      Item: 'Item }

and Time = System.DateTimeOffset


module Stamp =

    [<CompiledName("OfValues")>]
    let ofValues time item =
        { Time = time;
          Item = item }

    [<CompiledName("Map")>]
    let map mapping stamp =
        { Time = stamp.Time;
          Item = mapping stamp.Item }

    [<CompiledName("MapTo")>]
    let mapTo item stamp =
        { Time = stamp.Time;
          Item = item }
