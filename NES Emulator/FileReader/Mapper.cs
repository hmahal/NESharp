namespace NESEmu
{
    /// <summary>
    /// Abstract class for implmenting a member. Provides the methods and members all 
    /// inheriting classes must implement.
    /// </summary>
    public abstract class Mapper
    {
        public Cartridge cart { get; set; }

        /// <summary>
        /// Constructor for the Mapper class.  MMVC3 is used.  
        /// </summary>
        /// <param name="cart"></param>
        public Mapper(Cartridge cart)
        {
            this.cart = cart;
        }

        public abstract byte read(ushort addr);
        public abstract void Tick();
        public abstract void write(ushort addr, byte value);
    }
}