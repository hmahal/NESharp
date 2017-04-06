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
                    instructionBox.Text = cpu_.CurrentInstruction;
                    registerBox.Text = cpu_.ToString();
                    memoryBox.Text = mem_.ToString();
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

        /// <summary>
        /// Starts the cpu, to be called after CPU is properly set up
        /// </summary>
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

        /// <summary>
        /// The loop that is run by the cpu thread
        /// Run as past as possible, the thread may
        /// not keep up with the real cpu cycles per second (~556ns per cycle)
        /// </summary>
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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateText(string message)
        {
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
                runCPU.Content = "Start/Pause CPU";
            }
        }
    }
}