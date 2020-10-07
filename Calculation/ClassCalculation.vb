Imports BaseForm
Imports MathematicalLibrary.Integral
Imports MathematicalLibrary.Air

' в цикле сбора когда видимость=True запускается подсчет, он обновляет массив расчетных параметров, которые будут отображаться 
' до следующей видимость=True
' если видимость=True с частотой 10 герц то при времени вращения турели 10 минут или 600 сек на каждый градус придется 600/360=1.67 секунды
' или 1,67*10герц=16.7 замера
' значит в промежутке между отсечками по градусу надо накопить около 16 замеров в переменной .НакопленноеЗначение и вести счетчик для получения осреднения
' датчик положения на каждой отсечке выдаст сигнал для процедуры осреднения в .НакопленноеЗначение и накопления в классе ПараметрыПоляНакопленные
' который содержит значения всех входных и расчетных параметров через 1 градус для последующего анализа 
' через горелки, контрольные точки (5 штук ? надо сделать 6 кратное 60 градусам, где 1 на 60 а 6 на последнем 360 градусе)
' т.е. потом можно делать все что хочешь намного проще

' для COM видимости
'<System.Runtime.InteropServices.ProgId("ClassDiagram_NET.ClassDiagram")> Public Class ClassCalculation
'    Implements BaseForm.IClassCalculation
Public Class ClassCalculation
    Implements IClassCalculation

    Public Property Manager() As ProjectManager Implements IClassCalculation.Manager
        Get
            Return mProjectManager
        End Get
        Set(ByVal value As ProjectManager)
            mProjectManager = value
        End Set
    End Property

    ''' <summary>
    ''' Входные аргументы
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property InputParam() As InputParameters

    ''' <summary>
    ''' Настроечные параметры
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TuningParam() As TuningParameters

    ''' <summary>
    '''  Расчетные параметры
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CalculatedParam() As CalculatedParameters

    'Delegate Sub DataErrorventHandler(ByVal sender As Object, ByVal e As DataErrorEventArgs)
    'Public Event DataError(ByVal sender As Object, ByVal e As BaseForm.IClassCalculation.DataErrorEventArgs) Implements BaseForm.IClassCalculation.DataError
    'Public Event DataError(ByVal sender As BaseForm.IClassCalculation, ByVal e As BaseForm.DataErrorEventArgs) Implements BaseForm.DataError
    ''' <summary>
    ''' событие для выдачи ошибки в вызывающую программу
    ''' </summary>
    Public Event DataError As EventHandler(Of DataErrorEventArgs)

    Private mProjectManager As ProjectManager
    ' константы переменных
    Private Const КЕЛЬВИН As Double = 273.15 ' абс. ноль
    Private Const constG As Double = 9.80665 ' ускорение силы тяжести м/сек.кв.
    Private Const const735_6 As Double = 735.56 ' для барометра
    Private относительныйБарометр As Double ' барометр база
    Private Const СтехиометрическийК As Double = 14.94
    Private Const Rg As Double = 29.27 '29.29 ' универсальная газовая постоянная кГм/кг*К
    Private Const ДавлениеВоздухаНУ As Double = 0.101325 ' 1.0332 - давление воздуха в нормальных условиях МПа
    Private Const ПлотностьВоздухаНУ As Double = 1.20445 ' 1.205 - плотность воздуха в нормальных условиях

    '--- конфигурация стенда -------------------------------------------------
    Private Const Gрасход_Отбора_Газа_11 As Double = 11.0 ' Процент от расхода газа на выходе из КП
    Private D20отвОсн As Double 'диаметр сужающивающего устройства основной
    Private D20трубОсн As Double 'диаметр трубопровода основной
    'Private Ks As Double 'коф. сжимаемости измеряемой среды
    'Private КоэфЛинейногоТепловогоРасширенияМерногоСопла As Double '= 0.000014 '0.0000105 ' 0.0000105-коэф. линейного теплового расширения мерного сопла
    Private A0tr As Double = 16.206 'a0tr коэф. линейного расширения трубовпровода
    Private A1tr As Double = 6.571 'a1tr коэф. линейного расширения трубовпровода
    Private A2tr As Double = 0.0 'a2tr коэф. линейного расширения трубовпровода
    Private A0su As Double = 15.6 'a0su коэф. линейного расширения сужающего устр.
    Private A1su As Double = 8.3 'a1su коэф. линейного расширения сужающего устр.
    Private A2su As Double = -6.5 'a2su коэф. линейного расширения сужающего устр.
    Private Tkr As Double = 132.5 'критическая температура
    Private Pkr As Double = 316.5 'критическая плотность
    Private B1 As Double = 0.6 'ближайшее большее значение отн. диаметра СУ
    Private B2 As Double = 0.5 'ближайшее меньшее значение отн. диаметра СУ
    Private Ra1 As Double = 1.4 'ближайшее меньшее зн. пром. величины 100000*Ra/Dи
    Private Ra2 As Double = 1.8 'ближайшее большее зн. пром. величины 100000*Ra/Dи

    Public Sub New(ByVal manager As ProjectManager)
        MyBase.New()

        Me.Manager = manager

        InputParam = New InputParameters
        TuningParam = New TuningParameters
        CalculatedParam = New CalculatedParameters

        ' для того чтоба вначале всех таблиц и отчетов шли температуры по сечению
        ' вначале занесем имена ПроэкцияНаСтенку1, горелки, ПроэкцияНаСтенку2
        Dim имяПояса As String
        Dim listNameRows As New List(Of String)

        If ПроверитьНаличиеЗаписиРасчетныйПараметр(ПРОЭКЦИЯ_НА_СТЕНКУ1) Then
            'ListNameColumns.Add(Ordinal)
            listNameRows.Add(ПРОЭКЦИЯ_НА_СТЕНКУ1)
        End If

        For I = 1 To ЧИСЛО_ТЕРМОПАР
            имяПояса = "T340_" & I.ToString

            For Each rowИзмеренныйПараметр As BaseFormDataSet.ИзмеренныеПараметрыRow In manager.MeasurementDataTable.Rows
                If rowИзмеренныйПараметр.ИмяПараметра = имяПояса Then
                    listNameRows.Add(имяПояса)
                    Exit For
                End If
            Next
        Next

        If ПроверитьНаличиеЗаписиРасчетныйПараметр(ПРОЭКЦИЯ_НА_СТЕНКУ2) Then
            listNameRows.Add(ПРОЭКЦИЯ_НА_СТЕНКУ2)
        End If

        If ПроверитьНаличиеЗаписиРасчетныйПараметр(CalculatedParameters.conКАЧЕСТВО) Then
            listNameRows.Add(CalculatedParameters.conКАЧЕСТВО)
        End If

        ' вначале заполним имена из ListNameColumns
        For Each nameRow As String In listNameRows
            ПараметрыПоляНакопленные.Add(nameRow, ModelMeasurement.ИзмерениеВезде)
        Next

        ' для каждого измеренного и расчетного параметра сделать свою текстовую константу для произвольного доступа к значению параметра
        ' и цифровую переменную для записи в нее значения
        For Each rowИзмеренныйПараметр As BaseFormDataSet.ИзмеренныеПараметрыRow In manager.MeasurementDataTable.Rows
            ' затем добавлем только те имена которых ещё нет
            If Not listNameRows.Contains(rowИзмеренныйПараметр.ИмяПараметра) Then
                ПараметрыПоляНакопленные.Add(rowИзмеренныйПараметр.ИмяПараметра, ModelMeasurement.ИзмерениеВезде)
            End If
        Next

        For Each rowРасчетныйПараметр As BaseFormDataSet.РасчетныеПараметрыRow In manager.CalculatedDataTable.Rows
            If Not listNameRows.Contains(rowРасчетныйПараметр.ИмяПараметра) Then
                ПараметрыПоляНакопленные.Add(rowРасчетныйПараметр.ИмяПараметра, ModelMeasurement.ИзмерениеВезде)
            End If
        Next

        ПодготовитьТаблицы()
        ' далее добавить параметры которых нет в расчетных, но они есть вспомогательные
        ' пока не надо
        ' ПараметрыПоляНакопленные.Add(conТсредняя_газа_на_входе, enuТипИзмерения.ИзмерениеВезде)
    End Sub

    ''' <summary>
    ''' Последовательное прохождение по этапам приведениия и вычисления.
    ''' Здесь индивидуальные настройки для класса.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Calculate() Implements IClassCalculation.Calculate
        If gГеометрияВведена = False Then
            gMainFomMdiParent.varТемпературныеПоля.StatusBar.Items(conStatusLabelMessage).Text = "Для расчета необходимо ввести геометрию!"
            Exit Sub
        End If

        ' Для Приведенных и Пересчитанных параметров входные единицы измерения
        ' только в единицах СИ, выходные единицы измерения - любого типа
        'gMainFomMdiParent.varТемпературныеПоля.TextError.Visible = False

        Try
            ' здесь пока не надо получать от контролов
            If mProjectManager.NeedToRewrite Then ПолучитьЗначенияНастроечныхПараметров()
            ' Переводим в Си только измеренные пареметры
            mProjectManager.СonversionToSiUnitMeasurementParameters()
            'получение абсолютных давлений
            mProjectManager.CalculationBasePressure()
            ' весь подсчет производится исключительно в единицах СИ
            ' извлекаем значения измеренных параметров
            ИзвлечьЗначенияИзмеренныхПараметров()
            ВычислитьРасчетныеПараметры()
            mProjectManager.СonversionToTuningUnitCalculationParameters()

            If gНакопитьДляПоля Then НакопитьЗначенияИзмеренныхИРасчетныхПараметров()

            'Dim result As String = Await AsynchronouslyAsync()
            'Await CalcAsynchronouslyAsync()

            gMainFomMdiParent.varТемпературныеПоля.ОновитьИндикаторы()
            ' там же заполняется массив y()
            If gРисоватьГрафикСечений Then gMainFomMdiParent.varТемпературныеПоля.РисоватьПолеПоСечению()
        Catch ex As Exception
            ' ошибка проглатывается
            'Description = "Процедура: Подсчет"
            ''перенаправление встроенной ошибки
            'Dim fireDataErrorEventArgs As New IClassCalculation.DataErrorEventArgs(ex.Message, Description)
            ''  Теперь вызов события с помощью вызова делегата. Проходя в
            ''   object которое инициирует  событие (Me) такое же как FireEventArgs. 
            ''  Вызов обязан соответствовать сигнатуре FireEventHandler.
            'RaiseEvent DataError(Me, fireDataErrorEventArgs)
        End Try
    End Sub

    '''' <summary>
    '''' Подсчёты не связанные с графическим интерфейсом.
    '''' Графический интерфейс не блокируется.
    '''' </summary>
    '''' <returns></returns>
    'Public Async Function CalcAsynchronouslyAsync() As Task 'Task(Of String) '
    '    'Await Task.Delay(10000)
    '    'Return "Finished"
    '    Dim t As Task = Task.Factory.StartNew(Sub()
    '                                              ' здесь пока не надо получать от контролов
    '                                              If mProjectManager.NeedToRewrite Then ПолучитьЗначенияНастроечныхПараметров()
    '                                              ' Переводим в Си только измеренные пареметры
    '                                              mProjectManager.ПереводВЕдиницыСИИзмеренныеПараметры()
    '                                              'получение абсолютных давлений
    '                                              mProjectManager.УчетБазовыхВеличин()
    '                                              ' весь подсчет производится исключительно в единицах СИ
    '                                              ' извлекаем значения измеренных параметров
    '                                              ИзвлечьЗначенияИзмеренныхПараметров()
    '                                              ВычислитьРасчетныеПараметры()
    '                                              mProjectManager.ПереводВНастоечныеЕдиницыРасчетныхПараметров()

    '                                              If gНакопитьДляПоля Then НакопитьЗначенияИзмеренныхИРасчетныхПараметров()
    '                                          End Sub)
    '    t.Wait()

    '    Await t
    'End Function

    Dim description As String = $"Процедура: <{NameOf(ПолучитьЗначенияНастроечныхПараметров)}>"
    ''' <summary>
    ''' Получить значения параметров, используемых как настраиваемые глобальные переменные.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ПолучитьЗначенияНастроечныхПараметров()
        If Manager.TuningDataTable Is Nothing Then Exit Sub

        Dim success As Boolean = False

        ' Вначале проверяется наличие расчетных параметров в базе
        For Each имяНастроечногоПараметра As String In TuningParam.TuningDictionary.Keys.ToArray 'arrНастроечныеПараметры
            success = False

            For Each rowНастроечныйПараметр As BaseFormDataSet.НастроечныеПараметрыRow In Manager.TuningDataTable.Rows
                If rowНастроечныйПараметр.ИмяПараметра = имяНастроечногоПараметра Then
                    success = True
                    Exit For
                End If
            Next

            If success = False Then
                ' перенаправление встроенной ошибки
                RaiseEvent DataError(Me, New DataErrorEventArgs($"Настроечный параметр {имяНастроечногоПараметра} в базе параметров не найден!", description)) 'не ловит в конструкторе
                'MessageBox.Show(Message, Description, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                Exit Sub
            End If
        Next

        ' проверяется наличие в расчетном модуле переменных, соответствующих расчетным настроечным
        ' и присвоение им значений
        success = True
        Try
            For Each rowНастроечныйПараметр As BaseFormDataSet.НастроечныеПараметрыRow In Manager.TuningDataTable.Rows
                'If arrНастроечныеПараметры.Contains(rowНастроечныйПараметр.ИмяПараметра) Then
                If TuningParam.TuningDictionary.Keys.Contains(rowНастроечныйПараметр.ИмяПараметра) Then

                    Select Case rowНастроечныйПараметр.ИмяПараметра
                        'Case "GвМПитоПриводить"
                        '    'GвМПитоПриводить = rowНастроечныйПараметр.ЦифровоеЗначение
                        '    'n1ГПриводить = CInt(rowНастроечныйПараметр.ЛогическоеЗначение)
                        '    GвМПитоПриводить = rowНастроечныйПараметр.ЛогическоеЗначение
                        '    Exit Select
                        'Case "GвМПолеДавленийПриводить"
                        '    GвМПолеДавленийПриводить = rowНастроечныйПараметр.ЛогическоеЗначение
                        '    Exit Select
                        'Case "n1ГПриводить"
                        '    n1ГПриводить = rowНастроечныйПараметр.ЛогическоеЗначение
                        '    Exit Select
                        'Case "nИГ-03ГПриводить"
                        '    nИГ_03ГПриводить = rowНастроечныйПараметр.ЛогическоеЗначение
                    End Select
                Else
                    success = False
                    'перенаправление встроенной ошибки
                    RaiseEvent DataError(Me, New DataErrorEventArgs($"Настроечный параметр {rowНастроечныйПараметр.ИмяПараметра} не имеет соответствующей переменной в модуле расчета!", description)) ' не ловит в конструкторе
                    'MessageBox.Show(Message, Description, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
            Next

            With gMainFomMdiParent.Manager.TuningDataTable
                D20трубОсн = .FindByИмяПараметра(TuningParameters.conD20трубОсн).ЦифровоеЗначение
                D20отвОсн = .FindByИмяПараметра(TuningParameters.conD20отвОсн).ЦифровоеЗначение
                A0tr = .FindByИмяПараметра(TuningParameters.conA0tr).ЦифровоеЗначение
                A1tr = .FindByИмяПараметра(TuningParameters.conA1tr).ЦифровоеЗначение
                A2tr = .FindByИмяПараметра(TuningParameters.conA2tr).ЦифровоеЗначение
                A0su = .FindByИмяПараметра(TuningParameters.conA0su).ЦифровоеЗначение
                A1su = .FindByИмяПараметра(TuningParameters.conA1su).ЦифровоеЗначение
                A2su = .FindByИмяПараметра(TuningParameters.conA2su).ЦифровоеЗначение
                Tkr = .FindByИмяПараметра(TuningParameters.conTkr).ЦифровоеЗначение
                Pkr = .FindByИмяПараметра(TuningParameters.conPkr).ЦифровоеЗначение
                B1 = .FindByИмяПараметра(TuningParameters.conB1).ЦифровоеЗначение
                B2 = .FindByИмяПараметра(TuningParameters.conB2).ЦифровоеЗначение
                Ra1 = .FindByИмяПараметра(TuningParameters.conRa1).ЦифровоеЗначение
                Ra2 = .FindByИмяПараметра(TuningParameters.conRa2).ЦифровоеЗначение
            End With

            If success = False Then Exit Sub

            ' занести значения настроечных параметров
            With Manager.TuningDataTable
                For Each keysTuning As String In TuningParam.TuningDictionary.Keys.ToArray
                    If .FindByИмяПараметра(keysTuning).ЛогикаИлиЧисло Then
                        TuningParam.TuningDictionary(keysTuning).ЛогикаИлиЧисло = True
                        TuningParam.TuningDictionary(keysTuning).ЛогическоеЗначение = .FindByИмяПараметра(keysTuning).ЛогическоеЗначение
                    Else
                        TuningParam.TuningDictionary(keysTuning).ЛогикаИлиЧисло = False
                        TuningParam.TuningDictionary(keysTuning).ЦифровоеЗначение = .FindByИмяПараметра(keysTuning).ЦифровоеЗначение
                    End If
                Next
            End With

        Catch ex As Exception
            ' перенаправление встроенной ошибки
            RaiseEvent DataError(Me, New DataErrorEventArgs(ex.Message, description)) 'не ловит в конструкторе
            'MessageBox.Show(fireDataErrorEventArgs, Description, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try
    End Sub

    ''' <summary>
    ''' Поиск всех параметров по пользовательскому запросу в DataSet.ИзмеренныеПараметры
    ''' (с одним входным параметром являющимся именем связи для реального измеряемого канала Сервера).
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ИзвлечьЗначенияИзмеренныхПараметров()
        'Dim rowИзмеренныйПараметр As BaseFormDataSet.ИзмеренныеПараметрыRow
        Try
            With Manager.MeasurementDataTable
                ' вместо последовательного извлечения применяется обход по коллекции
                ' ARG1 = .FindByИмяПараметра(conARG1).ЗначениеВСИ
                ' ...
                ' ARG10 = .FindByИмяПараметра(conARG10).ЗначениеВСИ

                'For Each keysArg As String In inputArg.InputArgDictionary.Keys.ToArray
                '    inputArg.InputArgDictionary(keysArg) = .FindByИмяПараметра(keysArg).ЗначениеВСИ
                'Next

                ' '' иттератор по коллекции как KeyValuePair objects.
                ''For Each kvp As KeyValuePair(Of String, Double) In inputArg.InputArgDictionary
                ''    'Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value)
                ''    inputArg.InputArgDictionary(kvp.Key) = .FindByИмяПараметра(kvp.Key).ЗначениеВСИ
                ''Next

                ''For Each value As Double In inputArg.InputArgDictionary.Values
                ''    Console.WriteLine("Value = {0}", value)
                ''Next

                ' расчетные параметры
                InputParam.Tбокса = .FindByИмяПараметра(InputParameters.conTБОКСА).ЗначениеВСИ ' температура в боксе
                InputParam.Барометр = .FindByИмяПараметра(InputParameters.conБАРОМЕТР).ЗначениеВСИ ' БРС1-М
                ' учет атмосферного давления - относительного давления воздуха
                относительныйБарометр = InputParam.Барометр / const735_6
                InputParam.T3мерн_участка = .FindByИмяПараметра(InputParameters.T3_МЕРН_УЧАСТКА).ЗначениеВСИ
                ' можно так но хуже
                'rowИзмеренныйПараметр = .FindByИмяПараметра(conДавлениеВоздухаНаВходе)
                'rowИзмеренныйПараметр.ЗначениеВСИ = rowИзмеренныйПараметр.ЗначениеВСИ + относительныйБарометр
                'ДавлениеВоздухаНаВходе = rowИзмеренныйПараметр.ЗначениеВСИ

                InputParam.ДавлениеВоздухаНаВходе = .FindByИмяПараметра(InputParameters.ДАВЛЕНИЕ_ВОЗДУХА_НА_ВХОДЕ).ЗначениеВСИ + относительныйБарометр
                InputParam.ПерепадДавленияВоздухаНаВходе = .FindByИмяПараметра(InputParameters.ПЕРЕПАД_ДАВЛЕНИЯ_ВОЗДУХА_НА_ВХОДЕ).ЗначениеВСИ
                InputParam.ПерепадДавленияНаВходеКС = .FindByИмяПараметра(InputParameters.ПЕРЕПАД_ДАВЛЕНИЯ_НА_ВХОДЕ_КС).ЗначениеВСИ
                InputParam.Р310полное_воздуха_на_входе_КС = .FindByИмяПараметра(InputParameters.Р310_ПОЛНОЕ_ВОЗДУХА_НА_ВХОДЕ_КC).ЗначениеВСИ + относительныйБарометр
                InputParam.Р311статическое_воздуха_на_входе_КС = .FindByИмяПараметра(InputParameters.Р311_СТАТИЧЕСКОЕ_ВОЗДУХА_НА_ВХОДЕ_КС).ЗначениеВСИ + относительныйБарометр
                InputParam.ТтопливаКС = .FindByИмяПараметра(InputParameters.Т_ТОПЛИВА_КС).ЗначениеВСИ
                InputParam.ТтопливаКП = .FindByИмяПараметра(InputParameters.Т_ТОПЛИВА_КП).ЗначениеВСИ
                InputParam.РасходТопливаКамерыСгорания = .FindByИмяПараметра(InputParameters.Расход_Топлива_Камеры_Сгорания).ЗначениеВСИ
                InputParam.РасходТопливаКамерыПодогрева = .FindByИмяПараметра(InputParameters.РАСХОД_ТОПЛИВА_КАМЕРЫ_ПОДОГРЕВА).ЗначениеВСИ
                ' Test
#If EmulatorT340 = False Then
                ' закоментировал. т.к. реального входного канала нет
                'InputParam.Отсечка = .FindByИмяПараметра(InputParameter.ОТСЕЧКА_ТУРЕЛИ).ЗначениеВСИ
#End If
                ' постоянно обновляется в событии сбора, а когда отсечка следующего градуса сюда заносится 
                'arrПоясDictionary("Пояс" & I.ToString)(ИндексОтсечекДляПоля) = .FindByИмяПараметра("T340_" & I.ToString).НакопленноеЗначение 
                For I = 1 To ЧИСЛО_ТЕРМОПАР
                    'arrПоясDictionary("Пояс" & I.ToString)(ИндексОтсечекДляПоля) = .FindByИмяПараметра("T340_" & I.ToString).ЗначениеВСИ 'T340_1
#If EmulatorT340 = True Then
                    ' Test
                    arrТекущаяПоПоясам(I - 1) = gMainFomMdiParent.varТемпературныеПоля.arrDataTemperatureTest(ИндексОтсечекДляПоля, I - 1) '1506.3
#Else
                    arrТекущаяПоПоясам(I - 1) = .FindByИмяПараметра("T340_" & I.ToString).ЗначениеВСИ
#End If
                Next

                CalculatedParam.ТсредняяГазаНаВходе = 0
                For I As Integer = 1 To ЧИСЛО_Т_309
                    CalculatedParam.ТсредняяГазаНаВходе += .FindByИмяПараметра("T309_" & I.ToString).ЗначениеВСИ
                Next

                CalculatedParam.ТсредняяГазаНаВходе = CalculatedParam.ТсредняяГазаНаВходе / ЧИСЛО_Т_309
            End With
        Catch ex As Exception
            gMainFomMdiParent.varТемпературныеПоля.ShowError("Ошибка извлечения измеренных параметров")
            'перенаправление встроенной ошибки
            RaiseEvent DataError(Me, New DataErrorEventArgs(ex.Message, $"Процедура: <{NameOf(ИзвлечьЗначенияИзмеренныхПараметров)}>"))
        End Try
    End Sub

    ''' <summary>
    ''' накопление всех измеренных параметров
    ''' </summary>
    Private Sub НакопитьЗначенияИзмеренныхИРасчетныхПараметров()
        For Each rowИзмеренныйПараметр As BaseFormDataSet.ИзмеренныеПараметрыRow In Manager.MeasurementDataTable.Rows
            rowИзмеренныйПараметр.НакопленноеЗначение += rowИзмеренныйПараметр.ЗначениеВСИ
        Next

        For Each rowРасчетныйПараметр As BaseFormDataSet.РасчетныеПараметрыRow In Manager.CalculatedDataTable.Rows
            rowРасчетныйПараметр.НакопленноеЗначение += rowРасчетныйПараметр.ВычисленноеЗначениеВСИ
        Next

        СчетчикНакоплений += 1
    End Sub

    ''' <summary>
    ''' Поиск всех параметров по пользовательскому запросу в DataSet.РасчетныеПараметры
    ''' (с одним входным параметром являющимся именем расчётной величины).
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ВычислитьРасчетныеПараметры()
        Try
            If TuningParam.ИспытанияРозжигов.ЛогическоеЗначение Then
                'If Manager.НастроечныеПараметры.FindByИмяПараметра(TuningParameters.ИСПЫТАНИЯ_РОЗЖИГОВ).ЛогическоеЗначение Then
                ВычислениеОсновныхДляРозжигов()
            Else
                ВычислениеОсновных()
            End If
            '******************************************************************
            ' расчет интегральной температура газа по мерному сечению на поясе
            '******************************************************************
            Dim Ystart As Double, Yend As Double
            НайтиЗначенияТемпературНаСтенкахInterpolate(КоординатыТермопар, arrТекущаяПоПоясам, ШиринаМерногоУчастка, Ystart, Yend)
            y(0) = Ystart
            y(ЧИСЛО_ТЕРМОПАР + 1) = Yend

            CalculatedParam.T_интегр = ИнтегрированиеРадиальнойЭпюрыНаПроизвольныхКоординатах(КоординатыТермопар, arrТекущаяПоПоясам, ШиринаМерногоУчастка)

            '******************************************************************
            ' расчет Качество 
            '******************************************************************
            CalculatedParam.Качество = КачествоFun(CalculatedParam.T_интегр, CalculatedParam.ТсредняяГазаНаВходе, CalculatedParam.Тг_расчет)
            CalculatedParam.ПоложениеТурели = gMainFomMdiParent.varEncoder.AnglePosition

            ' занести вычисленные значения
            With Manager.CalculatedDataTable
                '' вместо последовательного извлечения применяется обход по коллекции
                '' .FindByИмяПараметра(conCalc1).ВычисленноеЗначениеВСИ = Calc1
                ' ********************************** и т.д. ********************************
                '' .FindByИмяПараметра(conCalc10).ВычисленноеЗначениеВСИ = Calc10

                '.FindByИмяПараметра(CalculatedParameter.G_СУМ_РАСХОД_ТОПЛИВА_КС_КП).ВычисленноеЗначениеВСИ = CalculatedParam.Gсум_расход_топливаКС_КП_кг_час
                '.FindByИмяПараметра(CalculatedParameter.G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ).ВычисленноеЗначениеВСИ = CalculatedParam.Gотбора_относительный
                ' ********************************** и т.д. ********************************

                For Each keysCalc As String In CalculatedParam.CalcDictionary.Keys.ToArray
                    .FindByИмяПараметра(keysCalc).ВычисленноеЗначениеВСИ = CalculatedParam(keysCalc)
                Next

                ' расчетные вспомогательные ...

                .FindByИмяПараметра(ПРОЭКЦИЯ_НА_СТЕНКУ1).ВычисленноеЗначениеВСИ = Ystart
                .FindByИмяПараметра(ПРОЭКЦИЯ_НА_СТЕНКУ2).ВычисленноеЗначениеВСИ = Yend
                '.FindByИмяПараметра(conЭпюрнаяНеравномерность).ВычисленноеЗначениеВСИ = вычисляются в РасчетПоля
                '.FindByИмяПараметра(conОкружнаяНеравномерность).ВычисленноеЗначениеВСИ = вычисляются в РасчетПоля

                '  Эпюрная Неравномерность и Окружная Неравномерность
                Dim средняя, temp As Double
                Dim max As Double = Double.MinValue

                For I As Integer = 1 To ЧИСЛО_ТЕРМОПАР
                    temp = arrТекущаяПоПоясам(I - 1)
                    средняя += temp
                    If temp > max Then
                        max = temp
                    End If
                Next

                средняя = средняя / ЧИСЛО_ТЕРМОПАР

                '.FindByИмяПараметра(ОКРУЖНАЯ_НЕРАВНОМЕРНОСТЬ).ВычисленноеЗначениеВСИ = НеравномерностьFun(max, CalculatedParam.ТсредняяГазаНаВходе, CalculatedParam.Тг_расчет)
                '.FindByИмяПараметра(ЭПЮРНАЯ_НЕРАВНОМЕРНОСТЬ).ВычисленноеЗначениеВСИ = НеравномерностьFun(средняя, CalculatedParam.ТсредняяГазаНаВходе, CalculatedParam.Тг_расчет)
                .FindByИмяПараметра(ОКРУЖНАЯ_НЕРАВНОМЕРНОСТЬ).ВычисленноеЗначениеВСИ = НеравномерностьFun(max, CalculatedParam.ТсредняяГазаНаВходе, CalculatedParam.T_интегр)
                .FindByИмяПараметра(ЭПЮРНАЯ_НЕРАВНОМЕРНОСТЬ).ВычисленноеЗначениеВСИ = НеравномерностьFun(средняя, CalculatedParam.ТсредняяГазаНаВходе, CalculatedParam.T_интегр)

                '.FindByИмяПараметра(conПоясMax).ВычисленноеЗначениеВСИ = вычисляются в РасчетПоля
            End With

            'With Manager
            '    ''по имени параметра strИмяПараметраГрафика определяем нужную функцию приведения
            '    ''("n1") 'который измеряет
            '    ''должна быть вызвана функция приведения параметра "n1" например
            '    'If n1ГПриводить Then
            '    '    'должна быть вызвана функция приведения параметра "n1" например
            '    '    .FindByИмяПараметра(cn1Г).ВычисленноеЗначениеВСИ = Air.funПривестиN(.ИзмеренныеПараметры.FindByИмяПараметра("n1").ЗначениеВСИ, tm)
            '    'Else
            '    '    'приводить не надо, просто копирование
            '    '    .FindByИмяПараметра(cn1Г).ВычисленноеЗначениеВСИ = .ИзмеренныеПараметры.FindByИмяПараметра("n1").ЗначениеВСИ
            '    'End If

            '    ''cnИГ_03Г
            '    ''который измеряет
            '    'nИГ_03 = .ИзмеренныеПараметры.FindByИмяПараметра("nИГ-03").ЗначениеВСИ / 46325 'коэф. перевода  n=1 при N=45190

            '    'If nИГ_03ГПриводить Then
            '    '    'должна быть вызвана функция приведения параметра "n1" например
            '    '    .FindByИмяПараметра(cnИГ_03Г).ВычисленноеЗначениеВСИ = Air.funПривестиN(nИГ_03, tm)
            '    'Else
            '    '    'приводить не надо, просто копирование
            '    '    .FindByИмяПараметра(cnИГ_03Г).ВычисленноеЗначениеВСИ = nИГ_03
            '    'End If
            'End With
        Catch ex As Exception
            gMainFomMdiParent.varТемпературныеПоля.ShowError("Ошибка вычисления расчётных параметров")
            'перенаправление встроенной ошибки
            RaiseEvent DataError(Me, New DataErrorEventArgs(ex.Message, $"Процедура: <{NameOf(ВычислитьРасчетныеПараметры)}>"))
        End Try

        If CBool(CalculatedParam.Тг_расчет = 0 - КЕЛЬВИН) Then
            gMainFomMdiParent.varТемпературныеПоля.ShowError("Ошибка вычисления расчётной температуры газа")
        End If
    End Sub

    ''' <summary>
    ''' Роэжиги
    ''' Основные вычисления, накопления, запись в базу для протокола.
    ''' Индивидуальные для наследуемого класса.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ВычислениеОсновныхДляРозжигов()
        With Manager.MeasurementDataTable
            ' Осреднить 6 значений T340
            Dim T340 As Double
            For I As Integer = 1 To 6
                T340 += .FindByИмяПараметра("T340_" & I.ToString).ЗначениеВСИ
            Next
            T340 = T340 / 6.0
            CalculatedParam.T340 = T340

            For I = 7 To ЧИСЛО_ТЕРМОПАР
                arrТекущаяПоПоясам(I - 1) = T340
            Next

            ' Осреднить 15 значений Р310
            Dim Р310 As Double
            For I As Integer = 1 To 3
                For J As Integer = 1 To 5
                    Р310 += .FindByИмяПараметра($"Р310_{I.ToString}_{J.ToString}").ЗначениеВСИ
                Next
            Next
            Р310 = Р310 / 15.0
            InputParam.Р310полное_воздуха_на_входе_КС = Р310 + относительныйБарометр


            ' Осреднить 3 значения Р311_1
            InputParam.Р311статическое_воздуха_на_входе_КС = (.FindByИмяПараметра(InputParameters.conР311_1).ЗначениеВСИ +
                                                            .FindByИмяПараметра(InputParameters.conР311_2).ЗначениеВСИ +
                                                            .FindByИмяПараметра(InputParameters.conР311_3).ЗначениеВСИ) / 3.0 + относительныйБарометр

            ' перепад давления топлива
            CalculatedParam.DPт1 = .FindByИмяПараметра(InputParameters.conPт1кОКС).ЗначениеВСИ - Р310
            CalculatedParam.DPт2 = .FindByИмяПараметра(InputParameters.conPт2кОКС).ЗначениеВСИ - Р310
        End With
        '**********************************************************************
        ' расчёт основных параметров
        '**********************************************************************
        ' Барометр-барометр
        ' РасходТопливаКамерыПодогреваКгСек-расход топлива камеры подогрева
        ' РасходТопливаКамерыСгоранияКгСек-расход топлива камеры сгорания
        ' Gвоздуха-расход Gв основной
        ' go-расход Go отбора
        ' КплотностьКП-плотность топлива от температуры
        ' КплотностьКС-плотность топлива от температуры
        ' Gрасход_газа-расход газа
        ' Gвоздуха_в_горении-расход воздуха участвующий в горении
        ' Gсум_расход_топливаКС_КП-суммарный расход топлива через к.с. и к.п.
        ' Gотбора_относительный-относительный расход отбираемого газа
        ' АльфаКамеры-коэффициент избытка воздуха
        ' АльфаСуммарный-суммарный коэффициент избытка воздуха
        ' Лямбда-приведенная скорость газового потока на входе в К.С.

        '--- коэф. учитывающий плотность --------------------------------------
        Dim КплотностьКС As Double = clAir.КоэфУчитывающийПлотность(InputParam.ТтопливаКС)
        Dim КплотностьТоплива1кКС As Double = clAir.КоэфУчитывающийПлотность(Manager.MeasurementDataTable.FindByИмяПараметра(InputParameters.conTт1кОКС).ЗначениеВСИ)
        '--- расход топлива камеры сгорания и камеры подогрева-----------------
        Dim РасходТопливаКамерыСгоранияКгСек As Double = InputParam.РасходТопливаКамерыСгорания * КплотностьКС / 3600.0# 'перевод л/час в кг/сек камеры сгорания
        Dim РасходТоплива1кКСКгСек As Double = Manager.MeasurementDataTable.FindByИмяПараметра(InputParameters.conGт1кКС).ЗначениеВСИ * КплотностьТоплива1кКС / 3600.0# 'перевод л/час в кг/сек камеры подогрева

        '--- расход Gв основной -----------------------------------------------
        CalculatedParam.Gвоздуха = Расход(InputParam.ДавлениеВоздухаНаВходе - относительныйБарометр,
                                          InputParam.ПерепадДавленияВоздухаНаВходе,
                                          InputParam.T3мерн_участка,
                                          D20отвОсн,
                                          D20трубОсн,
                                          InputParam.Барометр)
        ' Тест CalculatedParam.Gвоздуха = 6.550767
        '--- 35 расход газа Gрасход_газа на выходе из КП -------------------------
        Dim Gрасход_газа As Double = CalculatedParam.Gвоздуха ' топливо не учитывается + РасходТопливаКамерыПодогреваКгСек
        '--- 36 газ отбираемый от камеры на охлаждение ------------------------
        Dim Gрасход_отбора_нар As Double = Gрасход_газа * Gрасход_Отбора_Газа_11 / 100.0
        '--- относительный расход отбираемого газа ----------------------------
        'Dim Отн_расход_отбир_газа As Double = Gрасход_отбора_нар / Gасход_газа ' 99 изделие
        '--- 37 кол. топлива в отбираемом газе -----------------------------------
        'Gтопл_в_отбир_газе = РасходТопливаКамерыПодогреваКгСек * Отн_расход_отбир_газа ' 99 изделие
        'Dim Gтопл_в_отбир_газе As Double = РасходТопливаКамерыПодогреваКгСек * Gрасход_Отбора_Газа_11 / 100.0
        '--- 38 расход Go отбора наружного кольцевого канал ----------------------
        'Gвоздуха_отбора = Какая-то константа 11%
        ' в инструкции формула выглядет по другому, результат тот же : CalculatedParam.Gвоздуха_отбора = Gрасход_отбора_нар - Gтопл_в_отбир_газе
        CalculatedParam.Gвоздуха_отбора = CalculatedParam.Gвоздуха * Gрасход_Отбора_Газа_11 / 100.0
        '--- 39 газ на входе в КС (газ участвующий в горении) ------------------------------------------------
        Dim Gг_вх_кс As Double = Gрасход_газа - CalculatedParam.Gвоздуха_отбора
        '--- 40 топливо поступившее из подогревателя в зону горения КС
        'Dim GтоплВхКс As Double = РасходТопливаКамерыПодогреваКгСек - Gтопл_в_отбир_газе
        '--- 41 расход воздуха участвующий в горении К.С. (Gвкс) -----------------
        Dim Gвоздуха_в_горении As Double = CalculatedParam.Gвоздуха - CalculatedParam.Gвоздуха_отбора
        '--- 44 55 суммарный расход топлива через к.с. и к.п. Gсум_расход_топливаКС_КП_кг_час
        Dim Gсум_расход_топливаКС_КП As Double
        If TuningParam.ИспользоватьРасходВторогоКаскада.ЛогическоеЗначение Then
            'If Manager.НастроечныеПараметры.FindByИмяПараметра(TuningParameters.ИСПОЛЬЗОВАТЬ_РАСХОД_ВТОРОГО_КАСКАДА).ЛогическоеЗначение Then
            CalculatedParam.Gт2кКС = РасходТопливаКамерыСгоранияКгСек - РасходТоплива1кКСКгСек
            Gсум_расход_топливаКС_КП = РасходТоплива1кКСКгСек + CalculatedParam.Gт2кКС
        Else
            CalculatedParam.Gт2кКС = 0.0
            Gсум_расход_топливаКС_КП = РасходТоплива1кКСКгСек
        End If

        CalculatedParam.Gсум_расход_топливаКС_КП_кг_час = Gсум_расход_топливаКС_КП * 3600.0#
        '--- 45 коэффициент избытка воздуха К.П. ---------------------------------
        'CalculatedParam.АльфаКП = CalculatedParam.Gвоздуха / (СтехиометрическийК * РасходТоплива1кКСКгСек)
        '--- 46 56 коэффициент избытка воздуха К.С. ---------------------------------
        CalculatedParam.АльфаКамеры = Gвоздуха_в_горении / (СтехиометрическийК * Gсум_расход_топливаКС_КП)
        '--- 47 коэффициент избытка воздуха суммарный ----------------------------
        CalculatedParam.АльфаСуммарный = Gвоздуха_в_горении / (СтехиометрическийК * Gсум_расход_топливаКС_КП)
        '--- относительный расход отбираемого газа ----------------------------
        CalculatedParam.Gотбора_относительный = Gрасход_отбора_нар * 100.0# / Gрасход_газа ' равен = Gрасход_Отбора_Газа_11

        ' k=1.358 при Tk на входе в камеру сгорания примерно 500 гр. цельсия
        'Const k As Double = 1.358
        'Const Rg As Double = 29.27 '29.29 'была 29.4 при tk=500; при tk=200 - 29.27 ' универсальная газовая постоянная кГм/кг*К
        '--- (3) перевод давления их избыточного кгс/см2 в абсолютное в Паскалях
        Dim P311абсПа As Double = (InputParam.Р311статическое_воздуха_на_входе_КС - относительныйБарометр) * constG * 10000.0 + InputParam.Барометр * 1.3332 * 100.0
        '--- (9) коэф. адиабаты при рабочих давлениях и тепмературах ----------
        ВычислитьКоэфПересчетаОтТемпературы(CalculatedParam.ТсредняяГазаНаВходе + КЕЛЬВИН, P311абсПа / 1000000.0, RegimeType.Ksg_Kadiabatic)
        Dim tempM As Double = Math.Sqrt(Kadiabatic * (2.0 / (Kadiabatic + 1.0)) ^ ((Kadiabatic + 1.0) / (Kadiabatic - 1.0))) * Math.Sqrt(constG / Rg)
        '--- 50 вычисление g(лямбда) --------------------------------------------------
        Dim funGLambda As Double = Gрасход_газа * Math.Sqrt(CalculatedParam.ТсредняяГазаНаВходе + КЕЛЬВИН) / (tempM * InputParam.Р310полное_воздуха_на_входе_КС * Fdif)
        '--- приведенная скорость газового потока -----------------------------
        ВычислитьКоэфПересчетаQlambda(funGLambda * 100.0, Kadiabatic, RegimeType.Lambda)
        CalculatedParam.Лямбда = Lambda
        '--- расчетная температура газа в Цельсий -----------------------------
        CalculatedParam.Тг_расчет = clAir.РасчётнаяТемпература(InputParam.T3мерн_участка + КЕЛЬВИН, Gсум_расход_топливаКС_КП, Gвоздуха_в_горении, CalculatedParam.АльфаСуммарный)
    End Sub

    ''' <summary>
    ''' Основные вычисления, накопления, запись в базу для протокола.
    ''' Индивидуальные для наследуемого класса.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ВычислениеОсновных()
        '**********************************************************************
        ' расчёт основных параметров
        '**********************************************************************
        ' Барометр-барометр
        ' РасходТопливаКамерыПодогреваКгСек-расход топлива камеры подогрева
        ' РасходТопливаКамерыСгоранияКгСек-расход топлива камеры сгорания
        ' Gвоздуха-расход Gв основной
        ' go-расход Go отбора
        ' КплотностьКП-плотность топлива от температуры
        ' КплотностьКС-плотность топлива от температуры
        ' Gрасход_газа-расход газа
        ' Gвоздуха_в_горении-расход воздуха участвующий в горении
        ' Gсум_расход_топливаКС_КП-суммарный расход топлива через к.с. и к.п.
        ' Gотбора_относительный-относительный расход отбираемого газа
        ' АльфаКамеры-коэффициент избытка воздуха
        ' АльфаСуммарный-суммарный коэффициент избытка воздуха
        ' Лямбда-приведенная скорость газового потока на входе в К.С.

        '--- коэф. учитывающий плотность --------------------------------------
        Dim КплотностьКС As Double = clAir.КоэфУчитывающийПлотность(InputParam.ТтопливаКС)
        Dim КплотностьКП As Double = clAir.КоэфУчитывающийПлотность(InputParam.ТтопливаКП)
        '--- расход топлива камеры сгорания и камеры подогрева-----------------
        Dim РасходТопливаКамерыСгоранияКгСек As Double = InputParam.РасходТопливаКамерыСгорания * КплотностьКС / 3600.0# 'перевод л/час в кг/сек камеры сгорания
        Dim РасходТопливаКамерыПодогреваКгСек As Double = InputParam.РасходТопливаКамерыПодогрева * КплотностьКП / 3600.0# 'перевод л/час в кг/сек камеры подогрева
        '--- расход Gв основной -----------------------------------------------
        CalculatedParam.Gвоздуха = Расход(InputParam.ДавлениеВоздухаНаВходе - относительныйБарометр,
                                          InputParam.ПерепадДавленияВоздухаНаВходе,
                                          InputParam.T3мерн_участка,
                                          D20отвОсн,
                                          D20трубОсн,
                                          InputParam.Барометр)
        ' Тест CalculatedParam.Gвоздуха = 6.550767
        '--- 35 расход газа Gрасход_газа на выходе из КП -------------------------
        Dim Gрасход_газа As Double = CalculatedParam.Gвоздуха + РасходТопливаКамерыПодогреваКгСек
        '--- 36 газ отбираемый от камеры на охлаждение ------------------------
        Dim Gрасход_отбора_нар As Double = Gрасход_газа * Gрасход_Отбора_Газа_11 / 100.0
        '--- относительный расход отбираемого газа ----------------------------
        'Dim Отн_расход_отбир_газа As Double = Gрасход_отбора_нар / Gасход_газа ' 99 изделие
        '--- 37 кол. топлива в отбираемом газе -----------------------------------
        'Gтопл_в_отбир_газе = РасходТопливаКамерыПодогреваКгСек * Отн_расход_отбир_газа ' 99 изделие
        Dim Gтопл_в_отбир_газе As Double = РасходТопливаКамерыПодогреваКгСек * Gрасход_Отбора_Газа_11 / 100.0
        '--- 38 расход Go отбора наружного кольцевого канал ----------------------
        'Gвоздуха_отбора = Какая-то константа 11%
        ' в инструкции формула выглядет по другому, результат тот же : CalculatedParam.Gвоздуха_отбора = Gрасход_отбора_нар - Gтопл_в_отбир_газе
        CalculatedParam.Gвоздуха_отбора = CalculatedParam.Gвоздуха * Gрасход_Отбора_Газа_11 / 100.0
        '--- 39 газ на входе в КС (газ участвующий в горении) ------------------------------------------------
        Dim Gг_вх_кс As Double = Gрасход_газа - CalculatedParam.Gвоздуха_отбора
        '--- 40 топливо поступившее из подогревателя в зону горения КС
        Dim GтоплВхКс As Double = РасходТопливаКамерыПодогреваКгСек - Gтопл_в_отбир_газе
        '--- 41 расход воздуха участвующий в горении К.С. (Gвкс) -----------------
        'Gвоздуха_в_горении = CalculatedParam.Gвоздуха - Gрасход_отбора_нар + Gтопл_в_отбир_газе ' 99 изделие
        Dim Gвоздуха_в_горении As Double = CalculatedParam.Gвоздуха - CalculatedParam.Gвоздуха_отбора
        '--- 44 суммарный расход топлива через к.с. и к.п. Gсум_расход_топливаКС_КП_кг_час
        Dim Gсум_расход_топливаКС_КП As Double = РасходТопливаКамерыСгоранияКгСек + GтоплВхКс
        CalculatedParam.Gсум_расход_топливаКС_КП_кг_час = Gсум_расход_топливаКС_КП * 3600.0#
        '--- 45 коэффициент избытка воздуха К.П. ---------------------------------
        CalculatedParam.АльфаКП = CalculatedParam.Gвоздуха / (СтехиометрическийК * РасходТопливаКамерыПодогреваКгСек)
        '--- 46 коэффициент избытка воздуха К.С. ---------------------------------
        'CalculatedParam.АльфаКамеры = Gвоздуха_в_горении / (СтехиометрическийК * РасходТопливаКамерыСгоранияКгСек) ' 99 изделие
        CalculatedParam.АльфаКамеры = Gвоздуха_в_горении / (СтехиометрическийК * РасходТопливаКамерыСгоранияКгСек)
        '--- 47 коэффициент избытка воздуха суммарный ----------------------------
        'CalculatedParam.АльфаСуммарный = Gвоздуха_в_горении / (СтехиометрическийК * Gсум_расход_топливаКС_КП)
        CalculatedParam.АльфаСуммарный = Gвоздуха_в_горении / (СтехиометрическийК * Gсум_расход_топливаКС_КП)
        '--- относительный расход отбираемого газа ----------------------------
        CalculatedParam.Gотбора_относительный = Gрасход_отбора_нар * 100.0# / Gрасход_газа ' равен = Gрасход_Отбора_Газа_11

        ' k=1.358 при Tk на входе в камеру сгорания примерно 500 гр. цельсия
        'Const k As Double = 1.358
        'Const Rg As Double = 29.27 '29.29 'была 29.4 при tk=500; при tk=200 - 29.27 ' универсальная газовая постоянная кГм/кг*К
        '--- (3) перевод давления их избыточного кгс/см2 в абсолютное в Паскалях
        Dim P311абсПа As Double = (InputParam.Р311статическое_воздуха_на_входе_КС - относительныйБарометр) * constG * 10000.0 + InputParam.Барометр * 1.3332 * 100.0
        '--- (9) коэф. адиабаты при рабочих давлениях и тепмературах ----------
        ВычислитьКоэфПересчетаОтТемпературы(CalculatedParam.ТсредняяГазаНаВходе + КЕЛЬВИН, P311абсПа / 1000000.0, RegimeType.Ksg_Kadiabatic)
        Dim tempM As Double = Math.Sqrt(Kadiabatic * (2.0 / (Kadiabatic + 1.0)) ^ ((Kadiabatic + 1.0) / (Kadiabatic - 1.0))) * Math.Sqrt(constG / Rg)
        '--- 50 вычисление g(лямбда) --------------------------------------------------
        Dim funGLambda As Double = Gрасход_газа * Math.Sqrt(CalculatedParam.ТсредняяГазаНаВходе + КЕЛЬВИН) / (tempM * InputParam.Р310полное_воздуха_на_входе_КС * Fdif)
        '--- приведенная скорость газового потока -----------------------------
        'CalculatedParam.Лямбда = clAir.ПриведеннаяСкорость(funGотА, IzoentropaK.K135)' 99 изделие
        ВычислитьКоэфПересчетаQlambda(funGLambda * 100.0, Kadiabatic, RegimeType.Lambda)
        CalculatedParam.Лямбда = Lambda
        '--- расчетная температура газа в Цельсий -----------------------------
        CalculatedParam.Тг_расчет = clAir.РасчётнаяТемпература(InputParam.T3мерн_участка + КЕЛЬВИН, Gсум_расход_топливаКС_КП, Gвоздуха_в_горении, CalculatedParam.АльфаСуммарный)
        ' проверка ручного протокола
        'CalculatedParam.Тг_расчет = clAir.РасчётнаяТемпература(299.84 + КЕЛЬВИН, 1400 / 3600, 12, 2.4)
    End Sub

    ''' <summary>
    ''' Структура содержит значения строки массива для определения коэф. динамической вязкости
    ''' </summary>
    Private Structure N_tay_w
        Private bηj As Double
        Private rj As Double
        Private tj As Double
        Private ω_rj As Double
        Private τ_tj As Double
        Private bηj_ω_rj_τ_tj As Double

        Public Sub New(inbηj As Double, inrj As Double, intj As Double)
            bηj = inbηj
            rj = inrj
            tj = intj
        End Sub

        Public Function Calculate_bηj_ω_rj_τ_tj(tay As Double, w As Double) As Double
            ω_rj = w ^ rj
            τ_tj = tay ^ tj
            bηj_ω_rj_τ_tj = bηj * ω_rj / τ_tj

            Return bηj_ω_rj_τ_tj
        End Function
    End Structure

    Private arrN_tay_w As N_tay_w() = {
        New N_tay_w(93.697, 1, 1),
        New N_tay_w(-82.4089, 1, 2),
        New N_tay_w(132.488, 2, 0),
        New N_tay_w(-177.977, 3, 0),
        New N_tay_w(73.9072, 3, 1),
        New N_tay_w(20.544, 3, 2),
        New N_tay_w(137.268, 4, 0),
        New N_tay_w(-107.034, 4, 1),
        New N_tay_w(-27.9017, 5, 0),
        New N_tay_w(29.0736, 5, 1)}

    Private managerGraph As ManagerGraphKof
    Private Const nameKsg As String = "Ksg"
    Private Const nameKadiabatic As String = "Kadiabatic"
    Private Const nameLambda As String = "Lambda"

    'Private Режим As RegimeType
    ''' <summary>
    ''' коэф. сжимаемости
    ''' </summary>
    Private коэфСжимаемости As Double
    ''' <summary>
    ''' показатель адиабаты
    ''' </summary>
    Private Kadiabatic As Double
    ''' <summary>
    ''' Приведенная скорость газового потока
    ''' </summary>
    Private Lambda As Double

    Private Sub ПодготовитьТаблицы()
        'Dim ownCatalogue As String = IO.Path.Combine(IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Chamber67\Table")
        Dim ownCatalogue As String = IO.Path.Combine(PathResourses, "Table")
        Dim irregularGraphFromT As String() = {nameKsg, nameKadiabatic}
        Dim irregularFromQlambda As String() = {nameLambda}
        'Dim proportionalGraphs As String() = {gAkn1fi}

        managerGraph = New ManagerGraphKof(ownCatalogue)
        managerGraph.ДобавитьГрафики(irregularGraphFromT, GraphType.Irregular, RegimeType.Ksg_Kadiabatic)
        managerGraph.ДобавитьГрафики(irregularFromQlambda, GraphType.Irregular, RegimeType.Lambda)

        '_ManagerGraph.ДобавитьГрафики(proportionalGraphs, GraphType.Proportional, РежимEnum.Максимал)

        Try
            If managerGraph.СчитатьИнициализировать Then
            Else
                managerGraph = Nothing
            End If
        Catch ex As Exception
            'MessageBox.Show(ex.ToString, "ClassCalculation.New ", MessageBoxButtons.OK, MessageBoxIcon.Error)
            RaiseEvent DataError(Me, New DataErrorEventArgs($"Процедура {NameOf(ПодготовитьТаблицы)} в базе параметров не найден!", ex.ToString))
        End Try
    End Sub

    Private Structure ParamGb
        ''' <summary>
        ''' относительный диаметр отверстия СУ
        ''' </summary>
        Public b As Double
        ''' <summary>
        ''' диаметр измерительного трубопровода Dит при рабочей температкре газа
        ''' </summary>
        Public Dит As Double
        ''' <summary>
        ''' диаметр сужающего устройство dсу при рабочей температкре газа
        ''' </summary>
        Dim dсу As Double
        ''' <summary>
        ''' коэф. динамической вязкости
        ''' </summary>
        Public ДинамическаяВязкость As Double
        ''' <summary>
        ''' эквивалентная шероховатость
        ''' </summary>
        Public Rw As Double
        ''' <summary>
        ''' коэффициент скоорости входа
        ''' </summary>
        Public E As Double
        ''' <summary>
        ''' коэф. расширения сопла ИСА-1932
        ''' </summary>
        Dim eps As Double
        ''' <summary>
        ''' плотность воздуха
        ''' </summary>
        Dim ro As Double
        ''' <summary>
        ''' перепад dP давления в Паскалях
        ''' </summary>
        Dim dP As Double
    End Structure

    ''' <summary>
    ''' Функция расчёта расхода Gв основн и Gв отбора
    ''' </summary>
    ''' <param name="ДавлениеВоздухаНаВходе"></param>
    ''' <param name="ПерепадДавленияВоздухаНаВходе"></param>
    ''' <param name="ТпередСужающимУстройством"></param>
    ''' <param name="d20отв"></param>
    ''' <param name="D20труб"></param>
    ''' <returns></returns>
    Private Function Расход(ByVal ДавлениеВоздухаНаВходе As Double,
                            ByVal ПерепадДавленияВоздухаНаВходе As Double,
                            ByVal ТпередСужающимУстройством As Double,
                            ByVal d20отв As Double,
                            ByVal D20труб As Double,
                            ByVal inБарометр As Double
                            ) As Double
        ' ДавлениеВоздухаНаВходе - абсолютное давление
        ' ПерепадДавленияВоздухаНаВходе - перепад давлений
        ' ТпередСужающимУстройством - рабочая температура
        ' D20отв - диаметр сужащивающего устроуства
        ' D20труб - диаметр трубопровода

        Const ТцельсияНормУсловия As Double = 20.0 ' - нормальные условия
        Const ТнормУсловия As Double = КЕЛЬВИН + ТцельсияНормУсловия ' 293.15  - нормальные условия
        '--- (2) Перевод в Кельвин --------------------------------------------
        Dim Tk As Double = ТпередСужающимУстройством + КЕЛЬВИН
        '--- (3) перевод давления их избыточного кгс/см2 в абсолютное в Паскалях
        Dim P As Double = ДавлениеВоздухаНаВходе * constG * 10000.0 + inБарометр * 1.3332 * 100.0
        '--- (4) определить коэф. линейного расширения для материалов измерительного трубопровода 
        ' и сужающего устройства расходомерного устройства 
        ' измерительный трубопровод 12Х18Н10Т
        Dim temp1 As Double = ТпередСужающимУстройством / 1000.0
        Dim atит As Double = 0.000001 * (A0tr + A1tr * temp1 + A2tr * temp1 * temp1)
        ' сужающее устройство 12Х18Н9Т
        Dim atсу As Double = 0.000001 * (A0su + A1su * temp1 + A2su * temp1 * temp1)
        '--- (5) диаметр измерительного трубопровода Dит при рабочей температкре газа
        Dim Dит As Double = D20труб / 1000.0 * (1.0 + atит * (ТпередСужающимУстройством - ТцельсияНормУсловия))
        '--- (6) диаметр сужающего устройство dсу при рабочей температкре газа
        Dim dсу As Double = d20отв / 1000.0 * (1.0 + atсу * (ТпередСужающимУстройством - ТцельсияНормУсловия))
        '--- (7) относительный диаметр отверстия СУ ---------------------------
        Dim b As Double = dсу / Dит
        '--- (8) коэффициент скоорости входа E --------------------------------
        Dim E As Double = 1 / (Math.Sqrt(1 - b * b * b * b))
        '---(9) коэф. сжимаемости при рабочих давлениях -----------------------
        ВычислитьКоэфПересчетаОтТемпературы(Tk, P / 1000000.0, RegimeType.Ksg_Kadiabatic)
        ' там вычислено коэфСжимаемости = managerGraph.КоэфПересчета(nameKsg)
        '--- (11) плотность воздуха кг/см3 ------------------------------------
        ' ro - плотность воздуха при рабочих условиях
        Dim ro As Double = ПлотностьВоздухаНУ * ТнормУсловия * P / (ДавлениеВоздухаНУ * 1000000.0 * Tk * коэфСжимаемости)
        '--- (12) приведенная температура -------------------------------------
        Dim tay As Double = Tk / Tkr
        '--- (13) коэф. динамической вязкости воздуха в разреженном состоянии
        Dim notay As Double = -66.9619 / (tay ^ 1.5) + 322.119 / tay - 547.958 / (tay ^ 0.5) + 347.643 + 38.4042 * tay - 2.18923 * tay ^ 1.5
        '--- (14) приведенная плотность ---------------------------------------
        Dim w As Double = ro / Pkr
        '--- (15) избыточная вязкость -----------------------------------------
        'Dim deltaN_tay_w As Double

        'For Each itemdN_tay_w As N_tay_w In arrN_tay_w
        '    deltaN_tay_w += itemdN_tay_w.Calculate_bηj_ω_rj_τ_tj(tay, w)
        'Next

        Dim deltaN_tay_w As Double = arrN_tay_w.Sum(Function(itemdN_tay_w) itemdN_tay_w.Calculate_bηj_ω_rj_τ_tj(tay, w))
        '--- (16) коэф. динамической вязкости Па*с ------------------------------
        Dim ДинамическаяВязкость As Double = (notay + deltaN_tay_w) * 0.0000001
        '--- (17) показатель адиабаты -----------------------------------------
        ' там вычислено Kadiabatic = managerGraph.КоэфПересчета(nameKadiabatic)
        '--- (19) перевод dP давления их избыточного кгс/см2 в абсолютное в Паскалях
        Dim dP As Double = ПерепадДавленияВоздухаНаВходе * constG * 10000.0
        '--- (20) вычислить вспомогательный коэф. tb --------------------------
        Dim tb As Double = 1 - dP / P
        '--- (21) коэф. расширения сопла ИСА-1932 -----------------------------
        Dim eps As Double = (Kadiabatic * tb ^ (2 / Kadiabatic)) / (Kadiabatic - 1)
        eps = eps * (1 - b * b * b * b) / (1 - b * b * b * b * tb ^ (2 / Kadiabatic))
        eps = eps * (1 - tb ^ ((Kadiabatic - 1) / Kadiabatic)) / (1 - tb)
        eps = Math.Sqrt(eps)
        '--- (23) промежуточная величина Ra -----------------------------------
        Dim Ra_pr As Double = Ra2 + (b - B2) * (Ra1 - Ra2) / (B1 - B2)
        '--- (24) фактическое значение среднеарифметического отклонения профиля шероховатости
        Dim Ra As Double = Ra_pr * Dит / 10000.0
        '--- (25) эквивалентная шероховатость Rw ------------------------------
        Dim Rw As Double = Ra * Math.PI
        Dim isFirst As Boolean = True ' первое приближение
        Dim Re As Double = 1000000.0 ' Re - число Рейнольдса в первом приближении
        Dim mParamGb As New ParamGb With {.b = b,
                                            .Dит = Dит,
                                            .dсу = dсу,
                                            .ДинамическаяВязкость = ДинамическаяВязкость,
                                            .Rw = Rw,
                                            .E = E,
                                            .eps = eps,
                                            .ro = ro,
                                            .dP = dP}
        Dim index As Integer
        ' Расход - результат
        Return Gb(Re, isFirst, mParamGb, 0, index)
    End Function

    Private Function Gb(Re As Double, ByRef isFirst As Boolean, inParamGb As ParamGb, oldGb As Double, ByRef index As Integer) As Double
        If index > 100 Then Return 9999999.0
        '--- (26) вспомогательный коэф. Are -----------------------------------
        Dim Are As Double = AreHelp(Re, isFirst)

        With inParamGb
            '--- (27) коэф. шероховатости Kw --------------------------------------
            Dim Kw As Double = 1.0 + Are * .b ^ 4 * (0.045 * Math.Log10(.Rw * 100000.0 / .Dит) - 0.025)
            '--- (28) коэф. истечения сопла ИСА 1932 ------------------------------
            Dim C As Double = 0.99 - 0.2262 * .b ^ 4.1 - (0.00175 * .b ^ 2 - 0.0033 * .b ^ 4.15) * ((1000000.0 / Re) ^ 1.15)
            '--- (1) расход воздуха -----------------------------------------------
            Dim newGb As Double = (Math.PI * .dсу ^ 2 / 4.0) * Kw * .E * C * .eps * (2.0 * .ro * .dP) ^ 0.5
            index += 1

            If Math.Abs((newGb - oldGb) / newGb) < 0.00001 Then
                Return newGb
            Else
                ' выполнитьпересчет следующего приближения
                Re = Рейнольдс(newGb, .Dит, .ДинамическаяВязкость)
                Return Gb(Re, isFirst, inParamGb, newGb, index)
            End If
        End With
    End Function

    ''' <summary>
    ''' Коэф. Рейнольдса
    ''' </summary>
    ''' <param name="Gb">расход воздуха</param>
    ''' <param name="Dит">диаметр измерительного трубопровода Dит при рабочей температкре газа</param>
    ''' <param name="ДинамическаяВязкость">коэф. динамической вязкости</param>
    ''' <returns></returns>
    Private Function Рейнольдс(Gb As Double, Dит As Double, ДинамическаяВязкость As Double) As Double
        Return (4.0 * Gb) / (Math.PI * Dит * ДинамическаяВязкость)
    End Function

    ''' <summary>
    ''' Вспомогательный коэффициент
    ''' </summary>
    ''' <param name="Re">число Рейнольдса</param>
    ''' <param name="isFirst"></param>
    ''' <returns></returns>
    Private Function AreHelp(Re As Double, ByRef isFirst As Boolean) As Double
        If isFirst Then
            isFirst = False
            Return 1.0
        Else
            If Re >= 1000000.0 Then
                Return 1.0
            Else
                Return 1.0 - ((Math.Log10(Re) - 6.0) ^ 2) / 4.0
            End If
        End If
    End Function

    ''' <summary>
    ''' Вычисление табличных значений от 2-х параметров температуры и 
    ''' давления (или влажности {valFi унаследован} в зависимости от типа графика).
    ''' </summary>
    ''' <param name="TemperatureK"></param>
    ''' <param name="pressureMPa"></param>
    ''' <param name="valFi"></param>
    ''' <param name="Режим"></param>
    Private Sub ВычислитьКоэфПересчетаОтТемпературы(TemperatureK As Double, pressureMPa As Double, valFi As Double, Режим As RegimeType)
        managerGraph.ВычислитьВсеКоэфПересчета(TemperatureK, pressureMPa, valFi, Режим)
        Select Case Режим
            Case RegimeType.Ksg_Kadiabatic
                'температура
                коэфСжимаемости = managerGraph.КоэфПересчета(nameKsg)
                Kadiabatic = managerGraph.КоэфПересчета(nameKadiabatic)
                'влажность
                'kn1fi = _ManagerGraph.КоэфПересчета(gAkn1fi)

                'Case РежимEnum.Форсаж
                '    'температура
                '    kn1 = _ManagerGraph.КоэфПересчета(gFkn1)
                '    'влажность
                '    kn1fi = _ManagerGraph.КоэфПересчета(gFkn1fi)
        End Select
    End Sub

    ''' <summary>
    ''' Вычисление табличных значений от 2-х параметров температуры и давления.
    ''' </summary>
    ''' <param name="TemperatureK"></param>
    ''' <param name="pressureMPa"></param>
    ''' <param name="Режим"></param>
    Private Sub ВычислитьКоэфПересчетаОтТемпературы(TemperatureK As Double, pressureMPa As Double, Режим As RegimeType)
        ВычислитьКоэфПересчетаОтТемпературы(TemperatureK, pressureMPa, 0, Режим)
    End Sub

    ''' <summary>
    ''' Вычисление табличных значений от 2-х параметров Qlambda и Kadiabatic {valFi унаследован}
    ''' Приведенная скорость газового потока
    ''' </summary>
    ''' <param name="Qlambda">умножать на 100.0 т.к. таблица занесена промасштабированная</param>
    ''' <param name="Kadiabatic"></param>
    ''' <param name="valFi"></param>
    ''' <param name="Режим"></param>
    Private Sub ВычислитьКоэфПересчетаQlambda(Qlambda As Double, Kadiabatic As Double, valFi As Double, Режим As RegimeType)
        managerGraph.ВычислитьВсеКоэфПересчета(Qlambda, Kadiabatic, valFi, Режим)
        Select Case Режим
            Case RegimeType.Lambda
                Lambda = managerGraph.КоэфПересчета(nameLambda)
        End Select
    End Sub

    ''' <summary>
    ''' Вычисление табличных значений от 2-х параметров Qlambda и Kadiabatic.
    ''' Приведенная скорость газового потока
    ''' </summary>
    ''' <param name="Qlambda">умножать на 100.0 т.к. таблица занесена промасштабированная</param>
    ''' <param name="Kadiabatic"></param>
    ''' <param name="Режим"></param>
    Private Sub ВычислитьКоэфПересчетаQlambda(Qlambda As Double, Kadiabatic As Double, Режим As RegimeType)
        ВычислитьКоэфПересчетаQlambda(Qlambda, Kadiabatic, 0, Режим)
    End Sub

    ''' <summary>
    ''' Проверить наличие записей в таблице с именами специфичных для расчёта параметров.
    ''' </summary>
    ''' <remarks></remarks>
    Private Function ПроверитьНаличиеЗаписиРасчетныйПараметр(ByVal nameRow As String) As Boolean
        'If dt.Columns.Contains(NameColumn) Then
        '    Return dt.Columns(NameColumn).Ordinal
        'Else
        '    Return -1
        'End If

        For Each rowРасчетныйПараметр As BaseFormDataSet.РасчетныеПараметрыRow In Manager.CalculatedDataTable.Rows
            If rowРасчетныйПараметр.ИмяПараметра = nameRow Then
                Return True
                Exit Function
            End If
        Next

        Return False
    End Function

    'Protected Overrides Sub Finalize()
    '    MyBase.Finalize()
    'End Sub

    'Public Function ТестРасчетаБиблиотеки() As System.Data.DataSet Implements BaseForm.IClassCalculation.ТестРасчетаБиблиотеки
    '    Dim myLinearAlgebra As New LinearAlgebra
    '    With myLinearAlgebra
    '        .matrixADataTextBox = "4.00, 2.00, -1.00; 1.00, 4.00, 1.00; 0.10, 1.00, 2.00;"
    '        .matrixBDataTextBox = "2.00; 12.00; 10.00;"
    '        '.operationsComboBox = Global.MathematicalLibrary.LinearAlgebra.EnumOperationsComboBox.SolveLinearEquations_AxB
    '        .operationsComboBox = LinearAlgebra.EnumOperationsComboBox.SolveLinearEquations_AxB
    '        .Compute()
    '        ТестРасчетаБиблиотеки = .data
    '    End With
    'End Function
End Class

'''' <summary>
'''' Функция расчёта расхода Gв основн и Gв отбора
'''' </summary>
'''' <param name="P"></param>
'''' <param name="dP"></param>
'''' <param name="ТпередСужающимУстройством"></param>
'''' <param name="d20отв"></param>
'''' <param name="D20труб"></param>
'''' <param name="коэфСжимаемости"></param>
'''' <param name="inРасчетРасхода"></param>
'''' <returns></returns>
'Private Function Расход(ByVal P As Double,
'                        ByVal dP As Double,
'                        ByVal ТпередСужающимУстройством As Double,
'                        ByVal d20отв As Double,
'                        ByVal D20труб As Double,
'                        ByVal коэфСжимаемости As Double,
'                        ByVal коэфЛинейногоТепловогоРасширения As Double,
'                        ByVal inРасчетРасхода As РасчетРасхода) As Double
'    ' P - абсолютное давление
'    ' dP - перепад давлений
'    ' Т - рабочая температура
'    ' D20отв - диаметр сужащивающего устроуства
'    ' D20труб - диаметр трубопровода
'    ' Ks - коф. сжимаемости измеряемой среды
'    ' Flag - признак =1 для Gоснов и =2 для Gотб
'    ' введеено считывание с базы.


'    Const ТцельсияНормУсловия As Double = 20.0 ' - нормальные условия
'    Const ТнормУсловия As Double = КЕЛЬВИН + ТцельсияНормУсловия ' 293.15  - нормальные условия
'    Dim K, E, M As Double

'    '' учёт температурных потерь в магистрале перепуска и разныз материалов сопла и трубопровода
'    'Const РабочаяТемператураОсновногоЛудла As Double = 250.0
'    'Const РабочаяТемператураОтбораЛудла As Double = 500.0
'    'Const РабочаяТемператураОтбораТрубопровода As Double = 475.0
'    'Select Case inРасчетРасхода
'    '    Case РасчетРасхода.РасходGвОсновной
'    '        dотв = d20отв * (1 + КоэфЛинейногоТепловогоРасширенияМерногоСопла * (РабочаяТемператураОсновногоЛудла - ТцельсияНормУсловия))
'    '        Dтруб = D20труб * (1 + КоэфЛинейногоТепловогоРасширенияТрубопровода * (РабочаяТемператураОсновногоЛудла - ТцельсияНормУсловия))
'    '        Exit Select
'    '    Case РасчетРасхода.РасходGвОтбора
'    '        dотв = d20отв * (1 + КоэфЛинейногоТепловогоРасширенияМерногоСопла * (РабочаяТемператураОтбораЛудла - ТцельсияНормУсловия))
'    '        Dтруб = D20труб * (1 + КоэфЛинейногоТепловогоРасширенияТрубопровода * (РабочаяТемператураОтбораТрубопровода - ТцельсияНормУсловия))
'    '        Exit Select
'    '    Case РасчетРасхода.РасходGвОтбораВн
'    '        Exit Select
'    'End Select

'    ' здесь материал сопла и трубопровода подразумевается один и тот же
'    Dim dотв As Double = d20отв * (1 + коэфЛинейногоТепловогоРасширения * (ТпередСужающимУстройством - ТцельсияНормУсловия))
'    Dim Dтруб As Double = D20труб * (1 + коэфЛинейногоТепловогоРасширения * (ТпередСужающимУстройством - ТцельсияНормУсловия))

'    ' m - модуль сужающего устройства
'    M = dотв / Dтруб
'    M = M * M
'    ' e - поправочный множитель на расширение среды
'    ' K - показатель адиабаты
'    ' K = 1.4
'    If ТпередСужающимУстройством < 50 Then
'        K = 1.4
'    Else
'        K = Kadiobaty(ТпередСужающимУстройством)
'    End If

'    E = (1 - dP / P) ^ (2 / K)
'    E = E * (K / (K - 1))
'    E = E * ((1 - (1 - dP / P) ^ ((K - 1) / K)) / (dP / P))
'    E = E * (1 - M * M) / (1 - M * M * (1 - dP / P) ^ (2 / K))
'    E = Math.Sqrt(E)

'    Dim Re As Double ' Re - число Рейнольда
'    Dim ro As Double ' ro - плотность воздуха при рабочих условиях
'    Dim расходИтог As Double ' Расход - результат

'    Const ДавлениеВоздухаНормальныхУсловиях As Double = 1.0332 ' 1.0332 - давление воздуха в нормальных условиях
'    Const ПлотностьВоздухаНормальныхУсловиях As Double = 1.205 ' 1.205 - плотность воздуха в нормальных условиях
'    'Const КоэфСжимаемости As Double = 1.0025 ' 1.0025 - коэф. сжимаемости

'    '' учёт температурных потерь в магистрале перепуска
'    'Const РабочееДавление As Double = 5.0 ' 5 - рабочее давление
'    'Select Case inРасчетРасхода
'    '    Case РасчетРасхода.РасходGвОсновной
'    '        ro = ПлотностьВоздухаНормальныхУсловиях * ТнормУсловия * РабочееДавление / (ДавлениеВоздухаНормальныхУсловиях * (РабочаяТемператураОсновногоЛудла + КЕЛЬВИН) * коэфСжимаемости)
'    '    Case РасчетРасхода.РасходGвОтбора
'    '        ro = ПлотностьВоздухаНормальныхУсловиях * ТнормУсловия * РабочееДавление / (ДавлениеВоздухаНормальныхУсловиях * (РабочаяТемператураОтбораТрубопровода + КЕЛЬВИН) * коэфСжимаемости)
'    '    Case РасчетРасхода.РасходGвОтбораВн
'    '        Exit Select
'    'End Select

'    ro = ПлотностьВоздухаНормальныхУсловиях * ТнормУсловия * P / (ДавлениеВоздухаНормальныхУсловиях * (ТпередСужающимУстройством + КЕЛЬВИН) * коэфСжимаемости)

'    ' вычисление коэф. расхода (в инструкции даётся как константа)
'    Const ДинамическаяВязкость As Double = 0.00000283 ' 2,83е-6 динамическая вязкость
'    Const МаксРасходПриРабочихУсловиях As Double = 12.5 ' 12.5 - макс. расход при рабочих условиях из испытаний КС-99

'    Re = (0.0361 * МаксРасходПриРабочихУсловиях * ro) / (Dтруб * 0.001 * ДинамическаяВязкость)
'    ' alpha - средний расход сопла
'    Dim alpha As Double = 1 / Math.Sqrt(1 - M * M)
'    alpha = alpha * (0.99 - 0.2262 * M ^ 2.05 + (0.000215 - 0.001125 * M ^ 0.5 + 0.0249 * M ^ 2.35) * (1000000.0# / Re) ^ 1.15)
'    ' окончательный расход
'    'было в Салюте dотв = d20отв * (1 + КоэфЛинейногоТепловогоРасширенияМерногоСопла * (ТпередСужающимУстройством - ТцельсияНормУсловия))

'    расходИтог = 0.01252 * alpha * E * dотв * dотв / 3600
'    ' включить
'    расходИтог = расходИтог * Math.Sqrt(dP * P * 10000.0# * ПлотностьВоздухаНормальныхУсловиях * ТнормУсловия / (ДавлениеВоздухаНормальныхУсловиях * (ТпередСужающимУстройством + КЕЛЬВИН) * коэфСжимаемости))
'    '***************
'    ' отладка все убрать
'    'Dim aaa As Double
'    'aaa = dP * P * 10000# * ПлотностьВоздухаНормальныхУсловиях * ТнормУсловия / (ДавлениеВоздухаНормальныхУсловиях * (T + conКельвин) * Ks)
'    'r = r * Sqr(Abs(aaa))
'    '***************
'    Return расходИтог
'End Function



'Private action As System.Action(Of FileInfo)
'Private match As System.Predicate(Of FileInfo)
'Private fileList As New List(Of FileInfo)
'Private fileArray() As FileInfo

'Private Sub outputWindowRadioButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles outputWindowRadioButton.CheckedChanged
'    action = New System.Action(Of FileInfo) _
'     (AddressOf DisplayInOutputWindow)
'End Sub

'Private Sub forEachListButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles forEachListButton.Click
'    ResetListBox()
'    fileList.ForEach(action)
'End Sub

'Private Sub DisplayInOutputWindow(ByVal file As FileInfo)
'    Debug.WriteLine(String.Format("{0} ({1} bytes)", _
'     file.Name, file.Length))
'End Sub

'Private Sub smallFilesRadioButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles smallFilesRadioButton.CheckedChanged
'    match = New System.Predicate(Of FileInfo) _
'     (AddressOf IsSmall)
'End Sub
'Private Sub findAllListButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles findAllListButton.Click
'    ResetListBox()

'    ' Create a list containing matching files,
'    ' and then take the appropriate action.
'    Dim subList As List(Of FileInfo) = fileList.FindAll(match)
'    subList.ForEach(action)
'End Sub

'Private Function IsSmall( _
'ByVal file As FileInfo) As Boolean

'    ' Return True if the file's length is less than 500 bytes.
'    Return file.Length < 500
'End Function

