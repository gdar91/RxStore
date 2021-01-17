namespace RxStore

type Never = Never of Never

module Never =

    [<CompiledName "Absurd">]
    let absurd<'a> (never: Never) = failwith "Absurd."
