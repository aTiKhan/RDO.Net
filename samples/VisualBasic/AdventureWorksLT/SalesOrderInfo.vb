<InvisibleToDbDesigner>
Public Class SalesOrderInfo
    Inherits SalesOrder

    Shared Sub New()
        RegisterProjection(Function(x As SalesOrderInfo) x.Customer)
        RegisterProjection(Function(x As SalesOrderInfo) x.ShipToAddress)
        RegisterProjection(Function(x As SalesOrderInfo) x.BillToAddress)
    End Sub

    Private m_Customer As Customer.Lookup
    Public Property Customer As Customer.Lookup
        Get
            Return m_Customer
        End Get
        Private Set
            m_Customer = Value
        End Set
    End Property

    Private m_ShipToAddress As Address.Lookup
    Public Property ShipToAddress As Address.Lookup
        Get
            Return m_ShipToAddress
        End Get
        Private Set
            m_ShipToAddress = Value
        End Set
    End Property

    Private m_BillToAddress As Address.Lookup
    Public Property BillToAddress As Address.Lookup
        Get
            Return m_BillToAddress
        End Get
        Private Set
            m_BillToAddress = Value
        End Set
    End Property

    Public Overloads ReadOnly Property SalesOrderDetails As SalesOrderInfoDetail
        Get
            Return CType(MyBase.SalesOrderDetails, SalesOrderInfoDetail)
        End Get
    End Property
End Class
