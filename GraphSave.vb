﻿'Imports AxCWUIControlsLib
'Imports AxCW3DGraphLib
Imports System.Drawing
Imports System.Windows.Forms
Imports AxCW3DGraphLib

Public Class GraphSave
    Implements IDisposable

    Private myImage As Image
    Private bmp As Bitmap = Nothing
    Private myGraphics As Graphics
    Private strФайл As String

    'Private target As AxCWGraph
    Private target3D As AxCWGraph3D 'убралПоле89
    Private targetScatterGraph As NationalInstruments.UI.WindowsForms.ScatterGraph
    Private targetWaveformGraph As NationalInstruments.UI.WindowsForms.WaveformGraph

    Private SFDialog As SaveFileDialog

    'Public Sub New(ByVal saveTarget As AxCWGraph)
    '    If saveTarget Is Nothing Then
    '        Throw New ArgumentNullException("Ошибка Записи AxCWGraph")
    '    End If
    '    target = saveTarget
    '    SFDialog = New SaveFileDialog()
    '    bmp = New Bitmap(target.Width, target.Height)
    '    myGraphics = Graphics.FromImage(bmp)
    'End Sub

    'убралПоле89
    Public Sub New(ByVal saveTarget As AxCWGraph3D)
        If saveTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Записи AxCWGraph3D")
        End If

        target3D = saveTarget
        SFDialog = New SaveFileDialog()
        bmp = New Bitmap(target3D.Width, target3D.Height)
        myGraphics = Graphics.FromImage(bmp)
    End Sub

    Public Sub New(ByVal saveTarget As NationalInstruments.UI.WindowsForms.ScatterGraph)
        If saveTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Записи ScatterGraph")
        End If

        targetScatterGraph = saveTarget
        SFDialog = New SaveFileDialog()
        bmp = New Bitmap(targetScatterGraph.Width, targetScatterGraph.Height)
        myGraphics = Graphics.FromImage(bmp)
    End Sub

    Public Sub New(ByVal saveTarget As NationalInstruments.UI.WindowsForms.WaveformGraph)
        If saveTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Записи targetWaveformGraph")
        End If

        targetWaveformGraph = saveTarget
        SFDialog = New SaveFileDialog()
        bmp = New Bitmap(targetWaveformGraph.Width, targetWaveformGraph.Height)
        myGraphics = Graphics.FromImage(bmp)
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not myGraphics Is Nothing Then
                myGraphics.Dispose()
            End If

            If Not SFDialog Is Nothing Then
                SFDialog.Dispose()
            End If

            If Not bmp Is Nothing Then
                bmp.Dispose()
            End If
        End If
    End Sub

    Protected Overrides Sub Finalize()
        Try
            Dispose(False)
        Finally
            MyBase.Finalize()
        End Try
    End Sub

    Public Sub Save()
        With SFDialog
            .FileName = vbNullString
            .Title = "Сохранить график как рисунок"
            'установить флаг атрибутов
            .Filter = "Рисунок Png (*.png)|*.png|Рисунок BMP (*.bmp)|*.bmp|Формат Wmf  (*.wmf )|*.wmf"
            .FilterIndex = 2

            .RestoreDirectory = True
            If .ShowDialog() = DialogResult.Cancel Or Len(.FileName) = 0 Then
                Exit Sub
            End If

            strФайл = .FileName
        End With

        Try
            'If Not target Is Nothing Then
            '    myImage = target.ControlImage()
            'убралПоле89
            'Else
            If target3D IsNot Nothing Then
                'myImage = target3D.ControlImage()
                myImage = target3D.ControlImageEx(0, 0)
            ElseIf targetScatterGraph IsNot Nothing Then
                myImage = targetScatterGraph.ToImage
            ElseIf targetWaveformGraph IsNot Nothing Then
                myImage = targetWaveformGraph.ToImage
            End If

            myGraphics.DrawImage(myImage, 0, 0) ', 50, 50)

            If SFDialog.FilterIndex = 1 Then
                bmp.Save(strФайл, Imaging.ImageFormat.Png)
            ElseIf SFDialog.FilterIndex = 2 Then
                bmp.Save(strФайл, Imaging.ImageFormat.Bmp)
            ElseIf SFDialog.FilterIndex = 3 Then
                bmp.Save(strФайл, Imaging.ImageFormat.Wmf)
            End If
        Catch e As Exception
            MessageBox.Show(e.ToString, "Ошибка записи графика", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            myGraphics.Dispose()
        End Try
    End Sub

    'Private Sub OnPrintPage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
    '    e.Graphics.DrawImage(target.ControlImage(), 0, 0)
    'End Sub

    'Private Sub OnPrintPage3D(ByVal sender As Object, ByVal e As PrintPageEventArgs)
    '    e.Graphics.DrawImage(target3D.ControlImage(), 0, 0)
    'End Sub
End Class
