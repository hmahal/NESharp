using System;
using System.IO;

namespace FileReader
{
    public class CartridgeReader
    {
        private string _filePath;
        private Cartridge cart;
        public Exception FileNotFound = new FileNotFoundException();

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
            catch (Exception e)
            {
                throw FileNotFound;
            }
            if (cart.Header == null || cart.Header[0] != 78 || cart.Header[1] != 69 || cart.Header[2] != 83)
            {
                throw new Exception("Unable to open file due to incorrect format or corruption.");
            }

            return cart;
        }
    }
}