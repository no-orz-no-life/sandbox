open System.IO
open iText.Kernel.Colors
open iText.Kernel.Pdf
open iText.Kernel.Pdf.Canvas

(*
    https://kb.itextpdf.com/home/it7kb/examples
    https://api.itextpdf.com/iText7/dotnet/latest/
*)
[<EntryPoint>]
let main argv =
    Directory.CreateDirectory("output") |> ignore
    use pdfWriter = new PdfWriter("output/out.pdf")
    use pdfDocument = new PdfDocument(pdfWriter)
    let pdfCanvas = PdfCanvas(pdfDocument.AddNewPage())

    let color = DeviceCmyk(1.0f, 0f, 0f, 0.0f)

    pdfCanvas
        .SetStrokeColor(color)
        .MoveTo(36.0, 36.0)
        .LineTo(36.0, 806.0)
        .ClosePathStroke() |> ignore
    pdfDocument.Close()
    
    0 // return an integer exit code