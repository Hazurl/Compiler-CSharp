using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class Program
    {
        public Program (string filename, List<string> code)
        {
            Filename = filename;
            Code = code;
        }

        public static Program LoadfromFile(string filename)
        {
            return new Program(filename, Utility.getFileContentToList(filename));
        }

        public string Filename { get; private set; }
        public List<string> Code { get; private set; }
    }
}
