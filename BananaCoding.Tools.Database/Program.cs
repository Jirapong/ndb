using System;
using System.Collections.Generic;
using System.Text;

namespace BananaCoding.Tools.Database
{
    class Program
    {
        static void Main(string[] args)
        {
            DBRunner runner = new DBRunner();

            runner.Run(args);
        }
    }
}
