﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Samples.AdventureWorksLT {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UserMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UserMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Samples.AdventureWorksLT.UserMessages", typeof(UserMessages).Assembly);
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
        ///   Looks up a localized string similar to ListPrice cannot be negative value..
        /// </summary>
        internal static string CK_Product_ListPrice {
            get {
                return ResourceManager.GetString("CK_Product_ListPrice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SellEndDate cannot be earlier than SellStartDate..
        /// </summary>
        internal static string CK_Product_SellEndDate {
            get {
                return ResourceManager.GetString("CK_Product_SellEndDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to StandardCost cannot be negative value..
        /// </summary>
        internal static string CK_Product_StandardCost {
            get {
                return ResourceManager.GetString("CK_Product_StandardCost", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Weight cannot be negative value..
        /// </summary>
        internal static string CK_Product_Weight {
            get {
                return ResourceManager.GetString("CK_Product_Weight", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to OrderQty must be greater than zero..
        /// </summary>
        internal static string CK_SalesOrderDetail_OrderQty {
            get {
                return ResourceManager.GetString("CK_SalesOrderDetail_OrderQty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UnitPrice cannot be negative..
        /// </summary>
        internal static string CK_SalesOrderDetail_UnitPrice {
            get {
                return ResourceManager.GetString("CK_SalesOrderDetail_UnitPrice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UnitPriceDiscount cannot be negative..
        /// </summary>
        internal static string CK_SalesOrderDetail_UnitPriceDiscount {
            get {
                return ResourceManager.GetString("CK_SalesOrderDetail_UnitPriceDiscount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DueDate cannot be earlier than OrderDate..
        /// </summary>
        internal static string CK_SalesOrderHeader_DueDate {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_DueDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Freight cannot be a negtive value..
        /// </summary>
        internal static string CK_SalesOrderHeader_Freight {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_Freight", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ShipDate cannot be earlier than OrderDate..
        /// </summary>
        internal static string CK_SalesOrderHeader_ShipDate {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_ShipDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Status value..
        /// </summary>
        internal static string CK_SalesOrderHeader_Status {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_Status", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SubTotal cannot be a negative value..
        /// </summary>
        internal static string CK_SalesOrderHeader_SubTotal {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_SubTotal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TaxAmt cannot be a negtive value..
        /// </summary>
        internal static string CK_SalesOrderHeader_TaxAmt {
            get {
                return ResourceManager.GetString("CK_SalesOrderHeader_TaxAmt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sales order must have detail line(s)..
        /// </summary>
        internal static string Validation_SalesOrder_LineCount {
            get {
                return ResourceManager.GetString("Validation_SalesOrder_LineCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value is required for field &apos;{0}&apos;..
        /// </summary>
        internal static string Validation_ValueIsRequired {
            get {
                return ResourceManager.GetString("Validation_ValueIsRequired", resourceCulture);
            }
        }
    }
}
