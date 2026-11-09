Attribute VB_Name = "Module2_ExportPDF"
' Module2_ExportPDF.bas - export active drawing to PDF (direct port of the Autodesk API help sample)
Option Explicit

Public Sub ExportActiveDrawingToPDF()
    Dim doc As Document
    Set doc = ThisApplication.ActiveDocument
    If doc.DocumentType <> kDrawingDocumentObject Then MsgBox "Open a drawing": Exit Sub

    Dim pdfAddIn As TranslatorAddIn
    Set pdfAddIn = ThisApplication.ApplicationAddIns.ItemById("{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}")
    Dim ctx As TranslationContext
    Set ctx = ThisApplication.TransientObjects.CreateTranslationContext
    ctx.Type = kFileBrowseIOMechanism
    Dim opts As NameValueMap
    Set opts = ThisApplication.TransientObjects.CreateNameValueMap
    Dim dm As DataMedium
    Set dm = ThisApplication.TransientObjects.CreateDataMedium

    If pdfAddIn.HasSaveCopyAsOptions(doc, ctx, opts) Then
        opts.Value("All_Color_AS_Black") = 0
        opts.Value("Vector_Resolution") = 400
        opts.Value("Sheet_Range") = kPrintAllSheets
    End If
    dm.FileName = Left(doc.FullFileName, Len(doc.FullFileName) - 4) & ".pdf"
    Call pdfAddIn.SaveCopyAs(doc, ctx, opts, dm)
    MsgBox "Saved " & dm.FileName
End Sub
