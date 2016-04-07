using System;

namespace CPU_Concept
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetBufferSize(80, 25);
            BIOS systemBios = new BIOS();
            if (!systemBios.CPUFault)
            {
                systemBios.RunBios();
            }
            Console.Read();
        }
    }
}
