// <auto-generated />
namespace DevZest.Data
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class UserMessages
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("DevZest.Data.UserMessages", typeof(UserMessages).GetTypeInfo().Assembly);

        /// <summary>
        /// Retry
        /// </summary>
        public static string AsyncValidationFaultControlCommands_RetryCommandText
        {
            get { return GetString("AsyncValidationFaultControlCommands_RetryCommandText"); }
        }

        /// <summary>
        /// An error occurs while validating {asynValidatorDisplayName}: {exceptionMessage}.
        /// </summary>
        public static string AsyncValidationFault_FormatMessage(object asynValidatorDisplayName, object exceptionMessage)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("AsyncValidationFault_FormatMessage", "asynValidatorDisplayName", "exceptionMessage"), asynValidatorDisplayName, exceptionMessage);
        }

        /// <summary>
        /// _Sort...
        /// </summary>
        public static string ColumnHeaderCommands_SortCommandText
        {
            get { return GetString("ColumnHeaderCommands_SortCommandText"); }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        public static string DataViewCommands_CancelDataLoadCommandText
        {
            get { return GetString("DataViewCommands_CancelDataLoadCommandText"); }
        }

        /// <summary>
        /// Retry
        /// </summary>
        public static string DataViewCommands_RetryDataLoadCommandText
        {
            get { return GetString("DataViewCommands_RetryDataLoadCommandText"); }
        }

        /// <summary>
        /// The operation has been cancelled.
        /// </summary>
        public static string DataView_DataLoadCancelled
        {
            get { return GetString("DataView_DataLoadCancelled"); }
        }

        /// <summary>
        /// Cancelling...
        /// </summary>
        public static string DataView_DataLoadCancelling
        {
            get { return GetString("DataView_DataLoadCancelling"); }
        }

        /// <summary>
        /// We're sorry, an error occured while loading data.
        /// </summary>
        public static string DataView_DataLoadFailed
        {
            get { return GetString("DataView_DataLoadFailed"); }
        }

        /// <summary>
        /// Collapse
        /// </summary>
        public static string RowViewCommands_CollapseCommandText
        {
            get { return GetString("RowViewCommands_CollapseCommandText"); }
        }

        /// <summary>
        /// Expand
        /// </summary>
        public static string RowViewCommands_ExpandCommandText
        {
            get { return GetString("RowViewCommands_ExpandCommandText"); }
        }

        /// <summary>
        /// Duplicate Sort By is not allowed.
        /// </summary>
        public static string Sorting_DuplicateSortBy
        {
            get { return GetString("Sorting_DuplicateSortBy"); }
        }

        /// <summary>
        /// Field '{input}' is required.
        /// </summary>
        public static string Sorting_InputRequired(object input)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Sorting_InputRequired", "input"), input);
        }

        /// <summary>
        /// Order
        /// </summary>
        public static string Sorting_Order
        {
            get { return GetString("Sorting_Order"); }
        }

        /// <summary>
        /// Sort By
        /// </summary>
        public static string Sorting_SortBy
        {
            get { return GetString("Sorting_SortBy"); }
        }

        /// <summary>
        /// Delete Level
        /// </summary>
        public static string SortWindow_DeleteLevel
        {
            get { return GetString("SortWindow_DeleteLevel"); }
        }

        /// <summary>
        /// Ascending
        /// </summary>
        public static string SortWindow_EnumAscending
        {
            get { return GetString("SortWindow_EnumAscending"); }
        }

        /// <summary>
        /// Descending
        /// </summary>
        public static string SortWindow_EnumDescending
        {
            get { return GetString("SortWindow_EnumDescending"); }
        }

        /// <summary>
        /// Sort
        /// </summary>
        public static string SortWindow_Title
        {
            get { return GetString("SortWindow_Title"); }
        }

        /// <summary>
        /// Validated
        /// </summary>
        public static string Validation_Validated
        {
            get { return GetString("Validation_Validated"); }
        }

        /// <summary>
        /// Validating...
        /// </summary>
        public static string Validation_Validating
        {
            get { return GetString("Validation_Validating"); }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        public static string _Cancel
        {
            get { return GetString("_Cancel"); }
        }

        /// <summary>
        /// OK
        /// </summary>
        public static string _OK
        {
            get { return GetString("_OK"); }
        }

        /// <summary>
        /// Paste Append
        /// </summary>
        public static string PasteAppendWindow_Title
        {
            get { return GetString("PasteAppendWindow_Title"); }
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
