using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gauge.CSharp.Runner.Extensions
{
    public static class AttributeExtensions
    {
        public static AttributeListSyntax WithStepAttribute(this SyntaxList<AttributeListSyntax> list)
        {
            return list.First( syntax => GetStepAttribute(syntax.Attributes)!=null);
        }

        public static AttributeSyntax GetStepAttribute(this SeparatedSyntaxList<AttributeSyntax> list)
        {
            return list.FirstOrDefault(argumentSyntax => 
                string.CompareOrdinal(argumentSyntax.ToFullString(), typeof(Step).ToString()) > 0);
        }
    }
}