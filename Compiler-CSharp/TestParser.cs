using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ParserET = Compiler_CSharp.Parser.ErrorType;
using ParserTT = Compiler_CSharp.Parser.TokenType;

namespace Compiler_CSharp
{
    namespace Test
    {
        class TestParser
        {
            public static void DoTest ()
            {
                Test.Header("Parser");

                Test.Section("Error");

                Test.Code("\\").ParsingError(ParserET.UnknownCharacter);
                Test.Code("0.1.0").ParsingError(ParserET.MultipleFloatPoint);
                Test.Code("0x25.2").ParsingError(ParserET.CustomBaseAndFloat);
                Test.Code("10.2x24").ParsingError(ParserET.FloatAndCutomBase);
                Test.Code("0dFF0024").ParsingError(ParserET.WrongCustomBaseValue);
                Test.Code("1x32").ParsingError(ParserET.BadCustomBasePrefix);
                Test.Code("00x32").ParsingError(ParserET.BadCustomBasePrefix);
                Test.Code("0o").ParsingError(ParserET.BadCustomBasePrefix);
                Test.Code("0k12").ParsingError(ParserET.CustomBaseUnknown);
                Test.Code("\"\\y\"").ParsingError(ParserET.UnknownEscapeSequence);
                Test.Code("'string").ParsingError(ParserET.StringUnterminated);

                Test.Section("Token");
                
                Test.Code("").Tokens(new List<ParserTT>         { ParserTT.EOF });
                Test.Code("ok").Tokens(new List<ParserTT>       { ParserTT.Ident, ParserTT.EOF });
                Test.Code("'ok'").Tokens(new List<ParserTT>     { ParserTT.String, ParserTT.EOF });
                Test.Code("42").Tokens(new List<ParserTT>       { ParserTT.Integer, ParserTT.EOF });
                Test.Code("100.0").Tokens(new List<ParserTT>    { ParserTT.Float, ParserTT.EOF });
                Test.Code(".45").Tokens(new List<ParserTT>      { ParserTT.Float, ParserTT.EOF });
                Test.Code("100.").Tokens(new List<ParserTT>     { ParserTT.Float, ParserTT.EOF });
                Test.Code("0xFA45b2").Tokens(new List<ParserTT> { ParserTT.CustomBaseNumber, ParserTT.EOF });
                Test.Code("0b0110").Tokens(new List<ParserTT>   { ParserTT.CustomBaseNumber, ParserTT.EOF });
                Test.Code("0d0142").Tokens(new List<ParserTT>   { ParserTT.CustomBaseNumber, ParserTT.EOF });
                Test.Code("0o7710").Tokens(new List<ParserTT>   { ParserTT.CustomBaseNumber, ParserTT.EOF });
                Test.Code("()").Tokens(new List<ParserTT>       { ParserTT.ParenthesisL, ParserTT.ParenthesisR, ParserTT.EOF });
                Test.Code("{}").Tokens(new List<ParserTT>       { ParserTT.BracketL, ParserTT.BracketR, ParserTT.EOF });
                Test.Code("[]").Tokens(new List<ParserTT>       { ParserTT.BraceL, ParserTT.BraceR, ParserTT.EOF });
            }
        }
    }
}
