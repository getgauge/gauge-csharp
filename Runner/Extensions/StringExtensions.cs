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

using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Gauge.CSharp.Runner.Extensions
{
    public static class StringExtensions
    {
        public static string ToValidCSharpIdentifier(this string str, bool camelCase=true)
        {
            str = str.Trim();

            if (!IsCSharpKeyword(str) && SyntaxFacts.IsValidIdentifier(str))
            {
                return str;
            }

            str = camelCase ? str.Split(' ').Select(s => s.Capitalize()).Aggregate(string.Concat) : str.Replace(" ", "");
            var result = new StringBuilder();

            if (!SyntaxFacts.IsIdentifierStartCharacter(str[0]))
            {
                result.Append('_');
            }

            foreach (var c in str.Where(SyntaxFacts.IsIdentifierPartCharacter))
            {
                result.Append(c);
            }

            var retval = result.ToString();

            if (IsCSharpKeyword(retval))
            {
                retval = string.Concat('@', retval);
            }

            return retval;
        }

        private static bool IsCSharpKeyword(string retval)
        {
            return SyntaxFacts.GetKeywordKind(retval) != SyntaxKind.None;
        }

        private static string Capitalize(this string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }
    }
}