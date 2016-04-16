using System;
using System.Collections.Generic;

namespace CPU_Concept
{
    class CPU
    {
        #region private stuff
        private List<CPU_Registers> _registers;
        private CPU_Registers _tempRegister;
        private int _adressBus;
        private CPU_Registers _instructionRegister;
        private CPU_Registers _counter;
        private CPU_Registers _programCounter;
        private int _programLength;
        private Memory _ProgramMemory;
        private string[] _haltRegisters;
        private int _graphicsMemory;
        private int _programMemorySize;
        
        //Flags
        private bool _overFlow;
        private bool _underFlow;
        private bool _adressInRange;
        private bool _halt;
        private bool _fault;
        private bool _reset;
        #endregion

        #region public stuff
        public int Counter { get { return _counter.ReadRegister(); } }
        public int Adress { get { return _adressBus; } }
        public string[] HaltRegisters { get { return _haltRegisters; } }

        /// <summary>
        /// Writes a byte to memory, for BIOS-access to memory
        /// </summary>
        /// <param name="Adress">Int, Memory-adress you want to write to.</param>
        /// <param name="ByteToWrite">Byte-value to write.</param>
        public void WriteMemory(int Adress, byte ByteToWrite)
        {
            _ProgramMemory.WriteMemByte(Adress, ByteToWrite);
        }

        /// <summary>
        /// Read byte from memory. For BIOS-access to memory.
        /// </summary>
        /// <param name="Adress">Adress to read from.</param>
        /// <returns></returns>
        public int ReadMemory(int Adress)
        {
            return _ProgramMemory.ReadMemByte(Adress);
        }
        /// <summary>
        /// Reads byte-array from memory. For BIOS-access to memory.
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
        public int MemorySize { get { return _ProgramMemory.MemorySize; } }
        /// <summary>
        /// Returns size of program-allocatable memory.
        /// </summary>
        public int ProgramMemorySize { get { return _programMemorySize; } }

        //Flags
        public bool Overflow { get { return this._overFlow; } }
        public bool Underflow { get { return this._underFlow; } }
        public bool IndexOutOfRange { get { return !this._adressInRange; } }
        public bool Halt { get { return this._halt; } }
        public bool Reset { get { return this._reset; } }
        #endregion

        /// <summary>
        /// Sets fault-flag to True and halts the cpu.
        /// </summary>
        public void SetFault()
        {
            _fault = true;
            DoCrash();
        }
        /// <summary>
        /// Returns the status of the Fault-flag
        /// </summary>
        public bool Fault { get { return _fault; } }

        #region create and initialize
        /// <summary>
        /// Constructor for the CPU.
        /// </summary>
        /// <param name="BusWidth">Number specifies Bus-Width of CPU.</param>
        /// <param name="AddressBusWidth">Number specifies Adress-bus-Width of CPU.</param>
        /// <param name="NumberOfRegisters">Number specifies number of accessable registers on CPU.</param>
        public CPU(int BusWidth, int AddressBusWidth, int NumberOfRegisters)
        {
            this._ProgramMemory = new Memory(2256);
            this._graphicsMemory = 2000;
            this._programMemorySize = this._ProgramMemory.MemorySize - _graphicsMemory;
            this._counter = new CPU_Registers(AddressBusWidth);
            this._programCounter = new CPU_Registers(AddressBusWidth);
            this._tempRegister = new CPU_Registers(BusWidth * 2);
            this._instructionRegister = new CPU_Registers(BusWidth);
            this._haltRegisters = new string[8];

            //Create registers.
            this._registers = new List<CPU_Registers>();
            for (int i = 0; i < NumberOfRegisters; i++)
            {
                _registers.Add(new CPU_Registers(BusWidth));
            }
            
        }

        /// <summary>
        /// Basic initialization. Set's all flags to false, and reset's programcounter.
        /// </summary>
        public void Initialize()
        {
            this._overFlow = false;
            this._underFlow = false;
            this._halt = false;
            this._programCounter.WriteRegister(1);
            this._programLength = _ProgramMemory.ReadMemByte(0);
        }
        /// <summary>
        /// Does an initialization of the CPU.
        /// </summary>
        public void DoReset()
        {
            Initialize();
        }
        #endregion

        #region CPU-functions
        public enum InstructionSet
        {
            NOP = 0,
            MOV = 1,
            SAVE = 2,
            READ = 3,
            ADD = 4,
            SUB = 5,
            MUX = 6, // possible drop
            DIV = 7, // heavy process, might drop.
            SHL = 8,
            SHR = 9,
            WAIT = 10,
            DEC = 11,
            INC = 12,
            CDE = 13,
            CIN = 14,
            LOAD = 15,
            STORE = 16,
            JMP = 17,
            JZ = 18,
            RST = 250,  //Not required.
            HALT = 255
        }

        private int GetAdress(int ProgramAdress)
        {
            return (_ProgramMemory.ReadMemByte(ProgramAdress) * 255) + _ProgramMemory.ReadMemByte(ProgramAdress + 1);
        }
        /// <summary>
        /// Checks if the adress is in range of the memory.
        /// </summary>
        /// <param name="Adress">Adress to check</param>
        /// <returns>Bool</returns>
        public bool CheckAdressRange(int Adress)
        {
            bool isAdressInRange = true;
            return isAdressInRange;
        }

        #region CPU Operations
        private void DoCrash()
        {
            DoDumpRegisters();
            DoHalt();
        }
        private void DoUnknownOp()
        {
            DoHalt();
            DoDumpRegisters();
        }
        private void DoDumpRegisters()
        {
            _haltRegisters[0] = Convert.ToString(_programCounter);
            _haltRegisters[1] = Convert.ToString(_instructionRegister.ReadRegister());
            _haltRegisters[2] = Convert.ToString(_registers[0].ReadRegister());
            _haltRegisters[3] = Convert.ToString(_registers[1].ReadRegister());
            _haltRegisters[4] = Convert.ToString(_tempRegister.ReadRegister());
            _haltRegisters[5] = Convert.ToString(_overFlow);
            _haltRegisters[6] = Convert.ToString(_underFlow);
            _haltRegisters[7] = Convert.ToString(_counter.ReadRegister());
        }

        private void DoNoP()
        {
        }

        /// <summary>
        /// Writes the Data at DataAdress to the register referenced in RegisterNumber.
        /// </summary>
        /// <param name="DataAdress">Adress of the data in ProgramMemory to write</param>
        /// <param name="RegisterNumber">Adress of the RegisterNumber in ProgramMemory</param>
        private void DoMove(int DataAdress, int RegisterNumber)
        {
            _registers[_ProgramMemory.ReadMemByte(RegisterNumber)].WriteRegister(_ProgramMemory.ReadMemByte(DataAdress));
        }

        /// <summary>
        /// Saves the number in temp-register back to the register referenced in RegisterNumber
        /// </summary>
        /// <param name="RegisterNumber">Adress of the RegisterNumber in ProgramMemory</param>
        private void DoSave(int RegisterNumber)
        {
            if (_tempRegister.ReadRegister() < 0)
            {
                _registers[_ProgramMemory.ReadMemByte(RegisterNumber)].WriteRegister(0);
                _underFlow = true;
                DoCrash();
            }
            else if (_tempRegister.ReadRegister() > _registers[1].MaxValue)
            {
                _registers[_ProgramMemory.ReadMemByte(RegisterNumber)].WriteRegister(255);
                _overFlow = true;
                DoCrash();
            }
            else
            {
                _registers[_ProgramMemory.ReadMemByte(RegisterNumber)].WriteRegister((byte)_tempRegister.ReadRegister());
                _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
            }
        }

        /// <summary>
        /// Writes the content of the Register to temp-register
        /// </summary>
        /// <param name="RegisterNumber">Adress of the RegisterNumber in ProgramMemory</param>
        private void DoRead(int RegisterNumber)
        {
            _tempRegister.WriteRegister(_registers[_ProgramMemory.ReadMemByte(RegisterNumber)].ReadRegister());
            _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
        }
        
        private void DoAdd()
        {
            _tempRegister.WriteRegister(_registers[0].ReadRegister() + _registers[1].ReadRegister());
        }
        private void DoSubtract()
        {
            _tempRegister.WriteRegister(_registers[0].ReadRegister() - _registers[1].ReadRegister());
        }
        private void DoMultiply()
        {
            _counter.WriteRegister((byte)_registers[1].ReadRegister());
            _tempRegister.WriteRegister(_registers[1].ReadRegister());
            if (DoCounterDec())
            {
                _tempRegister.WriteRegister(0);

            }
            else
            {
                _tempRegister.WriteRegister(_registers[0].ReadRegister());
             }
            while (_counter.ReadRegister() > 0)
            {
                _registers[1].WriteRegister(_tempRegister.ReadRegister());
                DoAdd();
                DoCounterDec();
            }
        }
        private void DoDivision()
        {
            _tempRegister.WriteRegister(_registers[0].ReadRegister() / _registers[1].ReadRegister());
        }

        /// <summary>
        /// Decrements the value in the register.
        /// </summary>
        /// <param name="RegisterNumber">Adress of the RegisterNumber in ProgramMemory</param>
        /// <returns></returns>
        private bool DoDec(int RegisterNumber)
        {
            bool _isNegative = false;
            int _counterValue = _registers[RegisterNumber].ReadRegister();
            int newValue = _counterValue - 1;
            if (newValue < 0 )
            {
                _isNegative = true;
                _registers[RegisterNumber].WriteRegister(0);
            } else
            {
                _registers[RegisterNumber].WriteRegister(_counterValue - 1);
            }
            return _isNegative;
        }
        /// <summary>
        /// Increments the value in the register
        /// </summary>
        /// <param name="RegisterNumber">Adress of the RegisterNumber in ProgramMemory</param>
        /// <returns></returns>
        private bool DoInc(int RegisterNumber)
        {
            bool _overFlow = false;
            int _counterValue = _registers[RegisterNumber].ReadRegister();
            int newValue = _counterValue + 1;
            if (newValue > 255)
            {
                _overFlow = true;
                _registers[RegisterNumber].WriteRegister(255);
            }
            else
            {
                _registers[RegisterNumber].WriteRegister(_counterValue + 1);
            }
            return _overFlow;
        }
        /// <summary>
        /// Decrements the counter-register
        /// </summary>
        /// <returns>Bool</returns>
        private bool DoCounterDec()
        {
            bool _isNegative = false;
            int _counterValue = _counter.ReadRegister();
            int newValue = _counterValue - 1;
            if (newValue < 0)
            {
                _isNegative = true;
                _counter.WriteRegister(0);
            }
            else
            {
                _counter.WriteRegister(_counterValue - 1);
            }
            return _isNegative;
        }
        /// <summary>
        /// Increments the counter-register
        /// </summary>
        /// <returns>Bool</returns>
        private bool DoCounterInc()
        {
            bool _overFlow = false;
            int _counterValue = _counter.ReadRegister();
            int newValue = _counterValue + 1;
            if (newValue > 255)
            {
                _overFlow = true;
                _counter.WriteRegister(255);
            }
            else
            {
                _counter.WriteRegister(_counterValue + 1);
            }
            return _overFlow;
        }
        private void DoHalt()
        {
            DoDumpRegisters();
            this._halt = true;
        }
        private void DoWait()
        {
            //Need Wait-state/Interrupt code
        }
        private void DoShiftLeft()
        {
            _registers[_ProgramMemory.ReadMemByte(_programCounter.ReadRegister())].WriteRegister(_registers[_ProgramMemory.ReadMemByte(_programCounter.ReadRegister())].ReadRegister() << _ProgramMemory.ReadMemByte(_programCounter.ReadRegister() + 1));
            _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
        }
        private void DoShiftRight()
        {
            _registers[_ProgramMemory.ReadMemByte(_programCounter.ReadRegister())].WriteRegister(_registers[_ProgramMemory.ReadMemByte(_programCounter.ReadRegister())].ReadRegister() >> _ProgramMemory.ReadMemByte(_programCounter.ReadRegister() + 1));
            _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
        }
        /// <summary>
        /// Sets the programcounter to the new adress.
        /// </summary>
        /// <param name="AdressToJumpTo"></param>
        private void DoJump(int AdressToJumpTo)
        {
            _programCounter.WriteRegister(AdressToJumpTo);
        }
        /// <summary>
        /// Sets the programcounter to the new adress if the required parameter is zero
        /// </summary>
        /// <param name="AdressToJumpTo"></param>
        private void DoJumpIfZero(int AdressToJumpTo, int JumpCondition)
        {
            if (_ProgramMemory.ReadMemByte(JumpCondition) == 0)
            {
                _programCounter.WriteRegister(AdressToJumpTo);
            } else
            {
                _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
            }
        }

        /// <summary>
        /// Writes the conten of the register to the memory.
        /// </summary>
        /// <param name="RegisterToReadFrom"></param>
        /// <param name="AdressToWriteTo"></param>
        private void DoStore(int RegisterToReadFrom, int AdressToWriteTo)
        {
            _ProgramMemory.WriteMemByte(AdressToWriteTo, (byte)_registers[RegisterToReadFrom].ReadRegister());
        }

        /// <summary>
        /// Loads the content from Memory into register.
        /// </summary>
        /// <param name="AdressToReadFrom"></param>
        /// <param name="RegisterToWriteTo"></param>
        private void DoLoad(int AdressToReadFrom, int RegisterToWriteTo)
        {
            _registers[RegisterToWriteTo].WriteRegister(_ProgramMemory.ReadMemByte(AdressToReadFrom));
        }
        #endregion
        #endregion

        #region program-functions
        /// <summary>
        /// Main CPU-loop
        /// Reads the program-counter and checks the instruction to execute.
        /// </summary>
        public void Update()
        {
            _instructionRegister.WriteRegister(_ProgramMemory.ReadMemByte(_programCounter.ReadRegister()));
            _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
            
            switch ((InstructionSet)_instructionRegister.ReadRegister())
                {
                case InstructionSet.NOP:
                    DoNoP();
                    break;
                case InstructionSet.MOV:
                    _adressBus = _programCounter.ReadRegister();
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
                    DoMove(_adressBus, _programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
                    break;
                case InstructionSet.SAVE:
                    DoSave(_programCounter.ReadRegister());
                    break;
                case InstructionSet.READ:
                    DoRead(_programCounter.ReadRegister());
                    break;
                case InstructionSet.ADD:
                    DoAdd();
                    break;
                case InstructionSet.SUB:
                    DoSubtract();
                    break;
                case InstructionSet.MUX:
                    DoMultiply();
                    break;
                case InstructionSet.DIV:
                    DoDivision();
                    break;
                case InstructionSet.SHL:
                    DoShiftLeft();
                    break;
                case InstructionSet.SHR:
                    DoShiftRight();
                    break;
                case InstructionSet.HALT:
                    DoHalt();
                    break;
                case InstructionSet.WAIT:
                    DoWait();
                    break;
                case InstructionSet.RST:
                    DoReset();
                    break;
                case InstructionSet.DEC:
                    _adressBus= GetAdress(_programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
                    DoDec(_adressBus);
                    break;
                case InstructionSet.INC:
                    _adressBus = GetAdress(_programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
                    DoInc(_adressBus);
                    break;
                case InstructionSet.CDE:
                    DoCounterDec();
                    break;
                case InstructionSet.CIN:
                    DoCounterInc();
                    break;
                case InstructionSet.LOAD:
                    _adressBus = GetAdress(_programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
                    DoLoad(_adressBus, _ProgramMemory.ReadMemByte(_programCounter.ReadRegister()));
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 1);
                    break;
                case InstructionSet.STORE:
                    _adressBus= GetAdress(_programCounter.ReadRegister() + 1);
                    DoStore(_ProgramMemory.ReadMemByte(_programCounter.ReadRegister()), _adressBus);
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
                    break;
                case InstructionSet.JMP:
                    _adressBus= GetAdress(_programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
                    DoJump(_adressBus);
                    break;
                case InstructionSet.JZ:
                    _adressBus= GetAdress(_programCounter.ReadRegister());
                    _programCounter.WriteRegister(_programCounter.ReadRegister() + 2);
                    DoJumpIfZero(_adressBus, GetAdress(_programCounter.ReadRegister()));
                    break;
                default:
                    DoUnknownOp();
                    break;
            }
        }

        public void GraphicsTest()
        {
            int address = 256;
            byte[] byteToWrite = { 84, 101, 115, 116, 32 };
            int bytecounter = 0;
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < _graphicsMemory; i ++)
            {
                _ProgramMemory.WriteMemByte(address, byteToWrite[bytecounter]);
                if (bytecounter == 4)
                {
                    bytecounter = 0;
                } else
                {
                    bytecounter++;
                }
                address++;
            }
        }

        /// <summary>
        /// Draws the content of the graphics-part of memory to the screen.
        /// </summary>
        public void Draw()
        {
            
            byte[] screenMem = _ProgramMemory.ReadMemSequence(100, 2000);
            Console.SetCursorPosition(0, 0);
            foreach (int ByteToWrite in screenMem)
            {
                Console.Write(Convert.ToChar(ByteToWrite));
            }
        }
        #endregion
    }
}
