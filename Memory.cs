using System.Linq;

namespace Hacking_Game
{
    public class Memory
    {
        private byte[] _memory;
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

        public byte[] ReadMemSequence(int Adress, int Length)
        {
            byte[] returnSequence = new byte[Length];
            for (int i = 0; i < Length; i++)
            {
                returnSequence[i] = (byte)_memory[Adress];
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
            _memory = new byte[MemorySize];
        }
    }
}
