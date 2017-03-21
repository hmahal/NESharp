using System;
using System.IO;

namespace NESEmu
{
    public class CartridgeReader
    {

        private const byte _firstBits = 240;

        private string _filePath;
        private Cartridge cart;        
        private byte flags6;
        private byte flags7;


        public CartridgeReader(string FilePath)
        {
            this._filePath = FilePath;
        }

        public Cartridge readCart()
        {
            cart = new Cartridge();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(_filePath, FileMode.Open)))
                {
                    cart.Header = reader.ReadBytes(16);
                }
            }
            catch (FileNotFoundException filenotfound)
            {
                throw filenotfound;
            }
            if (cart.Header == null || cart.Header[0] != 'N' || cart.Header[1] != 'E' 
                || cart.Header[2] != 'S' || cart.Header[3] != 0x1A)
            {
                throw new Exception("Unable to open file due to incorrect format or corruption.");
            }
            
            flags6 = cart.Header[6];

            cart.VerticalMirroring = (flags6 & 1) == 1;
            cart.Save_RAM = (flags6 & 2) == 2;
            cart.Trainer_Present = (flags6 & 4) == 3;
            cart.Four_Screen_Mirroring = (flags6 & 8) == 4;

            cart.mapper = flags6 & _firstBits;
            cart.mapper = cart.mapper >> 4;

            this.flags7 = cart.Header[7];
            cart.mapper |= flags7 & _firstBits;

            

            

            return cart;
        }
    }
}