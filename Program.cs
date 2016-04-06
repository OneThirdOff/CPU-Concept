using System;

namespace CPU_Concept
{
    class Program
    {
        static void Main(string[] args)
        {
            BIOS systemBios = new BIOS();
            if (!systemBios.CPUFault)
            {
                systemBios.RunBios();
            }
            Console.Read();
        }
    }
}
