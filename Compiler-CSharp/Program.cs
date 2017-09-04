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

        public Program(params string[] code)
        {
            Code = new List<string>(code);
            Filename = "DefaultFilenameTest.txt";
        }

        public Program(List<string> code)
        {
            Code = code;
            Filename = "DefaultFilenameTest.txt";
        }

        public static Program LoadfromFile(string filename)
        {
            return new Program(filename, Utility.getFileContentToList(filename));
        }

        public string Filename { get; private set; }
        public List<string> Code { get; private set; }


        public override string ToString()
        {
            string s = "";
            bool first = true;
            foreach (string line in Code)
            {
                if (first) first = false;
                else s += "\\n";

                s += line;
            }

            return s;
        }
    }
}
