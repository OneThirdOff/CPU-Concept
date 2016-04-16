using System;
using System.Reflection;

namespace CPU_Concept
{
    class BIOS
    {
        private bool _inInterpreter;
        private byte[] _programToRun = new byte[256];
        private CPU systemCPU;
        private CPU.InstructionSet biosInstructions;

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
                systemCPU.WriteMemory(i, 0);
                 
            }
            systemCPU.WriteMemory(0, 255); //throw in a HALT as the first byte in the memory. That way if you start the cpu without software it just stops.
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
            Console.WriteLine("BIOS " + version + " Loaded.");
            DoRunInterpreter();

            while (!systemCPU.Halt)
            {
                systemCPU.Update();
                systemCPU.Draw();
                if (systemCPU.Reset)
                {
                    RunBios();
                }
            }
            Console.WriteLine("Dump registers? y/n ");
            string getResponse = Console.ReadLine();
            if (getResponse.ToUpper().Equals("Y") || getResponse.Equals(""))
            {
                DoDumpRegisters();
            }
            Console.WriteLine("\r\nSystem halted.");
        }

        public void DoRunInterpreter()
        {
            Console.WriteLine("Program 1 operation per line, prefix with linenumber to correct line.");
            int programAdress = 1;
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
                    var opCode = (CPU.InstructionSet)Enum.Parse(typeof(CPU.InstructionSet), _splitBiosInput[0].ToUpper());
                    switch (opCode)
                    {
                        case CPU.InstructionSet.NOP:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.NOP);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.MOV:
                            if (!_splitBiosInput[2].Equals("A") || !_splitBiosInput[2].Equals("B"))
                            {
                                break;
                            }
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.MOV);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            
                            switch (_splitBiosInput[2])
                            {
                                case "A":
                                    systemCPU.WriteMemory(programAdress, 0);
                                    break;
                                case "B":
                                    systemCPU.WriteMemory(programAdress, 1);
                                    break;
                                default:
                                    break;
                            }
                            
                            programAdress++;
                            break;
                        case CPU.InstructionSet.SAVE:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SAVE);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            break;
                        case CPU.InstructionSet.READ:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.READ);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.ADD:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.ADD);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.SUB:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SUB);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.MUX:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.MUX);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.DIV:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.DIV);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.SHL:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHL);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case CPU.InstructionSet.SHR:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHR);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case CPU.InstructionSet.WAIT:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.WAIT);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.HALT:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.HALT);
                            programAdress++;
                            break;
                        case CPU.InstructionSet.DEC:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.DEC);
                            break;
                        case CPU.InstructionSet.INC:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.INC);
                            break;
                        case CPU.InstructionSet.CDE:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.CDE);
                            break;
                        case CPU.InstructionSet.CIN:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.CIN);
                            break;
                        case CPU.InstructionSet.LOAD:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.LOAD);
                            programAdress++;
                            _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                            programAdress += 2;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                            programAdress++;
                            break;
                        case CPU.InstructionSet.STORE:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.STORE);
                            programAdress++;
                            systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                            programAdress++;
                            _writeAdress(Convert.ToInt32(_splitBiosInput[2]), programAdress);
                            programAdress += 2;
                            break;
                        case CPU.InstructionSet.RST:
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.RST);
                            programAdress++;
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
            systemCPU.WriteMemory(0, (byte)programAdress);
        }

        public void DoDumpRegisters()
        {
            Console.WriteLine("Dumping CPU registers.");
            Console.Write("Program counter: " + systemCPU.HaltRegisters[0] + "\t");
            Console.Write("Instruction: " + systemCPU.HaltRegisters[1] + "\r\n");
            Console.Write("Adress: " + systemCPU.Adress + "\t");
            Console.Write("Adress in range: " + systemCPU.IndexOutOfRange + "t");
            Console.Write("Counter: " + systemCPU.HaltRegisters[7] + "\r\n");
            Console.Write("A: " + systemCPU.HaltRegisters[2] + "dec" + "\t");
            Console.Write("B: " + systemCPU.HaltRegisters[3] + "dec" + "\t");
            Console.Write("Temporary register: " + systemCPU.HaltRegisters[4] + "\r\n");
            Console.Write("Overflow: " + systemCPU.HaltRegisters[5] + "\t\t");
            Console.Write("Underflow: " + systemCPU.HaltRegisters[6] + "\r\n");
            Console.WriteLine("Dumping memory:");
            for (int i = 1; i < systemCPU.ProgramMemorySize; i++)
            {
                Console.Write(systemCPU.ReadMemory(i - 1).ToString("x2"));
                if (i % 30 == 0)
                {
                    Console.Write("\r\n");
                }
                else if (i % 2 == 0)
                {
                    Console.Write(":");
                }
            }
        }

        private void _writeAdress(int Adress, int ProgramAdress)
        {
            //systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
            int mostSignificant = 0;
            int leastSignificant = 0;
            if (Adress > 255)
            {
                mostSignificant = Adress / 255;
                leastSignificant = Adress % 255;
            }
            else
            {
                mostSignificant = 0;
                leastSignificant = Adress;
            }
            systemCPU.WriteMemory(ProgramAdress, (byte)mostSignificant);
            systemCPU.WriteMemory(ProgramAdress  + 1, (byte)leastSignificant);
        }

        public void DoExitBios()
        {
            _inInterpreter = false;
        }
    }
}
