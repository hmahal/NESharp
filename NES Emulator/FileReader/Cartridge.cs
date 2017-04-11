namespace NESEmu
{
    public class Cartridge
    {       

        public byte Mirroring { get; set; }
        public bool Save_RAM { get; set; }
        public bool Trainer_Present { get; set; }
        public bool Four_Screen_Mirroring { get; set; }

        public int mapper { get; set; }

        public byte[] Sram { get; set; }
        public byte[] Header { get; set; }
        public byte[] Prgrom { get; set; }
        public byte[] Chrrom { get; set; }
        public byte[] Trainer { get; set; }
        public byte[] Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public Cartridge getCart(string filepath)
        {
            CartridgeReader read = new CartridgeReader(filepath);
            return read.readCart();
        }
    }
}