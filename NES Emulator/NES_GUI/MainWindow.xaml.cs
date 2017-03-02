using Microsoft.Win32;
using System.Windows;
using FileReader;


namespace NES_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openRomButtonClick(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            dialog.Filter = "NES Roms (*.nes)|*.nes"; // Filter files by extension
            dialog.FilterIndex = 1;            
            // Show open file dialog box
            if (dialog.ShowDialog() == true)
            {
                CartridgeReader cr = new CartridgeReader(dialog.FileName);
                cr.readCart();
            }
        }
    }
}
