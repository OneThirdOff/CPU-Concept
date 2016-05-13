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
        private bool _collectingLine;
        private int _currentScreenLine;
        private int _codeLineNumber = 0;
        private int programAdress = 1;

        private KeyboardState OldKeyboardState;
        private KeyboardState NewKeyboardState;
        private string KeyboardInputLine;

        private int _charactersPerLine = 80;
        private int _linesPerScreen = 25;

        string _biosInput;

        public bool InBios { get { return _inInterpreter; } }
        public byte[] ProgramToRun { get { return _programToRun; } }
        public bool CPUFault { get { return systemCPU.Fault; } }
        public Memory systemMemory;
        public Memory graphicsMemory;
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

        public BIOS(int BusWidth, int AddressBusWidth, int NumberOfRegisters, int MemorySize)
        {
            _collectingLine = false;
            version = Assembly.GetEntryAssembly().GetName().Version;
            systemCPU = new CPU(BusWidth, AddressBusWidth, NumberOfRegisters);
            systemMemory = new Memory(MemorySize);
            graphicsMemory = new Memory(2000);
            _currentScreenLine = 0;
            
            systemCPU.Initialize();
            _inInterpreter = true;
            checkMemory();
            DoBiosPostInfo();
            DoStartInterpreter();
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

        private void checkMemory()
        {
            for (int i = 0; i < systemMemory.MemorySize; i++)
            {
                WriteMemory(i, 255);
                if (ReadMemory(i) != 255)
                {
                    DoMemoryFault(i);
                    break;
                }
                WriteMemory(i, 0);

            }
            WriteMemory(0, 255); //throw in a HALT as the first byte in the memory. That way if you start the cpu without software it just stops.
        }
        public void DoMemoryFault(int FaultAddress)
        {
            systemCPU.SetFault();
            _inInterpreter = false;
            WriteToScreen("CPU Fault at " + FaultAddress, _currentScreenLine);
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            DoRunningInterpreter();

            NewKeyboardState = Keyboard.GetState();
            Keys[] CurrentKey = NewKeyboardState.GetPressedKeys();

            if (NewKeyboardState.IsKeyUp(Keys.Enter))
            {
                _collectingLine = false;
                DoRunInterpreter(KeyboardInputLine);
                KeyboardInputLine = "";
            } else
            {
                if (NewKeyboardState.IsKeyUp(CurrentKey[0]))
                {
                    KeyboardInputLine = KeyboardInputLine + CurrentKey[0].ToString();
                }
            }

            OldKeyboardState = NewKeyboardState;
        }

        //public void RunBios()
        //{
        //    DoBiosPostInfo();
            
        //    while (!systemCPU.Halt)
        //    {
        //        systemCPU.Update();
                
        //        if (systemCPU.Reset)
        //        {
        //            RunBios();
        //        }
        //    }
        //    WriteLine("Dump registers? y/n ");
        //    string getResponse = Console.ReadLine();
        //    if (getResponse.ToUpper().Equals("Y") || getResponse.Equals(""))
        //    {
        //        DoDumpRegisters();
        //    }
        //    Console.WriteLine("\r\nSystem halted.");
        //}

        public void DoBiosPostInfo()
        {
            WriteToScreen("BIOS " + version + " Loaded.", _currentScreenLine);
            _currentScreenLine++;
        }

        public void DoStartInterpreter()
        {
            WriteToScreen("Program 1 operation per line. '?' for help.", _currentScreenLine);
            _currentScreenLine++;
        }

        public void DoRunningInterpreter()
        {
            if (!_collectingLine)
            {
                WriteToScreen(programAdress + ": ", _currentScreenLine);
                _collectingLine = true;
            }
        }

        public void DoRunInterpreter(string LineToInterpret)
        {
            string[] _splitBiosInput = new string[3];
            for (int j = 0; j < _splitBiosInput.Length; j++)
            {
                _splitBiosInput[j] = "";
            }
            //ScreenLines LineNumber = (ScreenLines)_codeLineNumber;
            
            _splitBiosInput = LineToInterpret.Split(' ');
            if (_splitBiosInput[0].ToUpper().Equals("RUN"))
            {
                _inInterpreter = false;
            }
            else if (_splitBiosInput[0].Equals("?"))
            {
                WriteToScreen("CPU Operation-codes and usage. Valid registers are A and B.", 0);
                WriteToScreen("NOP - No Operation.", 1);
                WriteToScreen("MOV [value] [register] - Moves the value into the named register.", 2);
                WriteToScreen("SAVE [register] - Saves the value from temp-register to the register.", 3);
                WriteToScreen("READ [register] - Reads the value from the register, saves to temp-register.", 4);
                WriteToScreen("ADD - Adds register A and B and stores the result to the temp-register.", 5);
                WriteToScreen("SUB - Subrtracts register B from register A, saves to temp-register.", 6);
                WriteToScreen("MUX - Multiplies register A by register B, saves to temp-register. Writes to register B while multiplying", 7);
                WriteToScreen("DIV - Divides register A by register B, saves to temp-register.", 8);
                WriteToScreen("SHL [register] [amount] - Shift the register [amount] times to the left.", 9);
                WriteToScreen("SHR [register] [amount] - Shift the register [amount] times to the right.", 10);
                WriteToScreen("DEC/INC [register] - Decrements or Increments the register by one.", 11);
                WriteToScreen("CDE, CIN - Decrements or Increments the counter by one.", 12);
                WriteToScreen("JMP [address] - Jumps to the address of the memory.", 13);
                WriteToScreen("JZ [address to jump to] [address to check if zero] - jumps if checked is zero.", 14);
                WriteToScreen("LOAD [address] [register] - Loads the value in the memory address to the register", 15);
                WriteToScreen("STORE [register] [address] - Stores the value in the register to the memory.", 16);
                WriteToScreen("HALT - Halts the cpu. At the moment, the only way to stop the program.", 16);
                WriteToScreen("RUN - runs the program.", 17);
                _currentScreenLine = 18;
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
                            WriteToScreen("Missing value and/or register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                                    WriteToScreen("Incorrect register in entry.", _currentScreenLine);  //Error message if you try to write to a non-existent register
                                    _currentScreenLine++;
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
                                    WriteToScreen("incorrect register in entry.", _currentScreenLine);
                                    _currentScreenLine++;
                                    break;
                            }
                        }
                        else
                        {
                            WriteToScreen("Missing register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                                    WriteToScreen("Incorrect register in entry.", _currentScreenLine);
                                    _currentScreenLine++;
                                    break;
                            }
                        }
                        else
                        {
                            WriteToScreen("Missing register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing value and/or register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
                            break;
                        }
                        break;
                    #endregion
                    #region Shift Right
                    case "SHR":
                        if (_splitBiosInput.Length < 3)
                        {
                            WriteToScreen("Missing value and/or register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
                            break;
                        }
                        break;
                    #endregion
                    #region INC
                    case "INC":
                        if (_splitBiosInput.Length < 2)     //making sure the user actually specifies a register
                        {
                            WriteToScreen("Missing register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing address and/or register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
                            break;
                        }
                        break;
                    #endregion
                    #region STORE
                    case "STORE":
                        if (_splitBiosInput.Length < 3)     //making sure the user actually specifies an address and register
                        {
                            WriteToScreen("Missing address and/or register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                            WriteToScreen("Missing or incorrect register in entry.", _currentScreenLine);
                            _currentScreenLine++;
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
                        WriteToScreen("Unknown command.", _currentScreenLine);
                        _currentScreenLine++;
                        break;
                }
            }
            _collectingLine = true;
            //WriteMemory(0, (byte)programAdress);
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
                WriteToScreen(LineToWrite, lineNumber);
                lineNumber++;
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

        private void WriteToScreen(string StringToWrite, int LineToWriteAt)
        {
            graphicsMemory.WriteMemSequence(_graphicsMemoryFirstByte + (80 * LineToWriteAt), DoCreateByteArray(StringToWrite));
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
            _currentScreenLine = 0;
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
