using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class CompilationResult
    {
        public CompilerMode ModeUsed;
        public bool Sucess;

        // Program
        public Program Program;

        // Parsing
        public List<Parser.Error> ParsingErrors;
        public int ParsingErrorCount;
        public long ParsingTimeMs;

        //Token
        public List<Parser.Token> Tokens;

        // PreProcessor
        public Program ProgramBeforeProcess;
        public long PreProcTimeMs;
    }

    [Flags]
    enum CompilerMode
    {
        Nothing         = 0,
        Parsing         = 1 << 0,
        PreProcessor    = 1 << 1,
    }

    class Compiler
    {
        public Compiler(Program program, CompilerMode mode = CompilerMode.Parsing)
        {
            this.program = program;
            Mode = mode;
        }

        Program program;
        Parser.Parser parser;
        PreProcessor preProcessor;

        public CompilerMode Mode { get; private set; }

        public void SetMode(CompilerMode mode)
        {
            Mode = mode;
        }

        public CompilationResult Run ()
        {
            CompilationResult res = new CompilationResult();
            res.ModeUsed = Mode;
            res.Program = program;

            if ((Mode & CompilerMode.PreProcessor) != 0)
            {
                if (!PreProcessor(ref res))
                {
                    return res;
                }
            }

            if ((Mode & CompilerMode.Parsing) != 0)
            {
                if (!Parsing(ref res))
                {
                    return res;
                }
            }

            res.Sucess = true;
            return res;
        }

        private bool PreProcessor(ref CompilationResult res)
        {
            Program p = res.ProgramBeforeProcess = res.Program;
            res.PreProcTimeMs = Utility.TimeCounterMs(() =>
            {
                preProcessor = new PreProcessor(p);
            });

            res.Program = preProcessor.Program;

            return true;
        }

        private bool Parsing(ref CompilationResult res)
        {
            Program p = res.Program;
            res.ParsingTimeMs = Utility.TimeCounterMs(() =>
            {
                parser = new Parser.Parser(p);
            });

            res.Tokens = parser.Tokens;

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
