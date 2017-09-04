using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Parser
    {
        class Parser
        {
            public Parser(Program program)
            {
                this.program = program;
                this.code = program.Code;
                Parse();
            }

            Program program;
            List<string> code;
            ProgramIterator mover;

            public List<Token> Tokens { get; private set; }
            public int ErrorCount { get; private set; }
            public List<Error> Errors { get; private set; }
            Error current;

            private void Parse()
            {
                mover = new ProgramIterator(code);

                Tokens = new List<Token>();
                Errors = new List<Error>();
                ErrorCount = 0;

                if (!mover.EmptyProgram())
                {
                    mover.Forward(); // place on the first character
                    while (!mover.EOF())
                    {
                        Token t = getNextToken();
                        Tokens.Add(t);

                        if (current != null)
                        {
                            current.SetToken(t);
                            Errors.Add(current);
                            ErrorCount += current.Errors.Count;
                            current = null;
                        }

                        if (mover.EOF())
                            break;
                        mover.Forward();
                    }
                }

                if (Tokens.Count == 0 || Tokens.Last().Type != TokenType.EOF)
                    Tokens.Add(new Token(TokenType.EOF, "", mover.RegionSince(mover.Position())));
            }

            private Token getNextToken()
            {
                string content = "";
                ProgramPosition start = mover.Position();

                bool in_string = false;
                bool double_quotes = false;

                int escape = 0;

                bool in_ident = false;

                bool in_num = false;
                bool is_custom_base = false;
                bool is_float = false;

                bool can_be_double = false;
                bool can_be_arrow = false;
                bool equal_operator = false;

                int commentary = 0;

                SkipBlank();
                
                if (mover.EOF())
                    return new Token(TokenType.EOF, "", mover.RegionSince(mover.Position()));

                {
                    char c = mover.Current();
                    start = mover.Position();

                    if (c == '#')
                    {
                        mover.Forward();
                        if (!mover.EOF() && mover.Current() == ':')
                        {
                            commentary++;
                        }
                        else
                        {
                            mover.NextLine();
                            return getNextToken();
                        }
                    }
                    else if(c == '"' || c == '\'')
                    {
                        in_string = true;
                        double_quotes = c == '"';
                    }
                    else if (IsLetter(c) || c == '_')
                    {
                        in_ident = true;
                        content += c;
                    }
                    else if (IsDigit(c))
                    {
                        in_num = true;
                        content += c;
                    }
                    else if (c == '-')
                    {
                        can_be_arrow = true;
                        can_be_double = CanBeDouble(c);
                        equal_operator = IsPartOfEqualOperator(c);
                        content += c;
                    }
                    else if (CanBeDouble(c))
                    {
                        content += c;
                        can_be_double = true;
                        equal_operator = IsPartOfEqualOperator(c);
                    }
                    else if (IsPartOfEqualOperator(c))
                    {
                        content += c;
                        equal_operator = true;
                    }
                    else if (c == '.')
                    {
                        mover.Forward();
                        if (mover.EOF() || !IsDigit(mover.Current()))
                        {
                            mover.Backward();
                            return new Token(TokenType.Dot, ".", mover.RegionSince(start));
                        }
                        mover.Backward();
                        in_num = is_float = true;
                        content += c;
                    }
                    else if (c == '{')
                        return new Token(TokenType.BraceL, "{", mover.RegionSince(start));
                    else if (c == '}')
                        return new Token(TokenType.BraceR, "}", mover.RegionSince(start));
                    else if (c == '[')
                        return new Token(TokenType.BracketL, "[", mover.RegionSince(start));
                    else if (c == ']')
                        return new Token(TokenType.BracketR, "]", mover.RegionSince(start));
                    else if (c == '(')
                        return new Token(TokenType.ParenthesisL, "(", mover.RegionSince(start));
                    else if (c == ')')
                        return new Token(TokenType.ParenthesisR, ")", mover.RegionSince(start));
                    else if (c == ';')
                        return new Token(TokenType.SemiColon, ";", mover.RegionSince(start));
                    else if (c == ',')
                        return new Token(TokenType.Comma, ",", mover.RegionSince(start));
                    else
                    {
                        AddError(mover.Position(), ErrorType.UnknownCharacter);
                        return new Token(TokenType.Unknown, new string(c, 1), mover.RegionSince(start));
                    }
                }

                do
                {
                    if (escape > 0)
                        escape--;

                    mover.Forward();

                    if (mover.EOF())
                        break;

                    char c = mover.Current();

                    if (commentary > 0)
                    {
                        if (c == ':')
                        {
                            mover.Forward();
                            if (mover.Current() == '#')
                            {
                                commentary--;
                                if (commentary == 0)
                                {
                                    mover.Forward();
                                    return getNextToken();
                                }
                            }
                            else
                            {
                                mover.Backward();
                            }
                        }

                        if (c == '#')
                        {
                            mover.Forward();
                            if (!mover.EOF() && mover.Current() == ':')
                            {
                                commentary++;
                            }
                            else
                            {
                                mover.Backward();
                            }
                        }
                        continue;
                    }

                    if (c == '\\' && escape == 0)
                    {
                        escape = 2;
                        continue;
                    }

                    if (in_string)
                    {
                        if (escape > 0)
                        {
                            if (c == 'n')
                            {
                                content += '\n';
                                continue;
                            }
                            else if (c == 't')
                            {
                                content += '\t';
                                continue;
                            }
                            else if (c == '"' && double_quotes)
                            {
                                content += "\"";
                                continue;
                            }
                            else if (c == '\'' && !double_quotes)
                            {
                                content += "'";
                                continue;
                            }
                            else
                            {
                                AddError(mover.Position(), ErrorType.UnknownEscapeSequence);
                                continue;
                            }
                        }
                        else
                        {
                            if (c == '"' && double_quotes || c == '\'' && !double_quotes)
                            {
                                return new Token(TokenType.String, content, mover.RegionSince(start));
                            }
                            else
                            {
                                if (mover.LineHasChanged())
                                    content += '\n';
                                content += c;
                                continue;
                            }
                        }
                    }

                    if (in_ident)
                    {
                        if (!mover.LineHasChanged() && (IsLetter(c) || IsDigit(c) || c == '_'))
                        {
                            content += c;
                            continue;
                        }
                        else
                        {
                            mover.Backward();
                            return new Token(TypeOfIdent(content), content, mover.RegionSince(start));
                        }
                    }

                    if (in_num)
                    {
                        if (!mover.LineHasChanged() && IsDigit(c))
                        {
                            content += c;
                            continue;
                        }
                        else if (!mover.LineHasChanged() && is_custom_base && (c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
                        {
                            content += c;
                            continue;
                        }
                        else if (!mover.LineHasChanged() && c == '.')
                        {
                            if (is_custom_base)
                            {
                                AddError(mover.Position(), ErrorType.CustomBaseAndFloat);
                                continue;
                            }

                            if (is_float)
                            {
                                AddError(mover.Position(), ErrorType.MultipleFloatPoint);
                                continue;
                            }

                            is_float = true;
                            content += c;
                            continue;
                        }
                        else if (!mover.LineHasChanged() && IsLetter(c))
                        {
                            if (is_custom_base)
                            {
                                AddError(mover.Position(), ErrorType.MultipleBase);
                                continue;
                            }
                            
                            if (is_float)
                            {
                                AddError(mover.Position(), ErrorType.FloatAndCutomBase);
                                continue;
                            }

                            is_custom_base = true;
                            content += c;
                            continue;
                        }
                        else
                        {
                            mover.Backward();
                            TokenType type = is_custom_base ? TokenType.CustomBaseNumber : is_float ? TokenType.Float : TokenType.Integer;
                            ProgramRegion reg = mover.RegionSince(start);

                            if (type == TokenType.CustomBaseNumber)
                                CheckNumberRepresentation(content, reg);

                            return new Token(type, content, reg);
                        }
                    }

                    if (can_be_arrow && c == '>')
                    {
                        content += c;
                        return new Token(TokenType.Arrow, content + c, mover.RegionSince(start));
                    }
                    else if (can_be_double && content[0] == c)
                    {
                        content += c;
                        return new Token(TypeOfDouble(content), content, mover.RegionSince(start));
                    }
                    else if (equal_operator && c == '=')
                    {
                        content += c;
                        return new Token(TypeOfEqualOperator(content), content, mover.RegionSince(start));
                    }
                    else
                    {
                        mover.Backward();
                        return new Token(TypeOfSimple(content), content, mover.RegionSince(start));
                    }

                } while (true);

                ProgramRegion region = mover.RegionSince(start);

                if (in_string)
                {
                    AddError(start, ErrorType.StringUnterminated);
                    return new Token(TokenType.Unknown, content, region);
                }
                else if (in_num)
                {
                    TokenType type = is_custom_base ? TokenType.CustomBaseNumber : is_float ? TokenType.Float : TokenType.Integer;

                    if (type == TokenType.CustomBaseNumber)
                        CheckNumberRepresentation(content, region);

                    return new Token(type, content, region);
                }
                else if (in_ident)
                {
                    return new Token(TypeOfIdent(content), content, region);
                }
                else if (can_be_arrow || can_be_double || equal_operator)
                {
                    return new Token(TypeOfSimple(content), content, region);
                }

                if (content.Length == 0)
                {
                    return new Token(TokenType.EOF, content, mover.RegionSince(mover.Position()));
                }
                
                AddError(mover.Position(), ErrorType.UnknownCharacter);
                return new Token(TokenType.Unknown, content, region);
            }

            private void SkipBlank()
            {
                while (!mover.EOF() && IsSpace(mover.Current()))
                    mover.Forward();
            }

            private bool IsSpace(char c)
            {
                return c == ' ' || c == '\t' || c == '\n';
            }

            private bool CheckNumberRepresentation(string content, ProgramRegion region)
            {
                if (content.Length <= 2 || content[0] != '0')
                {
                    AddError(region.Start, ErrorType.BadCustomBasePrefix);
                    return false;
                }

                char num_base = content[1];
                int real_base;

                if (num_base == 'b' || num_base == 'B')
                    real_base = 2;
                else if (num_base == 'd' || num_base == 'D')
                    real_base = 10;
                else if (num_base == 'x' || num_base == 'X')
                    real_base = 16;
                else if (num_base == 'o' || num_base == 'O')
                    real_base = 8;
                else
                {
                    if (IsLetter(num_base))
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + 1), ErrorType.CustomBaseUnknown);
                    else
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + 1), ErrorType.BadCustomBasePrefix);
                    return false;
                }

                bool res = true;

                for (int pos = 2; pos < content.Length; ++pos)
                {
                    try
                    {
                        int value = Convert.ToInt32(new string(content[pos], 1), real_base);
                    }
                    catch (Exception )
                    {
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + pos), ErrorType.WrongCustomBaseValue);
                        res = false;
                    }
                }

                return res;
            }

            private bool IsLetter(char c)
            {
                return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
            }

            private bool IsDigit(char c)
            {
                return c >= '0' && c <= '9';
            }

            private bool CanBeDouble(char c)
            {
                return c == '+' || c == '-' || c == '*' || c == '/' ||
                       c == '%' || c == '~' || c == '!' || c == '|' ||
                       c == '&' || c == '=' || c == '<' || c == '>' ||
                       c == '@' || c == '^' || c == ':';
            }

            private bool IsPartOfEqualOperator(char c)
            {
                return c == '+' || c == '-' || c == '*' || c == '|' || c == '^' || c == '&' ||
                       c == '/' || c == '=' || c == '~' || c == '>' || c == '<' || c == '!' ||
                       c == '%';
            }

            private TokenType TypeOfDouble(string content)
            {
                switch (content[0])
                {
                    case '+': return TokenType.DoublePlus;
                    case '-': return TokenType.DoubleMinus;
                    case '/': return TokenType.DoubleSlash;
                    case '*': return TokenType.DoubleStar;
                    case '%': return TokenType.DoublePercent;
                    case '|': return TokenType.DoubleOr;
                    case '&': return TokenType.DoubleAnd;
                    case '^': return TokenType.DoubleCaret;
                    case '~': return TokenType.DoubleTilde;
                    case '!': return TokenType.DoubleExclamation;
                    case '<': return TokenType.DoubleLess;
                    case '>': return TokenType.DoubleGreater;
                    case '=': return TokenType.DoubleEqual;
                    case '@': return TokenType.DoubleAt;
                    case ':': return TokenType.DoubleColon;
                }

                throw new Exception("TypeOfDouble not updated ?");
            }

            private TokenType TypeOfSimple(string content)
            {
                switch (content[0])
                {
                    case '+': return TokenType.Plus;
                    case '-': return TokenType.Minus;
                    case '/': return TokenType.Slash;
                    case '*': return TokenType.Star;
                    case '%': return TokenType.Percent;
                    case '|': return TokenType.Or;
                    case '&': return TokenType.And;
                    case '^': return TokenType.Caret;
                    case '~': return TokenType.Tilde;
                    case '!': return TokenType.Exclamation;
                    case '<': return TokenType.Less;
                    case '>': return TokenType.Greater;
                    case '=': return TokenType.Equal;
                    case '@': return TokenType.At;
                    case ':': return TokenType.Colon;
                }

                throw new Exception("TypeOfSimple not updated ?");
            }

            private TokenType TypeOfEqualOperator(string content)
            {
                switch (content[0])
                {
                    case '+': return TokenType.PlusEqual;
                    case '-': return TokenType.MinusEqual;
                    case '/': return TokenType.SlashEqual;
                    case '*': return TokenType.StarEqual;
                    case '%': return TokenType.PercentEqual;
                    case '|': return TokenType.OrEqual;
                    case '&': return TokenType.AndEqual;
                    case '^': return TokenType.CaretEqual;
                    case '~': return TokenType.TildeEqual;
                    case '!': return TokenType.ExclamationEqual;
                    case '<': return TokenType.LessEqual;
                    case '>': return TokenType.GreaterEqual;
                    case '=': return TokenType.DoubleEqual;
                }

                throw new Exception("TypeOfEqualOperator not updated ?");
            }

            private TokenType TypeOfIdent(string content)
            {
                switch(content)
                {
                    case "if": return TokenType.If;
                    case "else": return TokenType.Else;
                    case "do": return TokenType.Do;
                    case "while": return TokenType.While;
                    case "for": return TokenType.For;
                    case "foreach": return TokenType.Foreach;
                    case "in": return TokenType.In;
                    case "is": return TokenType.Is;
                    case "inherit": return TokenType.Inherit;
                    case "not": return TokenType.Not;
                    case "delete": return TokenType.Delete;
                    case "new": return TokenType.New;
                    case "return": return TokenType.Return;
                    case "auto": return TokenType.Auto;
                    case "class": return TokenType.Class;
                    case "template": return TokenType.Template;
                    case "where": return TokenType.Where;
                    case "pub": return TokenType.Public;
                    case "priv": return TokenType.Private;
                    case "get": return TokenType.Get;
                    case "set": return TokenType.Set;
                    case "ptr": return TokenType.Pointer;
                    case "ref": return TokenType.Reference;
                    case "cst": return TokenType.Constant;
                }

                return TokenType.Ident;
            }

            private void AddError (ProgramPosition position, ErrorType type)
            {
                if (current == null)
                {
                    current = new Error();
                }
                current.AddError(type, position);
            }
        }
    }
}
