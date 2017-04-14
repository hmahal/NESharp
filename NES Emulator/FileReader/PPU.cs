using System;
using System.Drawing;

namespace NESEmu
{
    /// <summary>
    /// Emulates the Picture Processing Unit of the NES. This class is responsible for the
    /// graphics rendering. It currently renders the graphics to a bitmap by setting each pixel.
    /// </summary>
    public class PPU
    {
        //PPU cycles 0-340
        public int Cycle;
        //261 Scanlines. 0-239 rendering lines, 240 post rendering, 241-261 vblank operations 261 pre rendering
        public int Scanline;
        //Frame Counter
        public ulong Frame;

        //Storage tables to mimic the PPU memory
        private byte[] paletteData = new byte[32];
        public byte[] nameTableData = new byte[2048];
        private byte[] oamData = new byte[256];
        
        //PPU registers
        private ushort vramAddress;
        private ushort tempAddress;
        private ushort xScroll;
        private ushort writeToggle;
        private ushort frameFlag;

        private byte register;

        //NMI flags
        private bool nmiOccured;
        private bool nmiOutput;
        private bool nmiPrevious;
        private byte nmiDelay;

        //temporary variables
        private byte nameTableByte;
        private byte attributeTableByte;
        private byte lowTileByte;
        private byte highTileByte;
        private ulong tileData;
        private int spriteCount;
        private uint[] spritePatterns = new uint[8];
        private byte[] spritePositions = new byte[8];
        private byte[] spritePriorities = new byte[8];
        private byte[] spriteIndexes = new byte[8];

        //PPUCTRL register $2000
        private byte flagNameTable;
        private byte flagIncrement;
        private byte flagSpriteTable;
        private byte flagBackgroundTable;
        private byte flagSpriteSize;
        private byte flagMasterSlave;

        //PPUMASK register $2001
        private byte flagGrayscale;
        private byte flagShowLeftBackground;
        private byte flagShowLeftSprites;
        public byte flagShowbackground;
        public byte flagShowSprite;
        private byte flagRedTint;
        private byte flagGreenTint;
        private byte flagBlueTint;

        //PPUSTATUS register $2002
        private byte flagSpriteZeroHit;
        private byte flagSpriteOverflow;

        //OAMADDR register $2003
        private byte oamAddress;

        //PPUDATA register $2007
        private byte bufferedData;

        //Bitmap buffer
        public Bitmap Front { get; set; }
        //Rendering bitmap used in the background by the PPU
        private Bitmap back;


        private Palette palette;

        //Static instance used for Singleton pattern
        private static PPU ppu;

        /// <summary>
        /// Constructor for the PPU class. Instantiates the palette and the bitmaps.
        /// Calls the reset method on initialization.
        /// </summary>
        private PPU()
        {            
            palette = new Palette();
            Front = new Bitmap(256, 240);
            back = new Bitmap(256, 240);
            reset();
        }

        /// <summary>
        /// Returns the PPU instance if it exists. Creates one and returns it otherwise.
        /// Utilizes the singleton pattern.
        /// </summary>
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

        //Returns the current frame value.
        public Bitmap getFrame()
        {
            return Front;
        }

        /// <summary>
        /// Resets the Cycle/Scanline count and writes starting values to the registers.
        /// </summary>
        public void reset()
        {          
            Cycle = 340;
            Scanline = 240;
            Frame = 0;
            writeControl(0);
            writeMask(0);
            writeOAMAddress(0);
        }

        /// <summary>
        /// Returns the value held in the paletteData table at the address passed as the parameter.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>
        /// Value contained in the paletteData table.
        /// </returns>
        public byte readPalette(ushort addr)
        {
            if (addr >= 16 && addr % 4 == 0)
                addr -= 16;
            return paletteData[addr];
        }

        /// <summary>
        /// Write the value parameter at the address passed in.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        public void writePalette(ushort addr, byte value)
        {
            if (addr >= 16 && addr % 4 == 0)
                addr -= 16;
            paletteData[addr] = value;
        }

        /// <summary>
        /// Reads the PPU registers based on the address passed in as a parameter.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns>
        /// Value stored in the register specified by the address.
        /// </returns>
        public byte readRegister(ushort addr)
        {
            switch (addr)
            {
                case (0x2002):
                    return readStatus();
                case (0x2004):
                    return readOAMData();
                case (0x2007):
                    return readPPUData();
            }
            return 0;
        }

        /// <summary>
        /// Writes the value to the register specified by the address.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="value"></param>
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
                    writeOAMAddress(value);
                    break;

                case (0x2004):
                    writeOAMData(value);
                    break;

                case (0x2005):
                    writeScroll(value);
                    break;

                case (0x2006):
                    writeAddress(value);
                    break;

                case (0x2007):
                    writePPUData(value);
                    break;

                case (0x4014):
                    writeDMA(value);
                    break;
            }
        }

        /// <summary>
        /// Writes the value parameter to the PPUCTRL register flags
        /// </summary>
        /// <param name="value"></param>
        private void writeControl(byte value)
        {
            flagNameTable = (byte)((value >> 0) & 3);
            flagIncrement = (byte)((value >> 2) & 1);
            flagSpriteTable = (byte)((value >> 3) & 1);
            flagBackgroundTable = (byte)((value >> 4) & 1);
            flagSpriteSize = (byte)((value >> 5) & 1);
            flagMasterSlave = (byte)((value >> 6) & 1);
            nmiOutput = ((value >> 7) & 1) == 1;
            nmiChange();
            tempAddress = (ushort)((tempAddress & 0xF3FF) | ((value & 0x03) << 10));
        }

        /// <summary>
        /// writes the value to the PPUMASK register flags
        /// </summary>
        /// <param name="value"></param>
        private void writeMask(byte value)
        {
            flagGrayscale = (byte)((value >> 0) & 1);
            flagShowLeftBackground = (byte)((value >> 1) & 1);
            flagShowLeftSprites = (byte)((value >> 2) & 1);
            flagShowbackground = (byte)((value >> 3) & 1);
            flagShowSprite = (byte)((value >> 4) & 1);
            flagRedTint = (byte)((value >> 5) & 1);
            flagGreenTint = (byte)((value >> 6) & 1);
            flagBlueTint = (byte)((value >> 7) & 1);
        }

        /// <summary>
        /// Returns the values stored in the PPUStatus register flags.
        /// </summary>
        /// <returns>
        /// Values contained in the PPUSTATUS register flags
        /// </returns>
        private byte readStatus()
        {
            byte result = (byte)(register & 0x1F);
            result |= (byte)(flagSpriteOverflow << 5);
            result |= (byte)(flagSpriteZeroHit << 6);
            if (nmiOccured)
                result |= (byte)(1 << 7);
            nmiOccured = false;
            nmiChange();
            writeToggle = 0;
            return result;
        }

        /// <summary>
        /// Writes the value to the OAMADDR register
        /// </summary>
        /// <param name="value"></param>
        private void writeOAMAddress(byte value)
        {
            oamAddress = value;
        }

        /// <summary>
        /// returns the value stored in the oamData table at the oamAddr register value
        /// </summary>
        /// <returns>
        /// Value in the oamData table stored at the oamAddress
        /// </returns>
        private byte readOAMData()
        {
            return oamData[oamAddress];
        }

        /// <summary>
        /// Writes data to the oamData table
        /// </summary>
        /// <param name="value"></param>
        private void writeOAMData(byte value)
        {
            oamData[oamAddress] = value;
            oamAddress++;
        }

        /// <summary>
        /// Writes data to the PPUSCROLL register
        /// </summary>
        /// <param name="value"></param>
        private void writeScroll(byte value)
        {
            if (writeToggle == 0)
            {
                tempAddress = (ushort)((tempAddress & 0xFFE0) | value >> 3);
                xScroll = (byte)(value & 0x07);
                writeToggle = 1;
            }
            else
            {
                tempAddress = (ushort)((tempAddress & 0x8FFF) | (value & 0x07) << 12);
                tempAddress = (ushort)((tempAddress & 0xFC1F) | (value & 0xF8) << 2);
                writeToggle = 0;
            }
        }

        /// <summary>
        /// Writes data to the vramAddress of the PPU
        /// </summary>
        /// <param name="value"></param>
        private void writeAddress(byte value)
        {
            if (writeToggle == 0)
            {
                tempAddress = (ushort)((tempAddress & 0x80FF) | (value & 0x3F) << 8);
                writeToggle = 1;
            }
            else
            {
                tempAddress = (ushort)((tempAddress & 0xFF00) | value);
                vramAddress = tempAddress;
                writeToggle = 0;
            }
        }

        /// <summary>
        /// Reads data stored in memory at the vramAddress location.
        /// </summary>
        /// <returns>
        /// Returns the data stored in memory at the vramAddress location
        /// </returns>
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

            if (flagIncrement == 0)
                vramAddress = (ushort)(vramAddress + 1);
            else
                vramAddress = (ushort)(vramAddress + 32);

            return value;
        }

        /// <summary>
        /// Writes data to memory at the location specified by vramAddress
        /// </summary>
        /// <param name="value"></param>
        private void writePPUData(byte value)
        {
            Memory RAM = Memory.Instance;
            RAM.PpuWrite(vramAddress, value);

            if (flagIncrement == 0)
                vramAddress = (ushort)(vramAddress + 1);
            else
                vramAddress = (ushort)(vramAddress + 32);
        }
               


        /// <summary>
        /// Writes data to PPUDMA register
        /// </summary>
        /// <param name="value"></param>
        private void writeDMA(byte value)
        {
            CPU6502 cpu = CPU6502.Instance;
            ushort addr = (ushort)(value << 8);
            for (int i = 0; i < 256; i++)
            {
                oamData[oamAddress] = cpu.RAM.ReadMemory(addr);
                oamAddress++;
                addr++;
            }
            cpu.Stall += 513;
            if (cpu.Cycle % 2 == 1)
                cpu.Stall++;
        }

#region NTSCTIMING

        //Methods to help with NTSC timing for the PPU
        private void incrementX()
        {
            if ((vramAddress & 0x001F) == 31)
            {
                vramAddress = (ushort)(vramAddress & 0xFFE0);
                vramAddress = (ushort)(vramAddress ^ 0x0400);
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
                ushort y = (ushort)((vramAddress & 0x03E0) >> 5);
                if (y == 29)
                {
                    y = 0;
                    vramAddress = (ushort)(vramAddress ^ 0x0800);
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
        #endregion

        /// <summary>
        /// Sets the nmiDelay value if nmiOutput and nmiOccured evaluate to true
        /// And nmi has not occured previously. Otherwise, nmiPrevious is set to
        /// (nmiOutput && nmiOccured)
        /// </summary>
        private void nmiChange()
        {
            bool nmi = nmiOutput && nmiOccured;
            if (nmi && !nmiPrevious)
                nmiDelay = 15;
            nmiPrevious = nmi;
        }

        /// <summary>
        /// Switches the buffer bitmap and the rendered bitmap
        /// </summary>
        private void setVerticalBlank()
        {
            Bitmap tmp = new Bitmap(Front);
            Front = new Bitmap(back);
            back = new Bitmap(Front);
            nmiOccured = true;
            nmiChange();
        }

        /// <summary>
        /// Sets the nmiOccured to false and calls nmiChange() method
        /// </summary>
        private void clearVerticalBlank()
        {
            nmiOccured = false;
            nmiChange();
        }

        /// <summary>
        /// Get the value for the nameTableByte from the memory
        /// </summary>
        private void fetchNameTableByte()
        {
            Memory RAM = Memory.Instance;
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x2000 | (tmp & 0x0FFF));
            nameTableByte = RAM.PpuRead(addr);
        }

        /// <summary>
        /// Gets the value for the attributeTableByte from the Memory
        /// </summary>
        private void fetchAttributeTableByte()
        {
            Memory RAM = Memory.Instance;
            ushort tmp = vramAddress;
            ushort addr = (ushort)(0x23C0 | (tmp & 0x0C00)
                | ((tmp >> 4) & 0x38) | ((tmp >> 2) & 0x07));
            ushort shift = (ushort)(((tmp >> 4) & 4) | (tmp & 2));
            attributeTableByte = (byte)(((RAM.PpuRead(addr) >> shift) & 3) << 2);
        }

        /// <summary>
        /// Gets the value for the lowTileByte member from the memory
        /// </summary>
        private void fetchLowTileByte()
        {
            Memory RAM = Memory.Instance;
            ushort y = (ushort)((vramAddress >> 12) & 7);
            byte table = flagBackgroundTable;
            byte tile = nameTableByte;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTileByte = RAM.PpuRead(addr);
        }

        /// <summary>
        /// Gets the value for the highTile member from the memory
        /// </summary>
        private void fetchHighTileByte()
        {
            Memory RAM = Memory.Instance;
            ushort y = (ushort)((vramAddress >> 12) & 7);
            byte table = flagBackgroundTable;
            byte tile = nameTableByte;
            ushort addr = (ushort)(0x1000 * table + tile * 16 + y);
            lowTileByte = RAM.PpuRead((ushort)(addr + 8));
        }

        /// <summary>
        /// Constructs the tileData value from the lowTilebyte and highTileByte values
        /// </summary>
        private void storeTileData()
        {
            uint data = 0;
            for (int i = 0; i < 8; i++)
            {
                byte attr = attributeTableByte;
                byte low = (byte)((lowTileByte & 0x80) >> 7);
                byte high = (byte)((highTileByte & 0x80) >> 6);
                lowTileByte = (byte)(lowTileByte << 1);
                highTileByte <<= (byte)(highTileByte << 1);
                data <<= 4;
                data |= (uint)(attr | low | high);
            }
            tileData |= data;
        }

        /// <summary>
        /// Returns the first 32 bits of the tileData as an unsigned int
        /// </summary>
        /// <returns></returns>
        private uint fetchTileData()
        {
            return (uint)(tileData >> 32);
        }

        /// <summary>
        /// Returns 0 if background pixel is not going to be displayed.
        /// Returns a byte representing the colour of the background otherwise.
        /// </summary>
        /// <returns></returns>
        private byte backgroundPixel()
        {
            if (flagShowbackground == 0)
                return 0;
            uint data = fetchTileData() >> ((7 - xScroll) * 4);
            return (byte)(data & 0x0F);
        }

        /// <summary>
        /// Returns the index of the sprite pixel and the colour of the sprite pixel.
        /// If no sprite is to be displayed returns a tuple containing 0, 0
        /// </summary>
        /// <returns></returns>
        private Tuple<byte, byte> spritePixel()
        {
            if(flagShowSprite == 0)
                return Tuple.Create<byte, byte>(0, 0);
            for (int i = 0; i < spriteCount; i++)
            {
                int offset = (Cycle - 1) - spritePositions[i];
                if (offset < 0 || offset > 7)
                    continue;
                offset = 7 - offset;
                byte colour = (byte)((spritePatterns[i] >> (byte)(offset * 4)) & 0x0F);
                if (colour % 4 == 0)
                    continue;
                return Tuple.Create((byte)i, colour);
            }
            return Tuple.Create<byte, byte>(0, 0);
        }

        /// <summary>
        /// Sets the colour value of the bitmap pixel based on the values returned from the
        /// backgroundPixel() and the spritePixel() methods
        /// </summary>
        private void renderPixel()
        {
            int x_coord = Cycle - 1;
            int y_coord = Scanline;
            byte background = backgroundPixel();
            Tuple<byte, byte> spritePixel = this.spritePixel();
            byte i = spritePixel.Item1;
            byte sprite = spritePixel.Item2;           

            if (x_coord < 8 && flagShowLeftBackground == 0)
                background = 0;
            if (x_coord < 8 && flagShowLeftSprites == 0)
                sprite = 0;
            bool b = background % 4 != 0;
            bool s = sprite % 4 != 0;
            byte colour;
            if (!b && !s)
            {
                colour = 0;
            }
            else if (!b && s)
            {
                colour = (byte)(sprite | 0x10);
            }
            else if (b && !s)
            {
                colour = background;
            }
            else
            {
                if (spriteIndexes[i] == 0 && x_coord < 255)
                    flagSpriteZeroHit = 1;
                if (spritePriorities[i] == 0)
                    colour = (byte)(sprite | 0x10);
                else
                    colour = background;
            }            
            Color col = palette.ColorPalette[readPalette(colour) % 64];           
            back.SetPixel(x_coord, y_coord, col);
        }

        /// <summary>
        /// Get the spritePattern from the memory for the given index and row.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="row"></param>
        /// <returns>
        /// Returns spritePattern data to be stored in the spritePatterns table
        /// </returns>
        private uint fetchSpritePattern(int index, int row)
        {
            Memory RAM = Memory.Instance;
            byte tile = oamData[index * 4 + 1];
            byte attributes = oamData[index * 4 + 2];
            ushort address = 0;
            if(flagSpriteSize == 0)
            {
                if ((attributes & 0x80) == 0x80)
                    row = 7 - row;
                byte table = flagSpriteTable;
                address = (ushort)(0x1000 * table + tile * 16 + row);
            }
            else
            {
                if ((attributes & 0x80) == 0x80)
                    row = 15 - row;
                byte table = (byte)(tile & 1);
                tile = (byte)(tile & 0xFE);
                if (row > 7)
                {
                    tile++;
                    row = row - 8;
                }
                address = (ushort)(0x1000 * table + tile * 16 + row);
            }
            uint data = 0;
            int a = (attributes & 3) << 2;
            byte lowTileByte = RAM.PpuRead(address);
            byte highTileByte = RAM.PpuRead((ushort)(address + 8));
            for (int i = 0; i < 8; i++)
            {
                byte p1, p2;
                if ((byte)(attributes & 0x40) == 0x40)
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
                data |= (uint)(a | p1 | p2);
            }
            return data;
        }

        /// <summary>
        /// Checks the number of sprites contained in the oamData and set the spriteCount value.
        /// If the number of sprites is less than 8, sprite tables are updated.
        /// </summary>
        private void evaluateSprites()
        {
            int horizontal = 0;
            if (flagSpriteSize == 0)
                horizontal = 8;
            else
                horizontal = 16;
            int sprCount = 0;
            for (int i = 0; i < 64; i++)
            {
                byte y_coord = oamData[i * 4 + 0];
                byte tile_a_value = oamData[i * 4 + 2];
                byte x_coord = oamData[i * 4 + 3];
                
                int row = Scanline - y_coord;
                if (row < 0 || row >= horizontal)
                    continue;
                if (sprCount < 8)
                {
                    spritePatterns[sprCount] = fetchSpritePattern(i, row);
                    spritePositions[sprCount] = x_coord;
                    spritePriorities[sprCount] = (byte)((tile_a_value >> 5) & 1);
                    spriteIndexes[sprCount] = (byte)i;
                }
                sprCount++;
            }
            if (sprCount > 8)
            {
                sprCount = 8;
                flagSpriteOverflow = 1;
            }
            spriteCount = sprCount;
        }

        /// <summary>
        /// Updates the Cycle, Scanline and the Frame counters
        /// </summary>
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


            if(flagShowbackground != 0 && flagShowSprite != 0)
            {
                if(frameFlag == 1 && Scanline == 261 && Cycle == 339)
                {
                    //For debugging purposes only
                    //Front.Save(@"C:\Users\panda\Desktop\test2.png");
                    //back.Save(@"C:\Users\panda\Desktop\test3.png");
                    Cycle = 0;
                    Scanline = 0;
                    Frame++;
                    frameFlag = (ushort)(frameFlag ^ 1);
                    return;
                }
            }
            Cycle++;
            if (Cycle > 340)
            {
                Cycle = 0;
                Scanline++;
                if(Scanline > 261)
                {
                    //For debugging purposes only.
                    //Front.Save(@"C:\Users\panda\Desktop\test.png");
                    //back.Save(@"C:\Users\panda\Desktop\test1.png");
                    Scanline = 0;
                    Frame++;
                    frameFlag ^= 1;
                }
            }
        }

        /// <summary>
        /// Executes a single PPU cycle
        /// </summary>
        public void Step()
        {
            tick();

            bool render = flagShowbackground != 0 || flagShowSprite != 0;

            bool preRender = (Scanline == 261);
            bool visibleScanLine = Scanline < 240;
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
                    switch (cycleData)
                    {
                        case 1:
                            fetchNameTableByte();
                            break;
                        case 3:
                            fetchAttributeTableByte();
                            break;
                        case 5:
                            fetchLowTileByte();
                            break;
                        case 7:
                            fetchHighTileByte();
                            break;
                        case 0:
                            storeTileData();
                            break;
                    }                    
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
                        evaluateSprites();
                    else
                        spriteCount = 0;
                }
            }


            if (Scanline == 241 && Cycle == 1)
            {                
                setVerticalBlank();
            }

            if (preRender && Cycle == 1)
            {
                clearVerticalBlank();
                flagSpriteZeroHit = 0;
                flagSpriteOverflow = 0;
            } 
        }
    }
}