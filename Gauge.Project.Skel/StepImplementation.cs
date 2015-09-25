﻿using System;
using System.Linq;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace $safeprojectname$
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
            table.GetColumnNames().ForEach(Console.Write);
            var rows = table.GetTableRows();
            rows.ForEach(list => Console.WriteLine(list.Aggregate((a, b) => string.Format("{0}|{1}", a, b))));
        }
	}
}
