using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Concept
{
    class Program
    {
        static void Main(string[] args)
        {
            BIOS systemBios = new BIOS();
            if (!systemBios.CPUFault)
            {
                systemBios.Update();
            }
            Console.Read();
        }
    }
}
