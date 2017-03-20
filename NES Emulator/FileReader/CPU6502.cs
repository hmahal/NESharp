using System;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// Ricoh 6502 CPU
/// Author: Harman Mahal, George Lee, Steven Ma
/// Version: 1.0
/// </summary>
namespace NES
{
    /// <summary>
    /// Emulation of the Ricoh 6502 CPU and its functions and capabilities
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

        private byte overflow_flag; /*when an arithmetic operation produces a result
        too large to be represented in a byte, V is set.*/

        private byte sign_flag; /*this is set if the result of an operation is
        negative, cleared if positive.*/

        private bool _running; //set to false to shut down the cpu
        private uint _cyclesToWait; //the amount of cycles this operation takes
        private Thread _cpuThread;
        private uint _cycles; //Which cycle is the cpu on

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
        /// Starts the cpu, to be called after CPU is properly set up
        /// </summary>
        public void start()
        {
            string methodname = "CPU.start()";
            Debug.WriteLine("Entered: " + methodname);
            try
            {
                if (_cpuThread != null)
                {
                    _cpuThread = new Thread(new ThreadStart(run));
                    _running = true;
                    _cpuThread.Start();
                }
                else
                {
                    Debug.WriteLine("Thread already exists.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Debug.WriteLine("Exiting: " + methodname);
            }
        }

        /// <summary>
        /// The loop that is run by the cpu thread
        /// Run as past as possible, the thread may 
        /// not keep up with the real cpu cycles per second (~556ns per cycle)
        /// </summary>
        private void run()
        {
            string methodname = "CPU.run()";
            Debug.WriteLine("Entered: " + methodname);
            try
            {
                while (_running)
                {
                    if (_cyclesToWait <= 0)
                    {
                        doNext();
                    }
                    if (_cycles == 3)
                    {
                        //do ppu
                        //do apu
                        _cycles -= 3;
                    }
                    _cyclesToWait--;
                    _cycles++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Debug.WriteLine("Exiting: " + methodname);
            }
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
                #region Opcodes

                #region Immediate Addressing
                //ADC - Add memory to accumulator with carry  
                case 0x69:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x29:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC9:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xE0:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPY - CPY Compare memory and index Y
                case 0xC0:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x49:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA9:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDX - LDX Load index X with memory
                case 0xA2:
                    reg_x = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDY - LDY Load index Y with memory
                case 0xA0:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x09:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE9:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                #endregion

                #region Implied
                //BRK - BRK Force Break
                case 0x00:
                    pc_register = byte.MaxValue;
                    //interrupt_flag set to 1
                    break;
                //CLC - CLC Clear carry flag 
                case 0x18:
                    //carry_flag set to 0
                    break;
                //CLD - CLD Clear decimal mode
                case 0xD8:
                    //decimal_flag set to 0
                    break;
                //CLI - CLI Clear interrupt disable bit
                case 0x58:
                    //interrupt_flag set to 0
                    break;
                //CLV - CLV Clear overflow flag
                case 0xB8:
                    //overflow_flag set to 0
                    break;
                //DEX - DEX Decrement index X by one
                case 0xCA:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //DEY - DEY Decrement index Y by one
                case 0x88:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INX - INX Increment Index X by one
                case 0xE8:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INY - INY Increment Index Y by one
                case 0xC8:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //NOP - NOP No operation
                case 0xEA:
                    //no flags
                    break;
                //PHA - PHA Push accumulator on stack
                case 0x48:
                    //no flags
                    break;
                //PHP - PHP Push processor status on stack
                case 0x08:
                    //no flags
                    break;
                //PLA - PLA Pull accumulator from stack
                case 0x68:
                    // no flags
                    break;
                //PLP -  PLP Pull processor status from stack
                case 0x28:
                    //flags from stack
                    break;
                //RTI - RTI Return from interrupt
                case 0x40:
                    //flags from stack
                    break;
                //RTS - RTS Return from subroutine
                case 0x60:
                    //no flags
                    break;
                //SEC - SEC Set carry flag
                case 0x38:
                    //carry_flag set to 1
                    break;
                //SED - SED Set decimal mode
                case 0xF8:
                    //decimal_flag set to 1
                    break;
                //SEI - SEI Set interrupt disable status
                case 0x78:
                    //interrupt_flag set to 1
                    break;
                //TAX - TAX Transfer accumulator to index X
                case 0xAA:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //TAY - TAY Transfer accumulator to index Y 
                case 0xA8:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //TSX - TSX Transfer stack pointer to index X
                case 0xBA:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //TXA - TXA Transfer index X to accumulator
                case 0x8A:
                    accumulator = reg_x;
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //TXS - TXA Transfer index X to accumulator
                case 0x9A:
                    //no flags
                    break;
                //TYA - TYA Transfer index Y to accumulator
                case 0x98:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                #endregion

                #region Relative
                //BCC -  BCC Branch on Carry Clear
                case 0x90:

                    //no flags
                    break;
                //BCS - BCS Branch on carry set
                case 0xB0:
                    //no flags
                    break;
                //BEQ -  BEQ Branch on result zero
                case 0xF0:
                    //no flags
                    break;
                //BMI - BMI Branch on result minus
                case 0x30:
                    //no flags
                    break;
                //BNE - BNE Branch on result not zero 
                case 0xD0:
                    //no flags
                    break;
                //BPL - BPL Branch on result plus
                case 0x10:
                    //no flags
                    break;
                //BVC - BVC Branch on overflow clear
                case 0x50:
                    //no flags
                    break;
                //BVS - Branch on V = 1 
                case 0x70:
                    //no flags
                    break;
                #endregion

                #region Accumulator
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x0A:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x4A:
                    //sign_flag set to 0
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x2A:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x6A:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                #endregion

                #region Zero-page
                //ADC - Add memory to accumulator with carry  
                case 0x65:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x25:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x06:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //BIT - BIT Test bits in memory with accumulator 
                case 0x24:

                    //sign_flag transferred to status register
                    //overflow_flag transferred to status register
                    //If result A /\ M is zero then zero_flag = 1, otherwise zero_flag = 0 
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC5:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xE4:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPY - CPY Compare memory and index Y
                case 0xC4:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //DEC - DEC Decrement memory by one
                case 0xC6:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x45:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INC - INC Increment memory by one
                case 0xE6:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA5:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDX - LDX Load index X with memory
                case 0xA6:
                    reg_x = getNext();
                    break;
                //LDY - LDY Load index Y with memory
                case 0xA4:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x46:
                    //sign_flag set to 0
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x05:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x26:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x66:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE5:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x85:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                //STX - STX Store index X in memory
                case 0x86:
                    //no flags
                    break;
                //STY -  STY Store index Y in memory
                case 0x84:
                    //no flags
                    break;
                #endregion

                #region Absolute
                //ADC - Add memory to accumulator with carry  
                case 0x6D:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x2D:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x0E:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //BIT - BIT Test bits in memory with accumulator 
                case 0x2C:

                    //sign_flag transferred to status register
                    //overflow_flag transferred to status register
                    //If result A /\ M is zero then zero_flag = 1, otherwise zero_flag = 0 
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xCD:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPX - CPX Compare Memory and Index X
                case 0xEC:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CPY - CPY Compare memory and index Y
                case 0xCC:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //DEC - DEC Decrement memory by one
                case 0xCE:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x4D:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INC - INC Increment memory by one
                case 0xEE:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //JMP - JMP Jump to new location
                case 0x6C:
                    //no flag
                    break;
                //JSR - JSR Jump to new location saving return address
                case 0x20:
                    //no flag
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xAD:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDX - LDX Load index X with memory
                case 0xAE:
                    reg_x = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDY - LDY Load index Y with memory
                case 0xAC:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x4E:
                    //sign_flag set to 0
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x0D:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x2E:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x6E:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xED:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x8D:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                //STX - STX Store index X in memory
                case 0x8E:
                    //no flags
                    break;
                //STY -  STY Store index Y in memory
                case 0x8C:
                    //no flags
                    break;
                #endregion

                #region Zero Page X
                //ADC - Add memory to accumulator with carry  
                case 0x75:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x35:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x16:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xD5:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //DEC - DEC Decrement memory by one
                case 0xD6:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x55:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INC - INC Increment memory by one
                case 0xF6:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xB5:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDY - LDY Load index Y with memory
                case 0xB4:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x56:
                    //sign_flag set to 0
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x15:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x36:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x76:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xF5:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x95:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                //STY -  STY Store index Y in memory
                case 0x94:
                    //no flags
                    break;
                #endregion

                #region Zero Page Y
                //LDX - LDX Load index X with memory
                case 0xB6:
                    reg_x = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //STX - STX Store index X in memory
                case 0x96:
                    //no flags
                    break;
                #endregion

                #region Absolute X
                //ADC - Add memory to accumulator with carry  
                case 0x7D:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x3D:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ASL -  ASL Shift Left One Bit (Memory or Accumulator)
                case 0x1E:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xDD:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //DEC - DEC Decrement memory by one
                case 0xDE:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x5D:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //INC - INC Increment memory by one
                case 0xFE:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xBD:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDY - LDY Load index Y with memory
                case 0xBC:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LSR - LSR Shift right one bit (memory or accumulator) 
                case 0x5E:
                    //sign_flag set to 0
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x1D:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ROL - ROL Rotate one bit left (memory or accumulator)
                case 0x3E:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //ROR - ROR Rotate one bit right (memory or accumulator)
                case 0x7E:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xFD:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x9D:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                #endregion

                #region Absolute Y
                //ADC - Add memory to accumulator with carry  
                case 0x79:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x39:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xD9:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x59:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xB9:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDX - LDX Load index X with memory
                case 0xBE:
                    reg_x = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x19:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xF9:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x99:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                #endregion

                #region Indirect X
                //ADC - Add memory to accumulator with carry  
                case 0x61:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x21:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xC1:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x41:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xA1:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x01:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xE1:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x81:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                #endregion


                #region Indirect Y
                //ADC - Add memory to accumulator with carry  
                case 0x71:

                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //AND - "AND" memory with accumulator
                case 0x31:

                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //CMP - CMP Compare memory and accumulator
                case 0xD1:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    break;
                //EOR - EOR "Exclusive-Or" memory with accumulator
                case 0x51:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //LDA - LDA Load accumulator with memory
                case 0xB1:
                    accumulator = getNext();
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //ORA -  ORA "OR" memory with accumulator
                case 0x11:
                    //sign_flag flipped
                    //zero_flag flipped
                    break;
                //SBC - SBC Subtract memory from accumulator with borrow
                case 0xF1:
                    //sign_flag flipped
                    //zero_flag flipped
                    //carry_flag flipped
                    //overflow_flag flipped
                    break;
                //STA - STA Store accumulator in memory
                case 0x91:
                    RAM.WriteMemory(getNext(), accumulator);
                    //no flags
                    break;
                #endregion






                default:
                    break;
                    #endregion
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