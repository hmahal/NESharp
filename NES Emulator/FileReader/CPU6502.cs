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
//Immediate Addressing
                //ADC - Add memory to accumulator with carry  
                case 0x69:

                    break;
                //AND - "AND" memory with accumulator
                case 0x29:
                    
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC9:

                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xE0:

                    break;
                //CPY - CPY Compare memory and index Y
                case 0xC0:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x49:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA9:
                    accumulator = getNext();
                    break;
                //LDX - LDX Load index X with memory
                case 0xA2:
                    reg_x = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xA0:

                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x09:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE9:

                    break;



 /* Implied */
                //BRK - BRK Force Break
                case 0x00:
                    pc_register = byte.MaxValue;
                    break;
                //CLC - CLC Clear carry flag 
                case 0x18:

                    break;
                //CLD - CLD Clear decimal mode
                case 0xD8:

                    break;
                //CLI - CLI Clear interrupt disable bit
                case 0x58:

                    break;
                //CLV - CLV Clear overflow flag
                case 0xB8:

                    break;
                //DEX - DEX Decrement index X by one
                case 0xCA:

                    break;
                //DEY - DEY Decrement index Y by one
                case 0x88:

                    break;
                //INX - INX Increment Index X by one
                case 0xE8:

                    break;
                //INY - INY Increment Index Y by one
                case 0xC8:

                    break;
                //NOP - NOP No operation
                case 0xEA:

                    break;
                //PHA - PHA Push accumulator on stack
                case 0x48:

                    break;
                //PHP - PHP Push processor status on stack
                case 0x08:

                    break;
                //PLA - PLA Pull accumulator from stack
                case 0x68:

                    break;
                //PLP -  PLP Pull processor status from stack
                case 0x28:

                    break;
                //RTI - RTI Return from interrupt
                case 0x40:

                    break;
                //RTS - RTS Return from subroutine
                case 0x60:

                    break;
                //SEC - SEC Set carry flag
                case 0x38:

                    break;
                //SED - SED Set decimal mode
                case 0xF8:

                    break;
                //SEI - SEI Set interrupt disable status
                case 0x78:

                    break;
                //TAX - TAX Transfer accumulator to index X
                case 0xAA:

                    break;
                //TAY - TAY Transfer accumulator to index Y 
                case 0xA8:

                    break;
                //TSX - TSX Transfer stack pointer to index X
                case 0xBA:

                    break;
                //TXA - TXA Transfer index X to accumulator
                case 0x8A:
                    accumulator = reg_x;
                    break;
                //TXS - TXA Transfer index X to accumulator
                case 0x9A:

                    break;
                //TYA - TYA Transfer index Y to accumulator
                case 0x98:

                    break;

//Relative
                //BCC -  BCC Branch on Carry Clear
                case 0x90:

                    break;
                //BCS - BCS Branch on carry set
                case 0xB0:

                    break;
                //BEQ -  BEQ Branch on result zero
                case 0xF0:

                    break;
                //BMI - BMI Branch on result minus
                case 0x30:

                    break;
                //BNE - BNE Branch on result not zero 
                case 0xD0:

                    break;
                //BPL - BPL Branch on result plus
                case 0x10:

                    break;
                //BVC - BVC Branch on overflow clear
                case 0x50:

                    break;
                //BVS - Branch on V = 1 
                case 0x70:

                    break;

/*Accumulator*/
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x0A:

                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x4A:

                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x6A:

                    break;


/*Zero-page*/
                //ADC - Add memory to accumulator with carry  
                case 0x65:

                    break;
                //AND - "AND" memory with accumulator
                case 0x25:

                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x06:

                    break;
                //BIT - BIT Test bits in memory with accumulator 
                case 0x24:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC5:

                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xE4:

                    break;
                //CPY - CPY Compare memory and index Y
                case 0xC4:

                    break;
                //DEC - DEC Decrement memory by one
                case 0xC6:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x45:

                    break;
                //INC - INC Increment memory by one
                case 0xE6:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA5:
                    accumulator = getNext();
                    break;
                //LDX - LDX Load index X with memory
                case 0xA6:
                    reg_x = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xA4:

                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x46:

                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x05:

                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x26:

                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x66:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE5:

                    break;
                //STA
                case 0x85:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;
                //STX - STX Store index X in memory
                case 0x86:

                    break;
                //STY -  STY Store index Y in memory
                case 0x84:

                    break;


//Absolute
                //ADC - Add memory to accumulator with carry  
                case 0x6D:

                    break;
                //AND - "AND" memory with accumulator
                case 0x2D:

                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x0E:

                    break;
                //BIT - BIT Test bits in memory with accumulator 
                case 0x2C:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xCD:

                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xEC:

                    break;
                //CPY - CPY Compare memory and index Y
                case 0xCC:

                    break;
                //DEC - DEC Decrement memory by one
                case 0xCE:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x4D:

                    break;
                //INC - INC Increment memory by one
                case 0xEE:

                    break;
                //JMP - JMP Jump to new location
                case 0x6C:

                    break;
                //JSR - JSR Jump to new location saving return address
                case 0x20:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xAD:
                    accumulator = getNext();
                    break;
                //LDX - LDX Load index X with memory
                case 0xAE:
                    reg_x = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xAC:

                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x4E:

                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x0D:

                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x2E:

                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x6E:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xED:

                    break;
                //STA - STA Store accumulator in memory
                case 0x8D:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;
                //STX - STX Store index X in memory
                case 0x8E:

                    break;
                //STY -  STY Store index Y in memory
                case 0x8C:

                    break;

//Zero Page x
                //ADC - Add memory to accumulator with carry  
                case 0x75:

                    break;
                //AND - "AND" memory with accumulator
                case 0x35:

                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x16:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xD5:

                    break;
                //DEC - DEC Decrement memory by one
                case 0xD6:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x55:

                    break;
                //INC - INC Increment memory by one
                case 0xF6:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xB5:
                    accumulator = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xB4:

                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x56:

                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x15:

                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x36:

                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x76:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0x95:

                    break;
                //STY -  STY Store index Y in memory
                case 0x94:

                    break;


//Zero Page y
                //LDX - LDX Load index X with memory
                case 0xB6:
                    reg_x = getNext();
                    break;
                //STX - STX Store index X in memory
                case 0x96:

                    break;


//Absolute X
                //ADC - Add memory to accumulator with carry  
                case 0x7D:

                    break;
                //AND - "AND" memory with accumulator
                case 0x3D:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xDD:

                    break;
                //DEC - DEC Decrement memory by one
                case 0xDE:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x5D:

                    break;
                //INC - INC Increment memory by one
                case 0xFE:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xBD:
                    accumulator = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xBC:

                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x5E:

                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x1D:

                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x3E:

                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x7E:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xFD:

                    break;
                //STA - STA Store accumulator in memory
                case 0x9D:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;


//Absolute,y
                //ADC - Add memory to accumulator with carry  
                case 0x79:

                    break;
                //AND - "AND" memory with accumulator
                case 0x39:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xD9:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x59:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xB9:
                    accumulator = getNext();
                    break;
                //LDX - LDX Load index X with memory
                case 0xBE:
                    reg_x = getNext();
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x19:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xF9:

                    break;
                //STA - STA Store accumulator in memory
                case 0x99:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;


 //indirect,X
                //ADC - Add memory to accumulator with carry  
                case 0x61:

                    break;
                //AND - "AND" memory with accumulator
                case 0x21:

                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC1:

                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x41:

                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA1:
                    accumulator = getNext();
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x01:

                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE1:

                    break;
                //STA - STA Store accumulator in memory
                case 0x81:
                    RAM.WriteMemory(getNext(), accumulator);
                    break;



//indirect,Y
                //AND - "AND" memory with accumulator
                case 0x31:

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