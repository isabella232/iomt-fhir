﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using DevLab.JmesPath;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Health.Expressions
{
    public class FromUnixTimestampTests
    {
        private JmesPath _jmesPath;
        private JmesPath.Expression _expression;

        public FromUnixTimestampTests()
        {
            _jmesPath = new JmesPath();
            _jmesPath.FunctionRepository.Register<FromUnixTimestampFunction>();
            _expression = _jmesPath.Parse("fromUnixTimestamp(timestamp)");
        }

        [Theory]
        [InlineData(0, "1970-01-01T00:00:00+0")]
        [InlineData(1625677200, "2021-07-07T17:00:00+0")]
        public void UnixTimestampe_Parsing_Succeeds(long unixTime, string datetime)
        {
            var data = JObject.FromObject(new
            {
                timestamp = unixTime,
            });

            var jmesArgument = _expression.Transform(data);
            Assert.Equal(JTokenType.Date, jmesArgument.Token.Type);
            var myDate = jmesArgument.Token.Value<DateTime>();
            Assert.Equal(datetime, myDate.ToString("yyyy-MM-ddTHH:mm:ssz"));
        }

        [Fact]
        public void Bad_Data_Throws_Exception()
        {
            var data = JObject.FromObject(new
            {
                timestamp = "badData",
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                timestamp = new int[1, 2, 3],
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));

            data = JObject.FromObject(new
            {
                timestamp = new
                {
                    value = 1,
                },
            });

            Assert.Throws<Exception>(() => _expression.Transform(data));
        }

        [Fact]
        public void Invalid_Argument_Count_Throws_Exception()
        {
            Assert.Throws<Exception>(() => _jmesPath.Parse("fromUnixTimestamp()"));
            Assert.Throws<Exception>(() => _jmesPath.Parse("fromUnixTimestamp(timestamp, extraParam)"));
        }
    }
}
