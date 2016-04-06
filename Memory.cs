using System.Linq;

namespace CPU_Concept
{
    class Memory
    {
        private int[] _memory;
        private int _memorySize;
        private int _adress;
        private bool _adressInRange;

        public int MemorySize { get { return _memorySize; } }
        public void WriteMemByte(int Adress, byte ByteToWrite, out bool _adressInRange)
        {
            this._adress = Adress;
            _adressInRange = true;
            if (this._adress > this.MemorySize)
            {
                _adressInRange = false;
            }

            if(!_adressInRange)
            {
                _memory[_adress] = ByteToWrite;
            }
        }
        public int ReadMemByte(int Adress)
        {
            //, out bool _adressInRange
            this._adress = Adress;
            bool _adressInRange = true;
            if (this._adress > this.MemorySize)
            {
                _adressInRange = false;
            }

            if(_adressInRange)
            {
                return _memory[_adress];
            }
            else
            {
                return 0;
            }
            
        }
        public int[] ReadMemSequence(int Adress, int Length)
        {
            int[] returnSequence = new int[Length];
            for (int i = 0; i < Length; i++)
            {
                returnSequence[i] = _memory[Adress];
                Adress++;
            }
            return returnSequence;
        }
        public void WriteMemSequence(int Adress, byte[] BytesToWrite)
        {
            for (int i = 0; i < BytesToWrite.Count(); i++)
            {
                _memory[Adress] = BytesToWrite[i];
                Adress++;
            }
        }

        public Memory(int MemorySize)
        {
            this._adress = 0;
            this._adressInRange = false;
            this._memorySize = MemorySize;
            _memory = new int[MemorySize];
        }
    }
}
