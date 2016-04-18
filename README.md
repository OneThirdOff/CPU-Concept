# CPU-Concept

The CPU has, at the moment, two one-bit registers (A and B) and 256byte of memory plus 2k of graphics memory. Sequentially accessable.

The built in interpreter will allow the user to code then run the program.

###Instructionset:
NOP - No Operation.  
MOV [Data] [Register number] - Loads the Data in to the register  
SAVE [Register number] - Saves the Temporary-register to the register  
READ [Register number] - puts the register into the temp-register  
ADD - Adds register 0 and 1 and stores the sum in the temp-register  
SUB - Subtracts register 0 and 1 and stores the sum in the temp-register  
MUX - Multiplies register 0 by 1 and stores the sum in the temp-register  
DIV - Divides register 0 by 1 and stores the sum in the temp-register  
SHL [Register number] [number of shifts] - Bitwise-shifts the register by a number of hops to the left  
SHR [Register number] [number of shifts] - Bitwise-shifts the register by a number of hops to the right  
DEC [Register number] - Reduces the register by one if possible. Leaves at 0 if not.  
INC [Register number] - Increments the register by one if possible. Leaves at 255 if not.  
CDE - Reduces the counter-register by one if possible. Leaves at 0 if not.  
CIN - Increments the counter-register by one if possible. Leaves at 255 if not.  
WAIT - for now does nothing.  
LOAD [Memory Adress] [Register number] - Loads the data from memory to the register.  
STORE [Register number] [Memory Adress] - Stores the content of the register to memory.  
JMP [adress] - Sets instruction-counter to adress.  
JZ [adress to jump to] [adress to check if zero] - Jumps if checked adress is zero.  
RST - Resets the cpu clearing memory and registers.  
HALT - Halts the cpu, and ends the program. (for now needed to exit)  
RUN - run the program  
