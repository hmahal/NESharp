using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NESEmu
{
    public class PPU
    {
        //Utility Values
        public int Cycle { get; set; }

        public int Scanlines { get; set; }
        public int Frame { get; set; }

        private byte[] OAM = new byte[256];

        public byte[] nameTableData = new byte[2048];

        private byte[] paletteData = new byte[32];

        private byte oamdaddr_value;        //$2003 OAM

        private byte register;

        //Register Flags
        //PPUCTRL
        private byte nametableAddr;

        private byte addrIncrement;
        private byte sprTableAddr;
        private byte backPttrAddr;
        private byte spriteSize;
        private byte masterSlaveSelect;

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
        private byte writeFlag;
        private byte frameToggle;

        private byte nameTable;
        private byte attrTable;
        private byte lowTile;
        private byte highTile;
        private ulong tileData;

        private Palette palette;
        private byte bufferedData;

        private int spriteCount;
        private uint[] spritePatterns = new uint[8];
        private byte[] spritePositions = new byte[8];
        private byte[] spritePriority = new byte[8];
        private byte[] spriteIndex = new byte[8];

        private Bitmap back;
        private Bitmap front;

        private static PPU ppu;

        private PPU()
        {
            palette = new Palette();
            front = new Bitmap(256, 240);
            back = new Bitmap(256, 240);
            reset();
        }

        public static PPU Instance
        {
            get
            {
                if (ppu == null)
                {
                    ppu = new PPU();
                }
                return ppu;
            }
        }

        public Bitmap getFrame()
        {
            return front;
        }

        public void reset()
        {
            Cycle = 340;
            Scanlines = 240;
            Frame = 0;
            writeControl(0);
            writeMask(0);
            writeOAMaddr(0);
        }

        public byte readPalette(ushort addr)
        {
            if (addr >= 16 && addr % 4 == 0)
                addr = (ushort)(addr - 16);
            return paletteData[addr];
        }

        public void writePalette(ushort addr, byte value)
        {
            if (addr >= 16 && addr % 4 == 0)
                addr = (ushort)(addr - 16);
            paletteData[addr] = value;
        }

        public byte readRegister(ushort addr)
        {
            switch (addr)
            {
                case (0x2002):
                    return readStatus();

                case (0x2004):
                    return readOAMdata();

                case (0x2007):
                    return readPPUData();
            }
            return 0;
        }

        public void writeRegister(ushort addr, byte value)
        {
            register = value;
            switch (addr)
            {
                case (0x2000):
                    writeControl(value);
                    break;

                case (0x2001):
                    writeMask(value);
                    break;

                case (0x2003):
                    writeOAMaddr(value);
                    break;

                case (0x2004):
                    writeOAMdata(value);
                    break;

                case (0x2005):
                    writePPUScroll(value);
                    break;

                case (0x2006):
                    writePPUAddr(value);
                    break;

                case (0x2007):
                    writePPUData(value);
                    break;

                case (0x4014):
                    writeOAMDma(value);
                    break;
            }
        }

        private void writeControl(byte value)
        {
            nametableAddr = (byte)((value >> 0) & 3); // Since nametable addr occupies two bits instead of 1
            addrIncrement = (byte)((value >> 2) & 1);
            sprTableAddr = (byte)((value >> 3) & 1);
            backPttrAddr = (byte)((value >> 4) & 1);
            spriteSize = (byte)((value >> 5) & 1);
            masterSlaveSelect = (byte)((value >> 6) & 1);
            nmiOutput = (byte)((value >> 7) & 1) == 1;
            nmiChange();
            tempAddress = (ushort)((tempAddress & 0xF3FF) | ((value & 0x03) << 10));
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

        private byte readStatus()
        {
            byte result = (byte)(register & 0x1F);
            result |= (byte)(spriteOverflow << 5);
            result |= (byte)(spriteZero << 6);
            if (nmiOccured)
                result |= 1 << 7;
            nmiOccured = false;
            nmiChange();
            writeFlag = 0;
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
            Memory RAM = Memory.Instance;
            RAM.PpuWrite(vramAddress, value);

            if (addrIncrement == 0)
                vramAddress = (ushort)(vramAddress + 1);
            else
                vramAddress = (ushort)(vramAddress + 32);
        }

        private byte readPPUData()
        {
            Memory RAM = Memory.Instance;
            byte value = RAM.PpuRead(vramAddress);

            if ((vramAddress % 0x4000) < 0x3F00)
            {
                byte buffer = bufferedData;
                bufferedData = value;
                value = buffer;
            }
            else
            {
                bufferedData = RAM.PpuRead((ushort)(vramAddress - 0x1000));
            }

            if (addrIncrement == 0)
                vramAddress = (ushort)(vramAddress + 1);
            else
                vramAddress = (ushort)(vramAddress + 32);

            return value;
        }

        private void writePPUScroll(byte value)
        {
            if (writeFlag == 0)
            {
                tempAddress = (ushort)((tempAddress & 0xFFE0) | value >> 3);
                xScroll = (byte)(value & 0x07);
                writeFlag = 1;
            }
            else
            {
                tempAddress = (ushort)((tempAddress & 0x8FFF) | (value & 0x07) << 12);
                tempAddress = (ushort)((tempAddress & 0xFC1F) | (value & 0xF8) << 2);
                writeFlag = 0;
            }
        }

        private void writePPUAddr(byte value)
        {
            if (writeFlag == 0)
            {
                tempAddress = (ushort)((tempAddress & 0x80FF) | (value & 0x3F) << 8);
                writeFlag = 1;
            }
            else
            {
                tempAddress = (ushort)((tempAddress & 0xFF00) | value);
                vramAddress = tempAddress;
                writeFlag = 0;
            }
        }

        private void writeOAMDma(byte value)
        {
            CPU6502 cpu = CPU6502.Instance;
            ushort addr = (ushort)(value << 8);
            for (int i = 0; i < 256; i++)
            {
                OAM[oamdaddr_value] = cpu.RAM.ReadMemory(addr);
                oamdaddr_value++;
                addr++;
            }
            cpu.Stall += 513;
            if (cpu.Cycle % 2 == 1)
                cpu.Stall++;
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
                vramAddress = (ushort)(vramAddress + 0x1000);
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
            Bitmap tmp = new Bitmap(front);
            front = new Bitmap(back);
            back = new Bitmap(front);
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
            Memory RAM = Memory.Instance;
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x2000 | (tmp & 0x0FFF));
            nameTable = RAM.PpuRead(addr);
        }

        private void getAttrTable()
        {
            Memory RAM = Memory.Instance;
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x23C0 | (tmp & 0x0C00)
                | ((tmp >> 4) & 0x38) | ((tmp >> 2) & 0x07));
            ushort shift = (ushort)(((tmp >> 4) & 4) | (tmp & 2));
            attrTable = (byte)(((RAM.PpuRead(addr) >> shift) & 3) << 2);
        }

        private void getTileLowByte()
        {
            Memory RAM = Memory.Instance;
            byte y = (byte)((vramAddress >> 12) & 7);
            byte table = backPttrAddr;
            byte tile = nameTable;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTile = RAM.PpuRead(addr);
        }

        private void getTileHighByte()
        {
            Memory RAM = Memory.Instance;
            byte y = (byte)((vramAddress >> 12) & 7);
            byte table = backPttrAddr;
            byte tile = nameTable;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTile = RAM.PpuRead((ushort)(addr + 8));
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
            if (ShowSprite == 0)
                return Tuple.Create<byte, byte>(0, 0);
            for (int i = 0; i < spriteCount; i++)
            {
                int offset = (Cycle - 1) - spritePositions[i];
                if (offset < 0 || offset > 7)
                    continue;
                offset = 7 - offset;
                byte colour = (byte)(spritePatterns[i] >> (byte)(offset * 4) & 0x0F);
                if (colour % 4 == 0)
                    continue;
                return Tuple.Create((byte)i, colour);
            }
            return Tuple.Create<byte, byte>(0, 0);
        }

        private void renderPixel()
        {
            int x_coord = Cycle - 1;
            int y_coord = Scanlines;
            byte backPixel = background();
            Tuple<byte, byte> spritePixel = sprite();
            byte spriteColour = spritePixel.Item2;
            byte spritePix = spritePixel.Item1;

            if (x_coord < 8 && showLeftBack == 0)
                backPixel = 0;
            if (x_coord < 8 && ShowSprite == 0)
                spriteColour = 0;
            bool b = backPixel % 4 != 0;
            bool s = spriteColour % 4 != 0;
            byte colour;
            if (!b && !s)
            {
                colour = 0;
            }
            else if (!b && s)
            {
                colour = (byte)(spriteColour | 0x10);
            }
            else if (b && !s)
            {
                colour = backPixel;
            }
            else
            {
                if (spriteIndex[spritePix] == 0 && x_coord < 255)
                    spriteZero = 1;
                if (spritePriority[spritePix] == 0)
                    colour = (byte)(spriteColour | 0x10);
                else
                    colour = backPixel;
            }
            Color col = palette.ColorPalette[readPalette(colour) % 64];
            back.SetPixel(x_coord, y_coord, col);
        }

        private uint getSpritePattern(int index, int row)
        {
            Memory RAM = Memory.Instance;
            byte tile = OAM[index * 4 + 1];
            byte attr = OAM[index * 4 + 2];
            ushort addr;
            if (spriteSize == 0)
            {
                if ((byte)(attr & 0x80) == 0x80)
                    row = 7 - row;
                byte table = sprTableAddr;
                addr = (ushort)(0x1000 * table + tile * 16 + row);
            }
            else
            {
                if ((byte)(attr & 0x80) == 0x80)
                    row = 15 - row;
                byte table = (byte)(tile & 1);
                tile = (byte)(tile & 0xFE);
                if (row > 7)
                {
                    tile++;
                    row = row - 8;
                }
                addr = (ushort)(0x1000 * table + tile * 16 + row);
            }
            uint data = 0;
            int attrShf = (attr & 3) << 2;
            byte lowTileByte = RAM.PpuRead(addr);
            byte highTileByte = RAM.PpuRead((ushort)(addr + 8));
            for (int i = 0; i < 8; i++)
            {
                byte p1, p2;
                if ((byte)(attr & 0x40) == 0x40)
                {
                    p1 = (byte)((lowTileByte & 1) << 0);
                    p2 = (byte)((highTileByte & 1) << 1);
                    lowTileByte = (byte)(lowTileByte >> 1);
                    highTileByte = (byte)(highTileByte >> 1);
                }
                else
                {
                    p1 = (byte)((lowTileByte & 0x80) >> 7);
                    p2 = (byte)((highTileByte & 0x80) >> 6);
                    lowTileByte = (byte)(lowTileByte << 1);
                    highTileByte = (byte)(highTileByte << 1);
                }
                data = data << 4;
                data |= (uint)(attrShf | p1 | p2);
            }
            return data;
        }

        private void checkSprites()
        {
            int horizontal = 0;
            if (spriteSize == 0)
                horizontal = 8;
            else
                horizontal = 16;
            int sprCount = 0;
            for (int i = 0; i < 64; i++)
            {
                byte x_coord = OAM[i * 4 + 0];
                byte y_coord = OAM[i * 4 + 3];
                byte tile_a_value = OAM[i * 4 + 2];
                int row = Scanlines - y_coord;
                if (row < 0 || row >= horizontal)
                    continue;
                if (sprCount < 8)
                {
                    spritePatterns[sprCount] = getSpritePattern(i, row);
                    spritePositions[sprCount] = x_coord;
                    spritePriority[sprCount] = (byte)((tile_a_value >> 5) & 1);
                    spriteIndex[sprCount] = (byte)i;
                }
                sprCount++;
            }
            if (sprCount > 8)
            {
                sprCount = 8;
                spriteOverflow = 1;
            }
            spriteCount = sprCount;
        }

        public void tick()
        {
            if (nmiDelay > 0)
            {
                nmiDelay--;
                if (nmiDelay == 0 && nmiOutput && nmiOccured)
                {
                    CPU6502.Instance.triggerNMI();
                }
            }

            if (ShowBackground != 0 && ShowSprite != 0)
            {
                if (frameToggle == 1 && Scanlines == 261 && Cycle == 339)
                {
                    front.Save(@"C:\Users\panda\Desktop\test2.png");
                    back.Save(@"C:\Users\panda\Desktop\test3.png");
                    Cycle = 0;
                    Scanlines = 0;
                    Frame++;
                    frameToggle ^= 1;
                    return;
                }
            }
            Cycle++;
            if (Cycle > 340)
            {
                Cycle = 0;
                Scanlines++;
                if (Scanlines > 261)
                {
                    front.Save(@"C:\Users\panda\Desktop\test.png");
                    back.Save(@"C:\Users\panda\Desktop\test1.png");
                    Scanlines = 0;
                    Frame++;
                    frameToggle ^= 1;
                }
            }
        }

        public void run()
        {
            tick();
            bool render = ShowBackground != 0 || ShowSprite != 0;
            bool preRender = (Scanlines == 261);
            bool visibleScanLine = Scanlines < 240;
            bool renderLine = preRender || visibleScanLine;
            bool preFetchCycle = Cycle >= 321 && Cycle <= 336;
            bool visibleCycle = Cycle >= 1 && Cycle <= 256;
            bool fetchCycle = preFetchCycle || visibleCycle;

            if (render)
            {
                if (visibleScanLine && visibleCycle)
                    renderPixel();

                if (renderLine && fetchCycle)
                {
                    tileData = tileData << 4;
                    int cycleData = Cycle % 8;
                    if (cycleData == 1)
                        getNameTableValue();
                    else if (cycleData == 3)
                        getAttrTable();
                    else if (cycleData == 5)
                        getTileLowByte();
                    else if (cycleData == 7)
                        getTileHighByte();
                    else if (cycleData == 0)
                        setTile();
                }

                if (preRender && Cycle >= 280 && Cycle <= 304)
                    copyY();

                if (renderLine)
                {
                    if (fetchCycle && (Cycle % 8 == 0))
                        incrementX();
                    if (Cycle == 256)
                        incrementY();
                    if (Cycle == 257)
                        copyX();
                }
            }

            if (render)
            {
                if (Cycle == 257)
                {
                    if (visibleScanLine)
                        checkSprites();
                    else
                        spriteCount = 0;
                }
            }

            if (Scanlines == 241 && Cycle == 1)
            {
                setVerticalBlank();
            }

            if (preRender && Cycle == 1)
            {
                clearVerticalBlank();
                spriteZero = 0;
                spriteOverflow = 0;
            }
        }
    }
}