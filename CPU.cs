using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Concept
{
    class CPU
    {
        private CPU_Registers _register0;
        private CPU_Registers _register1;
        private CPU_Registers _tempRegister;
        private CPU_Registers _instructionRegister;
        private int _programCounter;
        private Memory _ProgramMemory;
        private string _haltRegisters;

        private bool _overFlow;
        private bool _underFlow;
        private bool _halt;
        private bool _fault;
        
        public bool Overflow { get { return this._overFlow; } }
        public bool Underflow { get { return this._underFlow; } }
        public bool Halt { get { return this._halt; } }
        public string HaltRegisters { get { return _haltRegisters; } }
        public void WriteMemory(int Adress, byte ByteToWrite)
        {
            _ProgramMemory.WriteMemByte(Adress, ByteToWrite);
        }
        public int ReadMemory(int Adress)
        {
            return _ProgramMemory.ReadMemByte(Adress);
        }
        public int MemorySize { get { return _ProgramMemory.MemorySize; } }
        public void SetFault()
        {
            _fault = true;
            DoCrash();
        }
        public bool Fault { get { return _fault; } }

        public enum InstructionSet
        {
            NoP,
            LOAD0,
            LOAD1,
            SAVE0,
            SAVE1,
            READ0,
            READ1,
            ADD,
            SUB,
            HALT,
            WAIT
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
            _haltRegisters = "P:" + _programCounter
                + " I:" + _instructionRegister.ReadRegister()
                + " 0:" + _register0.ReadRegister()
                + " 1:" + _register1.ReadRegister()
                + " t:" + _tempRegister.ReadRegister()
                + " O:" + _overFlow
                + " U:" + _underFlow + "\r\n"
                + " CPU-Mem:";
            foreach(int memByte in _ProgramMemory.ReadMemSequence(0, _ProgramMemory.MemorySize))
            {
                _haltRegisters = _haltRegisters + memByte;
            }
        }

        private void DoNoP()
        {
        }
        private void DoLoad0()
        {
            _register0.WriteRegister(_ProgramMemory.ReadMemByte(_programCounter));
            _programCounter++;
        }
        private void DoLoad1()
        {
            _register1.WriteRegister(_ProgramMemory.ReadMemByte(_programCounter));
            _programCounter++;
        }
        private void DoSave0()
        {
            if (_tempRegister.ReadRegister() < 0)
            {
                _register0.WriteRegister(0);
                _underFlow = true;
                DoCrash();
            }
            else if (_tempRegister.ReadRegister() > _register1.MaxValue)
            {
                _register0.WriteRegister(255);
                _overFlow = true;
                DoCrash();
            }
            else
            {
                _register0.WriteRegister((byte)_tempRegister.ReadRegister());
            }
        }
        private void DoSave1()
        {
            if (_tempRegister.ReadRegister() < 0)
            {
                _register1.WriteRegister(0);
                _underFlow = true;
                DoCrash();
            } else if (_tempRegister.ReadRegister() > _register1.MaxValue)
            {
                _register1.WriteRegister(255);
                _overFlow = true;
                DoCrash();
            } else 
            {
                _register1.WriteRegister((byte)_tempRegister.ReadRegister());
            }
        }
        private int DoRead0()
        {
            _tempRegister.WriteRegister(_register0.ReadRegister());
            return _tempRegister.ReadRegister();
        }
        private int DoRead1()
        {
            _tempRegister.WriteRegister(_register1.ReadRegister());
            return _tempRegister.ReadRegister();
        }
        private void DoAdd()
        {
            _tempRegister.WriteRegister(_register0.ReadRegister() + _register1.ReadRegister());
        }
        private void DoSubtract()
        {
            _tempRegister.WriteRegister(_register0.ReadRegister() - _register1.ReadRegister());
        }
        private void DoMultiply()
        {

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
        #endregion

        public CPU(int BusWidth)
        {
            this._register0 = new CPU_Registers(BusWidth);
            this._register1 = new CPU_Registers(BusWidth);
            this._tempRegister = new CPU_Registers(BusWidth * 2);
            this._instructionRegister = new CPU_Registers(BusWidth);
            this._ProgramMemory = new Memory(256);
        }

        public void Initialize()
        {
            this._overFlow = false;
            this._underFlow = false;
            this._halt = false;
            this._programCounter = 0;
        }

        public void Reset()
        {
            this._overFlow = false;
            this._underFlow = false;
            this._halt = false;
            this._programCounter = 0;
        }

        public void Update()
        {
            _instructionRegister.WriteRegister(_ProgramMemory.ReadMemByte(_programCounter));
            _programCounter++;
            
            switch ((InstructionSet)_instructionRegister.ReadRegister())
                {
                case InstructionSet.NoP:
                    DoNoP();
                    break;
                case InstructionSet.LOAD0:
                    DoLoad0();
                    break;
                case InstructionSet.LOAD1:
                    DoLoad1();
                    break;
                case InstructionSet.SAVE0:
                    DoSave0();
                    break;
                case InstructionSet.SAVE1:
                    DoSave1();
                    break;
                case InstructionSet.READ0:
                    DoRead0();
                    break;
                case InstructionSet.READ1:
                    DoRead1();
                    break;
                case InstructionSet.ADD:
                    DoAdd();
                    break;
                case InstructionSet.SUB:
                    DoSubtract();
                    break;
                case InstructionSet.HALT:
                    DoHalt();
                    break;
                case InstructionSet.WAIT:
                    DoWait();
                    break;
                default:
                    DoUnknownOp();
                    break;
            }
        }
    }
}
