using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json;
using System.Windows.Threading;

namespace Sans
{
    public interface IDialogWindow
    {
        public void DialogEnds(string key);
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDialogWindow
    {
        Thread? dialogThread;
        Thread? bgThread;
        bool doBGAnimation = true;
        int bgAnimationSpeed = 200;

        DialogClass dialogClass;

        static Save save = new Save();

        static void Save()
        {
            StreamWriter sw = new("Saves/s0.txt");
            sw.WriteLine(JsonSerializer.Serialize<Save>(save));
            sw.Close();
        }

        public MainWindow()
        {
            InitializeComponent();
            dialogClass = new(DTDialogBoxText, DTDialogBox);
            DialogClass.LoadDialogs();

            StartBGThread();

            if (File.Exists("Saves/s0.txt"))
            {
                try
                {
                    StreamReader sr = new("Saves/s0.txt");
                    var s = JsonSerializer.Deserialize<Save>(sr.ReadToEnd());
                    save = s ?? new Save();
                    StartButton.Visibility = Visibility.Hidden;
                    sr.Close();
                    CheckStart();
                }
                catch (Exception) { }
            }
        }

        private void StartBGThread()
        {
            bgThread = new Thread(DoBGAnimation);
            bgThread.Start();
            bgThread.IsBackground = true;
        }

        private void CheckStart()
        {
            if (save.CurDialog != "_start_tutorial_r" && save.CurDialog.StartsWith("_start_tutorial")) {
                BeginNewDialog("_start_tutorial_r", 3000);
                save.TutorialPassed = true;
                Save();
            }
            else if (!save.TutorialPassed) {
                SetBGAnimation("Screen");
                if (save.CurDialog == "" && save.LastDialog == "")
                    BeginNewDialog("_start", 1000);
                else BeginNewDialog((save.CurDialog != "") ? save.CurDialog : save.LastDialog, 1000);
            }
            else {
                BeginNewDialog("_run_away0", 3000);
            }
        }

        public void DialogEnds(string key)
        {
            switch (key)
            {
                case "_start":
                    BeginNewDialog("_start_ring-ring", 1000);
                    break;
                case "_start_ring-ring":
                    BeginNewDialog("_start_surprise", 1000);
                    break;
                case "_start_surprise":
                    BeginNewDialog("_start_surprise2", 1000);
                    break;
                case "_start_surprise2":
                    BeginNewDialog("_start_tutorial", 1000);
                    break;
                case "_start_tutorial":
                    BeginNewDialog("_start_tutorial1", 1000);
                    break;
                case "_start_tutorial1":
                    BeginNewDialog("_start_tutorial2", 1000);
                    break;
                case "_start_tutorial2":
                    BeginNewDialog("_start_tutorial3", 1000);
                    break;
                case "_start_tutorial3":
                    BeginNewDialog("_start_tutorial4", 1000);
                    save.TutorialPassed = true;
                    break;
                case "_run_away0":
                case "_start_tutorial_r":
                    SetBGAnimation("Screen");
                    break;
            }
            save.CurDialog = "";
            save.LastDialog = key;
            Save();
        }

        public void BeginNewDialog(string key, int timegap)
        {
            if (dialogThread != null)
                dialogThread.Interrupt();
            dialogThread = new Thread((ThreadStart)delegate () {
                Thread.Sleep(timegap);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    save.CurDialog = key;
                    Save();
                    dialogClass.StartNewDialogs(key, this);
                });
            });

            switch (key)
            {
                case "_start":
                    SetCGAnimation("man_stand");
                    break;
                case "_start_ring-ring":
                    SetCGAnimation("phone");
                    break;
                case "_start_surprise":
                    SetCGAnimation("nothing");
                    break;
            }

            dialogThread.Start();
            dialogThread.IsBackground = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void SetBGAnimation(string bgName)
        {
            BG1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}1.png"));
            BG2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}2.png"));
            BG3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}3.png"));
        }

        private void SetCGAnimation(string cgName)
        {
            CG1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{cgName}1.png"));
            CG2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{cgName}2.png"));
            CG3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{cgName}3.png"));
        }

        private void DoBGAnimation()
        {
            int i = 0;
            
            while (doBGAnimation)
            {
                i++;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    CG1.Visibility = BG1.Visibility = Visibility.Hidden;
                    CG2.Visibility = BG2.Visibility = Visibility.Hidden;
                    CG3.Visibility = BG3.Visibility = Visibility.Hidden;
                    if (i == 0)
                        CG1.Visibility = BG1.Visibility = Visibility.Visible;
                    else if (i == 1)
                        CG2.Visibility = BG2.Visibility = Visibility.Visible;
                    else
                    {
                        CG3.Visibility = BG3.Visibility = Visibility.Visible;
                        i = -1;
                    }
                });

                Thread.Sleep(bgAnimationSpeed);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Hidden;
            BeginNewDialog("_start", 500);
            SetBGAnimation("Screen");
            Save();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (dialogClass.InDialog)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        dialogClass.NextDialog(this);
                        break;
                    case Key.X:
                        dialogClass.DialogToEnd();
                        break;
                }
            }
        }
    }
}
