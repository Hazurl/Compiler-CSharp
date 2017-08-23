using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Parser
    {
        enum TokenType
        {
            Unknown,
            EOF,

            // Bracket
            BraceL, BraceR,                                     // { }
            BracketL, BracketR,                                 // [ ]
            ParenthesisL, ParenthesisR,                         // ( )

            // Basic
            Ident,                                              // uN_nom_DIdent00
            String,                                             // "ok", 'ok'
            Integer,                                            // 42, 1000
            Float,                                              // 0.12, .1, 0.0
            CustomBaseNumber,                                   // 5266x8, 00101b, 0003231x3
            
            // Simple Operator
            Plus, Minus, Slash, Star,                           // + - / *
            Equal,                                              // =
            Less, Greater,                                      // < >
            Exclamation, Tilde, At,                             // ! ~ @
            Percent, Caret,                                     // % ^
            And, Or,                                            // & |

            // Double Operator
            DoubleEqual, LessEqual, GreaterEqual, ExclamationEqual,// == <= >= !=
            PlusEqual, MinusEqual, SlashEqual, StarEqual,      // += -= /= *=
            TildeEqual, PercentEqual, CaretEqual,               // ~= %= ^=
            AndEqual, OrEqual,                                  // &= |=
            DoubleLess, DoubleGreater,                          // << >>
            DoublePlus, DoubleMinus, DoubleSlash, DoubleStar,   // ++ -- // **
            DoubleExclamation, DoubleTilde, DoubleAt,           // !! ~~ @@
            DoublePercent, DoubleCaret,                         // %% ^^
            DoubleAnd, DoubleOr,                                // && ||

            // Ponctuation
            Colon, SemiColon, Comma,                            // : ; ,
            DoubleColon,                                        // ::
            Dot,                                                // .
            Arrow,                                              // ->

            // Keyword
            If, Else,
            Do, While,
            For,
            Foreach,

            In, Is, Inherit, Not,

            Delete, New, Return, Auto,
            Class, Template, Where, Public, Private, Get, Set,
            Pointer, Reference, Constant,
        }

        class Token
        {
            public Token(TokenType type, string content, ProgramRegion location)
            {
                Location = location;
                Content = content;
                Type = type;
            }

            public ProgramRegion Location { get; private set; }
            public string Content { get; private set; }
            public TokenType Type { get; private set; }

            public override string ToString()
            {
                return "Token [" + Type.ToString() + ":" + Content.ToString() + "] at " + Location;
            }

            public bool IsSame(Token token)
            {
                return Type == token.Type && Content == token.Content;
            }
        }
    }
}
