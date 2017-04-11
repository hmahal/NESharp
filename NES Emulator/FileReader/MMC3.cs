using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NESEmu;

namespace NESEmu
{
    public class MMC3 : Mapper
    {
        private int[] prgbank = new int[4];
        private int[] chrbank = new int[8];
        public new Cartridge cart { get; set; }       
        //Registers
        private byte register;
        private byte[] registers = new byte[8];
        private byte irqReload;
        private bool irqEnable;
        private int counter;
        private int prgMode;
        private int chrMode;

        //At the beginning, value of registers is unspecified, so we initialize offsets using the values below.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cart"></param>
        public MMC3(Cartridge cart) : base(cart)
        {
            this.cart = cart;            
            prgbank[0] = prgBankOffset(0);
            prgbank[1] = prgBankOffset(1);
            prgbank[2] = prgBankOffset(-2);
            prgbank[3] = prgBankOffset(-1);
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
            if (ppu.Scanlines > 239 && ppu.Scanlines < 261)
                return;
            if (ppu.ShowBackground == 0 && ppu.ShowSprite == 0)
                return;
            scanLine();
        }

        /// <summary>
        /// 
        /// </summary>
        private void scanLine()
        {
            if (counter == 0)
                counter = irqReload;
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
                int bank = addr / 0x0400;
                int offset = addr % 0x0400;
                return cart.Chrrom[chrbank[bank] + offset];
            }
            else if (addr >= 0x8000)
            {
                addr = (ushort)(addr - 0x8000);
                int bank = addr / 0x2000;
                int offset = addr % 0x2000;
                return cart.Prgrom[prgbank[bank] + offset];
            }
            return 0;
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
                cart.Chrrom[chrbank[bank] + offset] = value;
            }
            else if (addr >= 0x8000)
            {
                writeRegister(addr, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void writeBankSelect(byte value)
        {
            prgMode = (value >> 6) & 1;
            chrMode = (value >> 7) & 1;
            register = (byte)(value & 7);
            resetOffsets();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void writeBankData(byte value)
        {
            registers[register] = value;
            resetOffsets();
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        private void writeRegister(ushort addr, byte value)
        {
            if((addr <= 0x9FFF) && (addr % 2 == 0))
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
            else if((addr <= 0xDFFF) && (addr % 2 == 0))
            {
                irqReload = value;
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
        private void resetOffsets()
        {
            if(prgMode == 0)
            {
                prgbank[0] = prgBankOffset(registers[6]);
                prgbank[1] = prgBankOffset(registers[7]);
                prgbank[2] = prgBankOffset(-2);
                prgbank[3] = prgBankOffset(-1);
            }
            else if(prgMode == 1)
            {
                prgbank[0] = prgBankOffset(-2);
                prgbank[1] = prgBankOffset(registers[7]);
                prgbank[2] = prgBankOffset(registers[6]);
                prgbank[3] = prgBankOffset(-1);
            }
            if(chrMode == 0)
            {
                chrbank[0] = chrBankOffset(registers[0] & 0xFE);
                chrbank[1] = chrBankOffset(registers[0] | 0x01);
                chrbank[2] = chrBankOffset(registers[1] & 0xFE);
                chrbank[3] = chrBankOffset(registers[1] | 0x01);
                chrbank[4] = chrBankOffset(registers[2]);
                chrbank[5] = chrBankOffset(registers[3]);
                chrbank[6] = chrBankOffset(registers[4]);
                chrbank[7] = chrBankOffset(registers[5]);
            }
            else if (chrMode == 1)
            {                
                chrbank[0] = chrBankOffset(registers[2]);
                chrbank[1] = chrBankOffset(registers[3]);
                chrbank[2] = chrBankOffset(registers[4]);
                chrbank[3] = chrBankOffset(registers[5]);
                chrbank[4] = chrBankOffset(registers[0] & 0xFE);
                chrbank[5] = chrBankOffset(registers[0] | 0x01);
                chrbank[6] = chrBankOffset(registers[1] & 0xFE);
                chrbank[7] = chrBankOffset(registers[1] | 0x01);
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
            index %= cart.Prgrom.Length / 0x2000;            
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
            int temp = cart.Chrrom.Length / 0x0400;
            if(temp != 0)
                index %= temp;            
            offset = index * 0x0400;
            if (offset < 0)
                offset += cart.Chrrom.Length;
            return offset;
        }
    }
}