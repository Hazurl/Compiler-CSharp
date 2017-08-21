using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Test
    {
        class TestParser
        {
            public static void DoTest ()
            {
                Test.Header("Parser");

                Test.Code("\\").ParsingError(Parser.ErrorType.UnknownCharacter);
                Test.Code("0.1.0").ParsingError(Parser.ErrorType.MultipleFloatPoint);
                Test.Code("0x25.2").ParsingError(Parser.ErrorType.CustomBaseAndFloat);
                Test.Code("10.2x24").ParsingError(Parser.ErrorType.FloatAndCutomBase);
                Test.Code("0dFF0024").ParsingError(Parser.ErrorType.WrongCustomBaseValue);
                Test.Code("1x32").ParsingError(Parser.ErrorType.BadCustomBasePrefix);
                Test.Code("00x32").ParsingError(Parser.ErrorType.BadCustomBasePrefix);
                Test.Code("0o").ParsingError(Parser.ErrorType.BadCustomBasePrefix);
                Test.Code("0k12").ParsingError(Parser.ErrorType.CustomBaseUnknown);
                Test.Code("\"\\y\"").ParsingError(Parser.ErrorType.UnknownEscapeSequence);
                Test.Code("'string").ParsingError(Parser.ErrorType.StringUnterminated);
            }
        }
    }
}
