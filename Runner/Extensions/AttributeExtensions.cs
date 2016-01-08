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