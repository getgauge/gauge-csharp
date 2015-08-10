﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gauge.CSharp.Lib.Attribute;
using Gauge.CSharp.Runner.Communication;
using Gauge.CSharp.Runner.Exceptions;
using Gauge.CSharp.Runner.Extensions;
using Gauge.Messages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.CSharp.Runner
{
    public class RefactorHelper
    {
        public static IEnumerable<string> Refactor(MethodInfo method, IList<ParameterPosition> parameterPositions, IList<string> parameters, string newStepValue)
        {
            var projectFile = Directory.EnumerateFiles(Utils.GaugeProjectRoot, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();

            if (projectFile == null)
            {
                throw new NotAValidGaugeProjectException();
            }
            
            var document = XDocument.Load(projectFile);

            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            var classFiles = document.Descendants(ns + "Project")
                .Where(t => t.Attribute("ToolsVersion") != null)
                .Elements(ns + "ItemGroup")
                .Elements(ns + "Compile")
                .Where(r => r.Attribute("Include") != null)
                .Select(r => Path.GetFullPath(Path.Combine(Utils.GaugeProjectRoot, r.Attribute("Include").Value)));

            var filesChanged = new ConcurrentBag<string>();

            Parallel.ForEach(classFiles, (f, state) =>
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(f));
                var root = tree.GetRoot();

                var stepMethods = from node in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    let attributeSyntaxes = node.AttributeLists.SelectMany(syntax => syntax.Attributes)
                    where string.CompareOrdinal(node.Identifier.ValueText, method.Name) == 0
                       && attributeSyntaxes.Any(syntax => string.CompareOrdinal(syntax.ToFullString(), typeof(Step).ToString()) > 0)
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

        private static ParameterListSyntax ReplaceParameters(MethodDeclarationSyntax methodDeclarationSyntax, IList<ParameterPosition> parameterPositions, IList<string> parameters)
        {
            var parameterListSyntax = methodDeclarationSyntax.ParameterList;
            var foo = new SeparatedSyntaxList<ParameterSyntax>();
            foo = parameterPositions.OrderBy(position => position.NewPosition)
                .Aggregate(foo, (current, parameterPosition) =>
                        current.Add(parameterPosition.OldPosition == -1
                            ? CreateParameter(parameters[parameterPosition.NewPosition])
                            : parameterListSyntax.Parameters[parameterPosition.OldPosition]));
            return parameterListSyntax.WithParameters(foo);
        }

        private static ParameterSyntax CreateParameter(string text)
        {
            // Could not get SyntaxFactory.Parameter to work properly, so ended up parsing code as string
            return SyntaxFactory.ParseParameterList(string.Format("string {0}", text)).Parameters[0];
        }

        private static SyntaxList<AttributeListSyntax> ReplaceAttribute(MethodDeclarationSyntax methodDeclarationSyntax, string newStepText)
        {
            var attributeListSyntax = methodDeclarationSyntax.AttributeLists.WithStepAttribute();
            var attributeSyntax = attributeListSyntax.Attributes.GetStepAttribute();
            var attributeArgumentSyntax = attributeSyntax.ArgumentList.Arguments.FirstOrDefault();

            if (attributeArgumentSyntax == null)
            {
                return default(SyntaxList<AttributeListSyntax>);
            }
            var newAttributeArgumentSyntax = attributeArgumentSyntax.WithExpression(
                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.ParseToken(string.Format("\"{0}\"", newStepText))));

            var attributeArgumentListSyntax = attributeSyntax.ArgumentList.WithArguments(new SeparatedSyntaxList<AttributeArgumentSyntax>().Add(newAttributeArgumentSyntax));
            var newAttributeSyntax = attributeSyntax.WithArgumentList(attributeArgumentListSyntax);

            var newAttributes = attributeListSyntax.Attributes.Remove(attributeSyntax).Add(newAttributeSyntax);
            var newAttributeListSyntax = attributeListSyntax.WithAttributes(newAttributes);

            return methodDeclarationSyntax.AttributeLists.Remove(attributeListSyntax).Add(newAttributeListSyntax);
        }
    }

    public class StepAttributeWalker : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return base.VisitMethodDeclaration(node);
        }
    }
}
