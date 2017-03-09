using System;

/// <summary>
/// CPU Prototype
/// Author: Harman Mahal
/// Version: 1.0
/// </summary>
namespace _6502Proto
{
    /// <summary>
    /// Prototype for the NES emulator. The CPU can Load to Accumulator, Reg X
    /// Move data from Acc register to memory and load the program into the memory.
    /// </summary>
    internal class CPU6502
    {
        //CPU Registers. The prototype only utilises accumulator, PC and reg x
        private ushort pc_register;

        private int stack_pointer;
        private byte accumulator;
        private byte reg_x;
        private byte reg_y;

        //CPU Status Flags
        private byte carry_flag; //http://www.zophar.net/fileuploads/2/10532krzvs/6502.txt
        /*this holds the carry out of the most significant
         bit in any arithmetic operation. In subtraction operations however, this
        flag is cleared - set to 0 - if a borrow is required, set to 1 - if no
        borrow is required. The carry flag is also used in shift and rotate
        logical operations.*/

        private byte zero_flag;/*this is set to 1 when any arithmetic or logical
         operation produces a zero result, and is set to 0 if the result is
         non-zero.*/

        private byte interrupt_flag;/*this is an interrupt enable/disable flag. If it is set,
        interrupts are disabled. If it is cleared, interrupts are enabled.*/

        private byte decimal_flag; /*this is the decimal mode status flag. When set, and an Add with
        Carry or Subtract with Carry instruction is executed, the source values are
        treated as valid BCD (Binary Coded Decimal, eg. 0x00-0x99 = 0-99) numbers.
        The result generated is also a BCD number.*/

        private byte break_flag; /*this is set when a software interrupt (BRK instruction) is
        executed.*/

        //Bit 5: not used.Supposed to be logical 1 at all times.

        private byte sign_flag; /*when an arithmetic operation produces a result
        too large to be represented in a byte, V is set.*/

        private byte overflow_flag; /*this is set if the result of an operation is
        negative, cleared if positive.*/

        private Memory RAM;

        /// <summary>
        /// Returns the memory of the CPU
        /// </summary>
        /// <returns></returns>
        public Memory getRAM()
        {
            return RAM;
        }

        /// <summary>
        /// Loads a byte array into the memory as program.
        /// </summary>
        /// <param name="program">Program to be loaded into the memory</param>
        public void LoadProgram(byte[] program)
        {
            RAM.LoadProgramIntoMemory(program);
        }

        /// <summary>
        /// Constructor for the CPU. Initializes memory object and provides default values
        /// for PC and Stack Pointer
        /// </summary>
        public CPU6502()
        {
            RAM = new Memory(0x10); //Create a constructor to accept size
            //initialize RAM to size 32
            stack_pointer = 0x20;
            pc_register = 0x00;
        }

        /// <summary>
        /// Executes the next instruction at the location of PC in the memory
        /// </summary>
        public void doNext()
        {
            ExecuteCode(RAM.ReadMemory(pc_register++));
        }

        /// <summary>
        /// Gets the value held at the PC location in memory and increments PC
        /// </summary>
        /// <returns>returns value held in memory</returns>
        private byte getNext()
        {
            return RAM.ReadMemory(pc_register++);
        }

        /// <summary>
        /// Checks the opcode and performs the appropriate action
        /// </summary>
        /// <param name="opcode">Opcode to be executed by the CPU</param>
        private void ExecuteCode(byte opcode)
        {
            switch (opcode)
            {
                //LDA
                case 0xA9:
                    accumulator = getNext();
                    break;
                //STA
                case 0x85:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;
                //LDX
                case 0xA2:
                    reg_x = getNext();
                    break;
                //TXA
                case 0x8A:
                    accumulator = reg_x;
                    break;
                //Break
                case 0x00:
                    pc_register = byte.MaxValue;
                    break;
                //AND
                case 0x29:
                    
                    break;
                //ASL
                case 0x0A:
                    pc_register = byte.MaxValue;
                    break;
                //BCC
                case 0x90:
                    
                    break;
                //BCS
                case 0xB0:

                    break;
                //BEQ
                case 0xF0:

                    break;
                //BIT
                case 0x24:

                    break;
                //BMI
                case 0x30:

                    break;
                //BNE
                case 0xD0:

                    break;
                //BPL
                case 0x10:

                    break;
                //BVC
                case 0x50:

                    break;
                //BVS
                case 0x70:

                    break;
                //CLC
                case 0x18:

                    break;
                //CLD
                case 0xD8:

                    break;
                //CLI
                case 0x58:

                    break;
                //CLV
                case 0xB8:

                    break;
                //CMP
                case 0xC9:

                    break;
                //CPX
                case 0xE0:

                    break;
                //CPY
                case 0xC0:

                    break;
                //DEC
                case 0xC6:

                    break;
                //DEX
                case 0xCA:

                    break;
                //DEY
                case 0x88:

                    break;
                //EOR
                case 0x49:

                    break;
                //INC
                case 0xE6:

                    break;
                //INX
                case 0xE8:

                    break;
                //INY
                case 0xC8:

                    break;
                //JMP
                case 0x6C:

                    break;
                //JSR
                case 0x20:

                    break;
                //LDY
                case 0xA0:

                    break;
                //LSR
                case 0x4A:

                    break;
                //NOP
                case 0xEA:

                    break;
                //ORA
                case 0x09:

                    break;
                //PHA
                case 0x48:

                    break;
                //PHP
                case 0x08:

                    break;
                //PLA
                case 0x68:

                    break;
                //PLP
                case 0x28:

                    break;
                //ROL
                case 0x2A:

                    break;
                //ROR
                case 0x6A:

                    break;
                //RTI
                case 0x40:

                    break;
                //RTS
                case 0x60:

                    break;
                //SBC
                case 0xE9:

                    break;
                //SEC
                case 0x38:

                    break;
                //SED
                case 0xF8:

                    break;
                //SEI
                case 0x78:

                    break;
                //STA
                case 0x95:

                    break;
                //STX
                case 0x86:

                    break;
                //STY
                case 0x84:

                    break;
                //TAX
                case 0xAA:

                    break;
                //TAY
                case 0xA8:

                    break;
                //TSX
                case 0xBA:

                    break;
                //TXS
                case 0x9A:

                    break;
                //TYA
                case 0x98:

                    break;




                default:
                    break;
            }
        }

        /// <summary>
        /// Prints the values held by ACC and the INDEX X registers
        /// </summary>
        public void printRegisters()
        {
            Console.WriteLine("{0,20} {1,20}", "Accumulator", "X Index Register");
            Console.WriteLine("{0,20} {1,20}", accumulator.ToString("X"), reg_x.ToString("X"));
        }
    }
}