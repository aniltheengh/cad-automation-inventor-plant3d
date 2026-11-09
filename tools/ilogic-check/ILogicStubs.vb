' Hand-written STUBS of the iLogic rule environment - enough surface to compile-check rules.
' Signatures follow the iLogic API reference (Autodesk.iLogic.Interfaces) as documented in
' Inventor help "iLogic API" - only members used in this repo are declared.
Imports System.Collections

Public Class ParamDyn
    Default Public Property Item(name As String) As Object
        Get
            Return 0.0
        End Get
        Set(value As Object)
        End Set
    End Property
    Default Public Property Item(componentName As String, name As String) As Object
        Get
            Return 0.0
        End Get
        Set(value As Object)
        End Set
    End Property
    Public Function Param(name As String) As Inventor.Parameter
        Return Nothing
    End Function
    Public Property Quiet As Boolean
End Class

Public Class IPropsDyn
    Public Property Value(setName As String, propName As String) As Object
        Get
            Return ""
        End Get
        Set(v As Object)
        End Set
    End Property
    Public Property Material As String
    Public ReadOnly Property Mass As Double
    Public ReadOnly Property Volume As Double
End Class

Public Class ThisDocDyn
    Public ReadOnly Property Document As Inventor.Document
    Public ReadOnly Property Path As String
    Public ReadOnly Property WorkspacePath As String
    Public Function FileName(includeExtension As Boolean) As String
        Return ""
    End Function
    Public Function PathAndFileName(includeExtension As Boolean) As String
        Return ""
    End Function
    Public Function ChangeExtension(newExt As String) As String
        Return ""
    End Function
    Public Sub Launch(file As String)
    End Sub
End Class

Public Class ILogicVbDyn
    Public Property UpdateWhenDone As Boolean
    Public Function RunRule(ruleName As String) As Integer
        Return 0
    End Function
    Public Function RunExternalRule(ruleName As String) As Integer
        Return 0
    End Function
    Public Sub DocumentUpdate()
    End Sub
End Class

Public Class InventorVbDyn
    Public Sub DocumentUpdate(Optional updateAll As Boolean = True)
    End Sub
End Class

Public Class MultiValueDyn
    Public Sub SetList(paramName As String, ParamArray values() As Object)
    End Sub
    Public Property List(paramName As String) As ArrayList
        Get
            Return New ArrayList()
        End Get
        Set(v As ArrayList)
        End Set
    End Property
End Class

Public Class GoExcelDyn
    Public Function Open(file As String, Optional sheet As String = "Sheet1") As Boolean
        Return True
    End Function
    Public Property CellValue(cell As String) As Object
        Get
            Return Nothing
        End Get
        Set(v As Object)
        End Set
    End Property
    Public Function CellValues(file As String, sheet As String, first As String, last As String) As ArrayList
        Return New ArrayList()
    End Function
    Public Function FindRow(file As String, sheet As String, ParamArray args() As Object) As Integer
        Return 0
    End Function
    Public Function CurrentRowValue(column As String) As Object
        Return Nothing
    End Function
    Public Sub Close()
    End Sub
    Public Sub Save()
    End Sub
End Class

Public Class FeatureDyn
    Public Property IsActive(featureName As String) As Boolean
        Get
            Return True
        End Get
        Set(v As Boolean)
        End Set
    End Property
End Class

Public Class ComponentDyn
    Public Property IsActive(occName As String) As Boolean
        Get
            Return True
        End Get
        Set(v As Boolean)
        End Set
    End Property
End Class

Public Class LoggerDyn
    Public Sub Info(msg As String) : End Sub
    Public Sub Warn(msg As String) : End Sub
    Public Sub [Error](msg As String) : End Sub
    Public Sub Debug(msg As String) : End Sub
End Class

Public Class ILogicFormDyn
    Public Function Show(formName As String) As Object
        Return Nothing
    End Function
    Public Function ShowGlobal(formName As String) As Object
        Return Nothing
    End Function
End Class

Public MustInherit Class RuleBase
    Protected ThisApplication As Inventor.Application
    Protected Parameter As New ParamDyn()
    Protected iProperties As New IPropsDyn()
    Protected ThisDoc As New ThisDocDyn()
    Protected iLogicVb As New ILogicVbDyn()
    Protected InventorVb As New InventorVbDyn()
    Protected MultiValue As New MultiValueDyn()
    Protected GoExcel As New GoExcelDyn()
    Protected Feature As New FeatureDyn()
    Protected Component As New ComponentDyn()
    Protected Logger As New LoggerDyn()
    Protected iLogicForm As New ILogicFormDyn()
    Protected SharedVariable As New Hashtable()
End Class
