Imports DevZest.Data
Imports DevZest.Data.Annotations
Imports DevZest.Data.SqlServer

Namespace DevZest.Samples.AdventureWorksLT
    Public MustInherit Class BaseModel(Of T As PrimaryKey)
        Inherits Model(Of T)

        Shared Sub New()
            RegisterColumn(Function(x As BaseModel(Of T)) x.RowGuid)
            RegisterColumn(Function(x As BaseModel(Of T)) x.ModifiedDate)
        End Sub

        Private m_RowGuid As _Guid
        <Required>
        <AutoGuid(Name:="DF_%_rowguid", Description:="Default constraint value of NEWID()")>
        <Unique(Name:="AK_%_rowguid", Description:="Unique nonclustered constraint. Used to support replication samples.")>
        <DbColumn(Description:="ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.")>
        Public Property RowGuid As _Guid
            Get
                Return m_RowGuid
            End Get
            Private Set
                m_RowGuid = Value
            End Set
        End Property

        Private _ModifiedDate As _DateTime
        <Required>
        <AsDateTime>
        <AutoDateTime(Name:="DF_%_ModifiedDate", Description:="Default constraint value of GETDATE()")>
        <DbColumn(Description:="Date and time the record was last updated.")>
        Public Property ModifiedDate As _DateTime
            Get
                Return _ModifiedDate
            End Get
            Private Set
                _ModifiedDate = Value
            End Set
        End Property

        Public Sub ResetRowIdentifiers()
            Dim vDataSet = DataSet
            If vDataSet Is Nothing Then Return

            For i As Integer = 0 To vDataSet.Count - 1
                RowGuid(i) = Guid.NewGuid()
                ModifiedDate(i) = DateTime.Now
            Next
        End Sub
    End Class
End Namespace