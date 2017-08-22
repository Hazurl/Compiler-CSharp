using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    namespace Test
    {
        class Test
        {
            public interface ITestable
            {
                void ParsingError(Parser.ErrorType type);
                void Tokens(List<Parser.TokenType> tokens);
            }

            class CodeTest : ITestable
            {
                public CodeTest(Test test, string code)
                {
                    Program = new Program(code);
                    Compiler = new Compiler(Program);
                    Test = test;
                }

                public Program Program { get; private set; }
                public Compiler Compiler { get; private set; }
                public Test Test { get; private set; }

                public void ParsingError(Parser.ErrorType type)
                {
                    Compiler.SetMode(CompilerMode.Parsing);
                    var result = Compiler.Run();
                    if (result.ParsingErrorCount == 1 && result.ParsingErrors[0].Errors.ContainsKey(type))
                    {
                        Test.Pass(Program.ToString(), "ParsingError." + type.ToString());
                    }
                    else
                    {
                        string res = "";
                        if (result.ParsingErrorCount == 0)
                        {
                            res = "Nothing";
                        }
                        else if (result.ParsingErrorCount == 1)
                        {
                            res = "ParsingError." + result.ParsingErrors[0].Errors.Keys.ElementAt(0).ToString();
                        }
                        else
                        {
                            foreach (var errToken in result.ParsingErrors)
                            {
                                foreach (var err in errToken.Errors)
                                {
                                    if (res != "") res += ", ";
                                    res += "ParsingError." + err.Key;
                                }
                            }
                        }

                        Test.Fail(Program.ToString(), res, "ParsingError." + type.ToString());
                    }
                }

                public void Tokens(List<Parser.TokenType> expectedTypes)
                {
                    Compiler.SetMode(CompilerMode.Parsing);
                    var result = Compiler.Run();

                    string resString = "";
                    List<Parser.TokenType> resTokens = new List<Parser.TokenType>();
                    foreach (var token in result.Tokens)
                    {
                        if (resString != "") resString += ", ";
                        resString += "TokenType." + token.Type;

                        resTokens.Add(token.Type);
                    }

                    if (resTokens.SequenceEqual(expectedTypes))
                    {
                        Test.Pass(Program.ToString(), resString);
                    }
                    else
                    {
                        string expected = "";
                        foreach (var type in expectedTypes)
                        {
                            if (expected != "") expected += ", ";
                            expected += "TokenType." + type;
                        }

                        Test.Fail(Program.ToString(), resString, expected);
                    }
                }
            }

            Test(string name)
            {
                failedTests = new Dictionary<string, List<Tuple<string, string, string>>>();
                this.name = name;
                testCount = success = 0;
            }

            string name;
            string header;
            string section;

            long success;
            long testCount;
            Dictionary<string, List<Tuple<string, string, string>>> failedTests;

            void End()
            {
                Utility.WriteLine("");
                Utility.WriteLine(success + "/" + testCount + " tests sucessfuly done", success == testCount ? ConsoleColor.Green : ConsoleColor.Red);

                if (success < testCount)
                {
                    Utility.WriteLine("Errors:", ConsoleColor.Red);
                    foreach (var headerSection in failedTests)
                    {
                        if (headerSection.Value.Count == 0)
                        {
                            continue;
                        }

                        string key = headerSection.Key.ToString();
                        Utility.WriteLine("# " + key, ConsoleColor.Cyan);

                        foreach (var err in headerSection.Value)
                        {
                            Utility.Write("\t" + err.Item1);
                            Utility.Write(" =/=> ", ConsoleColor.Magenta);
                            Utility.Write(err.Item2);
                            Utility.Write(" but ", ConsoleColor.Magenta);
                            Utility.WriteLine(err.Item3);
                        }
                    }
                }
            }

            void Pass(string code, string result)
            {
                Utility.Write("\tOK", ConsoleColor.Green);
                Utility.Write("   : " + code);
                Utility.Write(" ===> ", ConsoleColor.Magenta);
                Utility.WriteLine(result);
                success++;
                testCount++;
            }

            void Fail(string code, string result, string expected)
            {

                Utility.Write("\tERROR", ConsoleColor.Red);

                string key = section == "" ? header : header + '.' + section;
                failedTests[key].Add( new Tuple<string, string, string>(code, expected, result));

                Utility.Write(": " + code);
                Utility.Write(" =/=> ", ConsoleColor.Magenta);
                Utility.Write(expected);
                Utility.Write(" but ", ConsoleColor.Magenta);
                Utility.WriteLine(result);

                testCount++;
            }

            void EnterHeader(string name)
            {
                header = name;
                section = "";
                Utility.WriteLine("# " + name, ConsoleColor.Cyan);
                if (!failedTests.ContainsKey(name))
                {
                    failedTests[name] = new List<Tuple<string, string, string>>();
                }
            }

            void EnterSection(string name)
            {
                section = name;
                Utility.WriteLine("## " + name, ConsoleColor.DarkCyan);
                if (!failedTests.ContainsKey(header + '.' + name))
                {
                    failedTests[header + '.' + name] = new List<Tuple<string, string, string>>();
                }
            }




            // STATIC

            static Test current;

            public static void NewSession(string name)
            {
                current = new Test(name);
            }

            public static void EndSession()
            {
                if (current == null)
                {
                    throw new Exception("You must start a new session end it");
                }
                current.End();
                current = null;
            }

            public static ITestable Code(string code)
            {
                if (current == null)
                {
                    throw new Exception("You must start a new Test session to perform test");
                }
                return new CodeTest(current, code);
            }

            public static void Header(string header)
            {
                if (current == null)
                {
                    throw new Exception("You must start a new Test session to perform test");
                }
                current.EnterHeader(header);
            }

            public static void Section(string section)
            {
                if (current == null)
                {
                    throw new Exception("You must start a new Test session to perform test");
                }
                current.EnterSection(section);
            }

            public static void All()
            {
                NewSession("Test All");

                TestParser.DoTest();

                EndSession();
            }

        }
    }
}