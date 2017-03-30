using System;
using System.Threading;
using System.Diagnostics;
using FileReader;

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
        private ushort pc_register; //The 2-byte program counter ‘PC’ supports 65536 direct (unbanked) memory locations, however not all values are sent to the cartridge. It can be accessed either by allowing CPU's internal fetch logic increment the address bus, an interrupt (NMI, Reset, IRQ/BRQ), and using the RTS/JMP/JSR/Branch instructions.
        private byte stack_pointer; //The register is byte-wide and can be accessed using interrupts, pulls, pushes, and transfers.
        private byte accumulator; //The register is byte-wide and along with the arithmetic logic unit (ALU), supports using the status register for carrying, overflow detection, and so on.
        private byte reg_x; //X and Y are byte-wide and used for several addressing modes. They can be used as loop counters easily, using increment/decrement and branch instructions. Not being the accumulator, they have limited addressing modes themselves when loading and saving.
        private byte reg_y; //This is a very important register. There are instructions for nearly all of the transformations you can make to the accumulator, and the X register. But there are other instructions for things that only the Y register can do. Various machine language instructions allow you to copy the contents of a memory location into the Y register, copy the contents of the Y register into a memory location, and modify the contents of the Y, or some other register directly.


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

        //instruction names here
        private string[] instructions = new string[256] {
            "BRK", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
            "PHP", "ORA", "ASL", "ANC", "NOP", "ORA", "ASL", "SLO",
            "BPL", "ORA", "KIL", "SLO", "NOP", "ORA", "ASL", "SLO",
            "CLC", "ORA", "NOP", "SLO", "NOP", "ORA", "ASL", "SLO",
            "JSR", "AND", "KIL", "RLA", "BIT", "AND", "ROL", "RLA",
            "PLP", "AND", "ROL", "ANC", "BIT", "AND", "ROL", "RLA",
            "BMI", "AND", "KIL", "RLA", "NOP", "AND", "ROL", "RLA",
            "SEC", "AND", "NOP", "RLA", "NOP", "AND", "ROL", "RLA",
            "RTI", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
            "PHA", "EOR", "LSR", "ALR", "JMP", "EOR", "LSR", "SRE",
            "BVC", "EOR", "KIL", "SRE", "NOP", "EOR", "LSR", "SRE",
            "CLI", "EOR", "NOP", "SRE", "NOP", "EOR", "LSR", "SRE",
            "RTS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
            "PLA", "ADC", "ROR", "ARR", "JMP", "ADC", "ROR", "RRA",
            "BVS", "ADC", "KIL", "RRA", "NOP", "ADC", "ROR", "RRA",
            "SEI", "ADC", "NOP", "RRA", "NOP", "ADC", "ROR", "RRA",
            "NOP", "STA", "NOP", "SAX", "STY", "STA", "STX", "SAX",
            "DEY", "NOP", "TXA", "XAA", "STY", "STA", "STX", "SAX",
            "BCC", "STA", "KIL", "AHX", "STY", "STA", "STX", "SAX",
            "TYA", "STA", "TXS", "TAS", "SHY", "STA", "SHX", "AHX",
            "LDY", "LDA", "LDX", "LAX", "LDY", "LDA", "LDX", "LAX",
            "TAY", "LDA", "TAX", "LAX", "LDY", "LDA", "LDX", "LAX",
            "BCS", "LDA", "KIL", "LAX", "LDY", "LDA", "LDX", "LAX",
            "CLV", "LDA", "TSX", "LAS", "LDY", "LDA", "LDX", "LAX",
            "CPY", "CMP", "NOP", "DCP", "CPY", "CMP", "DEC", "DCP",
            "INY", "CMP", "DEX", "AXS", "CPY", "CMP", "DEC", "DCP",
            "BNE", "CMP", "KIL", "DCP", "NOP", "CMP", "DEC", "DCP",
            "CLD", "CMP", "NOP", "DCP", "NOP", "CMP", "DEC", "DCP",
            "CPX", "SBC", "NOP", "ISC", "CPX", "SBC", "INC", "ISC",
            "INX", "SBC", "NOP", "SBC", "CPX", "SBC", "INC", "ISC",
            "BEQ", "SBC", "KIL", "ISC", "NOP", "SBC", "INC", "ISC",
            "SED", "SBC", "NOP", "ISC", "NOP", "SBC", "INC", "ISC", };

        //Addressing mode table as defined in the addressingMode enum
        private int[] addressingMode = new int[256] {
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            1, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            6, 7, 6, 7, 11, 11, 11, 11, 6, 5, 4, 5, 8, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 13, 13, 6, 3, 6, 3, 2, 2, 3, 3,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2,
            5, 7, 5, 7, 11, 11, 11, 11, 6, 5, 6, 5, 1, 1, 1, 1,
            10, 9, 6, 9, 12, 12, 12, 12, 6, 3, 6, 3, 2, 2, 2, 2, };

        //# of cycles/instruction 
        private int[] instructionCycles = new int[256] {
            7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
            2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
            2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, };

        //# of bytes for a given instruction
        private int[] instructionSize = new int[256] {
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            3, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            1, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 0, 3, 0, 0,
            2, 2, 2, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0, };

        private Action[] instructionAction = new Action[256];

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

        private void setZero(int value)
        {
            if (value == 0)
                zero_flag = 1;
            else
                zero_flag = 0;
        }

        private void setBreak()
        {

        }

        private void setOverFlow()
        {

        }

        private void setSign(int value)
        {
            if ((value&0x80) != 0)
                sign_flag = 1;
            else
                sign_flag = 0;
        }

        public void Reset()
        {
            pc_register = 0xFFFC;
            stack_pointer = 0xFD;

        }

        private void Compare(int a, int b)
        {            
            setZero(a - b);
            setSign(a - b);

            if (a >= b)
                carry_flag = 1;
            else
                carry_flag = 0;
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
                        //doNext();
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
        public void Tick()
        {
            
        }


        /// <summary>
        /// Gets the value held at the PC location in memory and increments PC
        /// </summary>
        /// <returns>returns value held in memory</returns>
        private byte getNext()
        {
            return RAM.ReadMemory(pc_register++);
        }        

        private void adc(MemoryInfo mem)
        {

        }

        private void and(MemoryInfo mem)
        {
            accumulator = accumulator & RAM.ReadMemory(mem.Address);
            setZero(accumulator);
            setSign(accumulator);
        }

        private void asl(MemoryInfo mem)
        {

        }

        private void bcc(MemoryInfo mem)
        {

        }

        private void bcs(MemoryInfo mem)
        {

        }

        private void beq(MemoryInfo mem)
        {

        }

        private void bit(MemoryInfo mem)
        {

        }

        private void bmi(MemoryInfo mem)
        {
            if(sign_flag != 0)
            {

            }

        }

        private void bne(MemoryInfo mem)
        {

        }

        private void bpl(MemoryInfo mem)
        {

        }

        private void brk(MemoryInfo mem)
        {

        }

        private void bvc(MemoryInfo mem)
        {

        }

        private void bvs(MemoryInfo mem)
        {

        }

        private void clc(MemoryInfo mem)
        {
            carry_flag = 0;
        }

        private void cld(MemoryInfo mem)
        {
            interrupt_flag = 0;
        }

        private void cli(MemoryInfo mem)
        {
            interrupt_flag = 0;
        }

        private void clv(MemoryInfo mem)
        {
            overflow_flag = 0;
        }

        private void cmp(MemoryInfo mem)
        {
            Compare(accumulator, RAM.ReadMemory(mem.Address));
        }

        private void cpx(MemoryInfo mem)
        {
            Compare(reg_x, RAM.ReadMemory(mem.Address));
        }

        private void cpy(MemoryInfo mem)
        {
            Compare(reg_y, RAM.ReadMemory(mem.Address));
        }

        private void dec(MemoryInfo mem)
        {
            byte value = RAM.ReadMemory(mem.Address) - 1;
            RAM.WriteMemory(mem.Address, value);
            setZero(value);
            setSign(value);
        }

        private void dex(MemoryInfo mem)
        {
            reg_x--;
            setZero(reg_x);
            setSign(reg_x);
        }

        private void dey(MemoryInfo mem)
        {
            reg_y--;
            setZero(reg_y);
            setSign(reg_y);
        }

        private void eor(MemoryInfo mem)
        {
            accumulator = accumulator ^ RAM.ReadMemory(mem.Address);
            setZero(accumulator);
            setSign(accumulator);
        }

        private void inc(MemoryInfo mem)
        {
            byte value = RAM.ReadMemory(mem.Address) + 1;
            RAM.WriteMemory(mem.Address, value);            
            setZero(value);
            setSign(value);
        }

        private void inx(MemoryInfo mem)
        {
            reg_x++;
            setZero(reg_x);
            setSign(reg_x);
        }

        private void iny(MemoryInfo mem)
        {
            reg_y++;
            setZero(reg_y);
            setSign(reg_y);
        }

        private void jmp(MemoryInfo mem)
        {
            pc_register = mem.Address;
        }

        private void jsr(MemoryInfo mem)
        {

        }

        private void lda(MemoryInfo mem)
        {
            accumulator = RAM.ReadMemory(mem.Address);
            setZero(accumulator);
            setSign(accumulator);
        }

        private void ldx(MemoryInfo mem)
        {
            reg_x = RAM.ReadMemory(mem.Address);
            setZero(reg_x);
            setSign(reg_x);
        }

        private void ldy(MemoryInfo mem)
        {
            reg_y = RAM.ReadMemory(mem.Address);
            setZero(reg_y);
            setSign(reg_y);
        }

        private void lsr(MemoryInfo mem)
        {

        }

        private void nop(MemoryInfo mem)
        {
            //Do nothing;
        }

        private void ora(MemoryInfo mem)
        {
            accumulator = accumulator | RAM.ReadMemory(mem.Address);
            setZero(accumulator);
            setSign(accumulator);
        }

        private void pha(MemoryInfo mem)
        {

        }

        private void php(MemoryInfo mem)
        {

        }

        private void pla(MemoryInfo mem)
        {

        }

        private void plp(MemoryInfo mem)
        {

        }

        private void rol(MemoryInfo mem)
        {

        }

        private void ror(MemoryInfo mem)
        {

        }

        private void rti(MemoryInfo mem)
        {

        }

        private void rts(MemoryInfo mem)
        {

        }

        private void sbc(MemoryInfo mem)
        {
            byte memVar = RAM.ReadMemory(mem.Address);
            //TODO: Fix this
            accumulator = accumulator - memVar - (1 - carry_flag);
            setZero(accumulator);
            setSign(accumulator);

            if (accumulator >= 0)
            {
                
            }

        }

        private void sec(MemoryInfo mem)
        {
            carry_flag = 1;
        }

        private void sed(MemoryInfo mem)
        {
            decimal_flag = 1;
        }

        private void sei(MemoryInfo mem)
        {
            interrupt_flag = 1;
        }

        private void sta(MemoryInfo mem)
        {
            RAM.WriteMemory(mem.Address, accumulator);
        }

        private void stx(MemoryInfo mem)
        {
            RAM.WriteMemory(mem.Address, reg_x);
        }

        private void sty(MemoryInfo mem)
        {
            RAM.WriteMemory(mem.Address, reg_y);
        }

        private void tax(MemoryInfo mem)
        {
            reg_x = accumulator;
            setZero(reg_x);
            setSign(reg_x);
        }

        private void tay(MemoryInfo mem)
        {
            reg_y = accumulator;
            setZero(reg_y);
            setSign(reg_y);
        }

        private void tsx(MemoryInfo mem)
        {
            reg_x = stack_pointer;
            setZero(reg_x);
            setSign(reg_x);
        }

        private void txa(MemoryInfo mem)
        {
            accumulator = reg_x;
            setZero(accumulator);
            setSign(accumulator);
        }

        //Transfer the value stored at X to the stack pointer
        private void txs(MemoryInfo mem)
        {
            stack_pointer = reg_x;
        }

        private void tya(MemoryInfo mem)
        {
            accumulator = reg_y;
            setZero(accumulator);
            setSign(accumulator);
        }

        //TODO: Illegal Opcodes below 

        public void PrintInstruction()
        {

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