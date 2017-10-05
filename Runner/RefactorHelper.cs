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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Exceptions;
using Gauge.CSharp.Runner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.CSharp.Runner
{
    public class RefactorHelper
    {
        public static IEnumerable<string> Refactor(MethodInfo method, IList<Tuple<int, int>> parameterPositions,
            IList<string> parameters, string newStepValue)
        {
            var projectFile = Directory.EnumerateFiles(Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT"),
                "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

            if (projectFile == null)
                throw new NotAValidGaugeProjectException();

            var document = XDocument.Load(projectFile);

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            var classFiles = document.Descendants(ns + "Project")
                .Where(t => t.Attribute("ToolsVersion") != null)
                .Elements(ns + "ItemGroup")
                .Elements(ns + "Compile")
                .Where(r => r.Attribute("Include") != null)
                .Select(r => Path.GetFullPath(Path.Combine(Environment.GetEnvironmentVariable("GAUGE_PROJECT_ROOT"), r
                    .Attribute("Include").Value
                    .Replace('\\', Path.DirectorySeparatorChar))));

            var filesChanged = new ConcurrentBag<string>();

            Parallel.ForEach(classFiles, (f, state) =>
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(f));
                var root = tree.GetRoot();

                var stepMethods = from node in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    let attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)
                    let classDef = node.Parent as ClassDeclarationSyntax
                    where string.CompareOrdinal(node.Identifier.ValueText, method.Name) == 0
                          && string.CompareOrdinal(classDef.Identifier.ValueText, method.DeclaringType.Name) == 0
                          && attributeSyntaxes.Any(syntax =>
                              string.CompareOrdinal(syntax.ToFullString(), typeof(Step).ToString()) > 0)
                    select node;

                if (!stepMethods.Any()) return;

                //Found the method
                state.Break();

                //TODO: check for aliases and error out

                foreach (var methodDeclarationSyntax in stepMethods)
                {
                    var updatedAttribute = ReplaceAttribute(methodDeclarationSyntax, newStepValue);
                    var updatedParameters = ReplaceParameters(methodDeclarationSyntax, parameterPositions, parameters);
                    var declarationSyntax = methodDeclarationSyntax
                        .WithAttributeLists(updatedAttribute)
                        .WithParameterList(updatedParameters);
                    var replaceNode = root.ReplaceNode(methodDeclarationSyntax, declarationSyntax);

                    File.WriteAllText(f, replaceNode.ToFullString());
                    filesChanged.Add(f);
                }
            });
            return filesChanged;
        }

        private static ParameterListSyntax ReplaceParameters(MethodDeclarationSyntax methodDeclarationSyntax,
            IEnumerable<Tuple<int, int>> parameterPositions, IList<string> parameters)
        {
            var parameterListSyntax = methodDeclarationSyntax.ParameterList;
            var newParams = new SeparatedSyntaxList<ParameterSyntax>();
            newParams = parameterPositions.OrderBy(position => position.Item2)
                .Aggregate(newParams, (current, parameterPosition) =>
                    current.Add(parameterPosition.Item1 == -1
                        ? CreateParameter(parameters[parameterPosition.Item2])
                        : parameterListSyntax.Parameters[parameterPosition.Item1]));
            return parameterListSyntax.WithParameters(newParams);
        }

        private static ParameterSyntax CreateParameter(string text)
        {
            // Could not get SyntaxFactory.Parameter to work properly, so ended up parsing code as string
            return SyntaxFactory.ParseParameterList(string.Format("string {0}", text.ToValidCSharpIdentifier(false)))
                .Parameters[0];
        }

        private static SyntaxList<AttributeListSyntax> ReplaceAttribute(MethodDeclarationSyntax methodDeclarationSyntax,
            string newStepText)
        {
            var attributeListSyntax = methodDeclarationSyntax.AttributeLists.WithStepAttribute();
            var attributeSyntax = attributeListSyntax.Attributes.GetStepAttribute();
            var attributeArgumentSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault();

            if (attributeArgumentSyntax == null)
                return default(SyntaxList<AttributeListSyntax>);
            var newAttributeArgumentSyntax = attributeArgumentSyntax.WithExpression(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.ParseToken(string.Format("\"{0}\"", newStepText))));

            var attributeArgumentListSyntax =
                attributeSyntax.ArgumentList.WithArguments(
                    new SeparatedSyntaxList<AttributeArgumentSyntax>().Add(newAttributeArgumentSyntax));
            var newAttributeSyntax = attributeSyntax.WithArgumentList(attributeArgumentListSyntax);

            var newAttributes = attributeListSyntax.Attributes.Remove(attributeSyntax).Add(newAttributeSyntax);
            var newAttributeListSyntax = attributeListSyntax.WithAttributes(newAttributes);

            return methodDeclarationSyntax.AttributeLists.Remove(attributeListSyntax).Add(newAttributeListSyntax);
        }
    }
}