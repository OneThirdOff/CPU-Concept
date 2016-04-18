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
            Console.WriteLine("Program 1 operation per line. '?' for help.");
            int programAdress = 1;
            while (InBios)
            {
                _splitBiosInput = new string[3];
                for (int i = 0; i < _splitBiosInput.Length; i++)
                {
                    _splitBiosInput[i] = "";
                }

                Console.Write(programAdress + ": ");
                _biosInput = Console.ReadLine();
                _splitBiosInput = _biosInput.Split(' ');
                if (_splitBiosInput[0].ToUpper().Equals("RUN"))
                {
                    _inInterpreter = false;
                } else if (_splitBiosInput[0].Equals("?"))
                {
                    Console.WriteLine("CPU Opreation-codes and usage. Valid registers are A and B.");
                    Console.WriteLine("NOP - No Operation.");
                    Console.WriteLine("MOV [value] [register] - Moves the value into the named register.");
                    Console.WriteLine("SAVE [register] - Saves the value from temp-register to the register.");
                    Console.WriteLine("READ [register] - Reads the value from the register, saves to temp-register.");
                    Console.WriteLine("ADD - Adds register A and B and stores the result to the temp-register.");
                    Console.WriteLine("SUB - Subrtracts register B from register A, saves to temp-register.");
                    Console.WriteLine("MUX - Multiplies register A by register B, saves to temp-register.");
                    Console.WriteLine("DIV - Divides register A by register B, saves to temp-register.");
                    Console.WriteLine("SHL [register] [amount] - Shift the register [amount] times to the left.");
                    Console.WriteLine("SHR [register] [amount] - Shift the register [amount] times to the right.");
                    Console.WriteLine("DEC/INC [register] - Decrements or Increments the register by one.");
                    Console.WriteLine("CDE, CIN - Decrements or Increments the counter by one.");
                    Console.WriteLine("JMP [adress] - Jumps to the adress of the memory.");
                    Console.WriteLine("JZ [adress to jump to] [adress to check if zero] - jumps if checked is zero.");
                    Console.WriteLine("LOAD [adress] [register] - Loads the value in the memory adress to the register");
                    Console.WriteLine("STORE [register] [adress] - Stores the value in the register to the memory.");
                    Console.WriteLine("HALT - Halts the cpu. At the moment, the only way to stop the program.");
                }
                else if (!_splitBiosInput[0].Equals(""))
                {
                    switch (_splitBiosInput[0].ToUpper())
                    {
                        #region NoP
                        case "NOP":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.NOP);
                            programAdress++;
                            break;
                        #endregion
                        #region MOV
                        case "MOV":
                            if (_splitBiosInput.Length < 3)
                            {
                                Console.WriteLine("Missing value and/or register in entry.");
                                break;
                            }
                            if (_splitBiosInput[2].ToUpper().Equals('A') || _splitBiosInput[2].ToUpper().Equals('B') || !(_splitBiosInput[2].Equals("")))
                            {
                                systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.MOV);
                                programAdress++;
                                systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                                programAdress++;

                                switch (_splitBiosInput[2].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect register in entry.");  //Error message if you try to write to a non-existent register
                                        programAdress -= 2;                                 //and moves programAdress back to previous state
                                        break;
                                }
                                
                            } else  //do we even need this anymore?
                            {
                                Console.WriteLine("Missing register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region SAVE
                        case "SAVE":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SAVE);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SAVE);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region READ
                        case "READ":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.READ);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.READ);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region ADD
                        case "ADD":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.ADD);
                            programAdress++;
                            break;
                        #endregion
                        #region SUB
                        case "SUB":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SUB);
                            programAdress++;
                            break;
                        #endregion
                        #region MUX
                        case "MUX":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.MUX);
                            programAdress++;
                            break;
                        #endregion
                        #region DIV
                        case "DIV":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.DIV);
                            programAdress++;
                            break;
                        #endregion
                        #region Shift Left
                        case "SHL":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHL);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHL);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                    default:
                                        break;
                                }
                            } else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region Shift Right
                        case "SHR":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHR);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.SHR);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                }
                            } else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region WAIT
                        case "WAIT":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.WAIT);
                            programAdress++;
                            break;
                        #endregion
                        #region HALT
                        case "HALT":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.HALT);
                            programAdress++;
                            break;
                        #endregion
                        #region DEC
                        case "DEC":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.DEC);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.DEC);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                }
                            } else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region INC
                        case "INC":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.INC);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.INC);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                }
                            } else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region Counter Decrement
                        case "CDE":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.CDE);
                            break;
                        #endregion
                        #region Counter Increment
                        case "CIN":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.CIN);
                            break;
                        #endregion
                        #region JMP
                        case "JMP":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.JMP);
                            programAdress++;
                            _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                            programAdress++;
                            break;
                        #endregion
                        #region LOAD
                        case "LOAD":
                            if (_splitBiosInput[2].ToUpper().Equals('A') || _splitBiosInput[2].ToUpper().Equals('B') || !(_splitBiosInput[2].Equals("")))
                            {
                                switch (_splitBiosInput[2].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.LOAD);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                                        programAdress += 2;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.LOAD);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                                        programAdress += 2;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region STORE
                        case "STORE":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.STORE);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 0);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[2]), programAdress);
                                        programAdress += 2;
                                        break;
                                    case "B":
                                        systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.STORE);
                                        programAdress++;
                                        systemCPU.WriteMemory(programAdress, 1);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[2]), programAdress);
                                        programAdress += 2;
                                        break;
                                }
                            } else
                            {
                                Console.WriteLine("Missing or incorrect register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region Reset
                        case "RST":
                            systemCPU.WriteMemory(programAdress, (int)CPU.InstructionSet.RST);
                            programAdress++;
                            break;
#endregion
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
            Console.Write("Adress in range: " + systemCPU.IndexOutOfRange + "\t");
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
