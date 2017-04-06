using FileReader;
using System;

/// <summary>
/// Memory Prototype
/// Author: Harman
/// Version: 1.0
/// </summary>
namespace NES
{
    /// <summary>
    /// CPU Memory. Memory Map based on https://wiki.nesdev.com/w/index.php/CPU_memory_map
    /// </summary>
    internal class Memory
    {
        private byte[] memory;

        private Mapper mapper;

        /// <summary>
        /// Constructor for the memory object. Initializes memory array to
        /// the specified size
        /// </summary>
        /// <param name="size">Size to initialize memory array with</param>
        public Memory(int size, Mapper mapper)
        {
            memory = new byte[size];
            this.mapper = mapper;
            ClearMemory();
        }

        /// <summary>
        /// Returns value held at the position passed in by the parameter
        /// Check if memory location is valid
        /// </summary>
        /// <param name="location">Memory index to be returned</param>
        /// <returns>Value held at index location</returns>
        public byte ReadMemory(ushort address)
        {
            if(address < 0x2000)
            {
                return memory[address % 0x0800];
            } else if(address < 0x4000)
            {
                //return PPU memory
            } else if(address == 0x4014)
            {
                //return PPU register value
            } else if(address == 0x4015)
            {
                //apu memory value
            } else if(address == 0x4016)
            {
                //input 1
            } else if(address == 0x4017)
            {
                //input2
            } else if(address >= 0x6000)
            {
                mapper.read(address);
            } else
            {
                throw new Exception("Invalid memory access requested");
            }
            return 0;
        }


        /// <summary>
        /// Put value in memory in the specified location
        /// </summary>
        /// <param name="location">Index to be written at</param>
        /// <param name="value">Value to be written at given index</param>
        public void WriteMemory(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                memory[address % 0x0800] = value;
            }
            else if (address < 0x4000)
            {
                //set PPU memory
            }
            else if (address < 0x4014)
            {
                //set APU register value
            }
            else if (address == 0x4014)
            {
                //set PPU register value
            }
            else if (address == 0x4015)
            {
                //apu memory value
            }
            else if (address == 0x4016)
            {
                //input 1
            }            
            else if (address >= 0x6000)
            {
                mapper.write(address, value);
            }
            else
            {
                throw new Exception("Invalid memory access requested");
            }            
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
        /// Prints the current content of the memory. Only used for debugging purposes.
        /// </summary>
        public void printMemory()
        {
            for (int i = 0; i < memory.Length; i++)
                Console.WriteLine(memory[i].ToString("X"));
        }

        public override string ToString()
        {
            string rtn = "";
            for (int i = 0; i < memory.Length; i++)
            {
                if (i % 6 == 0)
                    rtn += "\n";
                rtn += memory[i].ToString("X");
            }
            return rtn;
        }
    }
}