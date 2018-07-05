using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void ShouldBeAbleToAccessRowValuesUsingColumnNames()
        {
            var headers = new List<string> {"foo", "bar"};
            var table = new Table(headers);
            table.AddRow(new List<string> {"foo_val", "bar_val"});
            table.AddRow(new List<string> {"foo_val1", "bar_val1"});

            Assert.AreEqual("foo_val", table.GetTableRows()[0].GetCell("foo"));
            Assert.AreEqual("bar_val", table.GetTableRows()[0].GetCell("bar"));
            Assert.AreEqual("foo_val1", table.GetTableRows()[1].GetCell("foo"));
            Assert.AreEqual("bar_val1", table.GetTableRows()[1].GetCell("bar"));
            Assert.AreEqual("", table.GetTableRows()[1].GetCell("bar1"));
            Assert.AreEqual("TableRow: cells: [foo = foo_val, bar = bar_val] ", table.GetTableRows()[0].ToString());
        }

        [Test]
        public void ShouldGetEmptyListColumnValuesForInvalidColumnName()
        {
            var headers = new List<string> {"foo", "bar"};
            var table = new Table(headers);
            table.AddRow(new List<string> {"foo_val", "bar_val"});
            table.AddRow(new List<string> {"foo_val1", "bar_val1"});

            var columnValues = table.GetColumnValues("baz");

            Assert.IsEmpty(columnValues);
        }

        [Test]
        public void ShouldGetTableAsMarkdownString()
        {
            var headers = new List<string> {"foo", "bar_with_big_header"};
            var table = new Table(headers);
            table.AddRow(new List<string> {"foo_val", "bar_val"});
            table.AddRow(new List<string> {"foo_val1", "bar_val1"});

            const string expected = "|foo     |bar_with_big_header|\n" +
                                    "|--------|-------------------|\n" +
                                    "|foo_val |bar_val            |\n" +
                                    "|foo_val1|bar_val1           |";

            Assert.AreEqual(expected, table.ToString());
        }

        [Test]
        public void ShouldGetValuesForGivenColumnName()
        {
            var headers = new List<string> {"foo", "bar"};
            var table = new Table(headers);
            table.AddRow(new List<string> {"foo_val", "bar_val"});
            table.AddRow(new List<string> {"foo_val1", "bar_val1"});

            var columnValues = table.GetColumnValues("foo").ToList();

            Assert.AreEqual(2, columnValues.Count);
            Assert.True(columnValues.Contains("foo_val"));
            Assert.True(columnValues.Contains("foo_val1"));
        }
    }
}