using System.Linq;

namespace CPU_Concept
{
    class Memory
    {
        private int[] _memory;
        private int _memorySize;

        public int MemorySize { get { return _memorySize; } }
        public void WriteMemByte(int Adress, byte ByteToWrite)
        {
            _memory[Adress] = ByteToWrite;
        }
        public int ReadMemByte(int Adress)
        {
            return _memory[Adress];
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
            this._memorySize = MemorySize;
            _memory = new int[MemorySize];
        }
    }
}
