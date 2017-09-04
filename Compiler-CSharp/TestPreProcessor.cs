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
                Test.Code("__LINE__").Preprocesseur(new List<string> { "1" });
                Test.Code("__COLUMN__").Preprocesseur(new List<string> { "0" });
                Test.Code("", "__LINE__").Preprocesseur(new List<string> { "", "2" });
                Test.Code("", " __COLUMN__").Preprocesseur(new List<string> { "", " 1" });
                Test.Code("__FILENAME__").Preprocesseur(new List<string> { "DefaultFilenameTest.txt" });
                Test.Code("__STR__(ok tout va bien)").Preprocesseur(new List<string> { "ok tout va bien" });
            }
        }
    }
}
