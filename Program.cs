using System;

namespace CPU_Concept
{
    class Program
    {
        static void Main(string[] args)
        {
            int height = 25;
            int width = 80;
            try
            {
                Console.SetBufferSize(width, height);
            } catch (Exception)
            {

            }
            
            BIOS systemBios = new BIOS();
            if (!systemBios.CPUFault)
            {
                systemBios.RunBios();
            }
            Console.Read();
        }
    }
}
