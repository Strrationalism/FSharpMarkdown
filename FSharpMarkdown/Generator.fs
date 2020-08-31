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

module private Html =
    let htmlTextElement = function
    | Plain t -> t
    | Italic t -> sprintf "<i>%s</i>" t
    | Bold t -> sprintf "<strong>%s</strong>" t
    | ItalicBold t -> sprintf "<i><strong>%s</strong></i>" t
    | Strike t -> sprintf "<s>%s</s>" t
    | Hyperlink (t,href) -> sprintf "<a href=\"%s\">%s</a>" href t
    | Directlink t -> sprintf "<a href=\"%s\">%s</a>" t t
    | Image (alt,src) -> sprintf "<img alt=\"%s\" src=\"%s\" />" alt src
    | InlineCode c -> sprintf "<code>%s</code>" c

    let htmlText x = x |> Seq.map htmlTextElement |> Seq.reduce (fun a b -> a + " " + b)

    let rec htmlList x =
        let wrapper =
            match x.listType with
            | Ordered -> sprintf "<ol>%s%s</ol>%s" newLine
            | Unordered -> sprintf "<ul>%s%s</ul>%s" newLine
        x.items
        |> Seq.map (fun (t,child) ->
            let child = 
                match child with
                | None -> ""
                | Some x -> htmlList x
            sprintf "<li>%s</li>%s" (htmlText t + child) newLine)
        |> Seq.reduce (+)
        |> fun x -> wrapper x newLine

    let rec htmlTasklist x =
        x.items
        |> Seq.map (fun (finished, t, child) -> 
            let child = 
                match child with
                | Some x -> htmlTasklist x
                | None -> ""
            let chk = if finished then "checked" else ""
            sprintf "<p><input type=\"checkbox\" disabled=\"disabled\" %s/>%s</p>%s" 
                chk (htmlText t + child) newLine)
        |> Seq.reduce (+)

    let htmlTable x =
        let (header,align) =
            x.header
            |> Seq.map (fun (text,align) -> htmlText text, align)
            |> Seq.toArray
            |> Array.unzip
        let align = 
            align
            |> Array.map (function
            | Left -> "left"
            | Center -> "center"
            | Right -> "right")
        let header =
            header
            |> Array.mapi (fun i -> sprintf "<th align=\"%s\">%s</th>" align.[i])
            |> Array.reduce (+)
            |> fun x -> sprintf "<tr>%s</tr>%s" x newLine

        let content =
            x.content
            |> Seq.map (fun x -> 
                x
                |> Seq.mapi (fun i x ->
                    htmlText x |> sprintf "<td align=\"%s\">%s</td>" align.[i])
                |> Seq.reduce (+)
                |> fun x -> sprintf "<tr>%s</tr>%s" x newLine)
            |> Seq.reduce (+)
        
        sprintf "<table>%s%s%s</table>%s" newLine header content newLine

    let htmlElement = function
    | Heading (level,x) ->
        if level >= 1 && level <= 6 then
            sprintf "<h%d>%s</h%d>%s" level (htmlText x) level newLine
        else raise (MarkdownException "Heading level must between 1 to 6.")
    | Code (_,c) -> sprintf "<code>%s%s%s</code>%s" newLine c newLine newLine
    | Separator -> sprintf "<hr/>%s" newLine
    | List ls -> htmlList ls
    | TaskList ls -> htmlTasklist ls
    | Table t -> htmlTable t
    | BlockQuote (level,t) -> 
        let leftWrapper = String.init level (fun _ -> "<r>")
        let rightWrapper = String.init level (fun _ -> "</r>")
        sprintf "%s%s%s%s%s%s" 
            leftWrapper newLine (htmlText t) newLine rightWrapper newLine
    | Paragraph p ->
        sprintf "<p>%s%s%s</p>%s" newLine (htmlText p) newLine newLine

let html (doc: Document) =
    doc
    |> Seq.map Html.htmlElement
    |> Seq.reduce (fun a b -> a + newLine + b)


