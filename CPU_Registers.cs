using System;

namespace CPU_Concept
{
    class CPU_Registers
    {
        private byte[] _register;
        private bool _isCounter;
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
        public int MaxValue;
        public void IncrementCounter()
        {
            if (ReadRegister() < MaxValue && _isCounter)
            {
                WriteRegister(ReadRegister() + 1);
            }
        }
        public void DecrementCounter()
        {
            if (ReadRegister() > 0 && _isCounter)
            {
                WriteRegister(ReadRegister() - 1);
            }
            
        }

        public CPU_Registers(int BusWidth, bool IsCounter)
        {
            this._isCounter = IsCounter;
            _register = new byte[BusWidth / 8];
            this.MaxValue = Byte.MaxValue * (BusWidth / 8);
        }
    }
}
