' BatchExportVB - export every drawing in a folder to PDF (VB.NET, .NET 8).
'   BatchExportVB.exe "C:\Work\Drawings" [outFolder]
' Same logic as the C# 03-BatchExport, written in VB so iLogic users can compare line by line.
Imports Inventor
Imports IO = System.IO

Module Program
    Private Const PdfAddinId As String = "{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}"

    <STAThread>
    Function Main(args As String()) As Integer
        If args.Length = 0 Then
            Console.WriteLine("usage: BatchExportVB <folder> [outFolder]")
            Return 2
        End If
        Dim src = IO.Path.GetFullPath(args(0))
        Dim outDir = If(args.Length > 1, args(1), IO.Path.Combine(src, "_pdf"))
        IO.Directory.CreateDirectory(outDir)

        Dim app = InventorConnector.Connect()
        Dim pdf = CType(app.ApplicationAddIns.ItemById(PdfAddinId), TranslatorAddIn)
        app.SilentOperation = True
        Dim okCount = 0, failCount = 0
        Try
            For Each f In IO.Directory.GetFiles(src, "*.idw")
                Dim doc As Document = Nothing
                Try
                    doc = app.Documents.Open(f, False)
                    Dim ctx = app.TransientObjects.CreateTranslationContext()
                    ctx.Type = IOMechanismEnum.kFileBrowseIOMechanism
                    Dim opts = app.TransientObjects.CreateNameValueMap()
                    Dim dm = app.TransientObjects.CreateDataMedium()
                    If pdf.HasSaveCopyAsOptions(doc, ctx, opts) Then
                        opts.Value("All_Color_AS_Black") = 0
                        opts.Value("Sheet_Range") = PrintRangeEnum.kPrintAllSheets
                    End If
                    dm.FileName = IO.Path.Combine(outDir, IO.Path.GetFileNameWithoutExtension(f) & ".pdf")
                    pdf.SaveCopyAs(doc, ctx, opts, dm)
                    okCount += 1
                    Console.WriteLine("OK   " & IO.Path.GetFileName(f))
                Catch ex As Exception
                    failCount += 1
                    Console.WriteLine("FAIL " & IO.Path.GetFileName(f) & ": " & ex.Message)
                Finally
                    If doc IsNot Nothing Then doc.Close(True)
                End Try
            Next
        Finally
            app.SilentOperation = False
        End Try
        Console.WriteLine($"{okCount} ok, {failCount} failed -> {outDir}")
        Return If(failCount = 0, 0, 1)
    End Function
End Module
