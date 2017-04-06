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
        private Cartridge cart;

        //At the beginning, value of registers is unspecified, so we initialize offsets using the values below.
        public MMC3(Cartridge cart) : base(cart)
        {
            this.cart = cart;
            prgbank[0] = prgBankOffset(0);
            prgbank[1] = prgBankOffset(1);
            prgbank[2] = prgBankOffset(-2);
            prgbank[3] = prgBankOffset(-1);
        }

        //TODO: Implement methods for the PPU operations. Scanline counting etc.
        //TODO: Implement methods to deal with IRQ and mirroring

        //CHR banks are located between 0x0000 and 0x1fff inclusive, so we can delineate at 0x2000
        //PRG banks are located between 0x8000 and 0xFFFF inclusive, so we can start at 0x8000
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
                int tmp = addr - 0x8000;
                int bank = tmp / 0x2000;
                int offset = tmp % 0x2000;
                return cart.Prgrom[prgbank[bank] + offset];
            }
            return 0;
        }

        public override void write(ushort addr, byte value)
        {
            if (addr < 0x2000)
            {
                //Does nothing yet
            }
            else if (addr >= 0x8000)
            {
                writeRegister(addr, value);
            }
        }

        private void writeRegister(ushort addr, byte value)
        {
        }

        //Since PRG banks start from 0x8000 and are 0x2000 long each
        private int prgBankOffset(int index)
        {
            int offset;
            if (index >= 0x80)
                index -= 0x100;
            int tmp = cart.Prgrom.Length / 0x2000;
            index = index % tmp;
            offset = index * 0x2000;
            if (offset < 0)
                offset += cart.Prgrom.Length;
            return offset;
        }
    }
}