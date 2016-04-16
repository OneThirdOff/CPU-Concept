using System;

namespace CPU_Concept
{
    class Program
    {
        static void Main(string[] args)
        {
            int height = 25;
            int width = 80;
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            BIOS systemBios = new BIOS();
            if (!systemBios.CPUFault)
            {
                systemBios.RunBios();
            }
            Console.Read();
        }
    }
}
