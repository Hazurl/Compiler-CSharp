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

            BracketL, BracketR,                 // { }
            BraceL, BraceR,                     // [ ]
            ParenthesisL, ParenthesisR,         // ( )

            Ident,                              // uN_nom_DIdent00
            String,                             // "ok", 'ok'

            Integer,                            // 42, 1000
            Float,                              // 0.12, .1, 0.0
            CustomBaseNumber,                   // 5266x8, 00101b, 0003231x3
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
        }
    }
}
