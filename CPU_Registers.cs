using System;

namespace CPU_Concept
{
    class CPU_Registers
    {
        private byte[] _register;

        public int MaxValue;
        public int ReadRegister()
        {
            try
            {
                return BitConverter.ToInt32(_register, 0);
            } catch (Exception)
            {
                return 0;
            }
            
        }
        public void WriteRegister(int DataToWrite)
        {
            _register = BitConverter.GetBytes(DataToWrite);
        }

        public CPU_Registers(int BusWidth)
        {
            _register = new byte[BusWidth / 8];
            this.MaxValue = Byte.MaxValue * (BusWidth / 8);
        }
    }
}
