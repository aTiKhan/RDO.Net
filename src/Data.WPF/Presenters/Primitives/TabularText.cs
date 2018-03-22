﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public sealed class TabularText : Model
    {
        public static bool CanPasteFromClipboard
        {
            get { return Clipboard.ContainsData(DataFormats.CommaSeparatedValue) || Clipboard.ContainsText(); }
        }

        public static DataSet<TabularText> PasteFromClipboard()
        {
            var csv = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            if (csv != null)
                return Parse(csv, ',');

            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
                return Parse(text, '\t');

            return null;
        }

        public static DataSet<TabularText> Parse(string s, char delimiter)
        {
            Check.NotNull(s, nameof(s));
            using (var textReader = new StringReader(s))
            {
                return Parse(textReader, delimiter);
            }
        }

        public static DataSet<TabularText> Parse(TextReader reader, char delimiter)
        {
            Check.NotNull(reader, nameof(reader));

            var result = DataSet<TabularText>.New();
            var _ = result._;

            bool? inQuote = null;   // three states to distinguish between null and string.Empty
            DataRow currentRow = null;
            var currentField = 0;
            var sb = new StringBuilder();

            while (reader.Peek() != -1)
            {
                if (currentRow == null)
                {
                    currentRow = new DataRow();
                    result.Add(currentRow);
                }

                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote == true)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        _.AddValue(currentRow, currentField++, sb, ref inQuote);
                        currentRow = null;
                        currentField = 0;
                    }
                }
                else if (sb.Length == 0 && inQuote != true)
                {
                    if (readChar == '"')
                        inQuote = true;
                    else if (readChar == delimiter)
                        _.AddValue(currentRow, currentField++, sb, ref inQuote);
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote == true)
                        sb.Append(delimiter);
                    else
                        _.AddValue(currentRow, currentField++, sb, ref inQuote);
                }
                else if (readChar == '"')
                {
                    if (inQuote == true)
                    {
                        if ((char)reader.Peek() == '"') // escaped quote
                        {
                            reader.Read();
                            sb.Append('"');
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            return result;
        }

        private readonly List<Column<string>> _textColumns = new List<Column<string>>();
        public IReadOnlyList<Column<string>> TextColumns
        {
            get { return _textColumns; }
        }

        private void AddValue(DataRow dataRow, int fieldIndex, StringBuilder sb, ref bool? inQuote)
        {
            Debug.Assert(dataRow.Index == DataSet.Count - 1);
            Debug.Assert(fieldIndex >= 0 && fieldIndex <= TextColumns.Count);
            Debug.Assert(inQuote != true);

            if (fieldIndex == TextColumns.Count)
                _textColumns.Add(CreateLocalColumn<string>());

            if (sb.Length > 0)
            {
                var value = sb.ToString();
                TextColumns[fieldIndex][dataRow] = value;
                sb.Clear();
            }
            else if (inQuote.HasValue)
                TextColumns[fieldIndex][dataRow] = string.Empty;

            inQuote = null;
        }
    }
}
