namespace RxStore

type Never = private Never of Never

module Never =

    [<CompiledName "Absurd">]
    let absurd<'a> (never: Never) : 'a = failwith "Absurd."
