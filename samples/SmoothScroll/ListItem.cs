﻿using DevZest.Data;
using System;

namespace SmoothScroll
{
    public class ListItem : Model
    {
        static ListItem()
        {
            RegisterLocalColumn((ListItem _) => _.Text);
            RegisterLocalColumn((ListItem _) => _.IsSectionHeader);
            RegisterLocalColumn((ListItem _) => _.BackgroundR);
            RegisterLocalColumn((ListItem _) => _.BackgroundG);
            RegisterLocalColumn((ListItem _) => _.BackgroundB);
        }

        public LocalColumn<string> Text { get; private set; }

        public LocalColumn<bool> IsSectionHeader { get; private set; }

        public LocalColumn<byte> BackgroundR { get; private set; }

        public LocalColumn<byte> BackgroundG { get; private set; }

        public LocalColumn<byte> BackgroundB { get; private set; }

        #region Mock

        private static void AddSectionHeader(DataSet<ListItem> dataSet, int index)
        {
            dataSet.AddRow((_, dataRow) =>
            {
                _.Text[dataRow] = "Section " + index;
                _.IsSectionHeader[dataRow] = true;
            });
        }

        private static void AddItem(DataSet<ListItem> dataSet, string text, byte r, byte g, byte b)
        {
            dataSet.AddRow((_, dataRow) =>
            {
                _.Text[dataRow] = text;
                _.IsSectionHeader[dataRow] = false;
                _.BackgroundR[dataRow] = r;
                _.BackgroundG[dataRow] = g;
                _.BackgroundB[dataRow] = b;
            });
        }

        public static DataSet<ListItem> Mock(int count)
        {
            var result = DataSet<ListItem>.Create();

            const string LoremIpsumText =
@"Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium,
totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt,
explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit,
sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est,
qui dolorem ipsum, quia dolor sit, amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt,
ut labore et dolore magnam aliquam quaerat voluptatem.";

            Random _rnd = new Random(0);

            for (int i = 0; i < count; i++)
            {
                if (i % 10 == 0)
                    AddSectionHeader(result, i / 10 + 1);

                var text = LoremIpsumText.Substring(0, _rnd.Next(LoremIpsumText.Length));

                // Add two very big items
                if (i == count - 2 || i == count / 2)
                {
                    for (int j = 0; j < 200; j++)
                        text += "\r\nLine " + j;
                }

                var r = (byte)(240 - _rnd.Next(50));
                var g = (byte)(240 - _rnd.Next(50));
                var b = (byte)(240 - _rnd.Next(50));
                AddItem(result, text, r, g, b);
            }
            return result;
        }

        #endregion
    }
}
