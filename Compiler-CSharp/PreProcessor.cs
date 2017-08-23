using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class PreProcessor
    {
        private class Symbol
        {
            public string Content;
            public Parser.TokenType Type;

            public override string ToString()
            {
                return "Symbol '" + Content + "' of type " + Type;
            }

            public override int GetHashCode()
            {
                return Content.GetHashCode() ^ Type.GetHashCode();
            }
        }

        public PreProcessor(List<Parser.Token> tokens)
        {
            previous = tokens;
            Transform();
        }

        List<Parser.Token> previous;
        public List<Parser.Token> Tokens { get; private set; }

        Dictionary<Symbol, Symbol> symbols;

        private void Transform()
        {
            Tokens = new List<Parser.Token>();
            symbols = new Dictionary<Symbol, Symbol>();

            int line = -1;

            foreach (Parser.Token token in previous)
            {
                if (line != token.Location.Start.Line)
                {
                    line = token.Location.Start.Line;
                    UpdateLine(line);
                }
                UpdateColumn(token.Location.Start.Columns);

                Symbol s = TokenToSymbol(token);
                if (symbols.ContainsKey(s))
                {
                    Utility.WriteLine("Find macro !", ConsoleColor.Yellow);
                    Symbol replacement = symbols[s];
                    Tokens.Add(new Parser.Token(replacement.Type, replacement.Content, token.Location));
                }
                else
                {
                    Tokens.Add(token);
                }
            }
        }

        private void UpdateLine(int line)
        {
            Symbol s = new Symbol
            {
                Content = "__LINE__",
                Type = Parser.TokenType.Ident
            };

            Utility.WriteLine("Line update : " + s.ToString(), ConsoleColor.Yellow);

            symbols[s] = new Symbol
            {
                Content = line.ToString(),
                Type = Parser.TokenType.Integer
            };
        }

        private void UpdateColumn(int col)
        {
            Symbol s = new Symbol
            {
                Content = "__COLUMN__",
                Type = Parser.TokenType.Ident
            };

            Utility.WriteLine("Columns update : " + s.ToString(), ConsoleColor.Yellow);

            symbols[s] = new Symbol
            {
                Content = col.ToString(),
                Type = Parser.TokenType.Integer
            };
        }

        private Symbol TokenToSymbol(Parser.Token token)
        {
            Utility.WriteLine("conv: " + new Symbol
            {
                Content = token.Content,
                Type = token.Type
            }.ToString(), ConsoleColor.Yellow);

            return new Symbol
            {
                Content = token.Content,
                Type = token.Type
            };
        }

    }
}
