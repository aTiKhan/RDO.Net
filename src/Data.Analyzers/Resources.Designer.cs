﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DevZest.Data.Analyzers {
    using System;
    using System.Reflection;
    
    
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DevZest.Data.Analyzers.Resources", typeof(Resources).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to Duplicate mounter registration for property {0}..
        /// </summary>
        internal static string MounterRegistration_Duplicate_Message {
            get {
                return ResourceManager.GetString("MounterRegistration_Duplicate_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate mounter registration..
        /// </summary>
        internal static string MounterRegistration_Duplicate_Title {
            get {
                return ResourceManager.GetString("MounterRegistration_Duplicate_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The getter parameter must be a lambda expression which returns a non-readonly property of current class..
        /// </summary>
        internal static string MounterRegistration_InvalidGetter_Message {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidGetter_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid getter of mounter registration..
        /// </summary>
        internal static string MounterRegistration_InvalidGetter_Title {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidGetter_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter registration is only allowed as static field initializer or static constructor statement..
        /// </summary>
        internal static string MounterRegistration_InvalidInvocation_Message {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidInvocation_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid invocation of mounter registration..
        /// </summary>
        internal static string MounterRegistration_InvalidInvocation_Title {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidInvocation_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local column {0} is invalid for RegisterColumn..
        /// </summary>
        internal static string MounterRegistration_InvalidLocalColumn_Message {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidLocalColumn_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Local column is invalid for RegisterColumn..
        /// </summary>
        internal static string MounterRegistration_InvalidLocalColumn_Title {
            get {
                return ResourceManager.GetString("MounterRegistration_InvalidLocalColumn_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter {0} of property {1} does conform to naming convention as {2}..
        /// </summary>
        internal static string MounterRegistration_MounterNaming_Message {
            get {
                return ResourceManager.GetString("MounterRegistration_MounterNaming_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mounter should conform to naming convention..
        /// </summary>
        internal static string MounterRegistration_MounterNaming_Title {
            get {
                return ResourceManager.GetString("MounterRegistration_MounterNaming_Title", resourceCulture);
            }
        }
    }
}
