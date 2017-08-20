using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Parser
    { 
        class ProgramPosition
        {
            public ProgramPosition(int line, int columns)
            {
                Line = line;
                Columns = columns;
            }

            public override string ToString()
            {
                return (Line + 1)+ ":" + Columns;
            }

            public int Line { get; private set; }
            public int Columns { get; private set; }
        }
    }
}
