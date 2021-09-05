open System.Xml
open System.Xml.Linq
open System.Xml.XPath

let doc = XDocument.Load("hwenc-test.xml")

let visit (node:XNode) =
    let rec visit (node:XNode) = 
        match node with
        | :? XDocument as doc -> 
            visit doc.Root
        | :? XElement as elem ->
            printfn "Element: %A" (elem.Name)
            for n in elem.Nodes() do
                visit n
        | node when node.NodeType = XmlNodeType.Text ->
            printfn "(text) %A" ((node :?> XText).Value)
        | node -> 
            printfn "(%A)" node.NodeType 
    visit node

visit doc.Root

