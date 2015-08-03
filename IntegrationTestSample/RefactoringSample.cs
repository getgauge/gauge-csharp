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
            table.GetColumnNames().ForEach(Console.Write);
            var rows = table.GetRows();
            // typeof(rows) = List<List<string>> i.e a 2-dimensional representation of a table.
            rows.ForEach(list => Console.WriteLine(list.Aggregate((a, b) => string.Format("{0}|{1}", a, b))));
        }
	}
}
