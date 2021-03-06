﻿'Imports AxCWUIControlsLib
Imports System.Drawing.Printing
Imports System.Windows.Forms
Imports AxCW3DGraphLib

Public Class GraphPrinter
    Implements IDisposable

    'Private target As AxCWGraph
    Private target3D As AxCWGraph3D 'убралПоле89
    Private document As PrintDocument
    Private dialog As PrintDialog
    Private dlg As New PrintPreviewDialog()
    Private psDlg As New PageSetupDialog()
    Private storedPageSettings As PageSettings
    Private targetScatterGraph As NationalInstruments.UI.WindowsForms.ScatterGraph
    Private targetWaveformGraph As NationalInstruments.UI.WindowsForms.WaveformGraph

    'Public Sub New(ByVal printTarget As AxCWGraph)
    '    If printTarget Is Nothing Then
    '        Throw New ArgumentNullException("Ошибка Печати AxCWGraph")
    '    End If
    '    target = printTarget
    '    document = New PrintDocument()
    '    dialog = New PrintDialog()
    '    AddHandler document.PrintPage, AddressOf OnPrintPage
    '    dialog.Document = document
    'End Sub

    'убралПоле89
    Public Sub New(ByVal printTarget As AxCWGraph3D)
        If printTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Печати AxCWGraph3D")
        End If

        target3D = printTarget
        document = New PrintDocument()
        dialog = New PrintDialog()
        AddHandler document.PrintPage, AddressOf OnPrintPage3D
        dialog.Document = document
    End Sub

    Public Sub New(ByVal printTarget As NationalInstruments.UI.WindowsForms.ScatterGraph)
        If printTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Печати targetScatterGraph")
        End If

        targetScatterGraph = printTarget
        document = New PrintDocument()
        dialog = New PrintDialog()
        AddHandler document.PrintPage, AddressOf OnPrintPageScatterGraph
        dialog.Document = document
    End Sub

    Public Sub New(ByVal printTarget As NationalInstruments.UI.WindowsForms.WaveformGraph)
        If printTarget Is Nothing Then
            Throw New ArgumentNullException("Ошибка Печати targetWaveformGraph")
        End If

        targetWaveformGraph = printTarget
        document = New PrintDocument()
        dialog = New PrintDialog()
        AddHandler document.PrintPage, AddressOf OnPrintPageWaveformGraph
        dialog.Document = document
    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not document Is Nothing Then
                document.Dispose()
            End If
            If Not dialog Is Nothing Then
                dialog.Dispose()
            End If
            If Not dlg Is Nothing Then
                dlg.Dispose()
            End If
            If Not psDlg Is Nothing Then
                psDlg.Dispose()
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

    'Private Sub OnPrintPage(ByVal sender As Object, ByVal e As PrintPageEventArgs)
    '    e.Graphics.DrawImage(target.ControlImage(), 0, 0)
    'End Sub
    'убралПоле89
    Private Sub OnPrintPage3D(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        'e.Graphics.DrawImage(target3D.ControlImage(), 0, 0)
        e.Graphics.DrawImage(target3D.ControlImageEx(0, 0), 0, 0)
    End Sub

    Private Sub OnPrintPageScatterGraph(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        e.Graphics.DrawImage(targetScatterGraph.ToImage(), 0, 0)
    End Sub

    Private Sub OnPrintPageWaveformGraph(ByVal sender As Object, ByVal e As PrintPageEventArgs)
        e.Graphics.DrawImage(targetWaveformGraph.ToImage(), 0, 0)
    End Sub

    Private Sub PageSetup()
        Try
            If (storedPageSettings Is Nothing) Then
                storedPageSettings = New PageSettings()
            End If

            psDlg.PageSettings = storedPageSettings
            psDlg.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("Произошла ошибка - " + ex.Message, "Печать графика", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub PrintPreview()
        Try
            If Not (storedPageSettings Is Nothing) Then
                document.DefaultPageSettings = storedPageSettings
            End If

            dlg.Document = document
            dlg.ShowDialog()
        Catch ex As Exception
            MessageBox.Show("Произошла ошибка - " + ex.Message, "Печать графика", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Public Sub Print()
        PageSetup()
        PrintPreview()

        Try
            If Not (storedPageSettings Is Nothing) Then
                document.DefaultPageSettings = storedPageSettings
            End If

            dialog.Document = document
            If dialog.ShowDialog() = DialogResult.OK Then
                document.PrinterSettings.PrinterName = dialog.PrinterSettings.PrinterName

                If document.PrinterSettings.IsValid Then
                    document.Print()
                Else
                    MessageBox.Show("Принтер не установлен.", "Печать", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Произошла ошибка при печати в файл - " + ex.Message, "Печать графика", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
