using Microsoft.Win32;
using System.Windows;
using NESEmu;
using System;

namespace NESCPUTEST
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CartridgeReader cartridgeReader_;
        private Cartridge testCartridge_;
        private CPU6502 cpu_;
        private Memory mem_;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void runCPUTick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cpu_.Tick();
                instructionBox.Text = cpu_.CurrentInstruction;
                registerBox.Text = cpu_.ToString();
                memoryBox.Text = mem_.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
            }
        }

        private void runCPUButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void openNesRom_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            dialog.Filter = "NES Roms (*.nes)|*.nes"; // Filter files by extension
            dialog.FilterIndex = 1;
            // Show open file dialog box
            if (dialog.ShowDialog() == true)
            {
                cartridgeReader_ = new CartridgeReader(dialog.FileName);
                try
                {
                    testCartridge_ = cartridgeReader_.readCart();
                    MMC3 mapper = new MMC3(testCartridge_, new PPU());
                    mem_ = new Memory(2048, mapper);
                    cpu_ = new CPU6502(mem_);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                }
            }
        }
    }
}