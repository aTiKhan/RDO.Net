﻿using DevZest.Data;
using DevZest.Data.Annotations;
using DevZest.Data.SqlServer;

namespace DevZest.Samples.AdventureWorksLT
{
    public class ProductModelProductDescription : BaseModel<ProductModelProductDescription.PK>
    {
        [DbPrimaryKey("PK_ProductModelProductDescription_ProductModelID_ProductDescriptionID_Culture", Description = "Primary key (clustered) constraint")]
        public sealed class PK : PrimaryKey
        {
            public static IDataValues ValueOf(int productModelID, int productDescriptionID, string culture)
            {
                return DataValues.Create(_Int32.Const(productModelID), _Int32.Const(productDescriptionID), _String.Const(culture));
            }

            public PK(_Int32 productModelID, _Int32 productDescriptionID, _String culture)
                : base(productModelID, productDescriptionID, culture)
            {
            }

            public _Int32 ProductModelID
            {
                get { return GetColumn<_Int32>(0); }
            }

            public _Int32 ProductDescriptionID
            {
                get { return GetColumn<_Int32>(1); }
            }

            public _Int32 Culture
            {
                get { return GetColumn<_Int32>(2); }
            }
        }

        public static readonly Mounter<_Int32> _ProductModelID = RegisterColumn((ProductModelProductDescription _) => _.ProductModelID);
        public static readonly Mounter<_Int32> _ProductDescriptionID = RegisterColumn((ProductModelProductDescription _) => _.ProductDescriptionID);
        public static readonly Mounter<_String> _Culture = RegisterColumn((ProductModelProductDescription _) => _.Culture);

        protected sealed override PK CreatePrimaryKey()
        {
            return new PK(ProductModelID, ProductDescriptionID, Culture);
        }

        private ProductModel.PK _fk_productModel;
        public ProductModel.PK FK_ProductModel
        {
            get { return _fk_productModel ?? (_fk_productModel = new ProductModel.PK(ProductModelID)); }
        }

        private ProductDescription.PK _fk_productDescription;
        public ProductDescription.PK FK_ProductDescription
        {
            get { return _fk_productDescription ?? (_fk_productDescription = new ProductDescription.PK(ProductDescriptionID)); }
        }

        [DbColumn(Description = "Primary key. Foreign key to ProductModel.ProductModelID.")]
        public _Int32 ProductModelID { get; private set; }

        [DbColumn(Description = "Primary key. Foreign key to ProductDescription.ProductDescriptionID.")]
        public _Int32 ProductDescriptionID { get; private set; }

        [AsNChar(6)]
        [DbColumn(Description = "The culture for which the description is written.")]
        public _String Culture { get; private set; }
    }
}
