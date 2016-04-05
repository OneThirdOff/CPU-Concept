using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Concept
{
    class CPU_Registers
    {
        private int _register;
        public int ReadRegister() { return _register; }
        public void WriteRegister(int DataToWrite)
        {
            _register = DataToWrite;
        }
        public int MaxValue;
        public CPU_Registers(int BusWidth)
        {
            this.MaxValue = Byte.MaxValue * (BusWidth / 8);
        }
    }
}
