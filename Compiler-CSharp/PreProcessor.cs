using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class PreProcessor
    {
        private class Replacer
        {
            public Replacer(string replacement)
            {
                Replacement = replacement;
                IsFunction = false;
            }

            public bool IsFunction = false;
            public int Argc = 0;
            public string Replacement;
        }

        public PreProcessor(Program program)
        {
            previous = program;
            Transform();
        }

        Program previous;
        public Program Program { get; private set; }
        ProgramIterator mover;

        Dictionary<string, Replacer> symbols;

        List<string> code;
        string codeLine;

        private void Transform()
        {
            mover = new ProgramIterator(previous.Code);

            symbols = new Dictionary<string, Replacer>();
            code = new List<string>();
            codeLine = "";

            symbols["__FILENAME__"] = new Replacer(previous.Filename);

            mover.Forward();
            for (int _ = mover.LineJumped(); _ > 0; --_)
            {
                code.Add(codeLine);
                codeLine = "";
            }

            while (!mover.EOF())
            {
                char c = mover.Current();

                if (IsLetter(c))
                {
                    string ident = NextIdent();
                    if (ident.Length == 0)
                    {
                        break;
                    }

                    if (symbols.ContainsKey(ident))
                    {
                        Replacer r = symbols[ident];
                        if (r.IsFunction)
                        {
                            MacroFunction(ident);
                        }
                        else
                        {
                            codeLine += r.Replacement;
                        }
                    }
                    else if (IsSpecialMacro(ident))
                    {
                        MacroFunction(ident);
                    }
                    else
                    {
                        codeLine += ident;
                    }
                }
                else
                {
                    codeLine += c;
                }

                mover.Forward();
                for (int _ = mover.LineJumped(); _ > 0; --_)
                {
                    code.Add(codeLine);
                    codeLine = "";
                }
            }
            code.Add(codeLine);
            
            Program = new Program(previous.Filename, code);
        }

        private bool IsLetter(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private string NextIdent()
        {
            symbols["__LINE__"] = new Replacer((mover.Line + 1).ToString());
            symbols["__COLUMN__"] = new Replacer(mover.Column.ToString());

            string ident = "";
            do
            {
                char c = mover.Current();
                if (IsLetter(c))
                {
                    ident += c;
                }
                else if (ident.Length > 0 && IsDigit(c))
                {
                    ident += c;
                }
                else
                {
                    return ident;
                }

                mover.Forward();
            } while (!mover.EOF() && !mover.LineHasChanged());

            return ident;
        }

        private void MacroFunction(string ident)
        {
            if (mover.Current() != '(')
            {
                codeLine += ident;
                return;
            }

            List<string> parameters = new List<string>();
            string cur = "";

            mover.Forward();
            for (int _ = mover.LineJumped(); _ > 0; --_)
            {
                parameters.Add("");
            }

            while (!mover.EOF() && mover.Current() != ')')
            {
                cur += mover.Current();
                mover.Forward();
                for (int _ = mover.LineJumped(); _ > 0; --_)
                {
                    parameters.Add(cur);
                    cur = "";
                }
            }

            parameters.Add(cur);
            if (mover.EOF())
            {
                codeLine += ident + '(';
                foreach(string s in parameters)
                {
                    codeLine += s;
                    code.Add(codeLine);
                    codeLine = "";
                }
            }
            else
            {
                if (IsSpecialMacro(ident))
                {
                    SpecialMacro(ident, parameters);
                }
                else
                {
                    Replace(symbols[ident], parameters);
                }

                mover.Forward();
                for (int _ = mover.LineJumped(); _ > 0; --_)
                {
                    code.Add(codeLine);
                    codeLine = "";
                }
            }
        }

        private void Replace (Replacer r, List<string> parameters)
        {

        }

        private bool IsSpecialMacro(string ident)
        {
            return ident == "__STR__" ||
                   ident == "__CONCAT__" ||
                   ident == "__MAP__";
        }

        private void SpecialMacro(string ident, List<string> parameters)
        {
            if(ident == "__STR__")
            {
                for(int p = 0; p < parameters.Count; ++p)
                {
                    if (p == 0)
                    {
                        code.Add(codeLine + "\"" + parameters[p]);
                        codeLine = "";
                    }
                    else if (p + 1 == parameters.Count)
                    {
                        code.Add(parameters[p] + "\"");
                    }
                    else
                    {
                        code.Add(parameters[p]);
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
