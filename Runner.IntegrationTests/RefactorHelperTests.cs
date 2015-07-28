using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.Messages;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.IntegrationTests
{
    [TestFixture]
    class RefactorHelperTests
    {
        readonly string _testProjectPath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\IntegrationTestSample");

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("GAUGE_PROJECT_ROOT", _testProjectPath);

            File.Copy(Path.Combine(_testProjectPath, "RefactoringSample.cs"), Path.Combine(_testProjectPath, "RefactoringSample_copy.cs"), true);
        }

        [Test]
        public void ShouldRefactorAttributeText()
        {
            var sandbox = SandboxFactory.Create(AppDomain.CurrentDomain.SetupInformation);
            var methodInfo = sandbox.GetStepMethods().First(info => info.Name== "RefactoringContext");

            RefactorHelper.Refactor(methodInfo, new List<ParameterPosition>(), new List<string>(), "foo");

            AssertStepAttributeWithTextExists(methodInfo.Name, "foo");
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
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(_testProjectPath, "RefactoringSample.cs")));
            var root = tree.GetRoot();

            var stepTexts = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Select(node => new {node, attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)})
                .Where(@t => string.CompareOrdinal(@t.node.Identifier.ValueText, methodName) == 0
                             && @t.attributeSyntaxes.Any(syntax => string.CompareOrdinal(syntax.ToFullString(), typeof (Step).ToString()) > 0))
                .SelectMany(@t => @t.node.AttributeLists.SelectMany(syntax => syntax.Attributes))
                .SelectMany(syntax => syntax.ArgumentList.Arguments)
                .Select(syntax => syntax.GetText().ToString().Trim('"'));

            Assert.True(stepTexts.Contains(text));
        }
    }
}
