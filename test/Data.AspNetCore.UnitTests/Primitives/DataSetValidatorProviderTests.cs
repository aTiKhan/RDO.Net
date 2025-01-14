﻿using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetValidatorProviderTests
    {
        private readonly IModelMetadataProvider _metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

        public class Person : Model
        {
            static Person()
            {
                RegisterColumn((Person _) => _.FirstName);
                RegisterColumn((Person _) => _.LastName);
            }

            [Required]
            public _String FirstName { get; private set; }

            [Required]
            public _String LastName { get; private set; }
        }

        private static IList<ValidatorItem> GetValidatorItems(ModelMetadata metadata)
        {
            var items = new List<ValidatorItem>(metadata.ValidatorMetadata.Count);
            for (var i = 0; i < metadata.ValidatorMetadata.Count; i++)
                items.Add(new ValidatorItem(metadata.ValidatorMetadata[i]));

            return items;
        }

        [Fact]
        public void CreateValidators_ReturnsValidatorForDataSetType()
        {
            // Arrange
            var provider = new DataSetValidatorProvider();
            var dataSet = DataSet<Person>.Create();
            var metadata = _metadataProvider.GetMetadataForType(dataSet.GetType());

            var providerContext = new ModelValidatorProviderContext(metadata, GetValidatorItems(metadata));

            // Act
            provider.CreateValidators(providerContext);

            // Assert
            var validatorItem = Assert.Single(providerContext.Results);
            Assert.IsType<DataSetValidator>(validatorItem.Validator);
        }

        [Fact]
        public void HasValidators_ReturnsTrue_IfModelIsDataSet()
        {
            // Arrange
            var provider = new DataSetValidatorProvider();
            var dataSet = DataSet<Person>.Create();

            // Act
            var result = provider.HasValidators(dataSet.GetType(), Array.Empty<object>());

            // Assert
            Assert.True(result);
        }
    }
}
