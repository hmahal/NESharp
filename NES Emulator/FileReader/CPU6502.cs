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
        private byte carry_flag;

        private byte zero_flag;
        private byte interrupt_flag;
        private byte decimal_flag;
        private byte break_flag;
        private byte sign_flag;
        private byte overflow_flag;

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