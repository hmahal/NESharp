using System;

/// <summary>
/// Memory Prototype
/// Author: Harman
/// Version: 1.0
/// </summary>
namespace _6502Proto
{
    /// <summary>
    /// Memory object prototype
    /// </summary>
    internal class Memory
    {
        private byte[] memory;

        /// <summary>
        /// Constructor for the memory object. Initializes memory array to
        /// the specified size
        /// </summary>
        /// <param name="size">Size to initialize memory array with</param>
        public Memory(int size)
        {
            memory = new byte[size];
            ClearMemory();
        }

        /// <summary>
        /// Put value in memory in the specified location
        /// </summary>
        /// <param name="location">Index to be written at</param>
        /// <param name="value">Value to be written at given index</param>
        public void WriteMemory(int location, byte value)
        {
            memory[location] = value;
        }

        /// <summary>
        /// Stores the program passed in as array into the memory
        /// </summary>
        /// <param name="program">Instructions passed in as an array</param>
        public void LoadProgramIntoMemory(byte[] program)
        {
            for (int i = 0; i < program.Length; i++)
            {
                memory[i] = program[i];
            }
        }

        /// <summary>
        /// Returns value held at the position passed in by the parameter
        /// </summary>
        /// <param name="location">Memory index to be returned</param>
        /// <returns>Value held at index location</returns>
        public byte ReadMemory(int location)
        {
            return memory[location];
        }

        /// <summary>
        /// Set all values in memory to 0x00
        /// </summary>
        public void ClearMemory()
        {
            for (int i = 0; i < memory.Length; i++)
                memory[i] = 0x00;
        }

        /// <summary>
        /// Prints the current content of the memory
        /// </summary>
        public void printMemory()
        {
            for (int i = 0; i < memory.Length; i++)
                Console.WriteLine(memory[i].ToString("X"));
        }
    }
}