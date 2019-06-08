using System;

namespace SQLDbCompare
{
    public static class Dumper
    {
        public static void Dump(this object obj, string msg)
        {
            Console.WriteLine("msg {0}", obj);
        }
    }
}


