namespace NESEmu
{
    /// <summary>
    /// struct to contain information passed to the opcode methods.
    /// Designed in order to create action delegates which take similar parameters
    /// Provides the address, Program Counter value and the addressing mode to the
    /// method dealing with the opcode.
    /// </summary>
    struct MemoryInfo
    {

        public ushort Address { get; set; }
        public ushort PC_register { get; set; }
        public int Addr_mode { get; set; }

        public MemoryInfo(ushort addr, ushort pCounter, int mode)
        {
            Address = addr;
            PC_register = pCounter;
            Addr_mode = mode;
        }
    }
}
