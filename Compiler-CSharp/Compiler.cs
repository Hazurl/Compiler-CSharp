using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class CompilationResult
    {
        public bool Sucess;

        // Parsing
        public Dictionary<Parser.Token, Dictionary<Parser.ParsingErrorType, List<Parser.ProgramPosition>>> ParsingErrors;
        public int ParsingErrorCount;
        public TimeSpan ParsingTime;
    }

    class Compiler
    {
        public Compiler(Program program)
        {
            this.program = program;
        }

        Program program;
        Parser.Parser parser;


        public CompilationResult Run ()
        {
            CompilationResult res = new CompilationResult();

            res.ParsingTime = Utility.TimeCounter(() =>
            {
               parser = new Parser.Parser(program);
            });

            res.ParsingErrors = parser.Errors;
            res.ParsingErrorCount = parser.ErrorCount;
            if (res.ParsingErrorCount > 0)
            {
                res.Sucess = false;
                return res;
            }


            res.Sucess = true;
            return res;
        }
    }
}
