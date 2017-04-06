using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    public class PPU
    {
        //Utility Values
        public int Cycle { get; set; }

        public int Scanlines { get; set; }
        public int Frame { get; set; }

        private byte[] OAM = new byte[256];

        //Registers
        private byte ppuctrl_register;      //$2000

        private byte ppumask_register;      //$2001
        private byte ppustatus_register;    //$2002
        private byte oamdaddr_value;        //$2003 OAM
        private byte oamdata_value;         //$2004 OAM
        private ushort ppuscroll_register;  //$2005
        private ushort ppuaddr_register;    //$2006
        private byte ppudata_register;      //$2007
        private byte oamdma_value;          //$4014 OAM

        //Register Flags
        //PPUCTRL
        private byte nametableAddr;

        private byte addrIncrement;
        private byte sprTableAddr;
        private byte backPttrAddr;
        private byte spriteSize;
        private byte masterSlaveSelect;
        private byte generateNMI;

        //PPUMASK
        private byte grayScale;

        private byte showLeftBack;
        private byte showLeftSprite;
        private byte empRed;
        private byte empGreen;
        private byte empBlue;

        //Access needed by the mapper
        public byte ShowBackground { get; set; }

        public byte ShowSprite { get; set; }

        //PPU STATUS
        private byte spriteOverflow;

        private byte spriteZero;

        //Addresses
        private ushort vramAddress;

        private ushort tempAddress;
        private byte xScroll;
        private bool writeFlag;
        private bool frameToggle;

        private Memory RAM;
        private CPU6502 cpu_;

        //TODO: Implement this
        public PPU()
        {
            //cpu_ = cpu;
            //RAM = cpu_.RAM;
        }

        //TODO: Palette

        public void reset()
        {
            Cycle = 0;
            Scanlines = 0;
            Frame = 0;
            ppuctrl_register = 0;
            ppumask_register = 0;
            frameToggle = false;
        }

        //TODO:Fix this
        private byte readRegister(ushort addr)
        {
            switch (addr)
            {
                case (0x2002):
                    return 0;
            }
            return 0;
        }

        private void writeControl(byte value)
        {
            nametableAddr = (byte)((value >> 0) & 3); // Since nametable addr occupies two bits instead of 1
            addrIncrement = (byte)((value >> 2) & 1);
            sprTableAddr = (byte)((value >> 3) & 1);
            backPttrAddr = (byte)((value >> 4) & 1);
            spriteSize = (byte)((value >> 5) & 1);
            masterSlaveSelect = (byte)((value >> 6) & 1);
            generateNMI = (byte)((value >> 7) & 1);
        }

        private void writeMask(byte value)
        {
            grayScale = (byte)((value >> 0) & 1);
            showLeftBack = (byte)((value >> 1) & 1);
            showLeftSprite = (byte)((value >> 2) & 1);
            ShowBackground = (byte)((value >> 3) & 1);
            ShowSprite = (byte)((value >> 4) & 1);
            empRed = (byte)((value >> 5) & 1);
            empGreen = (byte)((value >> 6) & 1);
            empBlue = (byte)((value >> 7) & 1);
        }

        //TODO: Fix this
        private byte readStatus()
        {
            byte result = 0;

            return result;
        }

        private void writeOAMaddr(byte value)
        {
            oamdaddr_value = value;
        }

        private byte readOAMdata()
        {
            return OAM[oamdaddr_value];
        }

        private void writeOAMdata(byte value)
        {
            OAM[oamdaddr_value] = value;
            oamdaddr_value++;
        }

        private void writePPUData(byte value)
        {
            RAM.WriteMemory(vramAddress, value);

            if (addrIncrement == 0)
                vramAddress++;
            else
                vramAddress += 32;
        }

        private byte readPPUData()
        {
            byte value = RAM.ReadMemory(vramAddress);

            //TODO: Add buffered reading

            if (addrIncrement == 0)
                vramAddress++;
            else
                vramAddress += 32;

            return value;
        }

        private void writePPUScroll(byte value)
        {
            if (writeFlag)
            {
                tempAddress = (ushort)((tempAddress & 0x8FFF) | (((ushort)(value) & 0x07) << 12));
                tempAddress = (ushort)((tempAddress & 0xFC1F) | (((ushort)(value) & 0xF8) << 2));
                writeFlag = false;
            }
            else
            {
                tempAddress = (ushort)((tempAddress & 0xFFE0) | ((ushort)(value) >> 3));
                tempAddress = (ushort)((ushort)(value) & 0x07);
                writeFlag = true;
            }
        }

        private void writePPUAddr(byte value)
        {
            if (writeFlag)
            {
                tempAddress = (ushort)((tempAddress & 0xFF00) | (ushort)(value));
                vramAddress = tempAddress;
                writeFlag = false;
            }
            else
            {
                tempAddress = (ushort)((tempAddress * 0x80FF) | (((ushort)(value) & 0x3F) << 8));
                writeFlag = true;
            }
        }

        //TODO: This
        private void writeOAMDma(byte value)
        {
        }

        private void renderPixel()
        {
        }

        public void Run()
        {
        }

        public void tick()
        {
        }
    }
}