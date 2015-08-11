using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests
{
    [TestFixture]
    public class TableTests
    {
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
    }
}
