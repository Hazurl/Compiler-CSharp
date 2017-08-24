using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class PreProcessor
    {
        private class TokenPart : IEquatable<TokenPart>
        {
            public TokenPart(Parser.TokenType type, string content)
            {
                Content = content;
                Type = type;
            }

            public string Content;
            public Parser.TokenType Type;

            public Parser.Token ToToken(Parser.ProgramRegion region)
            {
                return new Parser.Token(Type, Content, region);
            }

            public override string ToString()
            {
                return "'" + Content + "' of type " + Type;
            }

            public override int GetHashCode()
            {
                return Content.GetHashCode() ^ Type.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TokenPart);
            }

            public bool Equals(TokenPart t)
            {
                return Type == t.Type && Content == t.Content;
            }
        }

        private class Symbol : IEquatable<Symbol>
        {
            public Symbol(string content, Parser.TokenType type)
            {
                TokenPart = new TokenPart(type, content);
            }

            TokenPart TokenPart;
            public bool is_function = false;

            public override string ToString()
            {
                return "Symbol " + TokenPart;
            }

            public bool Equals(Symbol s)
            {
                return TokenPart.Equals(s.TokenPart);
            }

            public override int GetHashCode()
            {
                return TokenPart.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Symbol);
            }
        }

        private class SymbolInfo
        {
            public SymbolInfo(Replacer replacer)
            {
                Replacer = replacer;
            }

            public SymbolInfo(Replacer replacer, List<string> args)
            {
                Replacer = replacer;
                Args = args;
            }

            public Replacer Replacer;
            public List<string> Args = null;
        }

        private class Replacer : IEquatable<Replacer>
        {
            public Replacer(string content, Parser.TokenType type)
            {
                Tokens = new List<TokenPart> { new TokenPart(type, content) };
            }

            public Replacer(List<TokenPart> tokens)
            {
                Tokens = tokens;
            }

            public List<TokenPart> Tokens;

            public List<Parser.Token> ToTokenList(Parser.ProgramRegion region, List<string> argsName, List<List<TokenPart>> args)
            {
                List<Parser.Token> tokens = new List<Parser.Token>();

                for (int i = 0; i < Tokens.Count; i++)
                {
                    TokenPart tokenPart = Tokens[i];
                    if (tokenPart.Type == Parser.TokenType.String)
                    {
                        int idx = argsName.FindIndex((string s) => { return s == tokenPart.Content; });
                        if (idx == -1)
                        {
                            tokens.Add(tokenPart.ToToken(region));
                        }
                        else
                        {
                            foreach (TokenPart t in args[idx])
                            {
                                tokens.Add(t.ToToken(region));
                            }
                        }
                    }
                    else
                    {
                        tokens.Add(tokenPart.ToToken(region));
                    }
                }

                return tokens;
            }
            public override bool Equals(object obj)
            {
                return Equals(obj as Replacer);
            }

            public override int GetHashCode()
            {
                return Tokens.GetHashCode();
            }

            public bool Equals(Replacer r)
            {
                return Tokens.Equals(r.Tokens);
            }
        }

        public PreProcessor(Program program, List<Parser.Token> tokens)
        {
            previous = tokens;
            this.program = program;
            Transform();
        }

        List<Parser.Token> previous;
        public List<Parser.Token> Tokens { get; private set; }

        Program program;

        Dictionary<Symbol, SymbolInfo> symbols;

        private void Transform()
        {
            Tokens = new List<Parser.Token>();
            symbols = new Dictionary<Symbol, SymbolInfo>();

            int line = -1;

            InitCommonMacro();

            for (int i = 0; i < previous.Count - 1 /* EOF */; ++i)
            {
                Parser.Token token = previous[i];

                if (line != token.Location.Start.Line)
                {
                    line = token.Location.Start.Line;
                    Update(line_symbol, line.ToString(), Parser.TokenType.Integer);
                }
                Update(col_symbol, token.Location.Start.Columns.ToString(), Parser.TokenType.Integer);

                Symbol s = TokenToSymbol(token);
                if (symbols.ContainsKey(s))
                {
                    SymbolInfo info = symbols[s];

                    List<string> args = new List<string>();
                    List<List<TokenPart>> tokenParts = new List<List<TokenPart>>();

                    if (info.Args != null)
                    {
                        args = info.Args;
                        Utility.WriteLine("Arguments needed !", ConsoleColor.Yellow);
                        int ii = i;
                        ii++;
                        Parser.Token t = previous[ii];
                        if (t.Type == Parser.TokenType.ParenthesisL)
                        {
                            List<TokenPart> parts = new List<TokenPart>();
                            while (t.Type != Parser.TokenType.ParenthesisR)
                            {
                                ii++;
                                t = previous[ii];

                                if (t.Type == Parser.TokenType.Comma)
                                {
                                    tokenParts.Add(parts);
                                    parts = new List<TokenPart>();
                                }
                                else
                                {
                                    parts.Add(new TokenPart(t.Type, t.Content));
                                }
                            }

                            if (parts.Count != 0)
                            {
                                tokenParts.Add(parts);
                            }
                        }
                        else
                        {
                            Utility.WriteLine("EOF", ConsoleColor.Yellow);
                            continue;
                        }
                    }

                    if (tokenParts.Count != args.Count)
                    {
                        Utility.WriteLine("Error, " + args.Count + " argument expected, " + tokenParts.Count + " given.");
                        continue;
                    }

                    Utility.WriteLine("Replacement !", ConsoleColor.Magenta);

                    Tokens.AddRange(info.Replacer.ToTokenList(token.Location, args, tokenParts));
                }
                else
                {
                    Tokens.Add(token);
                }
            }

            Tokens.Add(previous.Last());

        }

        private Symbol line_symbol = new Symbol("__LINE__", Parser.TokenType.Ident);
        private Symbol col_symbol = new Symbol("__COLUMN__", Parser.TokenType.Ident);
        private Symbol file_symbol = new Symbol("__FILENAME__", Parser.TokenType.Ident);
        
        private void Update(Symbol key, string content, Parser.TokenType type)
        {
            Update(key, new SymbolInfo(new Replacer(content, type)));
        }

        private void Update(Symbol key, SymbolInfo info)
        {
            symbols[key] = info;
        }

        private void InitCommonMacro()
        {
            Update(file_symbol, program.Filename, Parser.TokenType.String);
        }

        private Symbol TokenToSymbol(Parser.Token token)
        {
            return new Symbol(token.Content, token.Type);
        }

    }
}
