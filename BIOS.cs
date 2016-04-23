using System;
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacking_Game
{
    public class BIOS
    {
        private bool _inInterpreter;
        private byte[] _programToRun = new byte[256];
        private CPU systemCPU;
        private int _graphicsMemorySize;
        private int _ProgramMemorySize;
        private int _graphicsMemoryFirstByte;
        private string _screenCharacters;
        private byte[] _screenBytes;

        private int _charactersPerLine = 80;
        private int _linesPerScreen = 25;

        string _biosInput;
        string[] _splitBiosInput;

        public bool InBios { get { return _inInterpreter; } }
        public byte[] ProgramToRun { get { return _programToRun; } }
        public bool CPUFault { get { return systemCPU.Fault; } }
        public Memory systemMemory;
        public enum ScreenLines
        {
            Line1 = 0,
            Line2 = 80,
            Line3 = 160,
            Line4 = 240,
            Line5 = 320,
            Line6 = 400,
            Line7 = 480,
            Line8 = 560,
            Line9 = 640,
            Line10 = 720,
            Line11 = 800,
            Line12 = 880,
            Line13 = 960,
            Line14 = 1040,
            Line15 = 1120,
            Line16 = 1200,
            Line17 = 1280,
            Line18 = 1360,
            Line19 = 1440,
            Line20 = 1520,
            Line21 = 1600,
            Line22 = 1680,
            Line23 = 1760,
            Line24 = 1840,
            Line25 = 1920
        }

        public Version version;

        public BIOS(int BusWidth, int AddressBusWidth, int NumberOfRegisters, int MemorySize, int ReservedMemForGraphics)
        {
            version = Assembly.GetEntryAssembly().GetName().Version;
            systemCPU = new CPU(BusWidth, AddressBusWidth, NumberOfRegisters);
            systemMemory = new Memory(MemorySize);
            _graphicsMemorySize = ReservedMemForGraphics;
            _ProgramMemorySize = MemorySize - ReservedMemForGraphics;
            _graphicsMemoryFirstByte = _ProgramMemorySize;

            systemCPU.Initialize();
            _inInterpreter = true;
            checkCPU();
        }

        #region Memory-handling
        /// <summary>
        /// Writes a byte to memory
        /// </summary>
        /// <param name="Adress">Int, Memory-adress you want to write to.</param>
        /// <param name="ByteToWrite">Byte-value to write.</param>
        public void WriteMemory(int Adress, byte ByteToWrite)
        {
            systemMemory.WriteMemByte(Adress, ByteToWrite);
        }

        /// <summary>
        /// Read byte from memory.
        /// </summary>
        /// <param name="Adress">Adress to read from.</param>
        /// <returns></returns>
        public int ReadMemory(int Adress)
        {
            return systemMemory.ReadMemByte(Adress);
        }

        /// <summary>
        /// Reads byte-array from memory.
        /// </summary>
        /// <param name="Address">Adress to start to read from.</param>
        /// <param name="Length">Length specifies how many bytes to read.</param>
        /// <returns></returns>
        public int[] ReadMemory(int Address, int Length)
        {
            int[] returnBytes = new int[Length];
            for (int i = Address; i < Length; i++)
            {
                returnBytes[Address] = ReadMemory(Address);
            }
            return returnBytes;
        }

        /// <summary>
        /// Returns size of cpu-memory.
        /// </summary>
        public int MemorySize { get { return systemMemory.MemorySize; } }
        /// <summary>
        /// Returns size of program-allocatable memory.
        /// </summary>
        public int ProgramMemorySize { get { return _ProgramMemorySize; } }
        #endregion

        private void checkCPU()
        {
            for (int i = 0; i < systemMemory.MemorySize; i++)
            {
                WriteMemory(i, 255);
                if (ReadMemory(i) != 255)
                {
                    DoCPUFault(i);
                    break;
                }
                WriteMemory(i, 0);

            }
            WriteMemory(0, 255); //throw in a HALT as the first byte in the memory. That way if you start the cpu without software it just stops.
        }

        public void DoCPUFault(int FaultAddress)
        {
            systemCPU.SetFault();
            _inInterpreter = false;
            Console.WriteLine("CPU Fault at " + FaultAddress);
            Console.WriteLine(systemCPU.HaltRegisters);
        }

        public void Update()
        {
            //fill with fancy!
            //DoRunInterpreter();
            
        }

        public void RunBios()
        {
            Console.WriteLine("BIOS " + version + " Loaded.");
            DoRunInterpreter();

            while (!systemCPU.Halt)
            {
                systemCPU.Update();
                
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
            int _lineNumber = 0;
            
            while (InBios)
            {
                _lineNumber = 1;
                _splitBiosInput = new string[3];
                for (int j = 0; j < _splitBiosInput.Length; j++)
                {
                    _splitBiosInput[j] = "";
                }
                ScreenLines LineNumber = (ScreenLines)_lineNumber;
                systemMemory.WriteMemSequence((int)LineNumber, DoCreateByteArray(": "));
                
                _biosInput = Console.ReadLine();
                _splitBiosInput = _biosInput.Split(' ');
                if (_splitBiosInput[0].ToUpper().Equals("RUN"))
                {
                    _inInterpreter = false;
                }
                else if (_splitBiosInput[0].Equals("?"))
                {
                    Console.WriteLine("CPU Operation-codes and usage. Valid registers are A and B.");
                    Console.WriteLine("NOP - No Operation.");
                    Console.WriteLine("MOV [value] [register] - Moves the value into the named register.");
                    Console.WriteLine("SAVE [register] - Saves the value from temp-register to the register.");
                    Console.WriteLine("READ [register] - Reads the value from the register, saves to temp-register.");
                    Console.WriteLine("ADD - Adds register A and B and stores the result to the temp-register.");
                    Console.WriteLine("SUB - Subrtracts register B from register A, saves to temp-register.");
                    Console.WriteLine("MUX - Multiplies register A by register B, saves to temp-register. Writes to register B while multiplying");
                    Console.WriteLine("DIV - Divides register A by register B, saves to temp-register.");
                    Console.WriteLine("SHL [register] [amount] - Shift the register [amount] times to the left.");
                    Console.WriteLine("SHR [register] [amount] - Shift the register [amount] times to the right.");
                    Console.WriteLine("DEC/INC [register] - Decrements or Increments the register by one.");
                    Console.WriteLine("CDE, CIN - Decrements or Increments the counter by one.");
                    Console.WriteLine("JMP [address] - Jumps to the address of the memory.");
                    Console.WriteLine("JZ [address to jump to] [address to check if zero] - jumps if checked is zero.");
                    Console.WriteLine("LOAD [address] [register] - Loads the value in the memory address to the register");
                    Console.WriteLine("STORE [register] [address] - Stores the value in the register to the memory.");
                    Console.WriteLine("HALT - Halts the cpu. At the moment, the only way to stop the program.");
                    Console.WriteLine("RUN - runs the program.");
                }
                else if (!_splitBiosInput[0].Equals(""))
                {
                    switch (_splitBiosInput[0].ToUpper())
                    {
                        #region NoP
                        case "NOP":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.NOP);
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
                                WriteMemory(programAdress, (int)CPU.InstructionSet.MOV);
                                programAdress++;
                                WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[1]));
                                programAdress++;

                                switch (_splitBiosInput[2].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect register in entry.");  //Error message if you try to write to a non-existent register
                                        programAdress -= 2;                                 //and moves programAdress back to previous state
                                        break;
                                }
                            }
                            //} else  //do we even need this anymore?
                            //{
                            //    Console.WriteLine("Missing register in entry.");
                            //    break;
                            //}
                            break;
                        #endregion
                        #region SAVE
                        case "SAVE":
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SAVE);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SAVE);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        Console.WriteLine("incorrect register in entry.");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Missing register in entry.");
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
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.READ);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.READ);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        break;
                                    default:
                                        Console.WriteLine("Incorrect register in entry.");
                                        break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Missing register in entry.");
                                break;
                            }
                            break;
                        #endregion
                        #region ADD
                        case "ADD":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.ADD);
                            programAdress++;
                            break;
                        #endregion
                        #region SUB
                        case "SUB":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.SUB);
                            programAdress++;
                            break;
                        #endregion
                        #region MUX
                        case "MUX":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.MUX);
                            programAdress++;
                            break;
                        #endregion
                        #region DIV
                        case "DIV":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.DIV);
                            programAdress++;
                            break;
                        #endregion
                        #region Shift Left
                        case "SHL":
                            if (_splitBiosInput.Length < 3)
                            {
                                Console.WriteLine("Missing value and/or register in entry.");
                                break;
                            }
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SHL);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SHL);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
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
                        #region Shift Right
                        case "SHR":
                            if (_splitBiosInput.Length < 3)
                            {
                                Console.WriteLine("Missing value and/or register in entry.");
                                break;
                            }
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SHR);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.SHR);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
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
                        #region WAIT
                        case "WAIT":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.WAIT);
                            programAdress++;
                            break;
                        #endregion
                        #region HALT
                        case "HALT":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.HALT);
                            programAdress++;
                            break;
                        #endregion
                        #region DEC
                        case "DEC":
                            if (_splitBiosInput.Length < 2)     //making sure the user actually specifies a register
                            {
                                Console.WriteLine("Missing register in entry.");
                                break;
                            }
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.DEC);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.DEC);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
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
                        #region INC
                        case "INC":
                            if (_splitBiosInput.Length < 2)     //making sure the user actually specifies a register
                            {
                                Console.WriteLine("Missing register in entry.");
                                break;
                            }
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.INC);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.INC);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
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
                        #region Counter Decrement
                        case "CDE":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.CDE);
                            break;
                        #endregion
                        #region Counter Increment
                        case "CIN":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.CIN);
                            break;
                        #endregion
                        #region JMP
                        case "JMP":
                            if (_splitBiosInput.Length < 2)     //making sure the user actually specifies an address
                            {
                                Console.WriteLine("Missing register in entry.");
                                break;
                            }
                            WriteMemory(programAdress, (int)CPU.InstructionSet.JMP);
                            programAdress++;
                            _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                            programAdress++;
                            break;
                        #endregion
                        #region LOAD
                        case "LOAD":
                            if (_splitBiosInput.Length < 3)     //making sure the user actually specifies an address and register
                            {
                                Console.WriteLine("Missing address and/or register in entry.");
                                break;
                            }
                            if (_splitBiosInput[2].ToUpper().Equals('A') || _splitBiosInput[2].ToUpper().Equals('B') || !(_splitBiosInput[2].Equals("")))
                            {
                                switch (_splitBiosInput[2].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.LOAD);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                                        programAdress += 2;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.LOAD);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[1]), programAdress);
                                        programAdress += 2;
                                        WriteMemory(programAdress, 0);
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
                            if (_splitBiosInput.Length < 3)     //making sure the user actually specifies an address and register
                            {
                                Console.WriteLine("Missing address and/or register in entry.");
                                break;
                            }
                            if (_splitBiosInput[1].ToUpper().Equals('A') || _splitBiosInput[1].ToUpper().Equals('B') || !(_splitBiosInput[1].Equals("")))
                            {
                                switch (_splitBiosInput[1].ToUpper())
                                {
                                    case "A":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.STORE);
                                        programAdress++;
                                        WriteMemory(programAdress, 0);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[2]), programAdress);
                                        programAdress += 2;
                                        break;
                                    case "B":
                                        WriteMemory(programAdress, (int)CPU.InstructionSet.STORE);
                                        programAdress++;
                                        WriteMemory(programAdress, 1);
                                        programAdress++;
                                        _writeAdress(Convert.ToInt32(_splitBiosInput[2]), programAdress);
                                        programAdress += 2;
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
                        #region Reset
                        case "RST":
                            WriteMemory(programAdress, (int)CPU.InstructionSet.RST);
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
            WriteMemory(0, (byte)programAdress);
        }

        public void DoDumpRegisters()
        {
            int lineNumber = 0;
            string[] LinesToWrite = new string[11];
            LinesToWrite[0] = "Dumping CPU registers.";
            LinesToWrite[1] = "Program counter: " + systemCPU.HaltRegisters[0];
            LinesToWrite[2] = "Instruction: " + systemCPU.HaltRegisters[1];
            LinesToWrite[3] = "Adress: " + systemCPU.Adress;
            LinesToWrite[4] = "Adress in range: " + systemCPU.IndexOutOfRange;
            LinesToWrite[5] = "Counter: " + systemCPU.HaltRegisters[7];
            LinesToWrite[6] = "A: " + systemCPU.HaltRegisters[2] + "dec";
            LinesToWrite[7] = "B: " + systemCPU.HaltRegisters[3] + "dec";
            LinesToWrite[8] = "Temporary register: " + systemCPU.HaltRegisters[4];
            LinesToWrite[9] = "Overflow: " + systemCPU.HaltRegisters[5];
            LinesToWrite[10] = "Underflow: " + systemCPU.HaltRegisters[6];

            foreach (string LineToWrite in LinesToWrite)
            {
                systemMemory.WriteMemSequence(_graphicsMemoryFirstByte + lineNumber , DoCreateByteArray(LineToWrite));
                lineNumber += 80;
            }
            //Console.WriteLine("Dumping memory:");
            //for (int i = 1; i < ProgramMemorySize; i++)
            //{
            //    Console.Write(ReadMemory(i - 1).ToString("x2"));
            //    if (i % 30 == 0)
            //    {
            //        Console.Write("\r\n");
            //    }
            //    else if (i % 2 == 0)
            //    {
            //        Console.Write(":");
            //    }
            //}
        }

        private void _writeAdress(int Adress, int ProgramAdress)
        {
            //WriteMemory(programAdress, Convert.ToByte(_splitBiosInput[2]));
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
            WriteMemory(ProgramAdress, (byte)mostSignificant);
            WriteMemory(ProgramAdress + 1, (byte)leastSignificant);
        }

        private byte[] DoCreateByteArray(string TextToConvert)
        {
            byte[] _returnText = new byte[TextToConvert.Length];
            int i = 0;
            foreach (byte character in TextToConvert)
            {
                _returnText[i] = character;
                i++;
            }
            return _returnText;
        }

        public void DoExitBios()
        {
            _inInterpreter = false;
        }

        public void GraphicsTest()
        {
            int address = 256;
            byte[] byteToWrite = { 84, 101, 115, 116, 32 };
            int bytecounter = 0;
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < _graphicsMemorySize; i++)
            {
                systemMemory.WriteMemByte(address, byteToWrite[bytecounter]);
                if (bytecounter == 4)
                {
                    bytecounter = 0;
                }
                else
                {
                    bytecounter++;
                }
                address++;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont Font, int ScreenWidth, int ScreenHeight)
        {
            byte[] screenMem = systemMemory.ReadMemSequence(_ProgramMemorySize, _graphicsMemorySize);

            int characterCounter = 1;
            int currentLine = 0;
            char printCharacter;
            string ScreenString = "";
            foreach (char character in screenMem)
            {
                if (characterCounter == _charactersPerLine)
                {
                    spriteBatch.DrawString(Font, ScreenString, new Vector2(0, currentLine), Color.Green);
                    ScreenString = "";
                    characterCounter = 1;
                    currentLine += ScreenHeight / _linesPerScreen;
                }
                else
                {
                    if (!Font.Characters.Contains(character))
                    {
                        printCharacter = ' ';
                    } else
                    {
                        printCharacter = character;
                    }
                    ScreenString = ScreenString + printCharacter;
                    characterCounter++;
                }
            }
            

            // Too Slow!
            //for (int memPosition = 0; memPosition < _graphicsMemorySize; memPosition++)
            //{
            //    try
            //    {
            //        spriteBatch.DrawString(Font, Convert.ToString((char)screenMem[memPosition]), new Vector2((memPosition % 80) * (ScreenWidth / _charactersPerLine), (memPosition / 80) * (ScreenHeight / _linesPerScreen)), Color.Green);
            //        } catch (Exception)
            //        { }
            //}
        }
    }
}
