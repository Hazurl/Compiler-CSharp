using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler_CSharp
{
    class ProgramIterator
    {
        public ProgramIterator(List<string> code)
        {
            this.code = code;
            Reset();
        }

        List<string> code;

        public int Line { get; private set; }
        public int Column { get; private set; }
        int line_jumped;

        public void Reset()
        {
            Line = 0;
            Column = -1;
            line_jumped = 0;
        }

        public void Forward ()
        {
            line_jumped = Line;
            Column++;

            while (!EOF() && Column >= code[Line].Length)
            {
                Line++;
                Column = 0;
            }

            line_jumped = Line - line_jumped;
        }

        public void Backward ()
        {
            line_jumped = Line;
            while (Column == 0)
            {
                if (Line == 0)
                    throw new Exception("Cannot move backward when the cursor is on the first character !");
                Line--;
                Column = code[Line].Length;
            }
            Column--;

            line_jumped = Line - line_jumped;
        }

        public void NextLine()
        {
            line_jumped = Line;
            Line++;
            Column = 0;
            while(!EOF() && code[Line].Length == 0)
            {
                Line++;
            }
            line_jumped = Line - line_jumped;
        }

        public bool EOF ()
        {
            return Line >= code.Count;
        }

        public int LineJumped ()
        {
            return line_jumped;
        }

        public bool LineHasChanged()
        {
            return line_jumped > 0;
        }

        public char Current ()
        {
            return code[Line][Column];
        }

        public char Next ()
        {
            int l = line_jumped;

            Forward();
            char c = Current();
            Backward();

            line_jumped = l;

            return c;
        }

        public bool EmptyProgram ()
        {
            foreach(string s in code)
            {
                if (s.Length == 0)
                    continue;

                foreach(char c in s)
                {
                    if (c != ' ' || c != '\t' || c != '\n')
                        return false;
                }
            }

            return true;
        }

        public ProgramPosition Position()
        {
            return new ProgramPosition(Line, Column);
        }

        public ProgramRegion Region()
        {
            return new ProgramRegion(Position(), Position());
        }

        public ProgramRegion RegionSince(ProgramPosition start)
        {
            return new ProgramRegion(start, Position());
        }
    }
}
