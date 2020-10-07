''' <summary>
''' Входные аргументы
''' </summary>
''' <remarks></remarks>
Public Class InputParameters
    Public Const conБАРОМЕТР As String = "Барометр"
    Public Const conTБОКСА As String = "Tбокса" ' температура в боксе
    Public Const T3_МЕРН_УЧАСТКА As String = "T3мерн_участка"
    Public Const ДАВЛЕНИЕ_ВОЗДУХА_НА_ВХОДЕ As String = "ДавлениеВоздухаНаВходе"
    Public Const ПЕРЕПАД_ДАВЛЕНИЯ_ВОЗДУХА_НА_ВХОДЕ As String = "ПерепадДавленияВоздухаНаВходе"
    Public Const ПЕРЕПАД_ДАВЛЕНИЯ_НА_ВХОДЕ_КС As String = "ПерепадДавленияНаВходеКС"
    Public Const Р310_ПОЛНОЕ_ВОЗДУХА_НА_ВХОДЕ_КC As String = "Р310полное_воздуха_на_входе_КС" ' Полное Давление воздуха на входе в КС
    Public Const Р311_СТАТИЧЕСКОЕ_ВОЗДУХА_НА_ВХОДЕ_КС As String = "Р311статическое_воздуха_на_входе_КС" ' Статическое давление на входе в  КС
    Public Const Т_ТОПЛИВА_КС As String = "ТтопливаКС"
    Public Const Т_ТОПЛИВА_КП As String = "ТтопливаКП"
    Public Const Расход_Топлива_Камеры_Сгорания As String = "РасходТопливаКамерыСгорания"
    Public Const РАСХОД_ТОПЛИВА_КАМЕРЫ_ПОДОГРЕВА As String = "РасходТопливаКамерыПодогрева"
    Public Const ТВОЗДУХА_НА_ВХОДЕ_КП As String = "ТвоздухаНаВходеКП"

    Public Const conР310_1_1 = "Р310_1_1" '	Розжиги
    Public Const conР310_1_2 = "Р310_1_2" '	Розжиги
    Public Const conР310_1_3 = "Р310_1_3" '	Розжиги
    Public Const conР310_1_4 = "Р310_1_4" '	Розжиги
    Public Const conР310_1_5 = "Р310_1_5" '	Розжиги
    Public Const conР310_2_1 = "Р310_2_1" '	Розжиги
    Public Const conР310_2_2 = "Р310_2_2" '	Розжиги
    Public Const conР310_2_3 = "Р310_2_3" '	Розжиги
    Public Const conР310_2_4 = "Р310_2_4" '	Розжиги
    Public Const conР310_2_5 = "Р310_2_5 " 'Розжиги
    Public Const conР310_3_1 = "Р310_3_1" '	Розжиги
    Public Const conР310_3_2 = "Р310_3_2" '	Розжиги
    Public Const conР310_3_3 = "Р310_3_3" '	Розжиги
    Public Const conР310_3_4 = "Р310_3_4" '	Розжиги
    Public Const conР310_3_5 = "Р310_3_5" '	Розжиги
    Public Const conР311_1 = "Р311_1"   '	Розжиги
    Public Const conР311_2 = "Р311_2"   '	Розжиги
    Public Const conР311_3 = "Р311_3"   '	Розжиги
    Public Const conGт1кКС = "Gт1кКС"   '	Розжиги
    Public Const conPт1кОКС = "Pт1кОКС" '	Розжиги
    Public Const conPт2кОКС = "Pт2кОКС" '	Розжиги
    Public Const conTт1кОКС = "Tт1кОКС" '	Розжиги
    Public Const conTт2кОКС = "Tт2кОКС" '	Розжиги

    Private mБарометр As Double
    Public Property Барометр() As Double
        Get
            Return mБарометр
        End Get
        Set(ByVal value As Double)
            mБарометр = value
            InputParameterDictionary(conБАРОМЕТР) = value
        End Set
    End Property

    Private mTбокса As Double
    Public Property Tбокса() As Double
        Get
            Return mTбокса
        End Get
        Set(ByVal value As Double)
            mTбокса = value
            InputParameterDictionary(conTБОКСА) = value
        End Set
    End Property

    Private mT3мерн_участка As Double
    Public Property T3мерн_участка() As Double
        Get
            Return mT3мерн_участка
        End Get
        Set(ByVal value As Double)
            mT3мерн_участка = value
            InputParameterDictionary(T3_МЕРН_УЧАСТКА) = value
        End Set
    End Property

    Private mДавлениеВоздухаНаВходе As Double
    Public Property ДавлениеВоздухаНаВходе() As Double
        Get
            Return mДавлениеВоздухаНаВходе
        End Get
        Set(ByVal value As Double)
            mДавлениеВоздухаНаВходе = value
            InputParameterDictionary(ДАВЛЕНИЕ_ВОЗДУХА_НА_ВХОДЕ) = value
        End Set
    End Property

    Private mПерепадДавленияВоздухаНаВходе As Double
    Public Property ПерепадДавленияВоздухаНаВходе() As Double
        Get
            Return mПерепадДавленияВоздухаНаВходе
        End Get
        Set(ByVal value As Double)
            mПерепадДавленияВоздухаНаВходе = value
            InputParameterDictionary(ПЕРЕПАД_ДАВЛЕНИЯ_ВОЗДУХА_НА_ВХОДЕ) = value
        End Set
    End Property

    Private mПерепадДавленияНаВходеКС As Double
    Public Property ПерепадДавленияНаВходеКС() As Double
        Get
            Return mПерепадДавленияНаВходеКС
        End Get
        Set(ByVal value As Double)
            mПерепадДавленияНаВходеКС = value
            InputParameterDictionary(ПЕРЕПАД_ДАВЛЕНИЯ_НА_ВХОДЕ_КС) = value
        End Set
    End Property

    Private mР310полное_воздуха_на_входе_КС As Double
    Public Property Р310полное_воздуха_на_входе_КС() As Double
        Get
            Return mР310полное_воздуха_на_входе_КС
        End Get
        Set(ByVal value As Double)
            mР310полное_воздуха_на_входе_КС = value
            InputParameterDictionary(Р310_ПОЛНОЕ_ВОЗДУХА_НА_ВХОДЕ_КC) = value
        End Set
    End Property

    Private mР311статическое_воздуха_на_входе_КС As Double
    Public Property Р311статическое_воздуха_на_входе_КС() As Double
        Get
            Return mР311статическое_воздуха_на_входе_КС
        End Get
        Set(ByVal value As Double)
            mР311статическое_воздуха_на_входе_КС = value
            InputParameterDictionary(Р311_СТАТИЧЕСКОЕ_ВОЗДУХА_НА_ВХОДЕ_КС) = value
        End Set
    End Property

    Private mТтопливаКС As Double
    Public Property ТтопливаКС() As Double
        Get
            Return mТтопливаКС
        End Get
        Set(ByVal value As Double)
            mТтопливаКС = value
            InputParameterDictionary(Т_ТОПЛИВА_КС) = value
        End Set
    End Property

    Private mТтопливаКП As Double
    Public Property ТтопливаКП() As Double
        Get
            Return mТтопливаКП
        End Get
        Set(ByVal value As Double)
            mТтопливаКП = value
            InputParameterDictionary(Т_ТОПЛИВА_КП) = value
        End Set
    End Property

    Private mРасходТопливаКамерыСгорания As Double
    Public Property РасходТопливаКамерыСгорания() As Double
        Get
            Return mРасходТопливаКамерыСгорания
        End Get
        Set(ByVal value As Double)
            mРасходТопливаКамерыСгорания = value
            InputParameterDictionary(Расход_Топлива_Камеры_Сгорания) = value
        End Set
    End Property

    Private mРасходТопливаКамерыПодогрева As Double
    Public Property РасходТопливаКамерыПодогрева() As Double
        Get
            Return mРасходТопливаКамерыПодогрева
        End Get
        Set(ByVal value As Double)
            mРасходТопливаКамерыПодогрева = value
            InputParameterDictionary(РАСХОД_ТОПЛИВА_КАМЕРЫ_ПОДОГРЕВА) = value
        End Set
    End Property

    Private mТвоздухаНаВходеКП As Double
    Public Property ТвоздухаНаВходеКП() As Double
        Get
            Return mТвоздухаНаВходеКП
        End Get
        Set(ByVal value As Double)
            mТвоздухаНаВходеКП = value
            InputParameterDictionary(ТВОЗДУХА_НА_ВХОДЕ_КП) = value
        End Set
    End Property

    'Public Const ОТСЕЧКА_ТУРЕЛИ As String = "Отсечка"
    'Private mОтсечка As Double
    'Public Property Отсечка() As Double
    '    Get
    '        Return mОтсечка
    '    End Get
    '    Set(ByVal value As Double)
    '        mОтсечка = value
    '        InputParameterDictionary(ОТСЕЧКА_ТУРЕЛИ) = value
    '    End Set
    'End Property

#Region "Розжиги"
    Private mР310_1_1 As Double
    Public Property Р310_1_1() As Double
        Get
            Return mР310_1_1
        End Get
        Set(ByVal value As Double)
            mР310_1_1 = value
            InputParameterDictionary(conР310_1_1) = value
        End Set
    End Property

    Private mР310_1_2 As Double
    Public Property Р310_1_2() As Double
        Get
            Return mР310_1_2
        End Get
        Set(ByVal value As Double)
            mР310_1_2 = value
            InputParameterDictionary(conР310_1_2) = value
        End Set
    End Property

    Private mР310_1_3 As Double
    Public Property Р310_1_3() As Double
        Get
            Return mР310_1_3
        End Get
        Set(ByVal value As Double)
            mР310_1_3 = value
            InputParameterDictionary(conР310_1_3) = value
        End Set
    End Property

    Private mР310_1_4 As Double
    Public Property Р310_1_4() As Double
        Get
            Return mР310_1_4
        End Get
        Set(ByVal value As Double)
            mР310_1_4 = value
            InputParameterDictionary(conР310_1_4) = value
        End Set
    End Property

    Private mР310_1_5 As Double
    Public Property Р310_1_5() As Double
        Get
            Return mР310_1_5
        End Get
        Set(ByVal value As Double)
            mР310_1_5 = value
            InputParameterDictionary(conР310_1_5) = value
        End Set
    End Property

    Private mР310_2_1 As Double
    Public Property Р310_2_1() As Double
        Get
            Return mР310_2_1
        End Get
        Set(ByVal value As Double)
            mР310_2_1 = value
            InputParameterDictionary(conР310_2_1) = value
        End Set
    End Property

    Private mР310_2_2 As Double
    Public Property Р310_2_2() As Double
        Get
            Return mР310_2_2
        End Get
        Set(ByVal value As Double)
            mР310_2_2 = value
            InputParameterDictionary(conР310_2_2) = value
        End Set
    End Property

    Private mР310_2_3 As Double
    Public Property Р310_2_3() As Double
        Get
            Return mР310_2_3
        End Get
        Set(ByVal value As Double)
            mР310_2_3 = value
            InputParameterDictionary(conР310_2_3) = value
        End Set
    End Property

    Private mР310_2_4 As Double
    Public Property Р310_2_4() As Double
        Get
            Return mР310_2_4
        End Get
        Set(ByVal value As Double)
            mР310_2_4 = value
            InputParameterDictionary(conР310_2_4) = value
        End Set
    End Property

    Private mР310_2_5 As Double
    Public Property Р310_2_5() As Double
        Get
            Return mР310_2_5
        End Get
        Set(ByVal value As Double)
            mР310_2_5 = value
            InputParameterDictionary(conР310_2_5) = value
        End Set
    End Property

    Private mР310_3_1 As Double
    Public Property Р310_3_1() As Double
        Get
            Return mР310_3_1
        End Get
        Set(ByVal value As Double)
            mР310_3_1 = value
            InputParameterDictionary(conР310_3_1) = value
        End Set
    End Property

    Private mР310_3_2 As Double
    Public Property Р310_3_2() As Double
        Get
            Return mР310_3_2
        End Get
        Set(ByVal value As Double)
            mР310_3_2 = value
            InputParameterDictionary(conР310_3_2) = value
        End Set
    End Property

    Private mР310_3_3 As Double
    Public Property Р310_3_3() As Double
        Get
            Return mР310_3_3
        End Get
        Set(ByVal value As Double)
            mР310_3_3 = value
            InputParameterDictionary(conР310_3_3) = value
        End Set
    End Property

    Private mР310_3_4 As Double
    Public Property Р310_3_4() As Double
        Get
            Return mР310_3_4
        End Get
        Set(ByVal value As Double)
            mР310_3_4 = value
            InputParameterDictionary(conР310_3_4) = value
        End Set
    End Property

    Private mР310_3_5 As Double
    Public Property Р310_3_5() As Double
        Get
            Return mР310_3_5
        End Get
        Set(ByVal value As Double)
            mР310_3_5 = value
            InputParameterDictionary(conР310_3_5) = value
        End Set
    End Property

    Private mР311_1 As Double
    Public Property Р311_1() As Double
        Get
            Return mР311_1
        End Get
        Set(ByVal value As Double)
            mР311_1 = value
            InputParameterDictionary(conР311_1) = value
        End Set
    End Property

    Private mР311_2 As Double
    Public Property Р311_2() As Double
        Get
            Return mР311_2
        End Get
        Set(ByVal value As Double)
            mР311_2 = value
            InputParameterDictionary(conР311_2) = value
        End Set
    End Property

    Private mР311_3 As Double
    Public Property Р311_3() As Double
        Get
            Return mР311_3
        End Get
        Set(ByVal value As Double)
            mР311_3 = value
            InputParameterDictionary(conР311_3) = value
        End Set
    End Property

    Private mGт1кКС As Double
    Public Property Gт1кКС() As Double
        Get
            Return mGт1кКС
        End Get
        Set(ByVal value As Double)
            mGт1кКС = value
            InputParameterDictionary(conGт1кКС) = value
        End Set
    End Property

    Private mPт1кОКС As Double
    Public Property Pт1кОКС() As Double
        Get
            Return mPт1кОКС
        End Get
        Set(ByVal value As Double)
            mPт1кОКС = value
            InputParameterDictionary(conPт1кОКС) = value
        End Set
    End Property

    Private mPт2кОКС As Double
    Public Property Pт2кОКС() As Double
        Get
            Return mPт2кОКС
        End Get
        Set(ByVal value As Double)
            mPт2кОКС = value
            InputParameterDictionary(conPт2кОКС) = value
        End Set
    End Property

    Private mTт1кОКС As Double
    Public Property Tт1кОКС() As Double
        Get
            Return mTт1кОКС
        End Get
        Set(ByVal value As Double)
            mTт1кОКС = value
            InputParameterDictionary(conTт1кОКС) = value
        End Set
    End Property

    Private mTт2кОКС As Double
    Public Property Tт2кОКС() As Double
        Get
            Return mTт2кОКС
        End Get
        Set(ByVal value As Double)
            mTт2кОКС = value
            InputParameterDictionary(conTт2кОКС) = value
        End Set
    End Property

#End Region

    Public Property InputParameterDictionary As Dictionary(Of String, Double)

    Public Sub New()
        InputParameterDictionary = New Dictionary(Of String, Double) From {
        {conБАРОМЕТР, Барометр},
        {conTБОКСА, Tбокса},
        {ДАВЛЕНИЕ_ВОЗДУХА_НА_ВХОДЕ, ДавлениеВоздухаНаВходе},
        {ПЕРЕПАД_ДАВЛЕНИЯ_ВОЗДУХА_НА_ВХОДЕ, ПерепадДавленияВоздухаНаВходе},
        {T3_МЕРН_УЧАСТКА, T3мерн_участка},
        {ПЕРЕПАД_ДАВЛЕНИЯ_НА_ВХОДЕ_КС, ПерепадДавленияНаВходеКС},
        {Р310_ПОЛНОЕ_ВОЗДУХА_НА_ВХОДЕ_КC, Р310полное_воздуха_на_входе_КС},
        {Р311_СТАТИЧЕСКОЕ_ВОЗДУХА_НА_ВХОДЕ_КС, Р311статическое_воздуха_на_входе_КС},
        {Т_ТОПЛИВА_КС, ТтопливаКС},
        {Т_ТОПЛИВА_КП, ТтопливаКП},
        {Расход_Топлива_Камеры_Сгорания, РасходТопливаКамерыСгорания},
        {РАСХОД_ТОПЛИВА_КАМЕРЫ_ПОДОГРЕВА, РасходТопливаКамерыПодогрева},
        {ТВОЗДУХА_НА_ВХОДЕ_КП, ТвоздухаНаВходеКП},
        {conР310_1_1, Р310_1_1},
        {conР310_1_2, Р310_1_2},
        {conР310_1_3, Р310_1_3},
        {conР310_1_4, Р310_1_4},
        {conР310_1_5, Р310_1_5},
        {conР310_2_1, Р310_2_1},
        {conР310_2_2, Р310_2_2},
        {conР310_2_3, Р310_2_3},
        {conР310_2_4, Р310_2_4},
        {conР310_2_5, Р310_2_5},
        {conР310_3_1, Р310_3_1},
        {conР310_3_2, Р310_3_2},
        {conР310_3_3, Р310_3_3},
        {conР310_3_4, Р310_3_4},
        {conР310_3_5, Р310_3_5},
        {conР311_1, Р311_1},
        {conР311_2, Р311_2},
        {conР311_3, Р311_3},
        {conGт1кКС, Gт1кКС},
        {conPт1кОКС, Pт1кОКС},
        {conPт2кОКС, Pт2кОКС},
        {conTт1кОКС, Tт1кОКС},
        {conTт2кОКС, Tт2кОКС}}

        'InputParameterDictionary.Add(ОТСЕЧКА_ТУРЕЛИ, Отсечка)
    End Sub

End Class
