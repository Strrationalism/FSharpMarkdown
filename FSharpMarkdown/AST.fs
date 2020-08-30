namespace FSharpMarkdown

type TextElement =
| Plain of string
| Italic of string
| Bold of string
| ItalicBold of string
| Strike of string
| Hyperlink of text:string * href:string
| Directlink of href:string
| Image of altText:string * src:string
| InlineCode of code:string

type Text = TextElement seq

type ListType =
| Unordered
| Ordered

type List = {
    listType : ListType
    items : (Text * List option) seq
}

type TaskList = {
    items : (bool * Text * TaskList option) seq
}

type Align =
| Left
| Center
| Right

type Table = {
    header : (Text * Align) seq
    content : Text seq seq
}

type Element = 
| Heading of level:int * Text
| Code of language:string * code:string
| Separator
| List of List
| TaskList of TaskList
| Table of Table
| BlockQuote of level:int * Text
| Paragraph of Text

type Document = Element seq
