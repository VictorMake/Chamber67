Imports NationalInstruments.UI
Imports NationalInstruments.UI.WindowsForms
''' <summary>
''' Расчетные параметры
''' </summary>
''' <remarks></remarks>
Public Class CalculatedParameters
    Implements IEnumerable
    Public Property CalcDictionary As Dictionary(Of String, Parameter)

    Default Public Property Item(key As String) As Double
        Get
            Return CalcDictionary(key).CalculatedValue
        End Get
        Set(value As Double)
            CalcDictionary(key).CalculatedValue = value
        End Set
    End Property

    'Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
    '    Return CalcDictionary.GetEnumerator()
    'End Function

    'Реализация интерфейса IEnumerable предполагает стандартную реализацию перечислителя.
    ' Однако мы можем не полагаться на стандартную реализацию, а создать свою логику итератора с помощью ключевых слов Iterator и Yield.
    ' Конструкция итератора представляет метод, в котором используется ключевое слово Yield для перебора по коллекции или массиву.
    Public Iterator Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        For Each keysCalc As String In CalcDictionary.Keys.ToArray
            Yield CalcDictionary(keysCalc)
        Next
    End Function

    Public Const G_СУМ_РАСХОД_ТОПЛИВА_КС_КП As String = "Gсум_расход_топливаКС_КП"
    Public Const G_ВОЗДУХА_ОТБОРА As String = "Gвоздуха_отбора"
    Public Const G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ As String = "Gотбора_относительный"
    Public Const АЛЬФА_КАМЕРЫ As String = "АльфаКамеры" ' суммарный коэф. избытка воздуха 
    Public Const АЛЬФА_КП As String = "АльфаКП" ' коэффициент избытка воздуха К.П.
    Public Const АЛЬФА_СУММАРНЫЙ As String = "АльфаСуммарный" ' коэффициент избытка воздуха суммарный
    Public Const conЛЯМБДА As String = "Лямбда" ' приведенная скорость газового потока 
    Public Const G_ВОЗДУХА As String = "Gвоздуха"
    Public Const Т_СРЕДНЯЯ_ГАЗА_НА_ВХОДЕ As String = "Тсредняя_газа_на_входе" ' Средняя температура воздуха на входе
    Public Const conT_ИНТЕГР As String = "T_интегр"
    Public Const Т_Г_РАСЧЕТ As String = "Тг_расчет" ' расчетная температура газа
    Public Const conКАЧЕСТВО As String = "Качество"
    Public Const ПОЛОЖЕНИЕ_ТУРЕЛИ As String = "Положение_Турели"

    Public Const conT340 As String = "T340" 'Розжиги - средняя Т газа на выходе из КС
    Public Const condPт1 As String = "dPт1" 'Розжиги - перепад давления топлива на 1 каскаде
    Public Const condPт2 As String = "dPт2" 'Розжиги - перепад давления топлива на 2 каскаде
    Public Const conGт2кКС = "Gт2кКС" '	Розжиги - расход топлива через 2 каскад

    Public Sub New()
        CalcDictionary = New Dictionary(Of String, Parameter) From {
        {G_СУМ_РАСХОД_ТОПЛИВА_КС_КП, New Parameter With {.Name = G_СУМ_РАСХОД_ТОПЛИВА_КС_КП}},
        {G_ВОЗДУХА_ОТБОРА, New Parameter With {.Name = G_ВОЗДУХА_ОТБОРА}},
        {G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ, New Parameter With {.Name = G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ}},
        {АЛЬФА_КАМЕРЫ, New Parameter With {.Name = АЛЬФА_КАМЕРЫ}},
        {АЛЬФА_КП, New Parameter With {.Name = АЛЬФА_КП}},
        {АЛЬФА_СУММАРНЫЙ, New Parameter With {.Name = АЛЬФА_СУММАРНЫЙ}},
        {conЛЯМБДА, New Parameter With {.Name = conЛЯМБДА}},
        {G_ВОЗДУХА, New Parameter With {.Name = G_ВОЗДУХА}},
        {Т_СРЕДНЯЯ_ГАЗА_НА_ВХОДЕ, New Parameter With {.Name = Т_СРЕДНЯЯ_ГАЗА_НА_ВХОДЕ}},
        {conT_ИНТЕГР, New Parameter With {.Name = conT_ИНТЕГР}},
        {Т_Г_РАСЧЕТ, New Parameter With {.Name = Т_Г_РАСЧЕТ}},
        {conКАЧЕСТВО, New Parameter With {.Name = conКАЧЕСТВО}},
        {ПОЛОЖЕНИЕ_ТУРЕЛИ, New Parameter With {.Name = ПОЛОЖЕНИЕ_ТУРЕЛИ}},
        {conT340, New Parameter With {.Name = conT340}},
        {condPт1, New Parameter With {.Name = condPт1}},
        {condPт2, New Parameter With {.Name = condPт2}},
        {conGт2кКС, New Parameter With {.Name = conGт2кКС}}}
    End Sub

    Public Sub BindingWithControls(key As String, inINumericPointer As INumericPointer, inNumericEdit As NumericEdit)
        CalcDictionary(key).ControlNumericPointer = inINumericPointer
        CalcDictionary(key).ControlNumericEdit = inNumericEdit
    End Sub

    Public Property Gсум_расход_топливаКС_КП_кг_час() As Double
        Get
            Return CalcDictionary(G_СУМ_РАСХОД_ТОПЛИВА_КС_КП).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(G_СУМ_РАСХОД_ТОПЛИВА_КС_КП).CalculatedValue = value
        End Set
    End Property

    Public Property Gвоздуха_отбора() As Double
        Get
            Return CalcDictionary(G_ВОЗДУХА_ОТБОРА).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(G_ВОЗДУХА_ОТБОРА).CalculatedValue = value
        End Set
    End Property

    Public Property Gотбора_относительный() As Double
        Get
            Return CalcDictionary(G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(G_ОТБОРА_ОТНОСИТЕЛЬНЫЙ).CalculatedValue = value
        End Set
    End Property

    Public Property АльфаКамеры() As Double
        Get
            Return CalcDictionary(АЛЬФА_КАМЕРЫ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(АЛЬФА_КАМЕРЫ).CalculatedValue = value
        End Set
    End Property

    Public Property АльфаКП() As Double
        Get
            Return CalcDictionary(АЛЬФА_КП).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(АЛЬФА_КП).CalculatedValue = value
        End Set
    End Property

    Public Property АльфаСуммарный() As Double
        Get
            Return CalcDictionary(АЛЬФА_СУММАРНЫЙ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(АЛЬФА_СУММАРНЫЙ).CalculatedValue = value
        End Set
    End Property

    Public Property Лямбда() As Double
        Get
            Return CalcDictionary(conЛЯМБДА).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(conЛЯМБДА).CalculatedValue = value
        End Set
    End Property

    Public Property Gвоздуха() As Double
        Get
            Return CalcDictionary(G_ВОЗДУХА).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(G_ВОЗДУХА).CalculatedValue = value
        End Set
    End Property

    Public Property ТсредняяГазаНаВходе() As Double
        Get
            Return CalcDictionary(Т_СРЕДНЯЯ_ГАЗА_НА_ВХОДЕ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(Т_СРЕДНЯЯ_ГАЗА_НА_ВХОДЕ).CalculatedValue = value
        End Set
    End Property

    Public Property T_интегр() As Double
        Get
            Return CalcDictionary(conT_ИНТЕГР).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(conT_ИНТЕГР).CalculatedValue = value
        End Set
    End Property

    Public Property Тг_расчет() As Double
        Get
            Return CalcDictionary(Т_Г_РАСЧЕТ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(Т_Г_РАСЧЕТ).CalculatedValue = value
        End Set
    End Property

    Public Property Качество() As Double
        Get
            Return CalcDictionary(conКАЧЕСТВО).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(conКАЧЕСТВО).CalculatedValue = value
        End Set
    End Property

    Public Property ПоложениеТурели() As Double
        Get
            Return CalcDictionary(ПОЛОЖЕНИЕ_ТУРЕЛИ).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(ПОЛОЖЕНИЕ_ТУРЕЛИ).CalculatedValue = value
        End Set
    End Property

#Region "Розжиги"
    Public Property T340() As Double
        Get
            Return CalcDictionary(conT340).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(conT340).CalculatedValue = value
        End Set
    End Property

    Public Property DPт1() As Double
        Get
            Return CalcDictionary(condPт1).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(condPт1).CalculatedValue = value
        End Set
    End Property

    Public Property DPт2() As Double
        Get
            Return CalcDictionary(condPт2).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(condPт2).CalculatedValue = value
        End Set
    End Property

    Public Property Gт2кКС() As Double
        Get
            Return CalcDictionary(conGт2кКС).CalculatedValue
        End Get
        Set(ByVal value As Double)
            CalcDictionary(conGт2кКС).CalculatedValue = value
        End Set
    End Property
#End Region

End Class

Public Class Parameter
    'Public Enum TypeEnum
    '    Pointer
    '    NumericEdit
    'End Enum
    'Public Property Type As TypeEnum
    Public Property Name As String
    Public Property ControlNumericPointer As INumericPointer
    Public Property ControlNumericEdit As NumericEdit
    Public Property CalculatedValue As Double
End Class
