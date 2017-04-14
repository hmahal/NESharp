using System;

namespace NESEmu
{
    /// <summary>
    /// This class emulates the MMC3 memory controller found in various game cartridges.
    /// https://wiki.nesdev.com/w/index.php/MMC3
    /// </summary>
    public class MMC3 : Mapper
    {
        private int[] prgOffsets = new int[4];
        private int[] chrOffsets = new int[8];
        private byte[] registers = new byte[8];

        //Registers
        private byte register;        
        private byte reload;
        private bool irqEnable;
        private byte counter;
        private byte prgMode;
        private byte chrMode;

        /// <summary>
        /// Value of registers is unspecified, so we initialize offsets using the values below.
        /// </summary>
        /// <param name="cart"></param>
        public MMC3(Cartridge cart) : base(cart)
        {
            this.cart = cart;            
            prgOffsets[0] = prgBankOffset(0);
            prgOffsets[1] = prgBankOffset(1);
            prgOffsets[2] = prgBankOffset(-2);
            prgOffsets[3] = prgBankOffset(-1);
        }
        
        /// <summary>
        /// Checks the PPU cycle and state and calls the scanLine() method when appropriate.
        /// </summary>
        public override void Tick()
        {
            PPU ppu = PPU.Instance;
            if (ppu.Cycle != 280)
                return;
            if (ppu.Scanline > 239 && ppu.Scanline < 261)
                return;
            if (ppu.flagShowbackground == 0 && ppu.flagShowSprite == 0)
                return;
            scanLine();
        }

        /// <summary>
        /// Provides the emulation for the Counter operation of the MMC3 mapper.
        /// https://wiki.nesdev.com/w/index.php/MMC3#IRQ_Specifics
        /// </summary>
        private void scanLine()
        {
            if (counter == 0)
                counter = reload;
            else
            {
                counter--;
                if(counter == 0 && irqEnable)
                {
                    CPU6502.Instance.triggerIRQ();
                }
            }
        }

        /// <summary>
        /// Returns memory stored at the address provided.
        /// CHR banks are located between 0x0000 and 0x1fff inclusive, so we can delineate at 0x2000.
        /// PRG banks are located between 0x8000 and 0xFFFF inclusive, so we can start at 0x8000.
        /// Save RAM is located between 0x6000 and 0x7FFF inclusive.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public override byte read(ushort addr)
        {
            if (addr < 0x2000)
            {
                ushort bank = (ushort)(addr / 0x0400);
                ushort offset = (ushort)(addr % 0x0400);
                return cart.Chrrom[chrOffsets[bank] + offset];
            }
            else if (addr >= 0x8000)
            {
                addr = (ushort)(addr - 0x8000);
                int bank = addr / 0x2000;
                int offset = addr % 0x2000;
                return cart.Prgrom[prgOffsets[bank] + offset];
            }
            else if (addr >= 0x6000)
            {
                return cart.Sram[addr - 0x6000];
            }
            else
            {
                throw new Exception("Memory out of bounds");
            }            
        }

        /// <summary>
        /// Writes the value passed in as a parameter to the memory location specified by the 
        /// address passed as the parameter.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public override void write(ushort addr, byte value)
        {
            if (addr < 0x2000)
            {
                int bank = addr / 0x0400;
                int offset = addr % 0x0400;
                cart.Chrrom[chrOffsets[bank] + offset] = value;
            }
            else if (addr >= 0x8000)
            {
                writeRegister(addr, value);
            }
            else if (addr >= 0x6000)
            {
                cart.Sram[addr - 0x6000] = value;
            }
            else
            {
                throw new Exception("Memory out of bounds");
            }
        }

        /// <summary>
        /// Checks the address passed in as a parameter and writes to the appropriate register.
        /// The 4 pairs of registers are based on the information provided here:
        /// https://wiki.nesdev.com/w/index.php/MMC3#Registers
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        private void writeRegister(ushort addr, byte value)
        {
            if ((addr <= 0x9FFF) && (addr % 2 == 0))
            {
                writeBankSelect(value);
            }
            else if ((addr <= 0x9FFF) && (addr % 2 == 1))
            {
                writeBankData(value);
            }
            else if ((addr <= 0xBFFF) && (addr % 2 == 0))
            {
                writeMirror(value);
            }
            else if ((addr <= 0xBFFF) && (addr % 2 == 1))
            {
                //SRAM not implemented
            }
            else if ((addr <= 0xDFFF) && (addr % 2 == 0))
            {
                reload = value;
            }
            else if ((addr <= 0xDFFF) && (addr % 2 == 1))
            {
                counter = 0;
            }
            else if ((addr <= 0xFFFF) && (addr % 2 == 0))
            {
                irqEnable = false;
            }
            else if ((addr <= 0xFFFF) && (addr % 2 == 1))
            {
                irqEnable = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void writeBankSelect(byte value)
        {
            prgMode = (byte)((value >> 6) & 1);
            chrMode = (byte)((value >> 7) & 1);
            register = (byte)(value & 7);
            updateOffsets();
        }

        /// <summary>
        /// Writes the value passed by parameter to the location specified by the register member.
        /// Calls updateOffsets to update the offsets.
        /// </summary>
        /// <param name="value"></param>
        private void writeBankData(byte value)
        {
            registers[register] = value;
            updateOffsets();
        }

        /// <summary>
        /// Sets the cart's mirroring based on the value of the parameter passed to the method.
        /// </summary>
        /// <param name="value"></param>
        private void writeMirror(byte value)
        {
            value = (byte)(value & 1);
            switch (value)
            {
                case 0:
                    cart.Mirroring = 1;
                    break;
                case 1:
                    cart.Mirroring = 0;
                    break;
            }
        }


        /// <summary>
        /// Returns the offset calculated based on the index provided.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The offset required to access the memory bank for PRG ROM calculated based on the index.
        /// </returns>
        private int prgBankOffset(int index)
        {
            //Since PRG banks start from 0x8000 and are 0x2000 long each
            int offset;
            if (index >= 0x80)
                index -= 0x100;
            index %= (cart.Prgrom.Length / 0x2000);
            offset = index * 0x2000;
            if (offset < 0)
                offset += cart.Prgrom.Length;
            return offset;
        }

        /// <summary>
        /// Returns the offset calculated based on the index provided.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The offset required to access the memory bank for CHR ROM calculated based on the index.
        /// </returns>
        private int chrBankOffset(int index)
        {
            int offset;
            if (index >= 0x80)
                index -= 0x100;
            index %= (cart.Chrrom.Length / 0x0400);
            offset = index * 0x0400;
            if (offset < 0)
                offset += cart.Chrrom.Length;
            return offset;
        }

        /// <summary>
        /// Calculates the offsets required to reach memory bank for both CHR and PRG roms.
        /// </summary>
        private void updateOffsets()
        {
            if(prgMode == 0)
            {
                prgOffsets[0] = prgBankOffset(registers[6]);
                prgOffsets[1] = prgBankOffset(registers[7]);
                prgOffsets[2] = prgBankOffset(-2);
                prgOffsets[3] = prgBankOffset(-1);
            }
            else if(prgMode == 1)
            {
                prgOffsets[0] = prgBankOffset(-2);
                prgOffsets[1] = prgBankOffset(registers[7]);
                prgOffsets[2] = prgBankOffset(registers[6]);
                prgOffsets[3] = prgBankOffset(-1);
            }
            if(chrMode == 0)
            {
                chrOffsets[0] = chrBankOffset(registers[0] & 0xFE);
                chrOffsets[1] = chrBankOffset(registers[0] | 0x01);
                chrOffsets[2] = chrBankOffset(registers[1] & 0xFE);
                chrOffsets[3] = chrBankOffset(registers[1] | 0x01);
                chrOffsets[4] = chrBankOffset(registers[2]);
                chrOffsets[5] = chrBankOffset(registers[3]);
                chrOffsets[6] = chrBankOffset(registers[4]);
                chrOffsets[7] = chrBankOffset(registers[5]);
            }
            else if (chrMode == 1)
            {
                chrOffsets[0] = chrBankOffset(registers[2]);
                chrOffsets[1] = chrBankOffset(registers[3]);
                chrOffsets[2] = chrBankOffset(registers[4]);
                chrOffsets[3] = chrBankOffset(registers[5]);
                chrOffsets[4] = chrBankOffset(registers[0] & 0xFE);
                chrOffsets[5] = chrBankOffset(registers[0] | 0x01);
                chrOffsets[6] = chrBankOffset(registers[1] & 0xFE);
                chrOffsets[7] = chrBankOffset(registers[1] | 0x01);
            }
        }       
    }
}