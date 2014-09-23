using System;
using System.Collections.Generic;

namespace Gauge.CSharp.Lib
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Step : Attribute
    {
        private readonly string[] _stepText ;

        public Step(string stepText)
        {
            _stepText = new[] {stepText};
        }

        public Step(params string[] stepText)
        {
            _stepText = stepText;
        }

        public IEnumerable<string> Names
        {
            get
            {
                return _stepText;
            }
        }
    }
}
