﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Data {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Data.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;ProductCategoryID&quot; : 1,
        ///      &quot;ParentProductCategoryID&quot; : null,
        ///      &quot;Name&quot; : &quot;Bikes&quot;,
        ///      &quot;RowGuid&quot; : &quot;cfbda25c-df71-47a7-b81b-64ee161aa37c&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///      &quot;SubCategories&quot; : [
        ///         {
        ///            &quot;ProductCategoryID&quot; : 5,
        ///            &quot;Name&quot; : &quot;Mountain Bikes&quot;,
        ///            &quot;RowGuid&quot; : &quot;2d364ade-264a-433c-b092-4fcbf3804e01&quot;,
        ///            &quot;ModifiedDate&quot; : &quot;2002-06-01T00:00:00.000&quot;,
        ///            &quot;SubCategories&quot; : [
        ///               
        ///       [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ExpectedJSON_ProductCategories {
            get {
                return ResourceManager.GetString("ExpectedJSON_ProductCategories", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;SalesOrderID&quot; : 71774,
        ///      &quot;RevisionNumber&quot; : 2,
        ///      &quot;OrderDate&quot; : &quot;2008-06-01T00:00:00.000&quot;,
        ///      &quot;DueDate&quot; : &quot;2008-06-13T00:00:00.000&quot;,
        ///      &quot;ShipDate&quot; : &quot;2008-06-08T00:00:00.000&quot;,
        ///      &quot;Status&quot; : 5,
        ///      &quot;OnlineOrderFlag&quot; : false,
        ///      &quot;SalesOrderNumber&quot; : &quot;SO71774&quot;,
        ///      &quot;PurchaseOrderNumber&quot; : &quot;PO348186287&quot;,
        ///      &quot;AccountNumber&quot; : &quot;10-4020-000609&quot;,
        ///      &quot;CustomerID&quot; : 29847,
        ///      &quot;ShipToAddressID&quot; : 1092,
        ///      &quot;BillToAddressID&quot; : 1092,
        ///      &quot;ShipMethod&quot; : &quot;CA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ExpectedJSON_SalesOrder_71774 {
            get {
                return ResourceManager.GetString("ExpectedJSON_SalesOrder_71774", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;ColumnList_0&quot; : 71774,
        ///      &quot;ColumnList_1&quot; : &quot;SO71774&quot;
        ///   },
        ///   {
        ///      &quot;ColumnList_0&quot; : 71776,
        ///      &quot;ColumnList_1&quot; : &quot;SO71776&quot;
        ///   }
        ///].
        /// </summary>
        internal static string ExpectedJSON_SalesOrderDynamicModel {
            get {
                return ResourceManager.GetString("ExpectedJSON_SalesOrderDynamicModel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;SalesOrderID&quot; : 1,
        ///      &quot;RevisionNumber&quot; : 2,
        ///      &quot;OrderDate&quot; : &quot;2008-06-01T00:00:00.000&quot;,
        ///      &quot;DueDate&quot; : &quot;2008-06-13T00:00:00.000&quot;,
        ///      &quot;ShipDate&quot; : &quot;2008-06-08T00:00:00.000&quot;,
        ///      &quot;Status&quot; : 5,
        ///      &quot;OnlineOrderFlag&quot; : false,
        ///      &quot;SalesOrderNumber&quot; : &quot;SO71774&quot;,
        ///      &quot;PurchaseOrderNumber&quot; : &quot;PO348186287&quot;,
        ///      &quot;AccountNumber&quot; : &quot;10-4020-000609&quot;,
        ///      &quot;CustomerID&quot; : 29847,
        ///      &quot;ShipToAddressID&quot; : 1092,
        ///      &quot;BillToAddressID&quot; : 1092,
        ///      &quot;ShipMethod&quot; : &quot;CARGO  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Mock_SalesOrder {
            get {
                return ResourceManager.GetString("Mock_SalesOrder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [
        ///   {
        ///      &quot;SalesOrderID&quot; : 1,
        ///      &quot;SalesOrderDetailID&quot; : 1,
        ///      &quot;OrderQty&quot; : 1,
        ///      &quot;ProductID&quot; : 836,
        ///      &quot;UnitPrice&quot; : 356.8980,
        ///      &quot;UnitPriceDiscount&quot; : 0.0000,
        ///      &quot;LineTotal&quot; : 356.8980,
        ///      &quot;RowGuid&quot; : &quot;e3a1994c-7a68-4ce8-96a3-77fdd3bbd730&quot;,
        ///      &quot;ModifiedDate&quot; : &quot;2008-06-01T00:00:00.000&quot;
        ///   },
        ///   {
        ///      &quot;SalesOrderID&quot; : 1,
        ///      &quot;SalesOrderDetailID&quot; : 2,
        ///      &quot;OrderQty&quot; : 1,
        ///      &quot;ProductID&quot; : 822,
        ///      &quot;UnitPrice&quot; : 356.8980,
        ///      &quot;UnitPriceDiscount&quot; : 0.000 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Mock_SalesOrderDetail {
            get {
                return ResourceManager.GetString("Mock_SalesOrderDetail", resourceCulture);
            }
        }
    }
}