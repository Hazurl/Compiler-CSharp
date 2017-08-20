using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class Utility
    {
        public delegate void LamdaDeleguate();
        public static TimeSpan TimeCounter(LamdaDeleguate func)
        {
            DateTime start = DateTime.Now;
            func();
            return DateTime.Now.Subtract(start);
        }

        public static List<string> getFileContentToList(string path)
        {
            List<string> content = new List<string>();

            System.IO.StreamReader file = new System.IO.StreamReader(path);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                content.Add(line);
            }

            file.Close();
            return content;
        }

        public static string getFileContentToString(string path)
        {
            string content = "";

            System.IO.StreamReader file = new System.IO.StreamReader(path);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                content += line + '\n';
            }

            file.Close();
            return content;
        }

        public static void Pause()
        {
            Console.ReadLine();
        }

        public static void Write(string s, ConsoleColor color = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            SetConsoleColors(color, background);
            Console.Write(s);
            ResetConsoleColors();
        }

        public static void WriteLine(string s, ConsoleColor color = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            SetConsoleColors(color, background);
            Console.WriteLine(s);
            ResetConsoleColors();
        }

        public static void SetConsoleColors(ConsoleColor color = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
        }

        public static void ResetConsoleColors()
        {
            Console.ResetColor();
        }

    }
}
