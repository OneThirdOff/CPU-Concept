using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Concept
{
    class BIOS
    {
        private bool _inBios;
        private byte[] _programToRun = new byte[256];
        private CPU systemCPU;

        string _biosInput;
        string[] _splitBiosInput;

        public bool InBios { get { return _inBios; } }
        public byte[] ProgramToRun { get { return _programToRun; } }
        public bool CPUFault { get { return systemCPU.Fault; } }

        public BIOS()
        {
            systemCPU = new CPU(8);
            systemCPU.Initialize();
            _inBios = true;
            checkCPU();
        }

        private void checkCPU()
        {
            for (int i = 0; i < systemCPU.MemorySize; i++)
            {
                systemCPU.WriteMemory(i, 255);
                if(systemCPU.ReadMemory(i) != 255)
                {
                    DoCPUFault(i);
                    break;
                }
                systemCPU.WriteMemory(i, 0);
            }
        }

        public void DoCPUFault(int FaultAddress)
        {
            systemCPU.SetFault();
            _inBios = false;
            Console.WriteLine("CPU Fault at " + FaultAddress);
            Console.WriteLine(systemCPU.HaltRegisters);
        }

        public void Update()
        {
            Console.WriteLine("BIOS Loaded. Program 1 op per line, prefix with linenumber.");
            while (InBios)
            {
                int programAdress = 0;
                _splitBiosInput = new string[3];
                _biosInput = Console.ReadLine();
                _splitBiosInput = _biosInput.Split(' ');
                programAdress = Convert.ToInt32(_splitBiosInput[0]);
                switch(_splitBiosInput[1].ToUpper())
                {
                    case "NOP":
                        _programToRun[programAdress] = 0;
                        break;
                    case "LOAD0":
                        _programToRun[programAdress] = 1;
                        _programToRun[programAdress + 1] = Convert.ToByte(_splitBiosInput[2]);
                        break;
                    case "LOAD1":
                        _programToRun[programAdress] = 2;
                        _programToRun[programAdress + 1] = Convert.ToByte(_splitBiosInput[2]);
                        break;
                    case "SAVE0":
                        _programToRun[programAdress] = 3;
                        break;
                    case "SAVE1":
                        _programToRun[programAdress] = 4;
                        break;
                    case "READ0":
                        _programToRun[programAdress] = 5;
                        break;
                    case "READ1":
                        _programToRun[programAdress] = 6;
                        break;
                    case "ADD":
                        _programToRun[programAdress] = 7;
                        break;
                    case "SUBTRACT":
                        _programToRun[programAdress] = 8;
                        break;
                    case "HALT":
                        _programToRun[programAdress] = 9;
                        break;
                    case "WAIT":
                        _programToRun[programAdress] = 10;
                        break;
                    case "RUN":
                        _inBios = false;
                        break;
                    default:
                        
                        break;
                }
            }

            for (int i = 0; i < ProgramToRun.Count(); i++)
            {
                systemCPU.WriteMemory(i, ProgramToRun[i]);
            }
            while (!systemCPU.Halt)
            {
                _inBios = false;
                systemCPU.Update();
            }
            Console.WriteLine(systemCPU.HaltRegisters);
        }

        public void DoExitBios()
        {
            _inBios = false;
        }
    }
}
