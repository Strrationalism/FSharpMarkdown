# FSharpMarkdown
The DSL for generating Markdown Document.

### Example

```fsharp
open FSharpMarkdown
open FSharpMarkdown.Markdown
open FSharpMarkdown.Generator

markdown [
    h1 "Hello"
    h2 "Hello"
    h3 "Hello"
    
    p "Hello"
    
    code "fsharp" """printfn "Hello, world!" """
    
    table 
        [ "col1"; "col2"; "col3" ]
        [ [ "1"; "2"; "3" ]
          [ "4"; "5"; "6" ] ]
          
    ul [
        "Hello"
        "Hello"
        "Hello"
    ]
]
|> printfn "%s"
```

# Hello

## Hello

### Hello

Hello

```fsharp
printfn "Hello, world!" 
```

| col1 | col2 | col3 |
| :--: | :--: | :--: |
| 1    | 2    | 3    |
| 4    | 5    | 6    |

* Hello
* Hello
* Hello


