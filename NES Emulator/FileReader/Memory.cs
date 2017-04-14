using System;

/// <summary>
/// Memory Prototype
/// Author: Harman
/// Version: 1.0
/// </summary>
namespace NESEmu
{
    /// <summary>
    /// CPU Memory. Memory Map based on https://wiki.nesdev.com/w/index.php/CPU_memory_map
    /// </summary>
    public class Memory
    {
        private byte[] memory;

        private Mapper mapper;
        private Input input1, input2;
        private static Memory instance;

        /// <summary>
        /// 
        /// </summary>
        private ushort[][] Mirror = new ushort[5][]
        {
            new ushort[] { 0,0,1,1 },
            new ushort[] { 0,1,0,1 },
            new ushort[] { 0,0,0,0 },
            new ushort[] { 1,1,1,1 },
            new ushort[] { 0,1,2,3 }
        };

        /// <summary>
        /// Constructor for the memory object. Initializes memory array to
        /// the specified size
        /// </summary>
        /// <param name="size">Size to initialize memory array with</param>
        private Memory(int size, Mapper mapper, Input input1, Input input2)
        {
            memory = new byte[size];
            this.mapper = mapper;
            this.input1 = input1;
            this.input2 = input2;       
            ClearMemory();
        }

        /// <summary>
        /// 
        /// </summary>
        public static Memory Instance
        {
            get
            {
                if(instance == null)
                {
                    throw new Exception("Memory doesn't exist");
                }
                return instance;
            }
        }

        /// <summary>
        /// Creating an instance of memory.  
        /// </summary>
        /// <param name="size"></param>
        /// <param name="mapper"></param>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        public static void Create(int size, Mapper mapper, Input input1, Input input2)
        {
            if(instance != null)
            {
                throw new Exception("Memory already exists");
            }
            instance = new Memory(size, mapper, input1, input2);
        }

        /// <summary>
        /// Returns value held at the position passed in by the parameter
        /// Check if memory location is valid
        /// </summary>
        /// <param name="location">Memory index to be returned</param>
        /// <returns>Value held at index location</returns>
        public byte ReadMemory(ushort address)
        {
            if (address < 0x2000)
            {
                return memory[address % 0x0800];
            }
            else if (address < 0x4000)
            {
                PPU ppu = PPU.Instance;
                return ppu.readRegister((ushort)(0x2000 + address%8));        
            }
            else if (address == 0x4014)
            {
                PPU ppu = PPU.Instance;
                return ppu.readRegister(address);
            }
            else if (address == 0x4015)
            {                
                throw new NotImplementedException();
            }
            else if (address == 0x4016)
            {
                return input1.Read();
            }
            else if (address == 0x4017)
            {
                return input2.Read();
            }
            else if (address >= 0x6000)
            {
                return mapper.read(address);
            }
            else
            {
                throw new Exception("Invalid memory access requested");
            }            
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
                PPU ppu = PPU.Instance;
                ppu.writeRegister((ushort)(0x2000 + address % 8), value);
            }
            else if (address < 0x4014)
            {
                //set APU register value
                //throw new NotImplementedException();
            }
            else if (address == 0x4014)
            {
                PPU ppu = PPU.Instance;
                ppu.writeRegister(address, value);
            }
            else if (address == 0x4015)
            {
                //throw new NotImplementedException();
            }
            else if (address == 0x4016)
            {
                input1.Write(value);
                input2.Write(value);
            }
            else if (address == 0x4017)
            {
                //throw new NotImplementedException();
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
        /// PPU reading from an address specified.  Method throws an exception
        /// if address specifies invalid memory access.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public byte PpuRead(ushort addr)
        {
            addr = (ushort)(addr % 0x4000);
            if(addr < 0x2000)
            {
                return mapper.read(addr);
            }
            else if(addr < 0x3F00)
            {
                PPU ppu = PPU.Instance;
                byte mode = mapper.cart.Mirroring;
                return ppu.nameTableData[MirroredAddress(mode, addr) % 2048];
            }
            else if(addr < 0x4000)
            {
                PPU ppu = PPU.Instance;
                return ppu.readPalette((ushort)(addr % 32));
            }
            else
            {
                throw new Exception("Invalid memory access requested");
            }            
        }

        /// <summary>
        /// PPU writes to an address location.  Method throws exception if
        /// PPU is given an address which leads to invalid memory access.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void PpuWrite(ushort addr, byte value)
        {
            addr = (ushort)(addr % 0x4000);
            if (addr < 0x2000)
            {
                mapper.write(addr, value);
            }
            else if (addr < 0x3F00)
            {
                PPU ppu = PPU.Instance;
                byte mode = mapper.cart.Mirroring;
                ppu.nameTableData[MirroredAddress(mode, addr) % 2048] = value;
            }
            else if (addr < 0x4000)
            {               
                PPU ppu = PPU.Instance;
                ppu.writePalette((ushort)(addr % 32), value);
            }
            else
            {
                throw new Exception("Invalid memory access requested");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public ushort MirroredAddress(byte mode, ushort address)
        {
            address = (ushort)((address - 0x2000) % 0x1000);
            ushort table = (ushort)(address / 0x0400);
            ushort offset = (ushort)(address % 0x0400);
            ushort result = (ushort)(0x2000 + Mirror[mode][table] * 0x0400 + offset);
            return result;
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

        /// <summary>
        /// Returns everything in memory to string.  
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string rtn = "";
            for (int i = 0; i < memory.Length; i++)
            {
                rtn += memory[i].ToString("X2");
                rtn += " ";
            }
            return rtn;
        }
    }
}