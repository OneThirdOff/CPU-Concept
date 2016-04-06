# CPU-Concept

Instructionset:
 NoP - No Operation.
 LOAD [Register number] [Data] - Loads the Data in to the register
 SAVE [Register number] - Saves the Temporary-register to the register
 READ [Register number] - puts the register into the temp-register
 ADD - Adds register 0 and 1 and stores the sum in the temp-register
 SUB - Subtracts register 0 and 1 and stores the sum in the temp-register
 MUX - Multiplies register 0 by 1 and stores the sum in the temp-register
 DIV - Divides register 0 by 1 and stores the sum in the temp-register
 SHL [Register number] [number of shifts] - Bitwise-shifts the register by a number of hops to the left
 SHR [Register number] [number of shifts] - Bitwise-shifts the register by a number of hops to the right
 WAIT - for now does nothing.
 HALT - Halts the cpu, and ends the program. (for now needed to exit)
