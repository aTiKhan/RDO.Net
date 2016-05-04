﻿using System;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    internal sealed class GridColumnCollection : GridTrackCollection<GridColumn>
    {
        internal GridColumnCollection(Template template)
            : base(template)
        {
        }

        public override Orientation Orientation
        {
            get { return Orientation.Horizontal; }
        }

        protected override bool SizeToContent
        {
            get { return Template.SizeToContentX; }
        }

        protected override double AvailableLength
        {
            get { return Template.AvailableWidth; }
        }
    }
}
