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

    enum CompilerMode
    {
        Parsing,
        Normal
    }

    class Compiler
    {
        public Compiler(Program program, CompilerMode mode = CompilerMode.Normal)
        {
            this.program = program;
            Mode = mode;
        }

        Program program;
        Parser.Parser parser;
        public CompilerMode Mode { get; private set; }

        public void SetMode(CompilerMode mode)
        {
            Mode = mode;
        }

        public CompilationResult Run ()
        {
            CompilationResult res = new CompilationResult();
            
            if (Mode == CompilerMode.Normal || Mode == CompilerMode.Parsing)
            {
                if (!Parsing(ref res) || Mode == CompilerMode.Parsing)
                {
                    return res;
                }
            }

            res.Sucess = true;
            return res;
        }

        private bool Parsing(ref CompilationResult res)
        {
            res.ParsingTime = Utility.TimeCounter(() =>
            {
                parser = new Parser.Parser(program);
            });

            res.ParsingErrors = parser.Errors;
            res.ParsingErrorCount = parser.ErrorCount;
            if (res.ParsingErrorCount > 0)
            {
                res.Sucess = false;
                return false;
            }

            return true;
        }
    }
}
