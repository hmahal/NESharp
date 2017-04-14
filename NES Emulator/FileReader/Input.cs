using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESEmu
{
    public class Input
    {
        private bool[] buttons;
        private byte index;
        private byte strobe;

        public Input()
        {
            buttons = new bool[8];
        }

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

        public void Write(byte value)
        {
            strobe = value;
            if ((byte)(strobe & 1) == 1)
                index = 0;
        }

    }
}
