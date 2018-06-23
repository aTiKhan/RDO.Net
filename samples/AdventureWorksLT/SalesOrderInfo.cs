﻿using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderInfo : SalesOrder
    {
        static SalesOrderInfo()
        {
            RegisterColumnGroup((SalesOrderInfo _) => _.Customer);
            RegisterColumnGroup((SalesOrderInfo _) => _.ShipToAddress);
            RegisterColumnGroup((SalesOrderInfo _) => _.BillToAddress);
        }

        public Customer.Lookup Customer { get; private set; }
        public Address.Lookup ShipToAddress { get; private set; }
        public Address.Lookup BillToAddress { get; private set; }

        public new SalesOrderInfoDetail SalesOrderDetails
        {
            get { return (SalesOrderInfoDetail)base.SalesOrderDetails; }
        }

        protected sealed override SalesOrderDetail CreateSalesOrderDetail()
        {
            return CreateSalesOrderDetailInfo();
        }

        protected virtual SalesOrderInfoDetail CreateSalesOrderDetailInfo()
        {
            return new SalesOrderInfoDetail();
        }
    }
}
