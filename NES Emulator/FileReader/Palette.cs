using System.Drawing;

namespace NESEmu
{
    /// <summary>
    /// Palette class contains the values used for generating the colour palette and 
    /// and array of Color objects which are used by PPU during rendering.
    /// </summary>
    class Palette
    {
        /// <summary>
        /// Sets and returns the ColorPalette array.
        /// </summary>
        public Color[] ColorPalette { get; set; }

        /// <summary>
        /// Values to emulate the NTSC signal output by the NES. Values courtest of:
        /// http://www.thealmightyguru.com/Games/Hacking/Wiki/index.php/NES_Palette
        /// </summary>
        private int[] colours = {
            0x666666, 0x002A88, 0x1412A7, 0x3B00A4, 0x5C007E, 0x6E0040, 0x6C0600, 0x561D00,
            0x333500, 0x0B4800, 0x005200, 0x004F08, 0x00404D, 0x000000, 0x000000, 0x000000,
            0xADADAD, 0x155FD9, 0x4240FF, 0x7527FE, 0xA01ACC, 0xB71E7B, 0xB53120, 0x994E00,
            0x6B6D00, 0x388700, 0x0C9300, 0x008F32, 0x007C8D, 0x000000, 0x000000, 0x000000,
            0xFFFEFF, 0x64B0FF, 0x9290FF, 0xC676FF, 0xF36AFF, 0xFE6ECC, 0xFE8170, 0xEA9E22,
            0xBCBE00, 0x88D800, 0x5CE430, 0x45E082, 0x48CDDE, 0x4F4F4F, 0x000000, 0x000000,
            0xFFFEFF, 0xC0DFFF, 0xD3D2FF, 0xE8C8FF, 0xFBC2FF, 0xFEC4EA, 0xFECCC5, 0xF7D8A5,
            0xE4E594, 0xCFEF96, 0xBDF4AB, 0xB3F3CC, 0xB5EBF2, 0xB8B8B8, 0x000000, 0x000000,
        };

        /// <summary>
        /// Constructor for the Palette object. Populates the ColorPalette array
        /// </summary>
        public Palette()
        {
            ColorPalette = new Color[64];
            coloursToPalette();
        }

        /// <summary>
        /// Takes the values from the colours array and converts it into Color object.
        /// Stores the Color objects in the ColorPalette array.
        /// </summary>
        private void coloursToPalette()
        {
            for(int i = 0; i < colours.Length; i++)
            {
                byte r = (byte)(colours[i] >> 16);
                byte g = (byte)(colours[i] >> 8);
                byte b = (byte)(colours[i]);
                ColorPalette[i] = Color.FromArgb(0xff, r, g, b);
            }
        }
    }
}
