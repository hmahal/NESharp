using Microsoft.Win32;
using System.Windows;
using NESEmu;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

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
        private PPU ppu_;
        private Input input1;
        private Input input2;
        private bool running;
        private Thread _cpuThread;
        private string filePath_;


        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            running = false;
            ppu_ = PPU.Instance;
        }

        /// <summary>
        /// Converts Bitmaps to ImageSource so it can be displayed to WPF controls
        /// </summary>
        /// <param name="bitmap">The bitmap to convert</param>
        /// <returns>BitmapImage which can be set as wpf imagesource</returns>
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        /// <summary>
        /// Run the CPU for one tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runCPUTick_Click(object sender, RoutedEventArgs e)
        {
            if (cpu_ != null)
            {
                try
                {
                    cpu_.Tick();
                    ppu_.run();
                    ppu_.run();
                    ppu_.run();
                    if (instructionBox.Text != "")
                    {
                        prevInstrBox.Text += instructionBox.Text + " " + addressBox.Text + "\n";
                        prevInstrBox.ScrollToEnd();
                    }
                    instructionBox.Text = cpu_.CurrentInstruction;
                    registerBox.Text = cpu_.ToString();
                    memoryBox.Text = mem_.ToString();
                    addressBox.Text = cpu_.CurrentAddress.ToString("X4");
                    Display.Source = BitmapToImageSource(ppu_.getFrame());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Run the CPU on a new thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Open a NES rom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    input1 = new Input();
                    input2 = new Input();
                    testCartridge_ = cartridgeReader_.readCart();
                    MMC3 mapper = new MMC3(testCartridge_);
                    ppu_ = PPU.Instance;
                    Memory.Create(2048, mapper, input1, input2);
                    mem_ = Memory.Instance;
                    CPU6502.Create(mem_);
                    cpu_ = CPU6502.Instance;
                    registerBox.Text = cpu_.ToString();
                    memoryBox.Text = mem_.ToString();
                    addressBox.Text = cpu_.CurrentAddress.ToString("X4");
                    Display.Source = BitmapToImageSource(ppu_.getFrame());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK);
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Creates a new thread and runs the cpu
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
                Debug.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Callback used to allow a seperate thread to update the WPF Textboxes
        /// </summary>
        /// <param name="message">The string text to update</param>
        public delegate void UpdateTextCallback(string message);

        /// <summary>
        /// Callback used to allow a seperate thread to update the WPF Display
        /// </summary>
        public delegate void UpdateDisplayCallback();

        /// <summary>
        /// The loop that the thread runs
        /// </summary>
        private void run()
        {
            try
            {
                while (running)
                {
                    cpu_.Tick();
                    ppu_.run();
                    ppu_.run();
                    ppu_.run();
                    instructionBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateInstr),
                        new object[] { cpu_.CurrentInstruction }
                    );
                    registerBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateRegister),
                        new object[] { cpu_.ToString() }
                    );
                    memoryBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateMemoryBox),
                        new object[] { mem_.ToString() }
                    );
                    memoryBox.Dispatcher.Invoke(
                        new UpdateTextCallback(this.UpdateAddrBox),
                        new object[] { cpu_.CurrentAddress.ToString("X4") }
                    );
                    Display.Dispatcher.Invoke(
                        new UpdateDisplayCallback(this.UpdateDisplay));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Called to update the instructionBox
        /// </summary>
        /// <param name="message">The string to update with</param>
        private void UpdateInstr(string message)
        {
            if (instructionBox.Text != "")
            {
                prevInstrBox.Text += instructionBox.Text + " " + addressBox.Text + "\n";
                prevInstrBox.ScrollToEnd();
            }
            instructionBox.Text = message;
        }

        /// <summary>
        /// Called to update the registerBox
        /// </summary>
        /// <param name="message">The string to update with</param>
        private void UpdateRegister(string message)
        {
            registerBox.Text = message;
        }

        /// <summary>
        /// Called to update the memoryBox
        /// </summary>
        /// <param name="message">The string to update with</param>
        private void UpdateMemoryBox(string message)
        {
            memoryBox.Text = message;
        }

        /// <summary>
        /// Called to update the addressBox
        /// </summary>
        /// <param name="message">The string to update with</param>
        private void UpdateAddrBox(string message)
        {
            addressBox.Text = message;
        }

        /// <summary>
        /// Called to update the Display
        /// </summary>
        private void UpdateDisplay()
        {
            Display.Source = BitmapToImageSource(ppu_.getFrame());
        }

        /// <summary>
        /// Resets the CPU to starting state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            cpu_.Reset();
            instructionBox.Text = "";
            registerBox.Text = "";
            memoryBox.Text = "";
            prevInstrBox.Text = "";
            addressBox.Text = "";
            runCPU.Content = "Start/Pause CPU";
        }

        /// <summary>
        /// Injects a opcode into the CPU based on whats in the opCodeInjectBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void injectButton_Click(object sender, RoutedEventArgs e)
        {
            if (opCodeInjectBox.Text != "" && cpu_ != null)
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