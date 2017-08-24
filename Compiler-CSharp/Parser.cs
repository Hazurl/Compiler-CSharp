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
            int line;
            int col;
            bool line_has_changed = false;

            public List<Token> Tokens { get; private set; }
            public int ErrorCount { get; private set; }
            public List<Error> Errors { get; private set; }
            Error current;

            private void Parse()
            {
                col = 0;
                line = 0;

                Tokens = new List<Token>();
                Errors = new List<Error>();
                ErrorCount = 0;

                if (!CheckEmptyProgram())
                {
                    while (!EndOfFile())
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

                        if (EndOfFile())
                            break;
                        MoveForward();
                    }
                }

                if (Tokens.Count == 0 || Tokens.Last().Type != TokenType.EOF)
                    Tokens.Add(new Token(TokenType.EOF, "", getRegion(getPosition())));
            }

            private Token getNextToken()
            {
                string content = "";
                ProgramPosition start = getPosition();

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
                
                if (EndOfFile())
                    return new Token(TokenType.EOF, "", getRegion(getPosition()));

                {
                    char c = CurrentChar();
                    start = getPosition();

                    if (c == '#')
                    {
                        MoveForward();
                        if (!EndOfFile() && CurrentChar() == ':')
                        {
                            commentary++;
                        }
                        else
                        {
                            MoveToNextLine();
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
                        MoveForward();
                        if (EndOfFile() || !IsDigit(CurrentChar()))
                        {
                            MoveBackward();
                            return new Token(TokenType.Dot, ".", getRegion(start));
                        }
                        MoveBackward();
                        in_num = is_float = true;
                        content += c;
                    }
                    else if (c == '{')
                        return new Token(TokenType.BraceL, "{", getRegion(start));
                    else if (c == '}')
                        return new Token(TokenType.BraceR, "}", getRegion(start));
                    else if (c == '[')
                        return new Token(TokenType.BracketL, "[", getRegion(start));
                    else if (c == ']')
                        return new Token(TokenType.BracketR, "]", getRegion(start));
                    else if (c == '(')
                        return new Token(TokenType.ParenthesisL, "(", getRegion(start));
                    else if (c == ')')
                        return new Token(TokenType.ParenthesisR, ")", getRegion(start));
                    else if (c == ';')
                        return new Token(TokenType.SemiColon, ";", getRegion(start));
                    else if (c == ',')
                        return new Token(TokenType.Comma, ",", getRegion(start));
                    else
                    {
                        AddError(getPosition(), ErrorType.UnknownCharacter);
                        return new Token(TokenType.Unknown, new string(c, 1), getRegion(start));
                    }
                }

                do
                {
                    if (escape > 0)
                        escape--;

                    MoveForward();

                    if (EndOfFile())
                        break;

                    char c = CurrentChar();

                    if (commentary > 0)
                    {
                        if (c == ':')
                        {
                            MoveForward();
                            if (CurrentChar() == '#')
                            {
                                commentary--;
                                if (commentary == 0)
                                {
                                    MoveForward();
                                    return getNextToken();
                                }
                            }
                            else
                            {
                                MoveBackward();
                            }
                        }

                        if (c == '#')
                        {
                            MoveForward();
                            if (!EndOfFile() && CurrentChar() == ':')
                            {
                                commentary++;
                            }
                            else
                            {
                                MoveBackward();
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
                                AddError(getPosition(), ErrorType.UnknownEscapeSequence);
                                continue;
                            }
                        }
                        else
                        {
                            if (c == '"' && double_quotes || c == '\'' && !double_quotes)
                            {
                                return new Token(TokenType.String, content, getRegion(start));
                            }
                            else
                            {
                                if (line_has_changed)
                                    content += '\n';
                                content += c;
                                continue;
                            }
                        }
                    }

                    if (in_ident)
                    {
                        if (!line_has_changed && (IsLetter(c) || IsDigit(c) || c == '_'))
                        {
                            content += c;
                            continue;
                        }
                        else
                        {
                            MoveBackward();
                            return new Token(TypeOfIdent(content), content, getRegion(start));
                        }
                    }

                    if (in_num)
                    {
                        if (!line_has_changed && IsDigit(c))
                        {
                            content += c;
                            continue;
                        }
                        else if (!line_has_changed && is_custom_base && (c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F'))
                        {
                            content += c;
                            continue;
                        }
                        else if (!line_has_changed && c == '.')
                        {
                            if (is_custom_base)
                            {
                                AddError(getPosition(), ErrorType.CustomBaseAndFloat);
                                continue;
                            }

                            if (is_float)
                            {
                                AddError(getPosition(), ErrorType.MultipleFloatPoint);
                                continue;
                            }

                            is_float = true;
                            content += c;
                            continue;
                        }
                        else if (!line_has_changed && IsLetter(c))
                        {
                            if (is_custom_base)
                            {
                                AddError(getPosition(), ErrorType.MultipleBase);
                                continue;
                            }
                            
                            if (is_float)
                            {
                                AddError(getPosition(), ErrorType.FloatAndCutomBase);
                                continue;
                            }

                            is_custom_base = true;
                            content += c;
                            continue;
                        }
                        else
                        {
                            MoveBackward();
                            TokenType type = is_custom_base ? TokenType.CustomBaseNumber : is_float ? TokenType.Float : TokenType.Integer;
                            ProgramRegion reg = getRegion(start);

                            if (type == TokenType.CustomBaseNumber)
                                CheckNumberRepresentation(content, reg);

                            return new Token(type, content, reg);
                        }
                    }

                    if (can_be_arrow && c == '>')
                    {
                        content += c;
                        return new Token(TokenType.Arrow, content + c, getRegion(start));
                    }
                    else if (can_be_double && content[0] == c)
                    {
                        content += c;
                        return new Token(TypeOfDouble(content), content, getRegion(start));
                    }
                    else if (equal_operator && c == '=')
                    {
                        content += c;
                        return new Token(TypeOfEqualOperator(content), content, getRegion(start));
                    }
                    else
                    {
                        MoveBackward();
                        return new Token(TypeOfSimple(content), content, getRegion(start));
                    }

                } while (true);

                ProgramRegion region = getRegion(start);

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
                    return new Token(TokenType.EOF, content, getRegion(getPosition()));
                }
                
                AddError(getPosition(), ErrorType.UnknownCharacter);
                return new Token(TokenType.Unknown, content, region);
            }

            private bool EndOfFile()
            {
                return line >= code.Count;
            }

            private char CurrentChar()
            {
                return code[line][col];
            }

            private char NextChar()
            {
                MoveForward();
                char c = CurrentChar();
                MoveBackward();
                return c;
            }

            private void SkipBlank()
            {
                while (!EndOfFile() && IsSpace(CurrentChar()))
                    MoveForward();
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

            private void MoveForward()
            {
                line_has_changed = false;
                col++;

                while (!EndOfFile() && col >= code[line].Length)
                {
                    line_has_changed = true;
                    line++;
                    col = 0;
                }
            }

            private void MoveToNextLine()
            {
                int last_line = line;
                while(!EndOfFile() && last_line == line)
                {
                    MoveForward();
                }
            }

            private bool CheckEmptyProgram()
            {
                int _col = col;
                int _line = line;

                col--;
                MoveForward();
                if (EndOfFile())
                {
                    col = _col;
                    line = _line;
                    line_has_changed = false;
                    return true;
                }
                else
                {
                    col = _col;
                    line = _line;
                    line_has_changed = false;
                    return false;
                }
            }

            private void MoveBackward()
            {
                line_has_changed = false;
                while (col == 0)
                {
                    if (line == 0)
                        throw new Exception("Cannot move backward when the cursor is on the first character !");
                    line--;
                    line_has_changed = true;
                    col = code[line].Length;
                }
                col--;
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

            private ProgramPosition getPosition()
            {
                return new ProgramPosition(line, col);
            }

            private ProgramRegion getRegion(ProgramPosition start)
            {
                return new ProgramRegion(start, getPosition());
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
