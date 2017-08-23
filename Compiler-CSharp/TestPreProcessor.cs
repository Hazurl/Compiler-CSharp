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
        class TestPreProcessor
        {
            public static void DoTest()
            {
                Test.Header("PreProcessor");
                Test.Code("__LINE__").PreprocesseurTokens(new List<TokenType> { TokenType.Integer, TokenType.EOF });
                Test.Code("__COLUMN__").PreprocesseurTokens(new List<TokenType> { TokenType.Integer, TokenType.EOF });
            }
        }
    }
}
