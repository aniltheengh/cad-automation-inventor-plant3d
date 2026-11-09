' InventorConnector.vb - VB.NET version of the .NET 8 replacement for Marshal.GetActiveObject.
Imports System.Runtime.InteropServices

Public Module InventorConnector
    <DllImport("ole32.dll")>
    Private Function CLSIDFromProgIDEx(<MarshalAs(UnmanagedType.LPWStr)> progId As String, ByRef clsid As Guid) As Integer
    End Function

    <DllImport("oleaut32.dll")>
    Private Function GetActiveObject(<MarshalAs(UnmanagedType.LPStruct)> rclsid As Guid, reserved As IntPtr,
                                     <MarshalAs(UnmanagedType.IUnknown)> ByRef ppunk As Object) As Integer
    End Function

    Public Function GetRunning(progId As String) As Object
        Dim clsid As Guid
        If CLSIDFromProgIDEx(progId, clsid) < 0 Then Return Nothing
        Dim obj As Object = Nothing
        If GetActiveObject(clsid, IntPtr.Zero, obj) < 0 Then Return Nothing
        Return obj
    End Function

    Public Function Connect(Optional startIfNotRunning As Boolean = True) As Inventor.Application
        Dim app = TryCast(GetRunning("Inventor.Application"), Inventor.Application)
        If app IsNot Nothing Then Return app
        If Not startIfNotRunning Then Throw New InvalidOperationException("Inventor is not running.")
        Dim t = Type.GetTypeFromProgID("Inventor.Application")
        If t Is Nothing Then Throw New InvalidOperationException("Inventor is not installed.")
        app = CType(Activator.CreateInstance(t), Inventor.Application)
        app.Visible = True
        Return app
    End Function
End Module
