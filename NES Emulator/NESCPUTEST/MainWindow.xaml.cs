using Microsoft.Win32;
using System.Windows;
using NESEmu;
using System;
using System.Diagnostics;
using System.Threading;

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
        private bool running;
        private Thread _cpuThread;
        private string filePath_;

        public MainWindow()
        {
            InitializeComponent();
            running = false;
        }

        private void runCPUTick_Click(object sender, RoutedEventArgs e)
        {
            if (cpu_ != null)
            {
                try
                {
                    cpu_.Tick();
                    if (instructionBox.Text != "")
                    {
                        prevInstrBox.Text += instructionBox.Text + "\n";
                        prevInstrBox.ScrollToEnd();
                    }
                    instructionBox.Text = cpu_.CurrentInstruction;
                    registerBox.Text = cpu_.ToString();
                    memoryBox.Text = mem_.ToString();
                    addressBox.Text = cpu_.CurrentAddress.ToString("X4");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                }
            }
        }

        private void runCPUButton_Click(object sender, RoutedEventArgs e)
        {
            if (cpu_ != null)
            {
                try
                {
                    if (running == false)
                    {
                        start();
                        runCPU.Content = "Running...";
                    }
                    else
                    {
                        running = false;
                        runCPU.Content = "Stopped...";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                }
            }
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
                filePath_ = dialog.FileName;
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

        public void start()
        {
            try
            {
                if (!running)
                {
                    _cpuThread = new Thread(new ThreadStart(run));
                    running = true;
                    _cpuThread.Start();
                }
                else
                {
                    Debug.WriteLine("Thread already exists.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public delegate void UpdateTextCallback(string message);

        private void run()
        {
            try
            {
                while (running)
                {
                    cpu_.Tick();
                    instructionBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateText),
                        new object[] { cpu_.CurrentInstruction }
                    );
                    registerBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateText1),
                        new object[] { cpu_.ToString() }
                    );
                    memoryBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateText2),
                        new object[] { mem_.ToString() }
                    );
                    memoryBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateText3),
                        new object[] { cpu_.CurrentAddress.ToString("X4") }
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateText(string message)
        {
            if (instructionBox.Text != "")
            {
                prevInstrBox.Text += instructionBox.Text + "\n";
                prevInstrBox.ScrollToEnd();
            }
            instructionBox.Text = message;
        }

        private void UpdateText1(string message)
        {
            registerBox.Text = message;
        }

        private void UpdateText2(string message)
        {
            memoryBox.Text = message;
        }

        private void UpdateText3(string message)
        {
            addressBox.Text = message;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            if (filePath_ != null)
            {
                cartridgeReader_ = new CartridgeReader(filePath_);
                testCartridge_ = cartridgeReader_.readCart();
                MMC3 mapper = new MMC3(testCartridge_, new PPU());
                mem_ = new Memory(2048, mapper);
                cpu_ = new CPU6502(mem_);
                instructionBox.Text = "";
                registerBox.Text = "";
                memoryBox.Text = "";
                prevInstrBox.Text = "";
                addressBox.Text = "";
                runCPU.Content = "Start/Pause CPU";
            }
        }

        private void injectButton_Click(object sender, RoutedEventArgs e)
        {
            if(opCodeInjectBox.Text != "" && cpu_ != null)
            {
                int opcode;
                bool success = int.TryParse(opCodeInjectBox.Text, out opcode);
                if (success)
                {
                    cpu_.Inject(opcode);
                }
            }
        }
    }
}