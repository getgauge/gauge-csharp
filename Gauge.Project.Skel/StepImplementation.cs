using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace $safeprojectname$
{
    public class StepImplementation
    {
        private HashSet<char> _vowels;

        [Step("Vowels in English language are <vowelString>.")]
        public void SetLanguageVowels(string vowelString)
        {
            _vowels = new HashSet<char>();
            foreach (var c in vowelString)
            {
                _vowels.Add(c);
            }
        }

        [Step("The word <word> has <expectedCount> vowels.")]
        public void VerifyVowelsCountInWord(string word, int expectedCount)
        {
            var actualCount = CountVowels(word);
            actualCount.Should().Be(expectedCount);
        }

        [Step("Almost all words have vowels <wordsTable>")]
        public void VerifyVowelsCountInMultipleWords(Table wordsTable)
        {
            var rows = wordsTable.GetTableRows();
            foreach (var row in rows)
            {
                var word = row.GetCell("Word");
                var expectedCount = Convert.ToInt32(row.GetCell("Vowel Count"));
                var actualCount = CountVowels(word);

                actualCount.Should().Be(expectedCount);
            }
        }

        private int CountVowels(string word)
        {
            return word.Count(c => _vowels.Contains(c));
        }
    }
}
