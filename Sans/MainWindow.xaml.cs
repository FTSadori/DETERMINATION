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
using System.Collections.ObjectModel;

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

        double DTperClick = 1;

        MachinesHandler machinesHandler = new();
        public static DTHandler? dthandler;

        public List<Upgrade> OpenedUpgrades { get; set; } = new();

        DialogClass dialogClass;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            dialogClass = new(DTDialogBoxText, DTDialogBox);
            dthandler = new(DTcounter, DTpscounter);
            DialogClass.LoadDialogs();
            StartBGThread();

            machinesHandler.AddMachine(new Machine(BuyMachine1, Machine1Count, Machine1Power, 6.0));
            machinesHandler.AddMachine(new Machine(BuyMachine2, Machine2Count, Machine2Power, 6.0e6));
            machinesHandler.AddMachine(new Machine(BuyMachine3, Machine3Count, Machine3Power, 6.0e16));
            machinesHandler.AddMachine(new Machine(BuyMachine4, Machine4Count, Machine4Power, 6.0e26));
            
            if (Save.DoLoad())
            {
                StartButton.Visibility = Visibility.Hidden;
                CheckStart();
                machinesHandler.ReloadMachines();
            }
            dthandler?.ReloadDTPS(machinesHandler.Machines);

            for (int i = 0; i < 33; ++i)
                OpenedUpgrades.Add(new Upgrade("", $"Улучшение просто нереальное {i}", $"Кайфарики {i}", Math.Pow(6.0, i)));
            OpenUpgradePage(0);
        }

        private int currentUpgradePage = 0;

        public void OpenUpgradePage(int num)
        {
            if (num > OpenedUpgrades.Count / 5 || num < 0) return;
            currentUpgradePage = num;

            UpgradesStack.Children.Clear();
            for (int i = num * 5; i < Math.Min(num * 5 + 5, OpenedUpgrades.Count); ++i)
            {
                UpgradesStack.Children.Add(ToStackPanel(OpenedUpgrades[i]));
            }
        }

        private void StartBGThread()
        {
            bgThread = new Thread(DoBGAnimation);
            bgThread.Start();
            bgThread.IsBackground = true;
        }

        private void SetStartScreen()
        {
            if (Save.save.TutorialPassed)
            {
                SetBGAnimation("MachineBG");
                SetCGAnimation("0pSoul");

                DTcounter.Visibility = Visibility.Visible;
                DTpscounter.Visibility = Visibility.Visible;
                
                DTGain.Visibility = Visibility.Visible;
                
                MachineMenu.Width = menuTabsSize;
                UpgradesMenu.Width = menuTabsSize;
                MachineGrid.Visibility = Visibility.Visible;

                if (Save.save.MachineOpened[1]) Machine2.Visibility = Visibility.Visible;
                if (Save.save.MachineOpened[2]) Machine3.Visibility = Visibility.Visible;
                if (Save.save.MachineOpened[3]) Machine4.Visibility = Visibility.Visible;

                dthandler?.ReloadCounter();
            }
        }

        private void CheckStart()
        {
            // Коли втік із туторіалу
            if (Save.save.CurDialog != "_start_tutorial_r" && Save.save.CurDialog.StartsWith("_start_tutorial")) {
                BeginNewDialog("_start_tutorial_r", 3000);
                Save.save.TutorialPassed = true;
                Save.DoSave(LOG);
            }
            // Коли втік до туторіалу
            else if (!Save.save.TutorialPassed) {
                SetBGAnimation("Screen");
                if (Save.save.CurDialog == "" && Save.save.LastDialog == "")
                    BeginNewDialog("_start", 1000);
                else BeginNewDialog((Save.save.CurDialog != "") ? Save.save.CurDialog : Save.save.LastDialog, 1000);
            }
            else
            {
                BeginNewDialog("_run_away0", 3000);
            }

            SetStartScreen();
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
                case "_start_tutorial2":
                    BeginNewDialog("_start_tutorial3", 1000);
                    break;
                case "_start_tutorial3":
                    BeginNewDialog("_start_tutorial4", 1000);
                    Save.save.TutorialPassed = true;
                    break;
                case "_run_away0":
                case "_start_tutorial_r":
                    SetBGAnimation("MachineBG");
                    break;
            }
            Save.save.CurDialog = "";
            Save.save.LastDialog = key;
            Save.DoSave(LOG);
        }

        double menuTabsSize = 230;

        public void BeginNewDialog(string key, int timegap)
        {
            if (dialogThread != null)
                dialogThread.Interrupt();
            dialogThread = new Thread((ThreadStart)delegate () {
                Thread.Sleep(timegap);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    Save.save.CurDialog = key;
                    Save.DoSave(LOG);
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
                case "_start_surprise2":
                    SetCGAnimation("machine");
                    break;
                case "_start_tutorial":
                    SetBGAnimation("MachineBG");
                    SetCGAnimation("0pSoul");
                    break;
                case "_start_tutorial1":
                    DTcounter.Visibility = Visibility.Visible;
                    DTGain.Visibility = Visibility.Visible;
                    break;
                case "_start_tutorial3":
                    MachineMenu.Width = menuTabsSize;
                    MachineGrid.Visibility = Visibility.Visible;
                    break;
                case "_start_tutorial4":
                    UpgradesMenu.Width = menuTabsSize;
                    DTpscounter.Visibility = Visibility.Visible;
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
                    DoTextAnimation(DTGain, i, 95, 85, 90, 10, -55);
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
       
        private void DoTextAnimation(Button text, int i, double size1, double size2, double size3, double margin1base, double margin2base)
        {
            if (i == 0) {
                text.Margin = new Thickness(margin1base - 5, margin2base - 3, text.Margin.Right, text.Margin.Bottom);
                text.FontSize = size1;
            }
            else if (i == 1) {
                text.Margin = new Thickness(margin1base + 3, margin2base + 2, text.Margin.Right, text.Margin.Bottom);
                text.FontSize = size2;
            }
            else {
                text.Margin = new Thickness(margin1base, margin2base, text.Margin.Right, text.Margin.Bottom);
                text.FontSize = size3;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Hidden;
            BeginNewDialog("_start", 500);
            SetBGAnimation("Screen");
            Save.DoSave(LOG);
        }

        private void DT_Click(object sender, RoutedEventArgs e)
        {
            Save.save.Clicks++;
            dthandler?.ChangeDT(DTperClick);

            if (Save.save.LastDialog == "_start_tutorial1" && Save.save.CurDialog != "_start_tutorial2")
            {
                BeginNewDialog("_start_tutorial2", 10);
            }
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

        public StackPanel ToStackPanel(Upgrade upgrade)
        {
            StackPanel panel = new();
            Image image = new();
            Button buybutton = new();
            Button closerbutton = new();

            panel.Width = 178.0;

            image.Height = 100.0;
            image.Width = 100.0;

            buybutton.Style = (Style)this.TryFindResource("UTDTButtonStyle");
            buybutton.FontSize = 30;
            buybutton.Content = BigNumsConverter.GetInPrettyENotation(upgrade.Price, 1000) + " реш.";
            buybutton.Click += delegate (object sender, RoutedEventArgs e)
            {
                //UpgradeBubble.Visibility = Visibility.Visible;
                //CurrentUpgradeDesc.Text = upgrade.Description;
                //CurrentUpgradeName.Text = upgrade.UpgradeName;
            };

            closerbutton.Style = (Style)this.TryFindResource("UTDTButtonStyle");
            closerbutton.FontSize = 30;
            closerbutton.Content = "...";
            closerbutton.Click += delegate (object sender, RoutedEventArgs e)
            {
                UpgradeBubble.Visibility = Visibility.Visible;
                CurrentUpgradeDesc.Text = upgrade.Description;
                CurrentUpgradeName.Text = upgrade.UpgradeName;
            };

            panel.Children.Add(closerbutton);
            panel.Children.Add(image);
            panel.Children.Add(buybutton);

            return panel;
        }

        private void BuyMachine1_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(0);
        private void BuyMachine2_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(1);
        private void BuyMachine3_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(2);
        private void BuyMachine4_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(3);

        private void CloseUpgradeBubble_Click(object sender, RoutedEventArgs e)
        {
            UpgradeBubble.Visibility = Visibility.Hidden;
        }

        private void GoNextButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUpgradePage(currentUpgradePage + 1);
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUpgradePage(currentUpgradePage - 1);
        }

        private void MachineMenu_Click(object sender, RoutedEventArgs e)
        {
            UpgradesGrid.Visibility = Visibility.Hidden;
            MachineGrid.Visibility = Visibility.Visible;
        }

        private void UpgradesMenu_Click(object sender, RoutedEventArgs e)
        {
            MachineGrid.Visibility = Visibility.Hidden;
            UpgradesGrid.Visibility = Visibility.Visible;

        }

        private void ResetMenu_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
