Attribute VB_Name = "Module1_Basics"
' Module1_Basics.bas - Inventor VBA basics (Tools > VBA Editor, File > Import File...)
' VBA is no longer installed with Inventor: get the separate "VBA module for Inventor 2026" download. Legacy:
' 64-bit VBA 7.1, no new language features, macros live in .ivb projects (Default.ivb / ApplicationProject).
' Use it to PROTOTYPE API calls quickly, then move to iLogic or an add-in.
Option Explicit

Public Sub HelloInventor()
    MsgBox "Inventor " & ThisApplication.SoftwareVersion.DisplayVersion & vbCrLf & _
           "Open documents: " & ThisApplication.Documents.Count
End Sub

' List all parameters of the active part/assembly to the Immediate window (Ctrl+G)
Public Sub ListParameters()
    Dim doc As Document
    Set doc = ThisApplication.ActiveDocument
    If doc Is Nothing Then Exit Sub
    Dim params As Parameters
    If doc.DocumentType = kPartDocumentObject Then
        Set params = doc.ComponentDefinition.Parameters
    ElseIf doc.DocumentType = kAssemblyDocumentObject Then
        Set params = doc.ComponentDefinition.Parameters
    Else
        MsgBox "Open a part or assembly": Exit Sub
    End If
    Dim p As Parameter
    For Each p In params
        Debug.Print p.Name, p.Expression, p.Value   ' Value is in cm / rad (database units)
    Next
End Sub

' Change a user parameter and update - the classic first macro
Public Sub SetLength()
    Dim doc As PartDocument
    Set doc = ThisApplication.ActiveDocument
    Dim p As UserParameter
    On Error Resume Next
    Set p = doc.ComponentDefinition.Parameters.UserParameters.Item("Length")
    On Error GoTo 0
    If p Is Nothing Then
        Set p = doc.ComponentDefinition.Parameters.UserParameters.AddByExpression("Length", "100 mm", "mm")
    End If
    Dim s As String
    s = InputBox("New length (mm):", "SetLength", "150")
    If s = "" Then Exit Sub
    p.Expression = s & " mm"
    doc.Update
End Sub
