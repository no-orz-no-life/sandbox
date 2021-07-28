open System
open System.IO
open iText.Kernel.Colors
open iText.Kernel.Geom
open iText.Kernel.Pdf
open iText.Kernel.Pdf.Canvas

open iText.IO.Font.Constants
open iText.Kernel.Font
(*
    https://kb.itextpdf.com/home/it7kb/examples
    https://api.itextpdf.com/iText7/dotnet/latest/
*)
[<EntryPoint>]
let main argv =
    Directory.CreateDirectory("output") |> ignore

    let birthDay = DateTime(1975, 12, 24)
    let today = DateTime.Today
    let doomsDay = birthDay.AddYears(90)

    use pdfWriter = new PdfWriter("output/out.pdf")
    use pdfDocument = new PdfDocument(pdfWriter)
    let page = pdfDocument.AddNewPage(PageSize.A3)
    let pdfCanvas = PdfCanvas(page)

    let black = DeviceCmyk(0.0f, 0f, 0f, 1.0f)
    let darkGray = DeviceCmyk(0f, 0f, 0f, 0.7f)
    let lightGray = DeviceCmyk(0f, 0f, 0f, 0.4f)
    let white = DeviceCmyk(0.0f, 0.0f, 0.0f, 0.0f)

    let leftMargin = 30.0f
    let rightMargin = 30.0f
    let topMargin = 40.0f
    let gap = 4.0f
    let columns = 68
    let gridSize = (page.GetPageSize().GetWidth() - (leftMargin + rightMargin) - gap * (float32  (columns - 1))) / (float32 columns)

    pdfCanvas.SetStrokeColor(black) |> ignore

    let drawRectPath x y =
        pdfCanvas.Rectangle(
            (double x) * (double (gridSize + gap)) + (double leftMargin),
            (page.GetPageSize().GetHeight() |> double) - ((double y) * (double (gridSize + gap)) + (double topMargin)),
            double gridSize,
            double gridSize
        )
    let mutable d = birthDay
    let mutable x = 0
    let mutable y = 0

    while d < doomsDay do
        (drawRectPath x y)
            .ClosePathStroke() |> ignore

        if d < today then
            pdfCanvas
                .SetFillColor(darkGray) |> ignore
            (drawRectPath x y)
                .FillStroke() |> ignore

        x <- x + 1
        if x >= columns then
            x <- 0
            y <- y + 1

        d <- d.AddDays(7.0)


    let pdfFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)
    pdfCanvas
        .SetFontAndSize(pdfFont, 2.0f)
        .ShowText("Hello, World.") |> ignore
   
    pdfDocument.Close()
    
    0 // return an integer exit code