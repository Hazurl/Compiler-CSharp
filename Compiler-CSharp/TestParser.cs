using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler_CSharp.Parser;

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

                Test.Code("\\").ParsingError(ErrorType.UnknownCharacter);
                Test.Code("0.1.0").ParsingError(ErrorType.MultipleFloatPoint);
                Test.Code("0x25.2").ParsingError(ErrorType.CustomBaseAndFloat);
                Test.Code("10.2x24").ParsingError(ErrorType.FloatAndCutomBase);
                Test.Code("0dFF0024").ParsingError(ErrorType.WrongCustomBaseValue);
                Test.Code("1x32").ParsingError(ErrorType.BadCustomBasePrefix);
                Test.Code("00x32").ParsingError(ErrorType.BadCustomBasePrefix);
                Test.Code("0o").ParsingError(ErrorType.BadCustomBasePrefix);
                Test.Code("0k12").ParsingError(ErrorType.CustomBaseUnknown);
                Test.Code("\"\\y\"").ParsingError(ErrorType.UnknownEscapeSequence);
                Test.Code("'string").ParsingError(ErrorType.StringUnterminated);

                Test.Section("Token");
                
                Test.Code("").Tokens(new List<TokenType>                     { TokenType.EOF });

                Test.Code("ok").Tokens(new List<TokenType>                   { TokenType.Ident, TokenType.EOF });
                Test.Code("'ok'").Tokens(new List<TokenType>                 { TokenType.String, TokenType.EOF });

                Test.Code("42").Tokens(new List<TokenType>                   { TokenType.Integer, TokenType.EOF });
                Test.Code("100.0").Tokens(new List<TokenType>                { TokenType.Float, TokenType.EOF });
                Test.Code(".45").Tokens(new List<TokenType>                  { TokenType.Float, TokenType.EOF });
                Test.Code("100.").Tokens(new List<TokenType>                 { TokenType.Float, TokenType.EOF });
                Test.Code("0xFA45b2").Tokens(new List<TokenType>             { TokenType.CustomBaseNumber, TokenType.EOF });
                Test.Code("0b0110").Tokens(new List<TokenType>               { TokenType.CustomBaseNumber, TokenType.EOF });
                Test.Code("0d0142").Tokens(new List<TokenType>               { TokenType.CustomBaseNumber, TokenType.EOF });
                Test.Code("0o7710").Tokens(new List<TokenType>               { TokenType.CustomBaseNumber, TokenType.EOF });

                Test.Code("()").Tokens(new List<TokenType>                   { TokenType.ParenthesisL, TokenType.ParenthesisR, TokenType.EOF });
                Test.Code("{}").Tokens(new List<TokenType>                   { TokenType.BraceL, TokenType.BraceR, TokenType.EOF });
                Test.Code("[]").Tokens(new List<TokenType>                   { TokenType.BracketL, TokenType.BracketR, TokenType.EOF });

                Test.Code("# com").Tokens(new List<TokenType>                { TokenType.EOF });
                Test.Code("# com", "0").Tokens(new List<TokenType>           { TokenType.Integer, TokenType.EOF });
                Test.Code("#:0:#0").Tokens(new List<TokenType>               { TokenType.Integer, TokenType.EOF });
                Test.Code("#:   0   :#0").Tokens(new List<TokenType>         { TokenType.Integer, TokenType.EOF });
                Test.Code("#:# :#0").Tokens(new List<TokenType>              { TokenType.Integer, TokenType.EOF });
                Test.Code("#: #::", "#:#0").Tokens(new List<TokenType>       { TokenType.Integer, TokenType.EOF });
                Test.Code("# :# 0").Tokens(new List<TokenType>               { TokenType.EOF });
                Test.Code("#:#:#0").Tokens(new List<TokenType>               { TokenType.EOF });

                Test.Code("+").Tokens(new List<TokenType> { TokenType.Plus, TokenType.EOF });
                Test.Code("-").Tokens(new List<TokenType> { TokenType.Minus, TokenType.EOF });
                Test.Code("*").Tokens(new List<TokenType> { TokenType.Star, TokenType.EOF });
                Test.Code("/").Tokens(new List<TokenType> { TokenType.Slash, TokenType.EOF });
                Test.Code("%").Tokens(new List<TokenType> { TokenType.Percent, TokenType.EOF });
                Test.Code("|").Tokens(new List<TokenType> { TokenType.Or, TokenType.EOF });
                Test.Code("&").Tokens(new List<TokenType> { TokenType.And, TokenType.EOF });
                Test.Code("~").Tokens(new List<TokenType> { TokenType.Tilde, TokenType.EOF });
                Test.Code("^").Tokens(new List<TokenType> { TokenType.Caret, TokenType.EOF });
                Test.Code("=").Tokens(new List<TokenType> { TokenType.Equal, TokenType.EOF });
                Test.Code("<").Tokens(new List<TokenType> { TokenType.Less, TokenType.EOF });
                Test.Code(">").Tokens(new List<TokenType> { TokenType.Greater, TokenType.EOF });
                Test.Code("@").Tokens(new List<TokenType> { TokenType.At, TokenType.EOF });
                Test.Code(":").Tokens(new List<TokenType> { TokenType.Colon, TokenType.EOF });
                Test.Code(",").Tokens(new List<TokenType> { TokenType.Comma, TokenType.EOF });
                Test.Code(";").Tokens(new List<TokenType> { TokenType.SemiColon, TokenType.EOF });
                Test.Code("!").Tokens(new List<TokenType> { TokenType.Exclamation, TokenType.EOF });
                Test.Code(".").Tokens(new List<TokenType> { TokenType.Dot, TokenType.EOF });

                Test.Code("->").Tokens(new List<TokenType> { TokenType.Arrow, TokenType.EOF });
                Test.Code("<=").Tokens(new List<TokenType> { TokenType.LessEqual, TokenType.EOF });
                Test.Code(">=").Tokens(new List<TokenType> { TokenType.GreaterEqual, TokenType.EOF });
                Test.Code("!=").Tokens(new List<TokenType> { TokenType.ExclamationEqual, TokenType.EOF });
                Test.Code("+=").Tokens(new List<TokenType> { TokenType.PlusEqual, TokenType.EOF });
                Test.Code("-=").Tokens(new List<TokenType> { TokenType.MinusEqual, TokenType.EOF });
                Test.Code("*=").Tokens(new List<TokenType> { TokenType.StarEqual, TokenType.EOF });
                Test.Code("/=").Tokens(new List<TokenType> { TokenType.SlashEqual, TokenType.EOF });
                Test.Code("%=").Tokens(new List<TokenType> { TokenType.PercentEqual, TokenType.EOF });
                Test.Code("~=").Tokens(new List<TokenType> { TokenType.TildeEqual, TokenType.EOF });
                Test.Code("^=").Tokens(new List<TokenType> { TokenType.CaretEqual, TokenType.EOF });
                Test.Code("|=").Tokens(new List<TokenType> { TokenType.OrEqual, TokenType.EOF });
                Test.Code("&=").Tokens(new List<TokenType> { TokenType.AndEqual, TokenType.EOF });
                Test.Code("==").Tokens(new List<TokenType> { TokenType.DoubleEqual, TokenType.EOF });

                Test.Code("<<").Tokens(new List<TokenType> { TokenType.DoubleLess, TokenType.EOF });
                Test.Code(">>").Tokens(new List<TokenType> { TokenType.DoubleGreater, TokenType.EOF });
                Test.Code("++").Tokens(new List<TokenType> { TokenType.DoublePlus, TokenType.EOF });
                Test.Code("--").Tokens(new List<TokenType> { TokenType.DoubleMinus, TokenType.EOF });
                Test.Code("**").Tokens(new List<TokenType> { TokenType.DoubleStar, TokenType.EOF });
                Test.Code("//").Tokens(new List<TokenType> { TokenType.DoubleSlash, TokenType.EOF });
                Test.Code("%%").Tokens(new List<TokenType> { TokenType.DoublePercent, TokenType.EOF });
                Test.Code("||").Tokens(new List<TokenType> { TokenType.DoubleOr, TokenType.EOF });
                Test.Code("&&").Tokens(new List<TokenType> { TokenType.DoubleAnd, TokenType.EOF });
                Test.Code("~~").Tokens(new List<TokenType> { TokenType.DoubleTilde, TokenType.EOF });
                Test.Code("^^").Tokens(new List<TokenType> { TokenType.DoubleCaret, TokenType.EOF });
                Test.Code("::").Tokens(new List<TokenType> { TokenType.DoubleColon, TokenType.EOF });
                Test.Code("@@").Tokens(new List<TokenType> { TokenType.DoubleAt, TokenType.EOF });
                Test.Code("!!").Tokens(new List<TokenType> { TokenType.DoubleExclamation, TokenType.EOF });

                Test.Code(" ->- **@< =.(ok .0 .^=!~~").Tokens(new List<TokenType>
                {
                    TokenType.Arrow, TokenType.Minus, TokenType.DoubleStar, TokenType.At, TokenType.Less, TokenType.Equal, TokenType.Dot, TokenType.ParenthesisL,
                    TokenType.Ident, TokenType.Float, TokenType.Dot, TokenType.CaretEqual, TokenType.Exclamation, TokenType.DoubleTilde, TokenType.EOF
                });

                Test.Code("if").Tokens(new List<TokenType> { TokenType.If, TokenType.EOF });
                Test.Code("else").Tokens(new List<TokenType> { TokenType.Else, TokenType.EOF });
                Test.Code("do").Tokens(new List<TokenType> { TokenType.Do, TokenType.EOF });
                Test.Code("while").Tokens(new List<TokenType> { TokenType.While, TokenType.EOF });
                Test.Code("for").Tokens(new List<TokenType> { TokenType.For, TokenType.EOF });
                Test.Code("foreach").Tokens(new List<TokenType> { TokenType.Foreach, TokenType.EOF });
                Test.Code("is").Tokens(new List<TokenType> { TokenType.Is, TokenType.EOF });
                Test.Code("in").Tokens(new List<TokenType> { TokenType.In, TokenType.EOF });
                Test.Code("inherit").Tokens(new List<TokenType> { TokenType.Inherit, TokenType.EOF });
                Test.Code("not").Tokens(new List<TokenType> { TokenType.Not, TokenType.EOF });
                Test.Code("class").Tokens(new List<TokenType> { TokenType.Class, TokenType.EOF });
                Test.Code("template").Tokens(new List<TokenType> { TokenType.Template, TokenType.EOF });
                Test.Code("where").Tokens(new List<TokenType> { TokenType.Where, TokenType.EOF });
                Test.Code("pub").Tokens(new List<TokenType> { TokenType.Public, TokenType.EOF });
                Test.Code("priv").Tokens(new List<TokenType> { TokenType.Private, TokenType.EOF });
                Test.Code("get").Tokens(new List<TokenType> { TokenType.Get, TokenType.EOF });
                Test.Code("set").Tokens(new List<TokenType> { TokenType.Set, TokenType.EOF });
                Test.Code("ptr").Tokens(new List<TokenType> { TokenType.Pointer, TokenType.EOF });
                Test.Code("ref").Tokens(new List<TokenType> { TokenType.Reference, TokenType.EOF });
                Test.Code("cst").Tokens(new List<TokenType> { TokenType.Constant, TokenType.EOF });
                Test.Code("delete").Tokens(new List<TokenType> { TokenType.Delete, TokenType.EOF });
                Test.Code("new").Tokens(new List<TokenType> { TokenType.New, TokenType.EOF });
                Test.Code("auto").Tokens(new List<TokenType> { TokenType.Auto, TokenType.EOF });
                Test.Code("return").Tokens(new List<TokenType> { TokenType.Return, TokenType.EOF });
            }
        }
    }
}
