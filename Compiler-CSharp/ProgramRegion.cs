using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class ProgramRegion
    {
        public ProgramRegion(ProgramPosition start, ProgramPosition end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return Start + " -> " + End;
        }

        public ProgramPosition Start { get; private set; }
        public ProgramPosition End { get; private set; }

        public string Content(List<string> code)
        {
            string s = "";
            int line = Start.Line;
            int col = Start.Columns;

            while (line != End.Line)
            {
                s += code[line].Substring(col) + '\n';
                line++;
                col = 0;
            }
            if (End.Line < code.Count)
                s += code[line].Substring(col, End.Columns - col + 1);

            return s;
        }
    }
}
