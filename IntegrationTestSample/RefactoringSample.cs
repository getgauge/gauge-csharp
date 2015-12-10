using System;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace IntegrationTestSample
{
	public class RefactoringSample
	{
        [Step("Refactoring A context step which gets executed before every scenario")]
        public void RefactoringContext()
        {
            Console.WriteLine("This is a sample context");
        }

        [Step("Refactoring this is a test step")]
        public void RefactoringSampleTest()
        {
            Console.WriteLine("This is a sample context");
        }

        [Step("Refactoring Say <what> to <who>")]
        public void RefactoringSaySomething(string what, string who)
        {
            Console.WriteLine("{0}, {1}!", what, who);
        }

        [Step("Refactoring Step that takes a table <table>")]
        public void RefactoringReadTable(Table table)
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
