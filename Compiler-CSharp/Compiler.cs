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
        public List<Parser.Error> ParsingErrors;
        public int ParsingErrorCount;
        public long ParsingTimeMs;

        //Token
        public List<Parser.Token> Tokens;

        // PreProcessor
        public List<Parser.Token> TokensPreProc;
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
            
            if ((Mode & CompilerMode.Parsing) != 0)
            {
                if (!Parsing(ref res))
                {
                    return res;
                }
            }

            if (res.Tokens != null && (Mode & CompilerMode.PreProcessor) != 0)
            {
                if (!PreProcessor(ref res))
                {
                    return res;
                }
            }

            res.Sucess = true;
            return res;
        }

        private bool Parsing(ref CompilationResult res)
        {
            res.ParsingTimeMs = Utility.TimeCounterMs(() =>
            {
                parser = new Parser.Parser(program);
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

        private bool PreProcessor(ref CompilationResult res)
        {
            var tokens = res.Tokens;
            res.PreProcTimeMs = Utility.TimeCounterMs(() =>
            {
                preProcessor = new PreProcessor(program, tokens);
            });

            res.TokensPreProc = preProcessor.Tokens;

            return true;
        }
    }
}
