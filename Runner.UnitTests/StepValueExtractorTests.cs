﻿// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Gauge.CSharp.Runner.Models;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class StepValueExtractorTests
    {
        StepValueExtractor stepValueExtractor;
        [SetUp]
        public void Setup()
        {
            stepValueExtractor = new StepValueExtractor();
        }

        [Test]
        public void ShoulReplaceSingleStepParameterWithThePlaceHolder()
        {
            var stepTexts = new List<string>
            {
                "Invoke method with <some values>",
                "Invoke another method with <some values>"
            };

            var expectedStepValues = new List<string>
            {
                "Invoke method with {}",
                "Invoke another method with {}"
            };

            Assert.AreEqual(stepValueExtractor.ExtractFrom(stepTexts), expectedStepValues);
        }

        [Test]
        public void ShoulReplaceMultipleStepParametersWithThePlaceHolders()
        {
            var stepTexts = new List<string>
            {
                "Step with multiple <first parameter> parameters <second parameter>",
            };

            var expectedStepValues = new List<string>
            {
                "Step with multiple {} parameters {}"
            };

            Assert.AreEqual(stepValueExtractor.ExtractFrom(stepTexts), expectedStepValues);
        }
    }
}