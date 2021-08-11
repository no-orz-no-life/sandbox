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

    // Font size
    let titleSize = 20.0f
    let revisionSize = 6.0f
    let labelSize = 5.0f

    // Colors
    let black = DeviceCmyk(0.0f, 0f, 0f, 1.0f)
    let darkGray = DeviceCmyk(0f, 0f, 0f, 0.7f)
    let lightGray = DeviceCmyk(0f, 0f, 0f, 0.4f)
    let white = DeviceCmyk(0.0f, 0.0f, 0.0f, 0.0f)

    // Layout
    let leftMargin = 40.0f
    let rightMargin = 40.0f
    let topMargin = 60.0f
    let gap = labelSize + 2.0f
    let columns = 58

    let gridSize = (page.GetPageSize().GetWidth() - (leftMargin + rightMargin) - gap * (float32  (columns - 1))) / (float32 columns)

    let pdfFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN)

    // Position
    let headerY = (page.GetPageSize().GetHeight() |> double)- 40.0
    let headerX = 30.0f

    pdfCanvas
        .SetStrokeColor(black)
        .SetFontAndSize(pdfFont, titleSize)
        .SetFillColor(black)
        .BeginText()
        .SetTextMatrix(headerX, float32 headerY)
        .ShowText("Lifetime Map")
        .EndText()
        .BeginText()
        .SetFontAndSize(pdfFont, revisionSize)
        .SetTextMatrix(page.GetPageSize().GetWidth() - 100.0f, float32 headerY)
        .ShowText("revision 20210812")
        .EndText() |> ignore
    pdfCanvas
        .SetStrokeColor(black)
        .SetFontAndSize(pdfFont, labelSize)
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

        let jitter = 1.0
        let textToWrite = sprintf "%d/%d" d.Year d.Month
        let labelBaseLineY = baseY + (double gridSize) / 2.0 - (double labelSize) / 2.0
        let textWidth = pdfFont.GetWidth(textToWrite, labelSize) |> double
        if x = 0 then
            (baseX - textWidth - jitter, labelBaseLineY) |> Some
        elif x = columns - 1 then
            (baseX + (double gridSize) + jitter, labelBaseLineY) |> Some
        elif d.Month <> lastMonth.Month then
            (baseX, baseY - (double labelSize))  |> Some
        else
            None
        |> Option.iter (fun (posX, posY) -> 
            pdfCanvas
                .SetFillColor(black)
                .BeginText()
                .SetTextMatrix(float32 posX, float32 posY)
                .ShowText(textToWrite)
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