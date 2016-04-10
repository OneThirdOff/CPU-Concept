using System;
using System.Collections.Generic;

namespace CPU_Concept
{
    class CPU
    {
        #region private stuff
        private List<CPU_Registers> _registers;
        private CPU_Registers _tempRegister;
        private CPU_Registers _adressRegister;
        private CPU_Registers _instructionRegister;
        private CPU_Registers _counter;
        private int _programCounter;
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
        public int Adress { get { return _adressRegister.ReadRegister(); } }
        public string[] HaltRegisters { get { return _haltRegisters; } }

        public void WriteMemory(int Adress, byte ByteToWrite)
        {
            _ProgramMemory.WriteMemByte(Adress, ByteToWrite);
        }
        public int ReadMemory(int Adress)
        {
            return _ProgramMemory.ReadMemByte(Adress);
        }
        public int[] ReadMemory(int Address, int Length)
        {
            int[] returnBytes = new int[Length];
            for (int i = Address; i < Length; i++)
            {
                returnBytes[Address] = ReadMemory(Address);
            }

            return returnBytes;
        }
        public int MemorySize { get { return _ProgramMemory.MemorySize; } }
        public int ProgramMemorySize { get { return _programMemorySize; } }

        //Flags
        public bool Overflow { get { return this._overFlow; } }
        public bool Underflow { get { return this._underFlow; } }
        public bool IndexOutOfRange { get { return !this._adressInRange; } }
        public bool Halt { get { return this._halt; } }
        public bool Reset { get { return this._reset; } }
        #endregion

        public void SetFault()
        {
            _fault = true;
            DoCrash();
        }
        public bool Fault { get { return _fault; } }

        #region create and initialize
        public CPU(int BusWidth, int AddressBusWidth, int NumberOfRegisters)
        {
            this._ProgramMemory = new Memory(2256);
            _graphicsMemory = 2000;
            _programMemorySize = this._ProgramMemory.MemorySize - _graphicsMemory;
            _counter = new CPU_Registers(AddressBusWidth);
            _adressRegister = new CPU_Registers(AddressBusWidth * 2);
            _registers = new List<CPU_Registers>();
            for (int i = 0; i < NumberOfRegisters; i++)
            {
                _registers.Add(new CPU_Registers(BusWidth));
            }
            this._tempRegister = new CPU_Registers(BusWidth * 2);
            this._instructionRegister = new CPU_Registers(BusWidth);
            this._haltRegisters = new string[8];
        }
        public void Initialize()
        {
            this._overFlow = false;
            this._underFlow = false;
            this._halt = false;
            this._programCounter = 1;
            this._programLength = _ProgramMemory.ReadMemByte(0);
        }
        public void DoReset()
        {
            this._overFlow = false;
            this._underFlow = false;
            this._halt = false;
            this._programCounter = 0;
            this._reset = false;
        }
        #endregion

        #region CPU-functions
        public enum InstructionSet
        {
            NoP = 0,
            LOAD = 1,
            SAVE = 2,
            READ = 3,
            ADD = 4,
            SUB = 5,
            MUX = 6,
            DIV = 7,
            SHL = 8,
            SHR = 9,
            WAIT = 10,
            DEC = 11,
            INC = 12,
            CDE = 13,
            CIN = 14,
            RST = 250,
            HALT = 255
        }
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
        private void DoLoad(int address)
        {
            _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister(_ProgramMemory.ReadMemByte(address));
        }
        private void DoSave()
        {
            if (_tempRegister.ReadRegister() < 0)
            {
                _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister(0);
                _underFlow = true;
                DoCrash();
            }
            else if (_tempRegister.ReadRegister() > _registers[1].MaxValue)
            {
                _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister(255);
                _overFlow = true;
                DoCrash();
            }
            else
            {
                _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister((byte)_tempRegister.ReadRegister());
                _programCounter++;
            }
        }
        private void DoRead()
        {
            _tempRegister.WriteRegister(_registers[_ProgramMemory.ReadMemByte(_programCounter)].ReadRegister());
            _programCounter++;
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
        private bool DoDec(int Adress)
        {
            bool _isNegative = false;
            int _counterValue = _registers[Adress].ReadRegister();
            int newValue = _counterValue - 1;
            if (newValue < 0 )
            {
                _isNegative = true;
                _registers[Adress].WriteRegister(0);
            } else
            {
                _registers[Adress].WriteRegister(_counterValue - 1);
            }
            return _isNegative;
        }
        private bool DoInc(int Adress)
        {
            bool _overFlow = false;
            int _counterValue = _registers[Adress].ReadRegister();
            int newValue = _counterValue + 1;
            if (newValue > 255)
            {
                _overFlow = true;
                _registers[Adress].WriteRegister(255);
            }
            else
            {
                _registers[Adress].WriteRegister(_counterValue + 1);
            }
            return _overFlow;
        }
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
            _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister(_registers[_ProgramMemory.ReadMemByte(_programCounter)].ReadRegister() << _ProgramMemory.ReadMemByte(_programCounter + 1));
            _programCounter += 2;
        }
        private void DoShiftRight()
        {
            _registers[_ProgramMemory.ReadMemByte(_programCounter)].WriteRegister(_registers[_ProgramMemory.ReadMemByte(_programCounter)].ReadRegister() >> _ProgramMemory.ReadMemByte(_programCounter + 1));
            _programCounter += 2;
        }
        #endregion
        #endregion

        #region program-functions
        public void Update()
        {
            _instructionRegister.WriteRegister(_ProgramMemory.ReadMemByte(_programCounter));
            _programCounter++;
            
            switch ((InstructionSet)_instructionRegister.ReadRegister())
                {
                case InstructionSet.NoP:
                    DoNoP();
                    break;
                case InstructionSet.LOAD:
                    _adressRegister.WriteRegister(_programCounter);
                    _programCounter++;
                    DoLoad(_adressRegister.ReadRegister());
                    _programCounter++;
                    break;
                case InstructionSet.SAVE:
                    DoSave();
                    break;
                case InstructionSet.READ:
                    DoRead();
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
                    _adressRegister.WriteRegister(_programCounter);
                    _programCounter++;
                    DoDec(_adressRegister.ReadRegister());
                    break;
                case InstructionSet.INC:
                    _adressRegister.WriteRegister(_programCounter);
                    _programCounter++;
                    DoInc(_adressRegister.ReadRegister());
                    break;
                case InstructionSet.CDE:
                    DoCounterDec();
                    break;
                case InstructionSet.CIN:
                    DoCounterInc();
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
