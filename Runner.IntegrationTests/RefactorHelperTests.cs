// Copyright 2015 ThoughtWorks, Inc.
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
using System.IO;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    [TestFixture]
    class RefactorHelperTests
    {
        private readonly string _testProjectPath = TestUtils.GetIntegrationTestSampleDirectory();

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);

            File.Copy(Path.Combine(_testProjectPath, "RefactoringSample.cs"),
                Path.Combine(_testProjectPath, "RefactoringSample_copy.cs"), true);
        }

        [Test]
        public void ShouldRefactorAttributeText()
        {
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringContext");

            sandbox.Refactor(gaugeMethod, new List<Tuple<int, int>>(), new List<string>(), "foo");

            AssertStepAttributeWithTextExists(gaugeMethod.Name, "foo");
        }

        [Test]
        public void ShouldRefactorAndReturnFilesChanged()
        {
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringContext");
            var expectedPath = Path.GetFullPath(Path.Combine(_testProjectPath, "RefactoringSample.cs"));

            var filesChanged =
                sandbox.Refactor(gaugeMethod, new List<Tuple<int, int>>(), new List<string>(), "foo").ToList();

            Assert.AreEqual(1, filesChanged.Count);
            Assert.AreEqual(expectedPath, filesChanged.First());
        }


        [Test]
        public void ShouldReorderParameters()
        {
            const string newStepValue = "Refactoring Say <who> to <what>";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething-StringwhatStringwho");

            var parameterPositions = new[] {new Tuple<int, int>(0, 1), new Tuple<int, int>(1, 0)};
            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string> {"who", "what"}, newStepValue);

            AssertStepAttributeWithTextExists(gaugeMethod.Name, newStepValue);
            AssertParametersExist(gaugeMethod.Name, new[] {"who", "what"});
        }

        [Test]
        public void ShouldAddParameters()
        {
            const string newStepValue = "Refactoring Say <what> to <who> in <where>";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething-StringwhatStringwho");

            var parameterPositions = new[]{new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 1), new Tuple<int, int>(-1, 2)};
            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string> {"what", "who", "where"}, newStepValue);

            AssertStepAttributeWithTextExists(gaugeMethod.Name, newStepValue);
            AssertParametersExist(gaugeMethod.Name, new[] {"what", "who", "where"});
        }

        [Test]
        public void ShouldAddParametersWhenNoneExisted()
        {
            const string newStepValue = "Refactoring this is a test step <foo>";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSampleTest");
            var parameterPositions = new[] {new Tuple<int, int>(-1, 0)};

            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string> {"foo"}, newStepValue);

            AssertStepAttributeWithTextExists(gaugeMethod.Name, newStepValue);
            AssertParametersExist(gaugeMethod.Name, new[] {"foo"});
        }

        [Test]
        public void ShouldAddParametersWithReservedKeywordName()
        {
            const string newStepValue = "Refactoring this is a test step <class>";
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSampleTest");
            var parameterPositions = new[] {new Tuple<int, int>(-1, 0)};

            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string> {"class"}, newStepValue);

            AssertStepAttributeWithTextExists(gaugeMethod.Name, newStepValue);
            AssertParametersExist(gaugeMethod.Name, new[] {"@class"});
        }

        [Test]
        public void ShouldRemoveParameters()
        {
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething-StringwhatStringwho");

            var parameterPositions = new[] {new Tuple<int, int>(0, 0)};

            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string>(), "Refactoring Say <what> to someone");

            AssertParametersExist(gaugeMethod.Name, new[] {"what"});
        }

        [Test]
        public void ShouldRemoveParametersInAnyOrder()
        {
            var sandbox = SandboxBuilder.Build();
            var gaugeMethod = sandbox.GetStepMethods().First(info => info.Name == "IntegrationTestSample.RefactoringSample.RefactoringSaySomething-StringwhatStringwho");

            var parameterPositions = new[] {new Tuple<int, int>(1, 0)};

            sandbox.Refactor(gaugeMethod, parameterPositions, new List<string>(), "Refactoring Say something to <who>");

            AssertParametersExist(gaugeMethod.Name, new[] {"who"});
        }

        [TearDown]
        public void TearDown()
        {
            var sourceFileName = Path.Combine(_testProjectPath, "RefactoringSample_copy.cs");
            File.Copy(sourceFileName, Path.Combine(_testProjectPath, "RefactoringSample.cs"), true);
            File.Delete(sourceFileName);
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", null);
        }

        private void AssertStepAttributeWithTextExists(string methodName, string text)
        {
            var name = methodName.Split('.').Last().Split('-').First();
            var tree =
                CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(_testProjectPath, "RefactoringSample.cs")));
            var root = tree.GetRoot();

            var stepTexts = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Select(
                    node => new {node, attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)})
                .Where(@t => string.CompareOrdinal(@t.node.Identifier.ValueText, name) == 0
                             &&
                             @t.attributeSyntaxes.Any(
                                 syntax => string.CompareOrdinal(syntax.ToFullString(), typeof(Step).ToString()) > 0))
                .SelectMany(@t => @t.node.AttributeLists.SelectMany(syntax => syntax.Attributes))
                .SelectMany(syntax => syntax.ArgumentList.Arguments)
                .Select(syntax => syntax.GetText().ToString().Trim('"'));

            Assert.True(stepTexts.Contains(text));
        }

        private void AssertParametersExist(string methodName, IReadOnlyList<string> parameters)
        {
            var name = methodName.Split('.').Last().Split('-').First();
            var tree =
                CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(_testProjectPath, "RefactoringSample.cs")));
            var root = tree.GetRoot();
            var methodParameters = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(syntax => string.CompareOrdinal(syntax.Identifier.Text, name) == 0)
                .Select(syntax => syntax.ParameterList)
                .SelectMany(syntax => syntax.Parameters)
                .Select(syntax => syntax.Identifier.Text)
                .ToArray();

            for (var i = 0; i < parameters.Count; i++)
            {
                Assert.AreEqual(parameters[i], methodParameters[i]);
            }
        }
    }
}
