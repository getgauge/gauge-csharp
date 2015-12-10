using System;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace IntegrationTestSample
{
	public class StepImplementation
	{
        [Step("A context step which gets executed before every scenario")]
        public void Context()
        {
            Console.WriteLine("This is a sample context");
        }

        [Step("Say <what> to <who>")]
        public void SaySomething(string what, string who)
        {
            Console.WriteLine("{0}, {1}!", what, who);
        }

        [Step("Step that takes a table <table>")]
        public void ReadTable(Table table)
        {
            var columnNames = table.GetColumnNames();
            columnNames.ForEach(Console.Write);
            var rows = table.GetTableRows();
            rows.ForEach(
                row => Console.WriteLine(columnNames.Select(row.GetCell)
                        .Aggregate((a, b) => string.Format("{0}|{1}", a, b))));
        }
	}
}
