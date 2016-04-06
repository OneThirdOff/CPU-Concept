using System;
using System.Reflection;

namespace CPU_Concept
{
    class BIOS
    {
        private bool _inInterpreter;
        private byte[] _programToRun = new byte[256];
        private CPU systemCPU;

        string _biosInput;
        string[] _splitBiosInput;

        public bool InBios { get { return _inInterpreter; } }
        public byte[] ProgramToRun { get { return _programToRun; } }
        public bool CPUFault { get { return systemCPU.Fault; } }

        public Version version;

        public BIOS()
        {
            version = Assembly.GetEntryAssembly().GetName().Version;
            systemCPU = new CPU(8, 16, 2);
            systemCPU.Initialize();
            _inInterpreter = true;
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
                systemCPU.WriteMemory(i, (byte)0);
                systemCPU.WriteMemory(0, (byte)10); //throw in a HALT as the first byte in the memory. That way if you start the cpu without software it just stops.
            }
        }

        public void DoCPUFault(int FaultAddress)
        {
            systemCPU.SetFault();
            _inInterpreter = false;
            Console.WriteLine("CPU Fault at " + FaultAddress);
            Console.WriteLine(systemCPU.HaltRegisters);
        }

        public void RunBios()
        {
            Console.WriteLine("BIOS " + version + " Loaded. \r\n Program 1 operation per line, prefix with linenumber to correct line.");
            DoRunInterpreter();

            while (!systemCPU.Halt)
            {
                systemCPU.Update();
            }
            Console.WriteLine("Dump registers? y/n ");
            string getResponse = Console.ReadLine();
            if (getResponse.ToUpper().Equals("Y") || getResponse.Equals(""))
            {
                Console.WriteLine("Dumping CPU registers.");
                Console.Write("Program counter: " + systemCPU.HaltRegisters[0] + "\t");
                Console.Write("Instruction: " + systemCPU.HaltRegisters[1] + "\r\n");
                Console.Write("Adress: " + systemCPU.Adress + "\t");
                Console.Write("Adress in range: " + systemCPU.IndexOutOfRange + "\r\n");
                //Console.Write("Counter: " + systemCPU.HaltRegisters[X] + "\t");
                Console.Write("Register 0: " + systemCPU.HaltRegisters[2] + "dec" + "\t");
                Console.Write("Register 1: " + systemCPU.HaltRegisters[3] + "dec" + "\t");
                Console.Write("Temporary register: " + systemCPU.HaltRegisters[4] + "\r\n");
                Console.Write("Overflow: " + systemCPU.HaltRegisters[5] + "\t\t");
                Console.Write("Underflow: " + systemCPU.HaltRegisters[6] + "\r\n");
                Console.WriteLine("Dumping memory:");
                for (int i = 1; i < systemCPU.MemorySize + 1; i++)
                {
                    Console.Write(systemCPU.ReadMemory(i - 1).ToString("x2"));
                    if (i % 30 == 0)
                    {
                        Console.Write("\r\n");
                    }
                    else if (i == systemCPU.MemorySize)
                    {
                        Console.Write("\r\n");
                    }
                    else if(i % 2 == 0)
                    {
                        Console.Write(":");
                    }
                }
                Console.WriteLine("System halted.");
            }
        }

        public void DoRunInterpreter()
        {
            int programAdress = 0;
            while (InBios)
            {
                Console.Write(programAdress + ": ");
                _splitBiosInput = new string[3];
                _biosInput = Console.ReadLine();
                _splitBiosInput = _biosInput.Split(' ');
                if (_splitBiosInput[0].ToUpper().Equals("RUN"))
                {
                    _inInterpreter = false;
                }
                else if (!_splitBiosInput[0].Equals(""))
                {
                    switch (_splitBiosInput[0].ToUpper())
                    {
                        case "NOP":
                            systemCPU.WriteMemory(programAdress, 0);
                            programAdress++;
                            break;
                        case "LOAD":
                            systemCPU.WriteMemory(programAdress, 1);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case "SAVE":
                            systemCPU.WriteMemory(programAdress, 2);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            break;
                        case "READ":
                            systemCPU.WriteMemory(programAdress, 3);
                            programAdress++;
                            break;
                        case "ADD":
                            systemCPU.WriteMemory(programAdress, 4);
                            programAdress++;
                            break;
                        case "SUB":
                            systemCPU.WriteMemory(programAdress, 5);
                            programAdress++;
                            break;
                        case "MUX":
                            systemCPU.WriteMemory(programAdress, 6);
                            programAdress++;
                            break;
                        case "DIV":
                            systemCPU.WriteMemory(programAdress, 7);
                            programAdress++;
                            break;
                        case "SHL":
                            systemCPU.WriteMemory(programAdress, 8);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case "SHR":
                            systemCPU.WriteMemory(programAdress, 9);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case "WAIT":
                            systemCPU.WriteMemory(programAdress, 10);
                            programAdress++;
                            break;
                        case "HALT":
                            systemCPU.WriteMemory(programAdress, 255);
                            programAdress++;
                            break;
                        case "RST":
                            systemCPU.Reset();
                            break;
                        default:
                            Console.WriteLine("Unknown command.");
                            break;
                    }
                }
                else
                {

                }
            }
        }

        public void DoExitBios()
        {
            _inInterpreter = false;
        }
    }
}
