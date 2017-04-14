using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NESEmu;

//TODO: Comments
namespace NESEmu
{
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

        //At the beginning, value of registers is unspecified, so we initialize offsets using the values below.
        /// <summary>
        /// 
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



        //TODO: Implement methods for the PPU operations. Scanline counting etc.
        /// <summary>
        /// 
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
        /// 
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
        
        //CHR banks are located between 0x0000 and 0x1fff inclusive, so we can delineate at 0x2000
        //PRG banks are located between 0x8000 and 0xFFFF inclusive, so we can start at 0x8000
        /// <summary>
        /// 
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
        /// 
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
        /// 
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


        private void writeBankSelect(byte value)
        {
            prgMode = (byte)((value >> 6) & 1);
            chrMode = (byte)((value >> 7) & 1);
            register = (byte)(value & 7);
            updateOffsets();
        }

        private void writeBankData(byte value)
        {
            registers[register] = value;
            updateOffsets();
        }

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

        //Since PRG banks start from 0x8000 and are 0x2000 long each
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int prgBankOffset(int index)
        {
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
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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