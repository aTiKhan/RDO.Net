﻿using DevZest.Data.Annotations;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers.Testing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    public class InputTagHelperTests
    {
        private class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text);
                RegisterChildModel((TestModel _) => _.NestedModel);
                RegisterColumn((TestModel _) => _.IsACar);
                RegisterColumn((TestModel _) => _.Date);
                RegisterColumn((TestModel _) => _.DateTime);
            }

            public TestModel()
            {
                Date.LogicalDataType = LogicalDataType.Date;
            }

            public _String Text { get; private set; }

            public NestedModel NestedModel { get; private set; }

            [Required]
            [DefaultValue(false)]
            public _Boolean IsACar { get; private set; }

            public _DateTime Date { get; private set; }

            public _DateTime DateTime { get; private set; }
        }

        private class NestedModel : Model
        {
            static NestedModel()
            {
                RegisterColumn((NestedModel _) => _.Text);
            }

            public _String Text { get; private set; }
        }


        private static InputTagHelper GetTagHelper(Column column, bool isScalar = true, DataRow dataRow = null, IDataSetHtmlGenerator generator = null, IModelMetadataProvider metadataProvider = null)
        {
            return Factory.GetTagHelper(column, isScalar, dataRow, generator, metadataProvider, g => new InputTagHelper(g));
        }

        public static TheoryData<TagHelperAttributeList, string> MultiAttributeCheckBoxData
        {
            get
            {
                // outputAttributes, expectedAttributeString
                return new TheoryData<TagHelperAttributeList, string>
                {
                    {
                        new TagHelperAttributeList
                        {
                            { "hello", "world" },
                            { "hello", "world2" }
                        },
                        "hello=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "hello", "world" },
                            { "hello", "world2" },
                            { "hello", "world3" }
                        },
                        "hello=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\" hello=\"HtmlEncode[[world3]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "HelLO", "world" },
                            { "HELLO", "world2" }
                        },
                        "HelLO=\"HtmlEncode[[world]]\" HELLO=\"HtmlEncode[[world2]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "Hello", "world" },
                            { "HELLO", "world2" },
                            { "hello", "world3" }
                        },
                        "Hello=\"HtmlEncode[[world]]\" HELLO=\"HtmlEncode[[world2]]\" hello=\"HtmlEncode[[world3]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "HeLlO", "world" },
                            { "hello", "world2" }
                        },
                        "HeLlO=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\""
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MultiAttributeCheckBoxData))]
        public async Task CheckBoxHandlesMultipleAttributesSameNameArePreserved(
            TagHelperAttributeList outputAttributes,
            string expectedAttributeString)
        {
            // Arrange
            var originalContent = "original content";
            var expectedContent = $"<input {expectedAttributeString} type=\"HtmlEncode[[checkbox]]\" data-val=\"HtmlEncode[[true]]\" " +
                "data-val-required=\"HtmlEncode[[Value is required for field 'IsACar'.]]\" id=\"HtmlEncode[[DataSet_IsACar]]\" " +
                $"name=\"HtmlEncode[[DataSet.IsACar]]\" value=\"HtmlEncode[[true]]\" />" +
                "<input name=\"HtmlEncode[[DataSet.IsACar]]\" type=\"HtmlEncode[[hidden]]\" value=\"HtmlEncode[[false]]\" />";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "input",
                outputAttributes,
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            output.Content.AppendHtml(originalContent);
            var dataSet = DataSet<TestModel>.Create();
            var tagHelper = GetTagHelper(dataSet._.IsACar);

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.NotNull(output.PostElement);
            Assert.Equal(originalContent, HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output));
        }

        [Theory]
        [InlineData("hidden")]
        [InlineData("number")]
        [InlineData("text")]
        public void Process_WithInputTypeName(string inputTypeName)
        {
            // Arrange
            var expectedTagName = "input";
            var attributes = new TagHelperAttributeList
            {
                { "type", inputTypeName }
            };

            var context = new TagHelperContext(attributes, new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                expectedTagName,
                new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            var dataSet = DataSet<TestModel>.Create();
            var tagHelper = GetTagHelper(dataSet._.IsACar);
            tagHelper.ViewContext.ClientValidationEnabled = false;
            tagHelper.InputTypeName = inputTypeName;

            // Act
            tagHelper.Process(context, output);

            // Assert
            var expectedAttributes = new TagHelperAttributeList
            {
                { "type", inputTypeName },
                { "id", "DataSet_IsACar" },
                { "name", "DataSet.IsACar" },
                { "value", "" }
            };

            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [InlineData("datetime", "datetime")]
        [InlineData(null, "datetime-local")]
        [InlineData("hidden", "hidden")]
        public void Process_GeneratesFormattedOutput_ForDateTime(string specifiedType, string expectedType)
        {
            // Arrange
            var expectedTagName = "not-input";

            var allAttributes = new TagHelperAttributeList
            {
                { "type", specifiedType },
            };
            var context = new TagHelperContext(allAttributes, new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                expectedTagName,
                new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.DateTime[0] = new DateTime(2011, 8, 31, hour: 5, minute: 30, second: 45, kind: DateTimeKind.Utc);
            var tagHelper = GetTagHelper(dataSet._.DateTime);
            tagHelper.Format = "datetime: {0:o}";
            tagHelper.InputTypeName = specifiedType;

            // Act
            tagHelper.Process(context, output);

            // Assert
            var expectedAttributes = new TagHelperAttributeList
            {
                { "type", expectedType },
                { "id", "DataSet_DateTime" },
                { "name", "DataSet.DateTime" },
                { "value", "datetime: 2011-08-31T05:30:45.0000000Z" },
            };
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Empty(output.PreContent.GetContent());
            Assert.Empty(output.Content.GetContent());
            Assert.Empty(output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsGenerateCheckBox_WithExpectedParameters()
        {
            // Arrange
            var originalContent = "original content";
            var expectedPreContent = "original pre-content";
            var expectedContent = "<input class=\"HtmlEncode[[form-control]]\" type=\"HtmlEncode[[checkbox]]\" /><hidden />";
            var expectedPostContent = "original post-content";
            var expectedPostElement = "<hidden />";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                "input",
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.SelfClosing,
            };
            output.PreContent.AppendHtml(expectedPreContent);
            output.Content.AppendHtml(originalContent);
            output.PostContent.AppendHtml(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.IsACar[0] = false;
            var tagHelper = GetTagHelper(dataSet._.IsACar, generator: generator.Object);
            tagHelper.Format = "somewhat-less-null"; // ignored

            var tagBuilder = new TagBuilder("input")
            {
                TagRenderMode = TagRenderMode.SelfClosing
            };
            generator
                .Setup(mock => mock.GenerateCheckBox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    false,                   // isChecked
                    It.IsAny<object>()))    // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();
            generator
                .Setup(mock => mock.GenerateHiddenForCheckbox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName))
                .Returns(new TagBuilder("hidden") { TagRenderMode = TagRenderMode.SelfClosing })
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.NotEmpty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(originalContent, HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output));
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedPostElement, output.PostElement.GetContent());
            Assert.Equal(TagMode.SelfClosing, output.TagMode);
        }

        [Theory]
        [InlineData("hidden", null, null)]
        [InlineData("Hidden", "not-null", "somewhat-less-null")]
        [InlineData("HIDden", null, "somewhat-less-null")]
        [InlineData("HIDDEN", "not-null", null)]
        public async Task ProcessAsync_CallsGenerateTextBox_WithExpectedParametersForHidden(
            string inputTypeName,
            string value,
            string format)
        {
            // Arrange
            var contextAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            if (!string.IsNullOrEmpty(inputTypeName))
            {
                contextAttributes.SetAttribute("type", inputTypeName);  // Support restoration of type attribute, if any.
            }

            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control hidden-control" },
                { "type", inputTypeName ?? "hidden" },      // Generator restores type attribute; adds "hidden" if none.
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var expectedTagName = "not-input";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: contextAttributes,
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                expectedTagName,
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.StartTagOnly,
            };
            output.PreContent.SetContent(expectedPreContent);
            output.Content.SetContent(expectedContent);
            output.PostContent.SetContent(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.Text[0] = value;
            var tagHelper = GetTagHelper(dataSet._.Text, generator: generator.Object);
            tagHelper.Format = format;
            tagHelper.InputTypeName = inputTypeName;

            var tagBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    { "class", "hidden-control" },
                },
            };
            generator
                .Setup(mock => mock.GenerateTextBox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    value,  // value
                    format,
                    new Dictionary<string, object> { { "type", "hidden" } }))   // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal(TagMode.StartTagOnly, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes, CaseSensitiveTagHelperAttributeComparer.Default);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(expectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [InlineData("password", null)]
        [InlineData("Password", "not-null")]
        [InlineData("PASSword", null)]
        [InlineData("PASSWORD", "not-null")]
        public async Task ProcessAsync_CallsGeneratePassword_WithExpectedParameters(
            string inputTypeName,
            string value)
        {
            // Arrange
            var contextAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            if (!string.IsNullOrEmpty(inputTypeName))
            {
                contextAttributes.SetAttribute("type", inputTypeName);  // Support restoration of type attribute, if any.
            }

            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control password-control" },
                { "type", inputTypeName ?? "password" },    // Generator restores type attribute; adds "password" if none.
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var expectedTagName = "not-input";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: contextAttributes,
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                expectedTagName,
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.StartTagOnly,
            };
            output.PreContent.SetContent(expectedPreContent);
            output.Content.SetContent(expectedContent);
            output.PostContent.SetContent(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.Text[0] = value;
            var tagHelper = GetTagHelper(dataSet._.Text, generator: generator.Object);
            tagHelper.Format = "somewhat-less-null"; // ignored
            tagHelper.InputTypeName = inputTypeName;

            var tagBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    { "class", "password-control" },
                },
            };
            generator
                .Setup(mock => mock.GeneratePassword(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    null,       // value
                    null))      // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal(TagMode.StartTagOnly, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes, CaseSensitiveTagHelperAttributeComparer.Default);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(expectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [InlineData("radio", null)]
        [InlineData("Radio", "not-null")]
        [InlineData("RADio", null)]
        [InlineData("RADIO", "not-null")]
        public async Task ProcessAsync_CallsGenerateRadioButton_WithExpectedParameters(
            string inputTypeName,
            string model)
        {
            // Arrange
            var value = "match";            // Real generator would use this for comparison with For.Metadata.Model.
            var contextAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
                { "value", value },
            };
            if (!string.IsNullOrEmpty(inputTypeName))
            {
                contextAttributes.SetAttribute("type", inputTypeName);  // Support restoration of type attribute, if any.
            }

            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control radio-control" },
                { "value", value },
                { "type", inputTypeName ?? "radio" },       // Generator restores type attribute; adds "radio" if none.
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var expectedTagName = "not-input";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: contextAttributes,
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                expectedTagName,
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.StartTagOnly,
            };
            output.PreContent.SetContent(expectedPreContent);
            output.Content.SetContent(expectedContent);
            output.PostContent.SetContent(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.Text[0] = model;
            var tagHelper = GetTagHelper(dataSet._.Text, generator: generator.Object);
            tagHelper.Format = "somewhat-less-null"; // ignored
            tagHelper.InputTypeName = inputTypeName;
            tagHelper.Value = value;

            var tagBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    { "class", "radio-control" },
                },
            };
            generator
                .Setup(mock => mock.GenerateRadioButton(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    value,
                    false,       // isChecked
                    null))      // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal(TagMode.StartTagOnly, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes, CaseSensitiveTagHelperAttributeComparer.Default);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(expectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [InlineData(null, null, "somewhat-less-null")]
        [InlineData(null, "not-null", null)]
        [InlineData("string", null, null)]
        [InlineData("String", "not-null", null)]
        [InlineData("STRing", null, "somewhat-less-null")]
        [InlineData("STRING", "not-null", null)]
        [InlineData("text", null, null)]
        [InlineData("Text", "not-null", "somewhat-less-null")]
        [InlineData("TExt", null, null)]
        [InlineData("TEXT", "not-null", null)]
        [InlineData("image", "not-null", null)]
        public async Task ProcessAsync_CallsGenerateTextBox_WithExpectedParameters(
            string inputTypeName,
            string model,
            string format)
        {
            // Arrange
            var contextAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            if (!string.IsNullOrEmpty(inputTypeName))
            {
                contextAttributes.SetAttribute("type", inputTypeName);  // Support restoration of type attribute, if any.
            }

            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control text-control" },
                { "type", inputTypeName ?? "text" },        // Generator restores type attribute; adds "text" if none.
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var expectedTagName = "not-input";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: contextAttributes,
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                expectedTagName,
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.StartTagOnly,
            };
            output.PreContent.SetContent(expectedPreContent);
            output.Content.SetContent(expectedContent);
            output.PostContent.SetContent(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.Text[0] = model;
            var tagHelper = GetTagHelper(dataSet._.Text, generator: generator.Object);
            tagHelper.Format = format;
            tagHelper.InputTypeName = inputTypeName;

            var tagBuilder = new TagBuilder("input")
            {
                Attributes =
                {
                    { "class", "text-control" },
                },
            };
            generator
                .Setup(mock => mock.GenerateTextBox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    model,  // value
                    format,
                    It.Is<Dictionary<string, object>>(m => m.ContainsKey("type"))))     // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal(TagMode.StartTagOnly, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes, CaseSensitiveTagHelperAttributeComparer.Default);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(expectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsGenerateTextBox_InputTypeDateTime_RendersAsDateTime()
        {
            // Arrange
            var expectedAttributes = new TagHelperAttributeList
            {
                { "type", "datetime" },                   // Calculated; not passed to HtmlGenerator.
            };
            var expectedTagName = "not-input";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: new TagHelperAttributeList()
                {
                    {"type", "datetime" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()))
            {
                TagMode = TagMode.SelfClosing,
            };

            var htmlAttributes = new Dictionary<string, object>
            {
                { "type", "datetime" }
            };

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            var tagHelper = GetTagHelper(dataSet._.DateTime, generator: generator.Object);
            tagHelper.ViewContext.Html5DateRenderingMode = Html5DateRenderingMode.Rfc3339;
            tagHelper.InputTypeName = "datetime";
            var tagBuilder = new TagBuilder("input");
            generator
                .Setup(mock => mock.GenerateTextBox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    null,                                   // value
                    @"{0:yyyy-MM-ddTHH\:mm\:ss.fffK}",
                    htmlAttributes))                    // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal(TagMode.SelfClosing, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Empty(output.PreContent.GetContent());
            Assert.Equal(string.Empty, output.Content.GetContent());
            Assert.Empty(output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }
    }
}
