using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Parser
    {
        enum ErrorType
        {
            UnknownCharacter, 

            MultipleFloatPoint,     // 0.0.0
            MultipleBase,           // 0x0x0
            CustomBaseAndFloat,     // 0x25.2
            FloatAndCutomBase,      // 0.0x12
            WrongCustomBaseValue,   // 0b2
            BadCustomBasePrefix,    // 1x32 or 00x2 or 0b
            CustomBaseUnknown,      // 0j56

            UnknownEscapeSequence,  // \p
            StringUnterminated      // "ok
        }

        class Error
        {
            public Error(Token token)
            {
                this.token = token;
                Errors = new Dictionary<ErrorType, List<ProgramPosition>>();
            }

            public Error()
            {
                Errors = new Dictionary<ErrorType, List<ProgramPosition>>();
            }

            public void SetToken(Token token)
            {
                this.token = token;
            }

            public void AddError(ErrorType type, ProgramPosition position)
            {
                if (!Errors.ContainsKey(type))
                {
                    Errors[type] = new List<ProgramPosition>();
                }
                Errors[type].Add(position);
            }

            public void AddErrors(ErrorType type, List<ProgramPosition> positions)
            {
                if (!Errors.ContainsKey(type))
                {
                    Errors[type] = new List<ProgramPosition>();
                }
                Errors[type].AddRange(positions);
            }

            Token token;
            public Dictionary<ErrorType, List<ProgramPosition>> Errors { get; private set; }

            public void Show(Program program)
            {
                foreach (var errorPair in Errors)
                {
                    ErrorType type = errorPair.Key;
                    List<ProgramPosition> positions = errorPair.Value;

                    Utility.Write("Parsing error: ", ConsoleColor.Red);
                    Utility.WriteLine(getMessage(type));

                    Dictionary<int, LinkedList<ProgramPosition>> pos = new Dictionary<int, LinkedList<ProgramPosition>>();
                    foreach (ProgramPosition p in positions)
                    {
                        if (!pos.ContainsKey(p.Line))
                        {
                            pos[p.Line] = new LinkedList<ProgramPosition>();
                        }

                        pos[p.Line].AddFirst(p);
                    }

                    foreach (var pair in pos)
                    {
                        LinkedList<ProgramPosition> list = pair.Value;
                        StringBuilder str = new StringBuilder("");
                        Utility.WriteLine(program.Code[pair.Key]);
                        foreach (ProgramPosition p in list)
                        {
                            int col = p.Columns;
                            if (col < str.Length)
                            {
                                str[col] = '^';
                            }
                            else
                            {
                                str.Append(new string(' ', col - str.Length) + '^');
                            }
                        }

                        Utility.WriteLine(str.ToString(), ConsoleColor.Red);
                    }
                }
            }

            private static string getMessage(ErrorType type)
            {
                switch (type)
                {
                    case ErrorType.CustomBaseUnknown:
                        return "Base can only be 'b', 'o', 'd' or 'x'";
                    case ErrorType.BadCustomBasePrefix:
                        return "To specify the base you must write '0x...' or '0b...' or '0d...' or '0o...'";
                    case ErrorType.WrongCustomBaseValue:
                        return "The specified base is too low for the value";
                    case ErrorType.MultipleFloatPoint:
                        return "Multiple floating point in the number is not valid";
                    case ErrorType.MultipleBase:
                        return "Multiple base in the number is not valid";
                    case ErrorType.FloatAndCutomBase:
                        return "The base of a floating number cannot be specified";
                    case ErrorType.CustomBaseAndFloat:
                        return "A custom base number cannot be a floating";
                    case ErrorType.StringUnterminated:
                        return "Quote open but not closed";
                    case ErrorType.UnknownCharacter:
                        return "Unknown character";
                    case ErrorType.UnknownEscapeSequence:
                        return "Unknown escape sequence";
                }
                throw new Exception("This type (Parsing ErrorType) has no message defined !");
            }
            
        }
    }
}
