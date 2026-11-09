Attribute VB_Name = "Module3_AssemblyTraverse"
' Module3_AssemblyTraverse.bas - recursive walk of an assembly (occurrences), with quantities
Option Explicit

Public Sub TraverseActiveAssembly()
    Dim asm As AssemblyDocument
    Set asm = ThisApplication.ActiveDocument
    Debug.Print "== " & asm.DisplayName
    Call Walk(asm.ComponentDefinition.Occurrences, 1)
End Sub

Private Sub Walk(occs As ComponentOccurrences, level As Integer)
    Dim occ As ComponentOccurrence
    For Each occ In occs
        Debug.Print String(level * 2, " ") & occ.Name & "  [" & occ.Definition.Document.DisplayName & "]" & _
                    IIf(occ.Suppressed, "  (suppressed)", "")
        ' Suppressed occurrences have no Definition sub-occurrences loaded - skip them
        If Not occ.Suppressed Then
            If occ.DefinitionDocumentType = kAssemblyDocumentObject Then
                Call Walk(occ.SubOccurrences, level + 1)
            End If
        End If
    Next
End Sub

' Faster alternative when you only need the unique files: AllReferencedDocuments
Public Sub ListUniqueFiles()
    Dim d As Document
    For Each d In ThisApplication.ActiveDocument.AllReferencedDocuments
        Debug.Print d.FullFileName
    Next
End Sub
