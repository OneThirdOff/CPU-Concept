# CPU-Concept

The CPU has, at the moment, two one-bit registers (A and B) and 256byte of memory plus 2k of graphics memory. Sequentially accessable.

The built in interpreter will allow the user to code then run the program.

###Instructionset:
NOP - No Operation.  
MOV [value] [register] - Moves the value into the named register. 
SAVE [register] - Saves the value from temp-register to the register. 
READ [register] - Reads the value from the register, saves to temp-register.  
ADD - Adds register A and B and stores the result to the temp-register.  
SUB - Subrtracts register B from register A, saves to temp-register.
MUX - Multiplies register A by register B, saves to temp-register. Writes register B while multiplying.
DIV - Divides register A by register B, saves to temp-register.
SHL [Register] [number of shifts] - Bitwise-shifts the register by a number of hops to the left.
SHR [Register] [number of shifts] - Bitwise-shifts the register by a number of hops to the right.
DEC/INC [register] - Decrements or Increments the register by one.
CDE, CIN - Decrements or Increments the counter by one.
WAIT - for now does nothing.  
<<<<<<< HEAD
LOAD [adress] [register] - Loads the value in the memory adress to the register.  
STORE [register] [adress] - Stores the value in the register to the memory.  
JMP [adress] - Jumps to the adress of the memory.
JZ [adress to jump to] [adress to check if zero] - jumps if checked is zero.
=======
LOAD [Memory Adress] [Register number] - Loads the data from memory to the register.  
STORE [Register number] [Memory Adress] - Stores the content of the register to memory.  
JMP [adress] - Sets instruction-counter to adress.  
JZ [adress to jump to] [adress to check if zero] - Jumps if checked adress is zero.  
>>>>>>> d7abf05e0fd731979288c8996fe77dcd3c3df0f5
RST - Resets the cpu clearing memory and registers.  
HALT - Halts the cpu. At the moment, the only way to stop the program.
RUN - runs the program  
