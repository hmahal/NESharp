using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NESEmu
{
    class PPU
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

        private byte register;
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

        private bool nmiOccured;
        private bool nmiOutput;
        private bool nmiPrevious;
        private byte nmiDelay;

        //PPU STATUS
        private byte spriteOverflow;
        private byte spriteZero;

        //Addresses
        private ushort vramAddress;
        private ushort tempAddress;
        private byte xScroll;
        private bool writeFlag;
        private bool frameToggle;

        private byte nameTable;
        private byte attrTable;
        private byte lowTile;
        private byte highTile;
        private ulong tileData;

        private Memory RAM;
        private CPU6502 cpu_;
        private Palette palette;
        private byte bufferedData;

        private int spriteCount;
        private uint[] spritePatterns = new uint[8];
        private byte[] spritePositions = new byte[8];
        private byte[] spritePriority = new byte[8];
        private byte[] spriteIndex = new byte[8];

        Bitmap front = new Bitmap(256, 240);
        Bitmap back = new Bitmap(256,240);

        //TODO: Implement this
        public PPU()
        {
            //cpu_ = cpu;
            //RAM = cpu_.RAM;
            palette = new Palette();
        }        

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
            nametableAddr       = (byte)((value >> 0) & 3); // Since nametable addr occupies two bits instead of 1
            addrIncrement       = (byte)((value >> 2) & 1);
            sprTableAddr        = (byte)((value >> 3) & 1);
            backPttrAddr        = (byte)((value >> 4) & 1);
            spriteSize          = (byte)((value >> 5) & 1);
            masterSlaveSelect   = (byte)((value >> 6) & 1);
            generateNMI         = (byte)((value >> 7) & 1);
        }

        private void writeMask(byte value)
        {
            grayScale       = (byte)((value >> 0) & 1);
            showLeftBack    = (byte)((value >> 1) & 1);
            showLeftSprite  = (byte)((value >> 2) & 1);
            ShowBackground  = (byte)((value >> 3) & 1);
            ShowSprite      = (byte)((value >> 4) & 1);
            empRed          = (byte)((value >> 5) & 1);
            empGreen        = (byte)((value >> 6) & 1);
            empBlue         = (byte)((value >> 7) & 1);
        }

        //TODO: Fix this
        private byte readStatus()
        {
            byte result = (byte)(register & 0x1F);
            result |= (byte)(spriteOverflow << 5);
            result |= (byte)(spriteZero << 6);
            if (nmiOccured)
                result |= 1 << 7;
            nmiOccured = false;
            nmiChange();
            writeFlag = false;            
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

            if(vramAddress % 0x4000 < 0x3F00)
            {
                byte buffer = bufferedData;
                bufferedData = value;
                value = buffer;
            } else
            {
                bufferedData = RAM.ReadMemory((ushort)(vramAddress - 0x1000));
            }

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
            } else
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
        
        private void incrementX()
        {
            if ((vramAddress & 0x001F) == 31)
            {
                vramAddress &= 0xFFE0;
                vramAddress ^= 0x0400;
            }
            else
                vramAddress++;
        }

        private void incrementY()
        {
            if ((vramAddress & 0x7000) != 0x7000)
                vramAddress = 0x1000;
            else
            {
                vramAddress &= 0x8FFF;
                byte y = (byte)((vramAddress & 0x03E0) >> 5);
                if (y == 29)
                {
                    y = 0;
                    vramAddress ^= 0x800;
                }
                else if (y == 31)
                    y = 0;
                else
                    y++;
                vramAddress = (ushort)((vramAddress & 0xFC1F) | (y << 5));
            }
        }

        private void copyX()
        {
            vramAddress = (ushort)((vramAddress & 0xFBE0) | (tempAddress & 0x041F));
        }

        private void copyY()
        {
            vramAddress = (ushort)((vramAddress & 0x841F) | (tempAddress & 0x7BE0));
        }

        private void nmiChange()
        {
            if ((nmiOutput && nmiOccured) && !nmiPrevious)
                nmiDelay = 15;
            nmiPrevious = (nmiOutput && nmiOccured);
        }

        private void setVerticalBlank()
        {
            Bitmap tmp = front;
            front = back;
            back = tmp;
            nmiOccured = true;
            nmiChange();
        }

        private void clearVerticalBlank()
        {
            nmiOccured = false;
            nmiChange();
        }

        private void getNameTableValue()
        {
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x2000 | (tmp & 0x0FFF));
            nameTable = RAM.ReadMemory(addr);
        }

        private void getAttrTable()
        {
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x23C0 | (tmp & 0x0C00)
                | ((tmp >> 4) & 0x38) | ((vramAddress >> 2) & 0x07));
            ushort shift = (ushort)(((tmp >> 4) & 4) | (tmp & 2));
            attrTable = (byte)(((RAM.ReadMemory(addr) >> shift) & 3) << 2);
        }

        private void getTileLowByte()
        {
            byte y = (byte)((vramAddress >> 12) & 7);
            byte table = backPttrAddr;
            byte tile = nameTable;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTile = RAM.ReadMemory(addr);
        }

        private void getTileHighByte()
        {
            byte y = (byte)((vramAddress >> 12) & 7);
            byte table = backPttrAddr;
            byte tile = nameTable;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTile = RAM.ReadMemory((ushort)(addr + 8));
        }

        private void setTile()
        {
            uint data = 0;
            for (int i = 0; i < 8; i++)
            {
                byte attr = attrTable;
                byte low = (byte)((lowTile & 0x80) >> 7);
                byte high = (byte)((highTile & 0x80) >> 6);
                lowTile <<= 1;
                highTile <<= 1;
                data <<= 4;
                data |= (uint)(attr | low | high);
            }
            tileData |= data;
        }

        private uint getTile()
        {
            return (uint)(tileData >> 32);
        }

        private byte background()
        {
            if (ShowBackground == 0)
                return 0;
            uint data = getTile() >> ((7 - xScroll) * 4);
            return (byte)(data & 0x0F);

        }

        private Tuple<byte, byte> sprite()
        {
            if(ShowSprite == 0)
                return Tuple.Create<byte, byte>(0, 0);
            for(int i =0; i < spriteCount; i++)
            {
                int offset = (Cycle - 1) - spritePositions[i];
                if (offset < 0 || offset > 7)
                    continue;                
                offset = 7 - offset;
                byte colour = (byte)(spritePatterns[i] >> (byte)(offset * 4) & 0x0F);
                if (colour % 4 == 0)
                    continue;
                return Tuple.Create<byte, byte>((byte)i, colour);
                
            }
            return Tuple.Create<byte, byte>(0, 0);
        }       


        private void renderPixel()
        {
            int x_coord = Cycle - 1;
            int y_coord = Scanlines;
            byte backPixel = background();
            Tuple<byte, byte> spritePixel = sprite();
            byte sprite_ = spritePixel.Item2;
            byte spritePix = spritePixel.Item1;

            if (x_coord < 8 && showLeftBack == 0)
                backPixel = 0;
            if (x_coord < 8 && ShowSprite == 0)
                sprite_ = 0;
            bool b = backPixel % 4 != 0;
            bool s = sprite_ % 4 != 0;
            byte colour;
            if(!b && !s)
            {
                colour = 0;
            }
            else if(!b && s)
            {
                colour = (byte)(sprite_ | 0x10);
            }
            else if(b && !s)
            {
                colour = backPixel;
            }
            else
            {
                if (spriteIndex[spritePix] == 0 && x_coord < 255)
                    spriteZero = 1;
                if (spritePriority[spritePix] == 0)
                    colour = (byte)(sprite_ | 0x10);
                else
                    colour = backPixel;
            }

            Color col = palette.ColorPalette[((ushort)(colour))%64];
            back.SetPixel(x_coord, y_coord, col);
        }

        //private uint getSpritePattern()
        //{

        //}

        private void checkSprites()
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
