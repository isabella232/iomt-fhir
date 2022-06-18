﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Health.Logging.Telemetry;
using Microsoft.Health.Tests.Common;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;

namespace Microsoft.Health.Fhir.Ingest.Template
{
    public class CollectionContentTemplateFactoryTests
    {
        private CollectionContentTemplateFactory _collectionContentTemplateFactory;

        public CollectionContentTemplateFactoryTests()
        {
            var logger = Substitute.For<ITelemetryLogger>();

            _collectionContentTemplateFactory = new CollectionContentTemplateFactory(
                new JsonPathContentTemplateFactory(),
                new IotJsonPathContentTemplateFactory(),
                new IotCentralJsonPathContentTemplateFactory(),
                new CalculatedFunctionContentTemplateFactory(new TemplateExpressionEvaluatorFactory(), logger));
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmpty.json")]
        public void GivenEmptyConfig_WhenCreate_ThenInvalidTemplateException_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateEmptyWithType.json")]
        public void GivenEmptyTemplateCollection_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            templateContext.EnsureValid();

            IEnumerable<ValidationResult> validationResult = templateContext.Validate(new ValidationContext(templateContext));
            Assert.Empty(validationResult);
            Assert.NotNull(templateContext.Template);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMultipleMocks.json")]
        public void GivenInputWithMatchingFactories_WhenCreate_ThenTemplateReturned_Test(string json)
        {
            IContentTemplate nullReturn = null;

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryB = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockB"))).Returns(nullReturn);

            var factory = new CollectionContentTemplateFactory(factoryA, factoryB);
            var templateContext = factory.Create(json);

            Assert.NotNull(templateContext);

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMultipleMocks.json")]
        public void GivenInputWithNoMatchingFactories_WhenCreate_ThenException_Test(string json)
        {
            IContentTemplate nullReturn = null;

            var factoryA = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryA.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockA"))).Returns(nullReturn);

            var factoryB = Substitute.For<ITemplateFactory<TemplateContainer, IContentTemplate>>();
            factoryB.Create(Arg.Is<TemplateContainer>(v => !v.MatchTemplateName("mockB"))).Returns(nullReturn);

            var factory = new CollectionContentTemplateFactory(factoryA, factoryB);
            factory.Create(json);

            factoryA.ReceivedWithAnyArgs().Create(null);
            factoryB.ReceivedWithAnyArgs().Create(null);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMixed.json")]
        public void GivenInputWithMultipleTemplates_WhenCreate_ThenTemplateReturn_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.True(templateContext.IsValid(out _));
            templateContext.EnsureValid();
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateInvalid.json")]
        public void GivenInvalidTemplateCollection_WhenCreate_ThenValidationShouldFail_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string errors));
            Assert.Contains("Required property 'DeviceIdExpression' not found in JSON.", errors);
            Assert.Contains("Required property 'TimestampExpression' not found in JSON.", errors);
            Assert.Contains("Required property 'TypeMatchExpression' not found in JSON.", errors);
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMixedValidity.json")]
        public void GivenMixedValidityTemplateCollection_WhenCreate_ItShouldWork_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));

            var token = JToken.FromObject(new { heartrate = "60", device = "myHrDevice", date = DateTime.UtcNow });
            var measurements = templateContext.Template.GetMeasurements(token);

            Assert.Collection(
                measurements,
                m =>
                {
                    Assert.Equal("heartrate", m.Type);
                    Assert.Equal("myHrDevice", m.DeviceId);
                    Assert.Collection(m.Properties, p =>
                    {
                        Assert.Equal("hr-calc-content", p.Name);
                        Assert.Equal("60", p.Value);
                    });
                },
                m =>
                {
                    Assert.Equal("heartrate", m.Type);
                    Assert.Equal("myHrDevice", m.DeviceId);
                    Assert.Collection(m.Properties, p =>
                    {
                        Assert.Equal("hr", p.Name);
                        Assert.Equal("60", p.Value);
                    });
                });
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidJson.txt")]
        public void GivenBadInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidTemplateType.json")]
        public void GivenMismatchedTemplateTypeInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected TemplateType value CollectionContentTemplate", error);
        }

        [Theory]
        [FileData(@"TestInput/data_InvalidCollectionContentTemplateWithNoTemplateArray.json")]
        public void GivenNoTemplateArrayInputJson_WhenCreate_ThenValidationFailed_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out string error));
            Assert.Throws<ValidationException>(() => templateContext.EnsureValid());
            Assert.Contains("Expected an array for the template property value for template type CollectionContentTemplate.", error);
        }

        [Theory]
        [FileData(@"TestInput/data_CollectionContentTemplateMixedValidity.json")]
        public void GivenMixedValidityTemplateCollection_WhenCreate_ItShouldPopulateLineNumbers_Test(string json)
        {
            var templateContext = _collectionContentTemplateFactory.Create(json);
            Assert.NotNull(templateContext);
            Assert.False(templateContext.IsValid(out _));

            var collectionTemplate = Assert.IsType<CollectionContentTemplate>(templateContext.Template);
            Assert.Equal(3, collectionTemplate.Templates.Count);

            // Calculated Function Template
            var measurementExtractor = Assert.IsType<MeasurementExtractor>(collectionTemplate.Templates[0]);
            CheckLineInfo(measurementExtractor.Template, 6);
            CheckLineInfo(measurementExtractor.Template.GetLineInfoForProperty(nameof(CalculatedFunctionContentTemplate.TypeName)), 7);
            CheckLineInfo(measurementExtractor.Template.DeviceIdExpression, 13, 40);
            CheckLineInfo(measurementExtractor.Template.DeviceIdExpression.GetLineInfoForProperty(nameof(TemplateExpression.Value)), 13);
            CheckLineInfo(measurementExtractor.Template.TimestampExpression, 9, 32);
            CheckLineInfo(measurementExtractor.Template.TimestampExpression.GetLineInfoForProperty(nameof(TemplateExpression.Value)), 10);
            CheckLineInfo(measurementExtractor.Template.TimestampExpression.GetLineInfoForProperty(nameof(TemplateExpression.Language)), 11);

            Assert.Single(measurementExtractor.Template.Values);
            var calcFunctionValueExpression = measurementExtractor.Template.Values[0];
            CheckLineInfo(calcFunctionValueExpression, 15, 11);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.Required)), 16);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueExpression)), 17);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueName)), 18);

            // JsonPath Template
            measurementExtractor = Assert.IsAssignableFrom<MeasurementExtractor>(collectionTemplate.Templates[1]);
            CheckLineInfo(measurementExtractor.Template, 25);
            CheckLineInfo(measurementExtractor.Template.GetLineInfoForProperty(nameof(CalculatedFunctionContentTemplate.TypeName)), 26);
            CheckLineInfo(measurementExtractor.Template.DeviceIdExpression, 28);
            CheckLineInfo(measurementExtractor.Template.DeviceIdExpression.GetLineInfoForProperty(nameof(TemplateExpression.Value)), 28);
            CheckLineInfo(measurementExtractor.Template.TimestampExpression, 29);
            CheckLineInfo(measurementExtractor.Template.TimestampExpression.GetLineInfoForProperty(nameof(TemplateExpression.Value)), 29);

            Assert.Single(measurementExtractor.Template.Values);
            calcFunctionValueExpression = measurementExtractor.Template.Values[0];
            CheckLineInfo(calcFunctionValueExpression, 31);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.Required)), 32);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueExpression)), 33);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueName)), 34);

            // IotCentralJsonPathContent Template
            measurementExtractor = Assert.IsAssignableFrom<MeasurementExtractor>(collectionTemplate.Templates[2]);
            CheckLineInfo(measurementExtractor.Template, 54);
            CheckLineInfo(measurementExtractor.Template.GetLineInfoForProperty(nameof(CalculatedFunctionContentTemplate.TypeName)), 55);
            CheckLineInfo(measurementExtractor.Template.TypeMatchExpression, 56);
            CheckLineInfo(measurementExtractor.Template.TypeMatchExpression.GetLineInfoForProperty(nameof(TemplateExpression.Value)), 56);

            Assert.Single(measurementExtractor.Template.Values);
            calcFunctionValueExpression = measurementExtractor.Template.Values[0];
            CheckLineInfo(calcFunctionValueExpression, 58);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.Required)), 59);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueExpression)), 60);
            CheckLineInfo(calcFunctionValueExpression.GetLineInfoForProperty(nameof(CalculatedFunctionValueExpression.ValueName)), 61);
        }

        private void CheckLineInfo(ILineInfo lineInfo, int expectedLine, int expectedPos = -1)
        {
            Assert.NotNull(lineInfo);
            Assert.True(lineInfo.HasLineInfo());
            Assert.Equal(expectedLine, lineInfo.LineNumber);

            if (expectedPos > -1)
            {
                Assert.Equal(expectedPos, lineInfo.LinePosition);
            }
        }
    }
}
