namespace RxStore.Entity


type Stamp<'Item> =
    { Time: Time;
      Item: 'Item }

and Time = System.DateTimeOffset


module Stamp =

    let ofValues time item =
        { Time = time;
          Item = item }

    let map mapping stamp =
        { Time = stamp.Time;
          Item = mapping stamp.Item }

    let mapTo item stamp =
        { Time = stamp.Time;
          Item = item }
