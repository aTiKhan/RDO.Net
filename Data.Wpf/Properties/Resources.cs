﻿// <auto-generated />

namespace DevZest.Data.Wpf.Resources
{
	using System;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Resources;
    using System.Reflection;
    using System.Threading;

    // <summary>
    // Strongly-typed and parameterized string resources.
    // </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal static class Strings
    {
        // <summary>
        // A string like "Cannot inherit grids after any GridRow or GridColumn added."
        // </summary>
        internal static string DataSetControl_InheritGrids
        {
            get { return ResourceLoader.GetString(ResourceLoader.DataSetControl_InheritGrids); }
        }

        // <summary>
        // A string like "The GridRow or GridColumn is invalid. It does not belong to this DataSetControl."
        // </summary>
        internal static string DataSetControl_InvalidGridDefinition
        {
            get { return ResourceLoader.GetString(ResourceLoader.DataSetControl_InvalidGridDefinition); }
        }

        // <summary>
        // A string like "Cannot add GridRow or GridColumn after grids inherited."
        // </summary>
        internal static string DataSetControl_VerifyAreGridsInherited
        {
            get { return ResourceLoader.GetString(ResourceLoader.DataSetControl_VerifyAreGridsInherited); }
        }
    }

    // <summary>
    // Strongly-typed and parameterized exception factory.
    // </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal static class Error
    {
        // <summary>
        // InvalidOperationException with message like "Cannot inherit grids after any GridRow or GridColumn added."
        // </summary>
        internal static Exception DataSetControl_InheritGrids()
        {
            return new InvalidOperationException(Strings.DataSetControl_InheritGrids);
        }

        // <summary>
        // InvalidOperationException with message like "Cannot add GridRow or GridColumn after grids inherited."
        // </summary>
        internal static Exception DataSetControl_VerifyAreGridsInherited()
        {
            return new InvalidOperationException(Strings.DataSetControl_VerifyAreGridsInherited);
        }

        // <summary>
        // The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.
        // </summary>
        internal static Exception Argument(string message, string paramName)
        {
            return new ArgumentException(message, paramName);
        }

        // <summary>
        // The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.
        // </summary>
        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }
    }

    // <summary>
    // AutoGenerated resource class. Usage:
    // string s = ResourceLoader.GetString(ResourceLoader.MyIdenfitier);
    // </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal sealed class ResourceLoader
    {
        internal const string DataSetControl_InheritGrids = "DataSetControl_InheritGrids";
        internal const string DataSetControl_InvalidGridDefinition = "DataSetControl_InvalidGridDefinition";
        internal const string DataSetControl_VerifyAreGridsInherited = "DataSetControl_VerifyAreGridsInherited";

        private static ResourceLoader loader;
        private readonly ResourceManager resources;

        private ResourceLoader()
        {
            resources = new ResourceManager("DevZest.Data.Wpf.Properties.Resources", typeof(ResourceLoader).GetTypeInfo().Assembly);
        }

        private static ResourceLoader GetLoader()
        {
            if (loader == null)
            {
                var sr = new ResourceLoader();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture
        {
            get { return null /*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        public static ResourceManager Resources
        {
            get { return GetLoader().resources; }
        }

        public static string GetString(string name, params object[] args)
        {
            var sys = GetLoader();
            if (sys == null)
            {
                return null;
            }

            var res = sys.resources.GetString(name, Culture);

            if (args != null
                && args.Length > 0)
            {
                for (var i = 0; i < args.Length; i ++)
                {
                    var value = args[i] as String;
                    if (value != null
                        && value.Length > 1024)
                    {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }

        public static string GetString(string name)
        {
            var sys = GetLoader();
            if (sys == null)
            {
                return null;
            }
            return sys.resources.GetString(name, Culture);
        }
    }
}
