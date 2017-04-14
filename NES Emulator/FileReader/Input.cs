//TODO: This is not fully functional. Needs more research
namespace NESEmu
{
    /// <summary>
    /// Implements and emulates the NES controller.
    /// </summary>
    public class Input
    {
        private bool[] buttons;
        private byte index;
        private byte strobe;

        /// <summary>
        /// Constructs the Input object and instantiates the buttons array.
        /// </summary>
        public Input()
        {
            buttons = new bool[8];
        }

        /// <summary>
        /// Reads from the input object and returns appropriate value.
        /// </summary>
        /// <returns>
        /// A byte representing the button press on the controller.
        /// </returns>
        public byte Read()
        {
            byte value = 0;
            if (index < 8 && buttons[index])
                value = 1;
            index++;
            if ((byte)(strobe & 1) == 1)
                index = 0;
            return value;
        }

        /// <summary>
        /// Writes to the strobe value of the Input object.
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte value)
        {
            strobe = value;
            if ((byte)(strobe & 1) == 1)
                index = 0;
        }

    }
}
