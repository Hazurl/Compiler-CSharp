using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Compiler_CSharp
{
    class Application
    {
        static string ProjectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        static string TestDirectory = Path.Combine(ProjectDirectory, "Test");

        static void Main(string[] args)
        {
            Program program = Program.LoadfromFile(args.Count() > 2 ? args[1] : Path.Combine(TestDirectory, "test.txt"));
            Compiler compiler = new Compiler(program);

            var result = compiler.Run();
            if (result.Sucess)
            {
                Utility.WriteLine("Compilation Sucess (" + result.ParsingTime.Milliseconds + "ms) !");
            }
            else
            {
                foreach (var tokenErr in result.ParsingErrors)
                {
                    foreach (var err in tokenErr.Value)
                    {
                        Parser.ParsingError.Show(program, tokenErr.Key, err.Key, err.Value);
                    }
                }

                Utility.WriteLine("Compilation failed !");
            }

            /*
            if (parser.Errors.Count > 0)
            {
                Console.WriteLine("Errors : ");
                foreach(Parser.ParsingError err in parser.Errors)
                {
                    err.Show(program);
                    Utility.Pause();
                }
                Console.WriteLine();
            }
            */



            /*
            foreach (var t in tokens)
            {
                Console.ReadLine();
                if (t.Type != Parser.TokenType.EOF)
                    Console.WriteLine(t.ToString().Replace("\n", "\\n") + "; Content : |" + t.Location.Content(program.Code).Replace("\n", "\\n") + "|");
                else
                    Console.WriteLine(t);
            }
            */
            Utility.Pause();
        }

    }
}
