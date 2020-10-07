Imports System.IO
Imports System.Windows.Forms
Imports MathematicalLibrary
Imports MathematicalLibrary.Spline3

' экземпляры классов организуются в коллекцию в менеджере
' менеджер создается в Public Sub New() ClassCalculation
' в конструктор менеджера передается строковый массив с именами файлов (массив состоит из строковых констант имен) и путь к БД
' в цикле создаются экземпляры классов и добавляются в коллекцию
' метод ("имя класса",X,Y) as double который инкапсулирует нахождение в коллекции экземпляра класса и запрос у него (X,Y) as double

Enum GraphType
    Irregular 'Нерегулярный
    Proportional 'Пропорциональный
End Enum

Enum RegimeType
    Ksg_Kadiabatic
    Lambda
End Enum

Friend Class ManagerGraphKof
    Implements IEnumerable

    ' внутренняя коллекция для управления формами
    Private mCollectionGraph2D As Dictionary(Of String, Graph2D)
    Private Property PathCatalog As String

    Public Sub New(ByVal inPathCatalog As String)
        PathCatalog = inPathCatalog
        mCollectionGraph2D = New Dictionary(Of String, Graph2D)
    End Sub

    Public ReadOnly Property Count() As Integer
        Get
            Return mCollectionGraph2D.Count
        End Get
    End Property

    Public ReadOnly Property CollectionGraph2D() As Dictionary(Of String, Graph2D)
        Get
            Return mCollectionGraph2D
        End Get
    End Property

    Public ReadOnly Property Item(ByVal indexKey As String) As Graph2D
        Get
            Return mCollectionGraph2D.Item(indexKey)
        End Get
    End Property

    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return mCollectionGraph2D.GetEnumerator
    End Function

    Public Sub Remove(ByVal indexKey As String)
        ' удаление по номеру или имени или объекту?
        ' если целый тип то по плавающему индексу, а если строковый то по ключу
        mCollectionGraph2D.Remove(indexKey)
    End Sub

    Public Sub Clear()
        mCollectionGraph2D.Clear()
    End Sub

    Protected Overrides Sub Finalize()
        mCollectionGraph2D = Nothing
        MyBase.Finalize()
    End Sub

    Public Sub ДобавитьГрафики(ByVal arrГрафик() As String, ByVal типГрафика As GraphType, ByVal режим As RegimeType)
        For Each sName As String In arrГрафик
            Add(sName, PathCatalog, типГрафика, режим)
        Next
    End Sub

    Public Sub Add(ByVal name As String, ByVal inPathCatalog As String, ByVal типГрафика As GraphType, ByVal режим As RegimeType)
        If Not ПроверкаИмени(name) Then Exit Sub

        mCollectionGraph2D.Add(name, New Graph2D(name, inPathCatalog, типГрафика, CType(режим, GraphType)))
    End Sub

    Private Function ПроверкаИмени(ByVal name As String) As Boolean
        If mCollectionGraph2D.ContainsKey(name) Then
            MessageBox.Show("График с именем " & name & " уже существует!", "Ошибка добавления графика в коллекцию", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Return True
    End Function

    Public Function СчитатьИнициализировать() As Boolean
        Dim success As Boolean = False
        Dim fileName As String = Nothing

        Try
            For Each tempGraph2D As Graph2D In mCollectionGraph2D.Values
                fileName = tempGraph2D.FileName
                tempGraph2D.Open()
            Next

            success = True
        Catch e As FileNotFoundException
            MessageBox.Show("Файл {" & fileName & "} отсутствует", "Функция СчитатьИнициализировать", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch e As ColdCallFileFormatException
            ' Console.WriteLine("The file {0} appears to have been corrupted", fileName)
            ' Console.WriteLine("Details of problem are: {0}", e.Message)
            ' If e.InnerException IsNot Nothing Then
            '     Console.WriteLine("Inner exception was: {0}", e.InnerException.Message)
            ' End If
            MessageBox.Show("Файл {" & fileName & "} имеет неправильный формат" & vbLf & e.Message, "Функция СчитатьИнициализировать", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch e As Exception
            ' Console.WriteLine("Exception occurred:" & vbLf & e.Message)
            MessageBox.Show("Файл {" & fileName & "} вызвал исключение:" & vbLf & e.Message, "Функция СчитатьИнициализировать", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
        End Try
        Return success
    End Function

    Public ReadOnly Property КоэфПересчета(ByVal indexKey As String) As Double
        Get
            Return mCollectionGraph2D.Item(indexKey).КоэфПересчета
        End Get
    End Property

    ''' <summary>
    ''' В зависимости от ТипГрафика графика выбирается метод аппроксимации,
    ''' а в зависимости от режима выбираются те таблицы, которые соответствуют этому режиму.
    ''' </summary>
    ''' <param name="row"></param>
    ''' <param name="colunm"></param>
    ''' <param name="colunm2"></param>
    ''' <param name="Режим"></param>
    Public Sub ВычислитьВсеКоэфПересчета(ByVal colunm As Double, ByVal row As Double, ByVal colunm2 As Double, ByVal Режим As RegimeType)
        For Each tempGraph2D As Graph2D In mCollectionGraph2D.Values
            Select Case tempGraph2D.ТипГрафика
                Case GraphType.Irregular
                    If tempGraph2D.Режим = Режим Then
                        tempGraph2D.ВычислитьКоэфПересчета(colunm, row)
                    End If
                Case GraphType.Proportional
                    If tempGraph2D.Режим = Режим Then
                        tempGraph2D.ВычислитьКоэфПересчета(row, colunm2)
                    End If
            End Select
        Next
    End Sub

End Class

''' <summary>
''' Класс считывания из файла таблицы
''' </summary>
''' <remarks></remarks>
Friend Class Graph2D
    Implements IDisposable

    ' Класс с методом считывания из файла таблицы, в случае ошибки выдать сообщение
    ' свойства:
    Public Property Name As String ' имя класса
    Public Property ПутьКаталога As String '- путь с БД
    Public Property FileName As String
    Public Property ТипГрафика As GraphType
    Public Property Режим As GraphType
    Public Property КоэфПересчета As Double ' метод(параметр X,Y) для выдачи интерполированного числа 

    Private fs As FileStream
    Private sr As StreamReader

    Private isDisposed As Boolean = False
    Private isOpen As Boolean = False

    Private КоэфПерBicubic As Bicubic

    Public Sub New(ByVal Name As String, ByVal ПутьКаталога As String, ByVal ТипГрафика As GraphType, ByVal Режим As GraphType)
        Me.Name = Name
        Me.ПутьКаталога = ПутьКаталога
        Me.ТипГрафика = ТипГрафика
        FileName = Path.Combine(ПутьКаталога, Name & ".txt")
        Me.Режим = Режим
    End Sub

    ''' <summary>
    ''' метод расчета внутренних коэф(,) (массив координат X(), Y(), Z(,)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Open()
        ' при создании New считывается файл как массив строк, строки разбираются как массивы слов, далее в цикле через шаг 2 производится разбор в X(),Y()
        ' для быстрого поиска применяются индексы, когда ищетсяи выходит за рамки диапазона выдает первое или последние значения массива
        ' Bicubic подходит для апроксимации влажности
        ' для К от температур нужно сделать равномерный квадрат по X - обороты, по Y -  температуры (они равномерны)
        ' считываются все массивы длиной 21 arrX(20) и arrY(20) по оборотам 
        '     Public Sub New(ByVal InputArray As Double(,), ByVal XIstart As Double, ByVal XIfinish As Double, ByVal YJstart As Double, ByVal YJfinish As Double)
        ' разобъём диапазон на N1End - N1Start  и создадим arrXNew(N1End - N1Start) и вычислим для каждого arrXNew(N1End - N1Start) новые значения в массиве ZNew(50) Spline3Interpolate   (для этого нужно предварительно вычислить таблицу методом Spline3BuildTable)
        ' тогда соединим все в массив ZNew(,)(массив коэффициентов) - где 20 это шкала температур Y, N1End - N1Star - ось оборотов X
        ' тогда можно вычислять так N1пересчитанныйКоэффициент = New Bicubic(ZNew(,), XIstart, XIfinish, YJstart, YJfinish)
        Dim delimiterTab As Char() = vbTab.ToCharArray
        Dim words() As String
        Dim I, J, L As Integer
        Dim strList As New List(Of String)
        Dim arrList() As String
        Dim line As String = Nothing

        Dim axisY() As Double
        Dim axisХ() As Double
        Dim Z(,) As Double

        If isDisposed Then
            Throw New ObjectDisposedException("Graph2D_" & _Name)
        End If

        If Not File.Exists(FileName) Then
            Throw New FileNotFoundException("Отсутствует файл " & FileName)
        End If

        fs = New FileStream(FileName, FileMode.Open)
        sr = New StreamReader(fs)

        Try
            Dim числоСтрок As Integer = Integer.Parse(sr.ReadLine())
            Dim числоСтолбцов As Integer = Integer.Parse(sr.ReadLine())
            ' считать 3 строку
            Dim firstLine As String = sr.ReadLine()
            ' определить число шлейфов
            words = firstLine.Split(delimiterTab)

            ' предпоследняя цифра содержит число столбцов
            If words.Length / 2 <> числоСтолбцов OrElse Integer.Parse(words(words.Length - 2)) <> числоСтолбцов Then
                Throw New ColdCallFileFormatException("Неправильное число столбцов в файле")
            End If

            J = 0
            ' определить для какого диапазона шлейф
            'ReDim_axisY(числоСтолбцов - 1)
            Re.Dim(axisY, числоСтолбцов - 1)
            For I = 1 To words.Length - 1 Step 2
                axisY(J) = Double.Parse(words(I))
                J += 1
            Next

            ' остальные строки в цикле
            ' здесь считывание и проверка числа столбцов из первой строки
            Do While sr.Peek() >= 0
                line = sr.ReadLine()
                strList.Add(line)
            Loop
            sr.Close()
            fs.Close()
            fs = Nothing

            If strList.Count <> числоСтрок Then
                Throw New ColdCallFileFormatException("Неправильное число строк в файле")
            End If
            isOpen = True ' присвоить только после проверки

            'ReDim_Z(числоСтрок - 1, числоСтолбцов - 1)
            Re.Dim(Z, числоСтрок - 1, числоСтолбцов - 1)

            Select Case ТипГрафика
                Case GraphType.Irregular
                    Dim XMinValue As Double = Double.MaxValue
                    Dim XMaxValue As Double = Double.MinValue
                    Dim arrОсьХ(числоСтрок - 1, числоСтолбцов - 1) As Double
                    Dim inputX(числоСтрок - 1), inputY(числоСтрок - 1) As Double ' для сплайн интерполяции
                    Dim inputX2(числоСтрок), inputY2(числоСтрок) As Double ' для линенйной интерполяции
                    Dim tblBuildTable(,) As Double = Nothing

                    'arrList = strList.AsEnumerable().Reverse().ToArray() ' в случае необходимости реверса
                    arrList = strList.AsEnumerable().ToArray()

                    For I = 0 To числоСтрок - 1
                        words = arrList(I).Split(delimiterTab)
                        If words.Length / 2 <> числоСтолбцов Then
                            Throw New ColdCallFileFormatException("Неправильное число столбцов в файле")
                        End If

                        For J = 0 To words.Length \ 2 - 1 ' Step 2
                            arrОсьХ(I, J) = Double.Parse(words(J * 2))
                            Z(I, J) = Double.Parse(words(J * 2 + 1))

                            If XMinValue > arrОсьХ(I, J) Then XMinValue = arrОсьХ(I, J)
                            If XMaxValue < arrОсьХ(I, J) Then XMaxValue = arrОсьХ(I, J)
                        Next
                    Next
                    ' внимание, если мин и макс значения лежат в диапазоне меньше 1, то необходимо промасштабировать 
                    ' целочисленные значения XStart и XEnd, а переменну index цикла при аппроксимировниии поделить на величину масштаба
                    Dim XStart As Integer = Convert.ToInt32(XMinValue) ' - 1
                    Dim XEnd As Integer = Convert.ToInt32(XMaxValue)

                    'ReDim_axisХ(XEnd - XStart)
                    Re.Dim(axisХ, XEnd - XStart)
                    Dim ZNew(XEnd - XStart, числоСтолбцов - 1) As Double

                    For J = 0 To числоСтолбцов - 1
                        For I = 0 To числоСтрок - 1
                            inputX(I) = arrОсьХ(I, J)
                            inputY(I) = Z(I, J)
                            inputX2(I + 1) = inputX(I)
                            inputY2(I + 1) = inputY(I)
                        Next

                        Spline3BuildTable(UBound(inputX) + 1, 2, inputX, inputY, 0, 0, tblBuildTable)
                        L = 0

                        For index As Integer = XStart To XEnd
                            If index < inputX(0) OrElse index > inputX(числоСтрок - 1) Then
                                ' если вне диапазона то линейная интерполяция
                                ZNew(L, J) = InterpLine(inputX2, inputY2, index)
                            Else ' сплайн интерполяции
                                ZNew(L, J) = Spline3Interpolate(UBound(tblBuildTable, 2) + 1, tblBuildTable, index)
                            End If
                            L += 1
                        Next
                    Next

                    L = 0
                    For index As Integer = XStart To XEnd
                        axisХ(L) = index
                        L += 1
                    Next

                    'КоэфПерBicubic = New Bicubic(ZNew, axisХ(0), axisХ(axisХ.Length - 1), axisY(0), axisY(числоСтолбцов - 1), 10)
                    КоэфПерBicubic = New Bicubic(ZNew, axisХ(0), axisХ(axisХ.Length - 1), axisY(0), axisY(числоСтолбцов - 1), (XEnd - XStart + 1) * 10, числоСтолбцов * 100)
                Case GraphType.Proportional
                    'ReDim_axisХ(числоСтрок - 1)
                    Re.Dim(axisХ, числоСтрок - 1)
                    arrList = strList.ToArray()

                    For I = 0 To числоСтрок - 1
                        words = arrList(I).Split(delimiterTab)
                        If words.Length / 2 <> числоСтолбцов Then
                            Throw New ColdCallFileFormatException("Неправильное число столбцов в файле")
                        End If

                        axisХ(I) = Double.Parse(words(0))
                        For J = 0 To CInt(words.Length / 2) - 1 ' Step 2
                            Z(I, J) = Double.Parse(words(J * 2 + 1))
                        Next
                    Next

                    КоэфПерBicubic = New Bicubic(Z, axisХ(0), axisХ(числоСтрок - 1), axisY(0), axisY(числоСтолбцов - 1), 100)
            End Select
        Catch e As FormatException
            Throw New ColdCallFileFormatException("Неправильное число строк в файле", e)
        End Try
    End Sub

    Public Sub ВычислитьКоэфПересчета(ByVal X As Double, ByVal Y As Double)
        КоэфПересчета = КоэфПерBicubic.Calculate(X, Y)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If isDisposed Then
            Return
        End If

        isDisposed = True
        isOpen = False
        If fs IsNot Nothing Then
            sr.Close()
            fs.Close()
            fs = Nothing
        End If
    End Sub
End Class

Class ColdCallFileFormatException
    Inherits ApplicationException
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(ByVal message As String, ByVal innerException As Exception)
        MyBase.New(message, innerException)
    End Sub
End Class



