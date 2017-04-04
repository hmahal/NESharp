
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
        private CartridgeReader cardridgeReader_;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void runCPUTick_Click(object sender, RoutedEventArgs e)
        {
            instructionBox.Text = "IDX";
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
                cardridgeReader_ = new CartridgeReader(dialog.FileName);
                try
                {
                    cardridgeReader_.readCart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                }
            }
        }
    }
}
