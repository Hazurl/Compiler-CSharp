using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Parser
    {
        enum ParsingErrorType
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

        class ParsingError
        {
            public static void Show(Program program, Token token, ParsingErrorType type, List<ProgramPosition> positions)
            {
                Utility.Write("Parsing error: ", ConsoleColor.Red);
                Utility.WriteLine(getMessage(type));

                Dictionary<int, LinkedList<ProgramPosition>> pos = new Dictionary<int, LinkedList<ProgramPosition>>();
                foreach(ProgramPosition p in positions)
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
                    foreach(ProgramPosition p in list)
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

            private static string getMessage(ParsingErrorType type)
            {
                switch (type)
                {
                    case ParsingErrorType.CustomBaseUnknown:
                        return "Base can only be 'b', 'o', 'd' or 'x'";
                    case ParsingErrorType.BadCustomBasePrefix:
                        return "To specify the base you must write '0x...' or '0b...' or '0d...' or '0o...'";
                    case ParsingErrorType.WrongCustomBaseValue:
                        return "The specified base is too low for the value";
                    case ParsingErrorType.MultipleFloatPoint:
                        return "Multiple floating point in the number is not valid";
                    case ParsingErrorType.MultipleBase:
                        return "Multiple base in the number is not valid";
                    case ParsingErrorType.FloatAndCutomBase:
                        return "The base of a floating number cannot be specified";
                    case ParsingErrorType.CustomBaseAndFloat:
                        return "A custom base number cannot be a floating";
                    case ParsingErrorType.StringUnterminated:
                        return "Quote open but not closed";
                    case ParsingErrorType.UnknownCharacter:
                        return "Unknown character";
                    case ParsingErrorType.UnknownEscapeSequence:
                        return "Unknown escape sequence";
                }
                throw new Exception("This type (Parsing ErrorType) has no message defined !");
            }
            
        }
    }
}
