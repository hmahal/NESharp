using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            catch(Exception e)
            {
                throw FileNotFound;
            }

            return cart;
        }
    }
}
