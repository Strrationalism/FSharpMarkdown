module FSharpMarkdown.Generator

open System.Text

let newLine = System.Environment.NewLine

exception MarkdownException of string

module private Markdown =
    let mdTextElement = function
    | Plain t -> t
    | Italic t -> sprintf "*%s*" t
    | Bold t -> sprintf "**%s**" t
    | ItalicBold t -> sprintf "***%s***" t
    | Strike t -> sprintf "~~%s~~" t
    | Hyperlink (text,href) -> sprintf "[%s](%s)" text href
    | Directlink href -> sprintf "<%s>" href
    | Image (altText,src) -> sprintf "![%s](%s)" altText src
    | InlineCode c -> sprintf "`%s`" c

    let mdText x = x |> Seq.map mdTextElement |> Seq.reduce (fun a b -> a + " " + b)

    let mdList x =
        let rec gen identLevel x = 
            match x with
            | None -> ""
            | Some x -> 
                let header = 
                    match x.listType with
                    | Unordered -> "*"
                    | Ordered -> "1."
                    |> (+) (String.init (3*identLevel) (fun _ -> " "))
                x.items
                |> Seq.map (fun (text,child) ->
                    sprintf "%s %s%s%s" header (mdText text) newLine (gen (identLevel+1) child))
                |> Seq.reduce (+)
        gen 0 (Some x)

    let mdTaskList x =
        let rec gen identLevel x = 
            match x with
            | None -> ""
            | Some x -> 
                x.items
                |> Seq.map (fun (f,text,child) ->
                    let header = 
                        match f with
                        | true -> "- [x]"
                        | false -> "- [ ]"
                        |> (+) (String.init (3*identLevel) (fun _ -> " "))
                    sprintf "%s %s%s%s" header (mdText text) newLine (gen (identLevel+1) child))
                |> Seq.reduce (+)
        gen 0 (Some x)

    let mdTable x =
        let content =
            x.content
            |> Seq.map (Seq.map mdText >> Seq.toArray)
            |> Seq.toArray
        let (headerText,align) = x.header |> Seq.toArray |> Array.unzip
        let headerText = headerText |> Array.map mdText
        let paddings = 
            Seq.append [headerText] content
            |> Seq.map (Array.map String.length)
            |> Seq.reduce (Array.map2 max)
            |> Seq.toArray
        let headerText = 
            headerText 
            |> Array.mapi (fun i x -> x.PadRight (paddings.[i],' '))
            |> Array.fold (fun s x -> s + " " + x + " |") "|"
        let alignInfo =
            align
            |> Array.mapi (fun i -> function
            | Left -> ":" + String.init (paddings.[i] - 1) (fun _ -> "-")
            | Right -> String.init (paddings.[i] - 1) (fun _ -> "-") + ":"
            | Center -> ":" + String.init (paddings.[i] - 2) (fun _ -> "-") + ":")
            |> Array.fold (fun s x -> s + " " + x + " |") "|"
        let content = 
            content
            |> Array.map (
                Array.mapi (fun i x -> x.PadRight (paddings.[i],' '))
                >> Array.fold (fun s x -> s + " " + x + " |") "|")
            |> Array.reduce (fun a b -> a + newLine + b)
        headerText + newLine + alignInfo + newLine + content + newLine
            
            

    let mdElement = function
    | Heading (level,text) ->
        if level >= 1 && level <= 6 then
            (String.init level (fun _ -> "#")) + " " + mdText text + newLine
        else raise (MarkdownException "Heading level must between 1 to 6.")
    | Code (language,code) ->
        sprintf "```%s%s%s%s```%s" (language.Trim ()) newLine code newLine newLine
    | Separator -> sprintf "* * *%s" newLine
    | List list -> mdList list
    | TaskList list -> mdTaskList list
    | Table t -> mdTable t
    | BlockQuote (i,t) -> (String.init i (fun _ -> "> ")) + (mdText t) + newLine
    | Paragraph t -> mdText t + newLine

let markdown (doc: Document) =
    doc
    |> Seq.map Markdown.mdElement
    |> Seq.reduce (fun a b -> a + newLine + b)

