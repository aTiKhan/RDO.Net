﻿using DevZest.Samples.AdventureWorksLT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
{
    [TestClass]
    public class ElementManagerTests : RowManagerTestsBase
    {
        #region Helpers
        private sealed class ConcreteElementManager : ElementManager
        {
            public ConcreteElementManager(DataSet dataSet)
                : base(new Template(), dataSet)
            {
            }
        }

        private static ElementManager MockElementManager(DataSet<ProductCategory> dataSet, int pinnedTop = 0, int pinnedBottom = 0)
        {
            return CreateElementManager(dataSet, (builder, model) => BuildTemplate(builder, model, pinnedTop, pinnedBottom));
        }

        private static void BuildTemplate(TemplateBuilder builder, ProductCategory _, int viewportTopMargin, int viewportBottomMargin)
        {
            builder.AddGridColumns("100", "100")
                .AddGridRows("100", "100", "100")
                .ViewportMargin(top: viewportTopMargin, bottom: viewportBottomMargin)
                .Orientation(Orientation.Vertical, 0)
                .BlockView((BlockView blockView) => blockView.InitializeElements(null))
                .RowView((RowView rowView) => rowView.RowPresenter.Initialize(null))
                [1, 0].BeginDataItem<TextBlock>().Bind((s, e) => e.Text = _.Name.DisplayName).End()
                [0, 1].BeginBlockItem<TextBlock>().Bind((s, e) => e.Text = s.Index.ToString()).End()
                [1, 1].BeginRowItem<TextBlock>().Bind((s, e) => e.Text = s.GetValue(_.Name)).End()
                [1, 2].BeginDataItem<TextBlock>(true).Bind((s, e) => e.Text = _.Name.DisplayName).End();
        }

        private static ElementManager CreateElementManager<T>(DataSet<T> dataSet, Action<TemplateBuilder, T> buildTemplateAction)
            where T : Model, new()
        {
            var result = new ConcreteElementManager(dataSet);
            using (var templateBuilder = new TemplateBuilder(result.Template, dataSet.Model))
            {
                buildTemplateAction(templateBuilder, dataSet._);
            }
            result.Initialize();
            result.InitializeElements(null);
            return result;
        }

        private static void Verify(TextBlock textBlock, TemplateItem templateItem, string text)
        {
            Assert.AreEqual(templateItem, textBlock.GetTemplateItem());
            Assert.AreEqual(text, textBlock.Text);
        }

        #endregion

        [TestMethod]
        public void ElementManager_Elements()
        {
            var dataSet = MockProductCategories(8, false);
            var _ = dataSet._;
            var elementManager = MockElementManager(dataSet);
            var template = elementManager.Template;
            var rows = elementManager.Rows;

            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockDimensions = 3;
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeFirstUnpinned(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizePrevUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeNextUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[6].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[7].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockDimensions = 2;
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeFirstUnpinned(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizePrevUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeNextUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeUnpinnedHead(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "2"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[4].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[5].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeUnpinnedTail(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeAll();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.ClearElements();
            Assert.IsNull(elementManager.Elements);
        }

        [TestMethod]
        public void ElementManager_RefreshElements()
        {
            var dataSet = MockProductCategories(8, false);
            var _ = dataSet._;
            var elementManager = MockElementManager(dataSet);
            var template = elementManager.Template;
            var rows = elementManager.Rows;

            elementManager.BlockViews.RealizeFirstUnpinned(1);
            dataSet._.Name[1] = "CHANGED NAME";
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], "CHANGED NAME"))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();
        }

        [TestMethod]
        public void ElementManager_Elements_Pinned()
        {
            var dataSet = MockProductCategories(19, false);
            var _ = dataSet._;
            var elementManager = MockElementManager(dataSet, 3, 2);
            var template = elementManager.Template;
            var rows = elementManager.Rows;

            elementManager.BlockViews.EnsureInitialized();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "18"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockDimensions = 2;
            elementManager.BlockViews.EnsureInitialized();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeFirstUnpinned(5);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "5"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[10].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[11].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizePrevUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "4"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[8].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[9].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "5"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[10].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[11].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.RealizeNextUnpinned();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "4"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[8].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[9].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "5"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[10].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[11].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "6"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[12].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[13].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeUnpinnedHead(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "5"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[10].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[11].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "6"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[12].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[13].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeUnpinnedTail(1);
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "0"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[0].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[1].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "1"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[2].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[3].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "5"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[10].GetValue(_.Name)))
                        .VerifyEof())
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[11].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((BlockView b) => b.Elements
                    .Verify((TextBlock y) => Verify(y, template.BlockItems[0], "9"))
                    .Verify((RowView r) => r.RowPresenter.Elements
                        .Verify((TextBlock t) => Verify(t, template.RowItemGroups[0][0], rows[18].GetValue(_.Name)))
                        .VerifyEof())
                    .VerifyEof())
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock t) => Verify(t, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.BlockViews.VirtualizeAll();
            elementManager.Elements
                .Verify((TextBlock t) => Verify(t, template.DataItems[0], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .Verify((TextBlock x) => Verify(x, template.DataItems[1], _.Name.DisplayName))
                .VerifyEof();

            elementManager.ClearElements();
            Assert.IsNull(elementManager.Elements);
        }
    }
}
