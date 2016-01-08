using System;
using System.Linq;
using System.Runtime.Serialization;
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

	    [Step("I throw an unserializable exception")]
	    public void ThrowUnserializableException()
	    {
	        throw new CustomException("I am a custom exception");
	    }

	    [Step("I throw a serializable exception")]
	    public void ThrowSerializableException()
	    {
	        throw new CustomSerializableException("I am a custom serializable exception");
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

        [Serializable]
        public class CustomSerializableException : Exception
        {
            public CustomSerializableException(string s) : base(s)
            {
            }

            public CustomSerializableException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        public class CustomException : Exception
        {
            public CustomException(string message) : base(message)
            {
            }
        }
    }
}
