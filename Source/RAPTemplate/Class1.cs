using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAPTemplate
{
    public class Class1
    {
        public string Name { get; set; }

        public Class1(string name)
        {
            Name = name;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
