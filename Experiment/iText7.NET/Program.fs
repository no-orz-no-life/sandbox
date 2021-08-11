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

    let leftMargin = 40.0f
    let rightMargin = 40.0f
    let topMargin = 60.0f
    let gap = 4.0f
    let columns = 68
    let gridSize = (page.GetPageSize().GetWidth() - (leftMargin + rightMargin) - gap * (float32  (columns - 1))) / (float32 columns)

    let pdfFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)

    let headY = (page.GetPageSize().GetHeight() |> double)- 40.0
    pdfCanvas
        .SetStrokeColor(black)
        .SetFontAndSize(pdfFont, 20.0f)
        .SetFillColor(black)
        .BeginText()
        .SetTextMatrix(30.0f, float32 headY)
        .ShowText("Lifetime Map")
        .EndText()
        .BeginText()
        .SetFontAndSize(pdfFont, 6.0f)
        .SetTextMatrix(page.GetPageSize().GetWidth() - 100.0f, float32 headY)
        .ShowText("revision 20210730")
        .EndText() |> ignore
    pdfCanvas
        .SetStrokeColor(black)
        .SetFontAndSize(pdfFont, 2.0f)
        |> ignore

    let drawRectPath x y =
        pdfCanvas.Rectangle(
            x,
            y,
            double gridSize,
            double gridSize
        )
    let mutable d = birthDay
    let mutable x = 0
    let mutable y = 0
    let mutable lastMonth = DateTime.MinValue
    while d < doomsDay do
        let baseX = (double x) * (double (gridSize + gap)) + (double leftMargin)
        let baseY = (page.GetPageSize().GetHeight() |> double) - ((double y) * (double (gridSize + gap)) + (double topMargin))
        (drawRectPath baseX baseY)
            .ClosePathStroke() |> ignore

        if d < today then
            (drawRectPath baseX baseY)
                .SetFillColor(darkGray)
                .FillStroke() |> ignore

        if x = 0 then
            (30.0, baseY + (double gridSize) / 2.0) |> Some
        elif x = columns - 1 then
            ((page.GetPageSize().GetWidth() |> double) - 35.0, baseY + (double gridSize) / 2.0) |> Some
        elif d.Month <> lastMonth.Month then
            (baseX, baseY  - 2.4) |> Some
        else
            None
        |> Option.iter (fun (posX, posY) -> 
            pdfCanvas
                .SetFillColor(black)
                .BeginText()
                .SetTextMatrix(float32 posX, float32 posY)
                .ShowText(sprintf "%d/%d" d.Year d.Month)
                .EndText() |> ignore
            )

        x <- x + 1
        if x >= columns then
            x <- 0
            y <- y + 1

        lastMonth <- d
        d <- d.AddDays(7.0)

    pdfDocument.Close()
    
    0 // return an integer exit code