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
            public Dictionary<Token, Dictionary<ParsingErrorType, List<ProgramPosition>>> Errors { get; private set; }
            public int ErrorCount { get; private set; }

            Dictionary<ParsingErrorType, List<ProgramPosition>> promiseErrors;
            
            private void Parse()
            {
                col = 0;
                line = 0;

                Tokens = new List<Token>();
                Errors = new Dictionary<Token, Dictionary<ParsingErrorType, List<ProgramPosition>>>();
                promiseErrors = new Dictionary<ParsingErrorType, List<ProgramPosition>>();
                ErrorCount = 0;

                while (! EndOfFile())
                {
                    Token t = getNextToken();
                    AddPromiseErrors(t);
                    Tokens.Add(t);
                    if (EndOfFile())
                        break;
                    MoveForward();
                }

                if (Tokens.Last().Type != TokenType.EOF)
                    Tokens.Add(new Token(TokenType.EOF, "", getRegion(getPosition())));
            }

            private void AddPromiseErrors(Token t)
            {
                ErrorCount += promiseErrors.Count;
                Errors.Add(t, promiseErrors);
                promiseErrors = new Dictionary<ParsingErrorType, List<ProgramPosition>>();
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

                SkipBlank();
                
                if (EndOfFile())
                    return new Token(TokenType.EOF, "", getRegion(getPosition()));

                {
                    char c = CurrentChar();
                    start = getPosition();

                    if (c == '"' || c == '\'')
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
                    else if (c == '.')
                    {
                        in_num = is_float = true;
                        content += c;
                    }
                    else if (c == '{')
                        return new Token(TokenType.BracketL, "{", getRegion(start));
                    else if (c == '}')
                        return new Token(TokenType.BracketR, "}", getRegion(start));
                    else if (c == '[')
                        return new Token(TokenType.BraceL, "[", getRegion(start));
                    else if (c == ']')
                        return new Token(TokenType.BraceR, "]", getRegion(start));
                    else if (c == '(')
                        return new Token(TokenType.ParenthesisL, "(", getRegion(start));
                    else if (c == ')')
                        return new Token(TokenType.ParenthesisR, ")", getRegion(start));

                    else
                    {
                        AddError(getPosition(), ParsingErrorType.UnknownCharacter);
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
                                AddError(getPosition(), ParsingErrorType.UnknownEscapeSequence);
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
                            return new Token(TokenType.Ident, content, getRegion(start));
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
                                AddError(getPosition(), ParsingErrorType.CustomBaseAndFloat);
                                continue;
                            }

                            if (is_float)
                            {
                                AddError(getPosition(), ParsingErrorType.MultipleFloatPoint);
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
                                AddError(getPosition(), ParsingErrorType.MultipleBase);
                                continue;
                            }
                            
                            if (is_float)
                            {
                                AddError(getPosition(), ParsingErrorType.BadCustomBasePrefix);
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


                } while (true);

                ProgramRegion region = getRegion(start);

                if (in_string)
                {
                    AddError(start, ParsingErrorType.StringUnterminated);
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
                    return new Token(TokenType.Ident, content, region);
                }

                if (content.Length == 0)
                {
                    return new Token(TokenType.EOF, content, getRegion(getPosition()));
                }
                
                AddError(getPosition(), ParsingErrorType.UnknownCharacter);
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
                    AddError(region.Start, ParsingErrorType.BadCustomBasePrefix);
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
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + 1), ParsingErrorType.CustomBaseUnknown);
                    else
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + 1), ParsingErrorType.BadCustomBasePrefix);
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
                        AddError(new ProgramPosition(region.Start.Line, region.Start.Columns + pos), ParsingErrorType.WrongCustomBaseValue);
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
            
            private ProgramPosition getPosition()
            {
                return new ProgramPosition(line, col);
            }

            private ProgramRegion getRegion(ProgramPosition start)
            {
                return new ProgramRegion(start, getPosition());
            }

            private void AddError (ProgramPosition position, ParsingErrorType type)
            {
                if (!promiseErrors.ContainsKey(type))
                    promiseErrors.Add(type, new List<ProgramPosition>());
                promiseErrors[type].Add(position);
            }
        }
    }
}
