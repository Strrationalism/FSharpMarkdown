module FSharpMarkdown.Markdown

let h1 text = Heading (1,[Plain text])
let h2 text = Heading (2,[Plain text])
let h3 text = Heading (3,[Plain text])
let h4 text = Heading (4,[Plain text])
let h5 text = Heading (5,[Plain text])
let h6 text = Heading (6,[Plain text])

let code language content = Code (language,content)

let p text = Paragraph [Plain text]

let q text = BlockQuote (1,[Plain text])

let img alt src = Paragraph [ Image(alt,src) ]

let ul items = 
    List { listType = Unordered; items = Seq.map (fun (t: string) -> Seq.singleton (Plain t), None) items }

let ol items =
    List { listType = Ordered; items = Seq.map (fun (t: string) -> Seq.singleton (Plain t), None) items }

let todo items =
    TaskList { items = Seq.map (fun (f: bool,t: string) -> f, Seq.singleton (Plain t), None) items }

let table header content =
    Table {
        header = header |> Seq.map (fun x -> Seq.singleton (Plain x),Center)
        content = content |> Seq.map (Seq.map (Plain >> Seq.singleton))
    }

