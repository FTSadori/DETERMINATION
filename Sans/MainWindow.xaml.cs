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
using System.Windows.Resources;
using System.Diagnostics.PerformanceData;
using System.Diagnostics.CodeAnalysis;

namespace Sans
{
    public class ManualPage
    {
        public string Label = "";
        public string Text = "";

        public ManualPage(string text, string label = "")
        {
            Text = text;
            Label = label;
        }
    }
    interface ICloseableView
    {
        void Close(bool confirm);
    }

    public interface IDialogWindow
    {
        public void DialogEnds(string key);
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDialogWindow, ICloseableView
    {
        public static MainWindow? This;

        Thread? dialogThread;
        Thread? bgThread;
        Thread? roomCheckerThread;
        bool doBGAnimation = true;
        int bgAnimationSpeed = 200;

        double DTperClick = 1;
        int PercentDTPower = 0;

        public static MachinesHandler machinesHandler = new();
        public static DTHandler? dthandler;
        public static RPHandler? rphandler;
        public static TickHandler? tickhandler;

        Thread? manualNotifThread;
        Thread? checkForLimit;

        RoutedEventHandler LastBuyDelegate = null;

        bool UseMaxBuy = false;

        double UIScale = 1.0;
        double UIScaleFullscreen = 1.15;

        public static void DoCmd(ThreadStart th)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, th);
        }

        public DialogClass dialogClass;

        public bool canLookAtRoom = true;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            This = this;

            dialogClass = new(DTDialogBoxText, DTDialogBox);
            dthandler = new(DTcounter, DTpscounter);
            rphandler = new(RPcounter, RPGainCounter);
            tickhandler = new(Tickcounter);
            DialogClass.LoadDialogs();
            Upgrades.ReadUpgrades();

            StartBGThread();

            machinesHandler.AddMachine(new Machine(BuyMachine1, Machine1Count, Machine1Power, 6.0));
            machinesHandler.AddMachine(new Machine(BuyMachine2, Machine2Count, Machine2Power, 6.0e3));
            machinesHandler.AddMachine(new Machine(BuyMachine3, Machine3Count, Machine3Power, 6.0e7));
            machinesHandler.AddMachine(new Machine(BuyMachine4, Machine4Count, Machine4Power, 6.0e14));

            machinesHandler.AddTimeMachine(new Machine(BuyTimeMachine1, TimeMachine1Count, TimeMachine1Power, 1.0));
            machinesHandler.AddTimeMachine(new Machine(BuyTimeMachine2, TimeMachine2Count, TimeMachine2Power, 50.0));
            machinesHandler.AddTimeMachine(new Machine(BuyTimeMachine3, TimeMachine3Count, TimeMachine3Power, 256.0));
            machinesHandler.AddTimeMachine(new Machine(BuyTimeMachine4, TimeMachine4Count, TimeMachine4Power, 1000.0));

            roomCheckerThread = new(delegate ()
            {
                int i = 0;
                int j = 0;
                while (true)
                {
                    DoCmd(delegate ()
                    {
                        if (Room256.amHandler != null && Room256.amHandler.AmalgamPlace != 0)
                        {
                            if (Room.Background == Brushes.DarkRed)
                                Room.Background = Brushes.Transparent;
                            else
                                Room.Background = Brushes.DarkRed;
                        }
                        else
                            Room.Background = Brushes.Transparent;
                    });
                    Thread.Sleep(500);
                    i++;
                    j = (j + 1) % 2;
                    if (i == 100)
                    {
                        i = 0;
                        DoCmd(delegate () { Save.DoSave(LOG); });
                    }
                    if (j == 1)
                    {
                        DoCmd(delegate () { dthandler?.ChangeDT(Save.save.DT * PercentDTPower / 100); });
                    }
                }
            });
            roomCheckerThread.Start();
            roomCheckerThread.IsBackground = true;

            if (Save.DoLoad())
            {
                StartButton.Visibility = Visibility.Hidden;
            
                rphandler?.ReloadPRGainCounter();
                CheckStart();
                machinesHandler.ReloadMachines();
                machinesHandler.ReloadTimeMachines();

                if (Save.save.OnLimit)
                    DTcounter.Text = "Предел реш.";
            }
            dthandler?.ReloadDTPS(machinesHandler.Machines);

            ReadManualFile();

            RefreshManual();

            manualNotifThread = new(delegate ()
            {
                while (true)
                {
                    if (Save.save.ManualPagesReadedMax < manualPages.Count - 1)
                    {
                        DoCmd(delegate ()
                        {
                            InfoButton.Opacity = 1.0;
                            if (InfoButton.Background == Brushes.Transparent)
                            {
                                NextPageButton.Background = Brushes.DarkRed;
                                InfoButton.Background = Brushes.DarkRed;
                            }
                            else
                            {
                                NextPageButton.Background = Brushes.Transparent;
                                InfoButton.Background = Brushes.Transparent;
                            }
                        });
                    }
                    else
                        DoCmd(delegate () {
                            NextPageButton.Background = Brushes.Transparent;
                            InfoButton.Background = Brushes.Transparent;
                            InfoButton.Opacity = 0.5;
                        });

                    Thread.Sleep(500);
                }
            });
            manualNotifThread.Start();
            manualNotifThread.IsBackground = true;

            checkForLimit = new(delegate ()
            {
                List<double> limits = new() { 1e6, 1e10, 1e15, 1e20, 1e25, 1e40 };

                while (true)
                {
                    Thread.Sleep(500);
                    for (int i = 0; i < limits.Count; i++)
                    {
                        if (Save.save.LimitLvl == i && Save.save.DT > limits[i])
                        {
                            DoCmd(delegate () {
                                Save.save.TryAddPages(new() { 10, 11 });
                                ++Save.save.LimitLvl;
                                Save.save.OnLimit = true;
                                DTcounter.Text = "Лимит реш.";
                            });
                        }
                    }
                }
            });
            checkForLimit.Start();
            checkForLimit.IsBackground = true;

            SetUIScale(UIScale = Save.save.UIScale);

            CheckRiverMan();

            int level = Save.save.InsaneLevel - 1;
            if (level > 0) SetHallAnimation(level);

            if (Save.save.IsLastBattle)
            {
                lastBattle = new(delegate ()
                {
                    Thread.Sleep(100);

                    DoCmd(delegate ()
                    {
                        bool end = false;
                        if (Save.save.LastBattleTimes > 4)
                        {
                            if (!Directory.Exists("Boat")) end = true;
                            else
                            if (File.Exists("Boat/Chara.txt") && File.Exists("Boat/Sans.txt"))
                            {
                                end = true;

                                StreamResourceInfo res = Application.GetResourceStream(new Uri($"pack://application:,,,/Images/DontForget.png"));
                                res.Stream.CopyTo(new FileStream("Don't Forget.png", FileMode.OpenOrCreate));

                                Directory.Delete("Boat", true);
                            }
                            else
                            {
                                StreamWriter sw = new("Chara.txt");
                                sw.WriteLine("* Скорее! Надо выбираться!");
                                sw.Close();
                                sw = new("Sans.txt");
                                sw.WriteLine("* ...");
                                sw.Close();
                            }
                        }
                        if (!end)
                        {
                            AmalgametHandler.IsActive = false;
                            UltraBlackScreen.Visibility = Visibility.Visible;

                            Room256 room256 = new();
                            room256.LastBattle();
                            room256.ShowDialog();
                        }
                        Close(false);
                    });
                });
                lastBattle.Start();
                lastBattle.IsBackground = true;
            }
        }

        Thread? lastBattle;

        List<ManualPage> allManualPages = new();
        List<ManualPage> manualPages = new();
        int currentManualPage = 0;

        public void RefreshManual()
        {
            manualPages.Clear();

            foreach (int num in Save.save.ManualPagesAvailable) {
                manualPages.Add(allManualPages[num]);
            }
        }

        private void ReadManualFile()
        {
            Stream resourceStream = Application.GetResourceStream(new Uri($"pack://application:,,,/Text/Manual.txt")).Stream;

            using StreamReader reader = new(resourceStream);
            var lines = reader.ReadToEnd().Split("\n");
            foreach (var line in lines)
            {
                var newline = line.Replace("\\n", "\n");

                if (newline.StartsWith("$"))
                    allManualPages.Add(new ManualPage("", newline.Substring(1)));
                else
                {
                    if (allManualPages[^1].Text == "")
                        allManualPages[^1].Text = newline ?? " ";
                    else
                        allManualPages.Add(new ManualPage(newline ?? " "));
                }
            }
        }

        public void CheckRiverMan()
        {
            if (!Directory.Exists("Boat")) return;

                List<string> AllTraLaLals = new List<string>() {
                "* Тра-ла-ла, никогда не доверяй большим числам...",
                "* Тра-ла-ла, не бездействуй, делай всё решительно, но аккуратно...",
                "* Тра-ла-ла, разве ОП дают только за убийства? Тра-ла-ла...",
                "* Тра-ла-ла, береги пальцы...",
                "* Тра-ла-ла, следи за комнатой, иначе ты можешь что-то пропустить.",
                "* Тра-ла-ла, от перенапряжения можно увидеть много интересного...",
                "* Тра-ла-ла... Да, я знаю, что я часто повторяюсь, тра-ла-ла",
                "* Тра-ла-ла, никогда не встречал 'e' в больших числах?",
                "* Тра-ла-ла, а зачем ты работаешь?",
                "* Тра-ла-ла, почему моя лодка так выглядит? А какая разница, главное, что она работает, тра-ла-ла",
                "* Тра-ла-ла, почему я текстовый файл? А ты разве нет? Тра-ла-ла..."
            };
            const string Path = "Boat/RiverPerson.txt";

            if (File.Exists(Path))
            {
                StreamWriter sw = new(Path);
                var ind = AmalgametHandler.random.Next(AllTraLaLals.Count);
                sw.WriteLine(AllTraLaLals[ind]);
                sw.Close();
            }
            else
            {
                StreamWriter sw = new(Path);
                sw.WriteLine("* Тра-ла-ла, зачем тебе это делать?");
                sw.Close();
            }
        }

        public void LoadUpgrades()
        {
            foreach (var upgrade in Upgrades.UpgradesList)
            {
                if (Save.save.OpenedUpgrades.ContainsKey(upgrade.GetUpgradeName()))
                {
                    if (Save.save.OpenedUpgrades[upgrade.GetUpgradeName()] == UpgradeMode.Bought)
                    { UseUpgrade(upgrade); }
                }
            }

            OpenUpgradePage(0);
        }

        public void ReloadCurrentUpgradePage()
        {
            OpenUpgradePage(currentUpgradePage);
        }

        private int currentUpgradePage = 0;

        public void OpenUpgradePage(int num)
        {
            var openedUpgradesNames = Save.save.OpenedUpgrades.Where(p => p.Value == UpgradeMode.Opened)
                .ToDictionary(x => x.Key, x => x.Value);
            List<Upgrade> openedUpgrades = new();
            openedUpgrades
                .AddRange(Upgrades.UpgradesList.Where(u => openedUpgradesNames.ContainsKey(u.GetUpgradeName()))
                .OrderBy(u => u.Price)
                .ToList());

            if (num > (openedUpgrades.Count - 1) / 5 || num < 0) return;
            currentUpgradePage = num;

            UpgradesStack.Children.Clear();
            for (int i = num * 5; i < Math.Min(num * 5 + 5, openedUpgrades.Count); ++i)
            {
                UpgradesStack.Children.Add(ToStackPanel(openedUpgrades[i]));
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
                InfoButton.Visibility = Visibility.Visible;

                ReloadMachineTab();

                LoadUpgrades();
            }
            if (Save.save.ResetWas)
            {
                TimeMachineMenu.Width = menuTabsSize;
                ResetMenu.Width = 0.0;
                RPcounter.Visibility = Visibility.Visible;
                Tickcounter.Visibility = Visibility.Visible;
                rphandler?.ReloadCounter();
                tickhandler?.ReloadCounter();
                machinesHandler.ReloadTimeMachines();
            }
            if (Save.save.RoomButton)
            {
                Room.Visibility = Visibility.Visible;
                Room256 room256 = new();
            }
            if (Save.save.StatButton)
                Stat.Visibility = Visibility.Visible;
        }

        public void ReloadMachineTab()
        {
            Machine2.Visibility = Save.save.MachineOpened[1] ? Visibility.Visible : Visibility.Hidden;
            Machine3.Visibility = Save.save.MachineOpened[2] ? Visibility.Visible : Visibility.Hidden;
            Machine4.Visibility = Save.save.MachineOpened[3] ? Visibility.Visible : Visibility.Hidden;

            dthandler?.ReloadCounter();
            machinesHandler.ReloadMachines();
        }

        private void CheckStart()
        {
            // Коли втік із туторіалу
            if (Save.save.InTutorial) {
                BeginNewDialog("_start_tutorial_r", 3000);
                Save.save.TutorialPassed = true;
                Save.save.InTutorial = false;
                Save.DoSave(LOG);
            }
            // Коли втік до туторіалу
            else if (!Save.save.TutorialPassed) {
                SetBGAnimation("Screen");
                if (Save.save.CurDialog == "" && Save.save.LastDialog == "")
                    BeginNewDialog("_start", 0);
                else BeginNewDialog((Save.save.CurDialog != "") ? Save.save.CurDialog : Save.save.LastDialog, 1000);
            }
            else
            {
                if (Save.save.RanAway)
                {
                    Save.save.RanAway = false;
                    ResetButton_Click(new(), new());
                }
                else
                    BeginNewDialog("_run_away0", 0);
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
                    Save.save.InTutorial = false;
                    Save.save.TutorialPassed = true;
                    break;
                case "_start_tutorial4":
                    InfoButton.Visibility = Visibility.Visible; 
                    break;
                case "_idling_1":
                case "_idling_2":
                case "_idling_3":
                case "_idling_4":
                case "_destroyed_1":
                case "_destroyed_2":
                case "_destroyed_3":
                case "_destroyed_4":
                    BeginNewDialog("_idling_end", 66);
                    break;
                case "_room1":
                    Room256 room256 = new();
                    room256.ShowDialog();
                    BeginNewDialog("_room2", 0);
                    break;
                case "_stat1":
                    break;
                case "_run_away0":
                case "_start_tutorial_r":
                    SetBGAnimation("MachineBG");
                    break;
                case "_second_machine":
                    BeginNewDialog("_second_machine2", 100);
                    break;
                case "_second_machine2":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_reset_av":
                    BeginNewDialog("_reset_av2", 100);
                    break;
                case "_reset_av2":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_reset_black":
                    BeginNewDialog("_dream1", 100);
                    break;
                case "_dream1":
                    BeginNewDialog("_dream_black1", 100);
                    break;
                case "_dream_black1":
                    BeginNewDialog("_dream_wake_up1", 100);
                    break;
                case "_dream_wake_up1":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_dream_start2":
                    BeginNewDialog("_dream2", 100);
                    break;
                case "_dream2":
                    BeginNewDialog("_dream_wake_up2", 100);
                    break;
                case "_dream_wake_up2":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_dream_start3n":
                    BeginNewDialog("_chara3n", 100);
                    break;
                case "_chara3n":
                case "_chara3i":
                case "_chara3s":
                case "_chara4n2":
                case "_chara4i2":
                case "_chara4s2":
                    BeginNewDialog("_dream_wake_up2", 100);
                    break;
                case "_dream_start3i":
                    BeginNewDialog("_chara3i", 100);
                    break;
                case "_dream_start3s":
                    BeginNewDialog("_chara3s", 100);
                    break;
                case "_dream_start4n":
                    BeginNewDialog("_chara4n", 100);
                    break;
                case "_dream_start4i":
                    BeginNewDialog("_chara4i", 100);
                    break;
                case "_dream_start4s":
                    BeginNewDialog("_chara4s", 100);
                    break;
                case "_chara4n":
                    BeginNewDialog("_chara4n2", 100);
                    break;
                case "_chara4s":
                    BeginNewDialog("_chara4s2", 100);
                    break;
                case "_chara4i":
                    BeginNewDialog("_chara4i2", 100);
                    break;
                case "_time1":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_time2":
                    BeginNewDialog("_timebreak", 100);
                    break;
                case "_timebreak":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_amalgam":
                    BlackScreen.Visibility = Visibility.Hidden;
                    break;
                case "_6.6e66_n1":
                    BeginNewDialog("_6.6e66_n2", 100);
                    break;
                case "_6.6e66_n2":
                    BeginNewDialog("_6.6e66_n3", 100);
                    break;
                case "_6.6e66_n3":
                    BeginNewDialog("_6.6e66_n4", 100);
                    break;
                case "_6.6e66_n4":
                    BeginNewDialog("_6.6e66_n5", 100);
                    break;
                case "_6.6e66_n5":
                    BeginNewDialog("_6.6e66_n6", 100);
                    break;
                case "_6.6e66_n6":
                    BeginNewDialog("_6.6e66_n7", 100);
                    break;
                case "_6.6e66_n7":
                    BeginNewDialog("_end_n1", 100);
                    break;
                case "_end_n1":
                    File.Delete("Saves/s0.txt");
                    Close(false);
                    break;
                case "_6.6e66_i1":
                    BeginNewDialog("_6.6e66_i2", 100);
                    break;
                case "_6.6e66_i2":
                    BeginNewDialog("_6.6e66_i3", 100);
                    break;
                case "_6.6e66_i3":
                    BeginNewDialog("_6.6e66_i4", 100);
                    break;
                case "_6.6e66_i4":
                    //BeginNewDialog("_6.6e66_i5", 100);
                    break;
                case "_you_cant":
                    BeginNewDialog("_end_i2", 100);
                    break;
                case "_end_i2":
                    BeginNewDialog("_end_i3", 100);
                    break;
                case "_end_i3":
                    File.Delete("Saves/s0.txt");
                    Close(false);
                    break;
                case "_he_notice":
                    BeginNewDialog("_he_notice2", 100);
                    break;
                case "_he_notice2":
                    Gaster();
                    break;
            }
            Save.save.CurDialog = "";
            Save.save.LastDialog = key;

            AmalgametHandler.IsActive = true;
            Save.DoSave(LOG);
        }

        public double menuTabsSize = 230;

        public void Gaster()
        {
            MessageBox.Show("Будь осторожен", "????", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            AmalgametHandler.IsActive = false;
            UltraBlackScreen.Visibility = Visibility.Visible;
            Room256 room256 = new();
            room256.GasterHere();
            room256.Show();
        }

        public void BeginNewDialog(string key, int timegap)
        {
            AmalgametHandler.IsActive = false;

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
                    Save.save.InTutorial = true;
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
                    canLookAtRoom = true;
                    break;
                case "_run_away0":
                    canLookAtRoom = true;
                    break;
                case "_idling_1":
                case "_idling_2":
                case "_idling_3":
                case "_idling_4":
                case "_destroyed_1":
                case "_destroyed_2":
                case "_destroyed_3":
                case "_destroyed_4":
                    DoCmd(delegate () { 
                        BlackScreen.Visibility = Visibility.Visible;
                        SetOnBlackAnimation("nothing");
                    });
                    break;
                case "_idling_end":
                    Save.save.RanAway = true;
                    SetOnBlackAnimation("Death");
                    gameClosingThread = new Thread(delegate () {
                        Thread.Sleep(500);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            Close(false);
                        });
                    });
                    gameClosingThread.Start();
                    gameClosingThread.IsBackground = true;
                    break;
                case "_second_machine2":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("Paper");
                    break;
                case "_reset_av":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("nothing");
                    break;
                case "_reset_av2":
                    SetOnBlackAnimation("SStation");
                    break;
                case "_reset_black":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("nothing");
                    break;
                case "_dream1":
                case "_dream2":
                    SetOnBlackAnimation("Chara");
                    break;
                case "_dream_black1":
                    SetOnBlackAnimation("nothing");
                    break;
                case "_dream_wake_up1":
                case "_dream_wake_up2":
                    SetOnBlackAnimation("nothing");
                    break;
                case "_dream_start2":
                case "_dream_start3n":
                case "_dream_start3i":
                case "_dream_start3s":
                case "_dream_start4n":
                case "_dream_start4i":
                case "_dream_start4s":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("nothing");
                    break;
                case "_time1":
                case "_time2":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("Time");
                    break;
                case "_timebreak":
                    SetOnBlackAnimation("TimeB");
                    break;
                case "_chara4n2":
                    SetOnBlackAnimation("SheN");
                    break;
                case "_chara3s":
                case "_chara4s":
                    SetOnBlackAnimation("Chara");
                    break;
                case "_chara4s2":
                    SetOnBlackAnimation("She");
                    break;
                case "_chara3n":
                case "_chara3i":
                case "_chara4i":
                case "_chara4n":
                    SetOnBlackAnimation("CharaE");
                    break;
                case "_chara4i2":
                    SetOnBlackAnimation("SheI");
                    break;
                case "_amalgam":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("Chung");
                    break;
                case "_6.6e66_n1":
                case "_6.6e66_i1":
                case "_he_notice":
                    confirmClosing = true;
                    break;
                case "_6.6e66_n2":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("nothing");
                    break;
                case "_6.6e66_n3":
                    SetOnBlackAnimation("phone");
                    break;
                case "_6.6e66_n4":
                    SetOnBlackAnimation("Messages");
                    break;
                case "_6.6e66_n5":
                    SetOnBlackAnimation("MessageC");
                    break;
                case "_6.6e66_n6":
                    SetOnBlackAnimation("nothing");
                    break;
                case "_6.6e66_n7":
                    SetOnBlackAnimation("He");
                    break;
                case "_end_n1":
                    SetOnBlackAnimation("nothing");
                    break;
                case "_6.6e66_i2":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("nothing");
                    break;
                case "_6.6e66_i3":
                    SetOnBlackAnimation("He");
                    break;
                case "_6.6e66_i4":
                    BlackScreen.Visibility = Visibility.Hidden;
                    Save.save.OpenedUpgrades["Все счастливы"] = UpgradeMode.Opened;
                    ReloadCurrentUpgradePage();
                    break;
                case "_you_cant":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("YouCant");
                    break;
                case "_end_i1":
                    SetOnBlackAnimation("nothing");
                    break;
                case "_end_i2":
                    SetOnBlackAnimation("He");
                    break;
                case "_end_i3":
                    SetOnBlackAnimation("SheI");
                    break;
                case "_he_notice2":
                    BlackScreen.Visibility = Visibility.Visible;
                    SetOnBlackAnimation("He");
                    break;
            }

            dialogThread.Start();
            dialogThread.IsBackground = true;
        }

        Thread? gameClosingThread;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Room256.amHandler != null && Room256.amHandler.AmalgamPlace != 0)
            {
                Save.save.RanAway = true;
                Save.DoSave(LOG);
            }
            Environment.Exit(Environment.ExitCode);
        }

        private void SetOnBlackAnimation(string bgName)
        {
            OnBlack1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}1.png"));
            OnBlack2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}2.png"));
            OnBlack3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{bgName}3.png"));
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

        public void SetHallAnimation(int level)
        {
            H1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Hall{level}1.png"));
            H2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Hall{level}2.png"));
            H3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Hall{level}3.png"));
        }

        private void DoBGAnimation()
        {
            int i = 0;

            while (doBGAnimation)
            {
                i++;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    DoTextAnimation(DTGain, i, 25, -70);
                    H1.Visibility = OnBlack1.Visibility = CG1.Visibility = BG1.Visibility = Visibility.Hidden;
                    H2.Visibility = OnBlack2.Visibility = CG2.Visibility = BG2.Visibility = Visibility.Hidden;
                    H3.Visibility = OnBlack3.Visibility = CG3.Visibility = BG3.Visibility = Visibility.Hidden;
                    if (i == 0)
                        H1.Visibility = OnBlack1.Visibility = CG1.Visibility = BG1.Visibility = Visibility.Visible;
                    else if (i == 1)
                        H2.Visibility = OnBlack2.Visibility = CG2.Visibility = BG2.Visibility = Visibility.Visible;
                    else
                    {
                        H3.Visibility = OnBlack3.Visibility = CG3.Visibility = BG3.Visibility = Visibility.Visible;
                        i = -1;
                    }
                });

                Thread.Sleep(bgAnimationSpeed);
            }
        }

        private void DoTextAnimation(Button text, int i, double margin1base, double margin2base)
        {
            if (i == 0) {
                text.Margin = new Thickness(margin1base - 5, margin2base - 3, text.Margin.Right, text.Margin.Bottom);
            }
            else if (i == 1) {
                text.Margin = new Thickness(margin1base + 3, margin2base + 2, text.Margin.Right, text.Margin.Bottom);
            }
            else {
                text.Margin = new Thickness(margin1base, margin2base, text.Margin.Right, text.Margin.Bottom);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Hidden;
            BeginNewDialog("_start", 500);
            SetBGAnimation("Screen");
            Save.DoSave(LOG);
        }

        public double CountDTPerClick()
        {
            Dictionary<string, double> mult = new Dictionary<string, double>()
            {
                { "Пятый корпус", Save.save.MachineCount[0] },
                { "Шестой корпус", Save.save.MachineCount[1] },
                { "Седьмой корпус", Save.save.MachineCount[2] },
                { "Восьмой корпус", Save.save.MachineCount[3] },
                { "[Вечное] Последний корпус", dthandler.DTPSValue },
            };

            double ans = DTperClick;
            foreach (var p in mult)
            {
                if (Save.save.OpenedUpgrades.ContainsKey(p.Key) && Save.save.OpenedUpgrades[p.Key] == UpgradeMode.Bought)
                {
                    ans *= Math.Max(p.Value, 1.0);
                }
            }
            return ans;
        }

        private void DT_Click(object sender, RoutedEventArgs e)
        {
            if (Save.save.End == Endings.Save)
            {
                BeginNewDialog("_i_refuse", 0);
                return;
            }

            Save.save.Clicks += Convert.ToInt32(1 * Math.Max(1.0, EXPBoost));
            Save.save.MaxClicks = Math.Max(Save.save.MaxClicks, Save.save.Clicks);
            dthandler?.ChangeDT(CountDTPerClick());

            if (Save.save.LastDialog == "_start_tutorial1" && Save.save.CurDialog != "_start_tutorial2")
            {
                BeginNewDialog("_start_tutorial2", 10);
            }
        }

        private void Fullscreen()
        {
            if (WindowStyle == WindowStyle.SingleBorderWindow)
            {
                SizeToContent = SizeToContent.Manual;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                SizeToContent = SizeToContent.WidthAndHeight;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                UseMaxBuy = true;
            }
            if (e.Key == Key.F4)
            {
                Fullscreen();
            }
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
            else
            {
                switch (e.Key)
                {
                    case Key.S:
                        Save.DoSave(LOG);
                        break;
                    case Key.Tab:
                        if (Save.save.TutorialPassed && canLookAtRoom && Save.save.RoomButton)
                        {
                            Room256 room256 = new();
                            room256.ShowDialog();
                        }
                        break;
                }
            }
        }

        public void UseUpgrade(Upgrade upgrade)
        {

            for (int i = 0; i < machinesHandler.Machines.Count; ++i)
                machinesHandler.Machines[i].Multipicator *= upgrade.MachineUpgrades[i];
            machinesHandler.ReloadMachines();

            DTperClick *= upgrade.CursorUpgrade;

            for (int i = 0; i < machinesHandler.TimeMachines.Count; ++i)
            {
                machinesHandler.TimeMachines[i].Multipicator *= upgrade.TimeMachineUpgrades[i];
            }
            machinesHandler.ReloadTimeMachines();
            MainWindow.tickhandler?.ReloadCounter();

            EXPBoost = Convert.ToInt32(EXPBoost * upgrade.ExpUpgrade);
            rphandler.RPGainMultiplier *= upgrade.TimePointsIncomeUpgrade;
            tickhandler.TimeSpeed *= upgrade.TimeSpeed;
            PercentDTPower += upgrade.AddPercentOfDT;
            DMG += upgrade.DamageUpgrade;

            AmalgametHandler.AvgSpawnTime = Convert.ToInt32(AmalgametHandler.AvgSpawnTime * upgrade.AmalgametSpawnTimeMult);
            // todo
        }

        int EXPBoost = 1;
        public int DMG = 1;

        public StackPanel ToStackPanel(Upgrade upgrade)
        {
            RoutedEventHandler buyDelegate = delegate (object sender, RoutedEventArgs e)
            {
                if (upgrade.GetIconName() == "goodending")
                {
                    YouCant();
                }
                else
                {
                    if (!DTHandler.IfEnough(upgrade.Price)) return;

                    dthandler?.ChangeDT(-upgrade.Price);

                    Save.save.OpenedUpgrades[upgrade.GetUpgradeName()] = UpgradeMode.Bought;
                    UseUpgrade(upgrade);

                    ReloadCurrentUpgradePage();
                }

                UpgradeBubble.Visibility = Visibility.Hidden;
            };

            StackPanel panel = new();
            Grid grid = new();
            Image image = new();
            Button buybutton = new();
            TextBlock closerbutton = new();
            Button newcloserbutton = new();

            panel.Width = 178.0 * UIScale;
            
            grid.Height = 100.0 * UIScale;
            grid.Width = 100.0 * UIScale;
            image.Stretch = Stretch.Fill;
            image.Source = new BitmapImage(new Uri($"pack://application:,,,/Upgrades/u_{upgrade.GetIconName()}.png"));

            newcloserbutton.Style = (Style)this.TryFindResource("UpgradeClose");
            newcloserbutton.Click += delegate (object sender, RoutedEventArgs e)
            {
                UpgradeBubble.Visibility = Visibility.Visible;
                CurrentUpgradeDesc.Text = upgrade.GetDescription();
                CurrentUpgradeName.Text = upgrade.GetUpgradeName();
                CurrentUpgrade.Source = image.Source;

                if (LastBuyDelegate != null)
                    BuyUpgradeBubble.Click -= LastBuyDelegate;
                BuyUpgradeBubble.Click += buyDelegate;
                BuyUpgradeBubble.Content = $"Купить ({BigNumsConverter.GetInPrettyENotation(upgrade.Price)} реш.)";
                LastBuyDelegate = buyDelegate;
            };

            grid.Children.Add(image);
            grid.Children.Add(newcloserbutton);
            
            buybutton.Style = (Style)this.TryFindResource("UTDTButtonStyle");
            buybutton.FontSize = 30 * UIScale;
            buybutton.Content = BigNumsConverter.GetInPrettyENotation(upgrade.Price, 1000) + " реш.";
            buybutton.Click += buyDelegate;
            
            closerbutton.Style = (Style)this.TryFindResource("VinoDTText");
            closerbutton.FontSize = 30 * UIScale;
            closerbutton.Text = "...";

            panel.Children.Add(closerbutton);
            panel.Children.Add(grid);
            panel.Children.Add(buybutton);

            return panel;
        }

        public void YouCant()
        {
            BeginNewDialog("_you_cant", 0);
        }
        
        private void BuyMachine1_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(0, UseMaxBuy);
        private void BuyMachine2_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(1, UseMaxBuy);
        private void BuyMachine3_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(2, UseMaxBuy);
        private void BuyMachine4_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyMachine(3, UseMaxBuy);

        private void BuyTimeMachine1_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyTimeMachine(0, UseMaxBuy);
        private void BuyTimeMachine2_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyTimeMachine(1, UseMaxBuy);
        private void BuyTimeMachine3_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyTimeMachine(2, UseMaxBuy);
        private void BuyTimeMachine4_Click(object sender, RoutedEventArgs e) => machinesHandler.BuyTimeMachine(3, UseMaxBuy);

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
            UpgradesGrid.Visibility = TimeMachineGrid.Visibility = ResetGrid.Visibility = Visibility.Hidden;
            MachineGrid.Visibility = Visibility.Visible;
        }

        private void UpgradesMenu_Click(object sender, RoutedEventArgs e)
        {
            MachineGrid.Visibility = TimeMachineGrid.Visibility = ResetGrid.Visibility = Visibility.Hidden;
            UpgradesGrid.Visibility = Visibility.Visible;
            ReloadCurrentUpgradePage();
        }

        private void TimeMenu_Click(object sender, RoutedEventArgs e)
        {
            MachineGrid.Visibility = UpgradesGrid.Visibility = ResetGrid.Visibility = Visibility.Hidden;
            TimeMachineGrid.Visibility = Visibility.Visible;
        }

        private void ResetMenu_Click(object sender, RoutedEventArgs e)
        {
            MachineGrid.Visibility = UpgradesGrid.Visibility = TimeMachineGrid.Visibility = Visibility.Hidden;
            ResetGrid.Visibility = Visibility.Visible;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Save.save.RP += rphandler?.RPGainOnReset ?? 0.0;
            Save.save.ResetWas = true;
            Save.save.MachineCount = new int[4] { 0, 0, 0, 0 };
            Save.save.MachineOpened = new bool[4] { true, false, false, false };
            Save.save.TimeMachinePowers = new double[4] { 0, 0, 0, 0 };

            for (int i = 0; i < Save.save.TimeMachinePowers.Length; ++i)
            {
                Save.save.TimeMachinePowers[i] = Save.save.TimeMachineCount[i];
                machinesHandler.TimeMachines[i].Multipicator = 1.0;
            }

            for (int i = 0; i < machinesHandler.Machines.Count; ++i)
                machinesHandler.Machines[i].Multipicator = 1.0;
            
            DTperClick = 1.0;

            Save.save.OpenedUpgrades = Save.save.OpenedUpgrades
                .Where(p => p.Key.StartsWith("[Вечное]"))
                .ToDictionary(x => x.Key, x => x.Value);

            Save.save.DT = 0.0;
            Save.save.TotalDT = 0.0;
            Save.save.Resets++;
            EXPBoost = 1;
            rphandler.RPGainMultiplier = 1.0;
            tickhandler.TimeSpeed = 1.0;
            PercentDTPower = 0;
            AmalgametHandler.AvgSpawnTime = 5 * 60 * 1000;
            DMG = 1;
            Save.save.Clicks = 0;

            Save.save.TryAddPages(new() { 8, 9 });

            dthandler?.Reset();

            if (Save.save.OnLimit)
            {
                switch (Save.save.LimitLvl)
                {
                    case 1:
                        BeginNewDialog("_reset_black", 0);
                        break;
                    case 2:
                        BeginNewDialog("_time1", 0);
                        break;
                    case 3:
                        BeginNewDialog("_dream_start2", 0);
                        break;
                    case 4:
                        BeginNewDialog("_time2", 0);
                        break;
                    case 5:
                        if (Save.save.End == Endings.Save)
                            BeginNewDialog("_dream_start3s", 0);
                        if (Save.save.End == Endings.Insane)
                            BeginNewDialog("_dream_start3i", 0);
                        if (Save.save.End == Endings.Neutral)
                            BeginNewDialog("_dream_start3n", 0);
                        break;
                    case 6:
                        if (Save.save.End == Endings.Save)
                            BeginNewDialog("_dream_start4s", 0);
                        if (Save.save.End == Endings.Insane)
                            BeginNewDialog("_dream_start4i", 0);
                        if (Save.save.End == Endings.Neutral)
                            BeginNewDialog("_dream_start4n", 0);
                        break;
                }
                Save.save.OnLimit = false;
            }

            MachineMenu_Click(new object(), new RoutedEventArgs());
            SetStartScreen();
        }

        private void Room_Click(object sender, RoutedEventArgs e)
        {
            if (Save.save.TutorialPassed && canLookAtRoom && Save.save.RoomButton)
            {
                Room256 room256 = new();
                room256.ShowDialog();
            }
        }

        private void Stat_Click(object sender, RoutedEventArgs e)
        {
            if (dialogClass.InDialog) return;
            try
            {
                StreamWriter sw = new("stat.txt");
                sw.WriteLine("+-----------------------+");
                sw.WriteLine("| Санс                  |");
                sw.WriteLine("+-----------------------+");
                sw.WriteLine("| УР : {0,-6}           |", GetLvlByExp(Save.save.Clicks));
                sw.WriteLine("| ОЗ : {0,-6}           |", 1);
                sw.WriteLine("| СИЛ: {0,-6}           |", (Save.save.Strengh > 999999) ? Double.PositiveInfinity : DMG);
                sw.WriteLine("| ОП : {0,-6}           |", (Save.save.Clicks > 999999) ? Double.PositiveInfinity : Save.save.Clicks);
                sw.WriteLine("| РЕШ: {0,-7}          |", BigNumsConverter.GetInPrettyENotation(Save.save.TotalDT));
                sw.WriteLine("| РЗК: {0,-7}          |", BigNumsConverter.GetInPrettyENotation(CountDTPerClick()));
                sw.WriteLine("| ОС : {0,-7}          |", BigNumsConverter.GetInPrettyENotation(Save.save.RP));
                sw.WriteLine("+-----------------------+");
                sw.Close();

                BeginNewDialog("_stat_use", 0);
            }
            catch
            {
                BeginNewDialog("_stat_use_error", 0);
            }
        }

        public int GetLvlByExp(int exp)
        {
            List<int> tolvls = new List<int>()
            { 10, 30, 70, 120, 200, 300, 500, 800, 1200, 1700, 2500, 3500, 5000, 7000, 10000, 15000, 25000, 50000, 99999 };
            return tolvls.Where(n => n <= exp).Count() + 1;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                UseMaxBuy = false;
            }
        }

        private void DTGain_MouseEnter(object sender, MouseEventArgs e)
        {
            DTButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/DT2.png"));
        }

        private void DTGain_MouseLeave(object sender, MouseEventArgs e)
        {
            DTButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/DT1.png"));
        }

        private void DTGain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DTButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/DT3.png"));
        }

        private void DTGain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DTButtonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/DT2.png"));
        }

        private void SetTextScale(double scale)
        {
            scale -= 0.15;

            const double DTDialogBoxTextSize = 35;
            const double RPcounterSize = 26;
            const double DTcounterSize = 45;
            const double TpscounterSize = 26;
            const double TickcounterSize = 17;

            const double GoBackButtonSize = 50;
            const double GoNextButtonSize = 50;
            const double UIlessSize = 20;
            const double UIbiggerSize = 20;
            const double FullscreenButtonSize = 20;
            const double ScalePercentSize = 25;

            const double ResetMenuSize = 50;
            const double RoomSize = 50;
            const double StatSize = 50;
            const double SettingsButtonSize = 50;
            const double MachineMenuSize = 50;
            const double UpgradesMenuSize = 50;
            const double TimeMachineMenuSize = 50;

            const double SettingsTitleSize = 50;
            const double UISizeTextSize = 30;
            const double OnFullscreenTextSize = 30;
            const double CloseSettingsButtonSize = 45;
            const double CurrentUpgradeNameSize = 40;
            const double CurrentUpgradeDescSize = 30;
            const double CloseUpgradeBubbleSize = 35;
            const double BuyUpgradeBubbleSize = 35;
            const double LOGSize = 30;

            DTDialogBoxText.FontSize = scale * DTDialogBoxTextSize;
            RPcounter.FontSize = scale * RPcounterSize;
            DTcounter.FontSize = scale * DTcounterSize;
            DTpscounter.FontSize = scale * TpscounterSize;
            Tickcounter.FontSize = scale * TickcounterSize;

            GoBackButton.FontSize = scale * GoBackButtonSize;
            GoNextButton.FontSize = scale * GoNextButtonSize;
            UIless.FontSize = scale * UIlessSize;
            UIbigger.FontSize = scale * UIbiggerSize;
            FullscreenButton.FontSize = scale * FullscreenButtonSize;
            ScalePercent.FontSize = scale * ScalePercentSize;

            ResetMenu.FontSize = scale * ResetMenuSize;
            Room.FontSize = scale * RoomSize;
            Stat.FontSize = scale * StatSize;
            SettingsButton.FontSize = scale * SettingsButtonSize;
            MachineMenu.FontSize = scale * MachineMenuSize;
            UpgradesMenu.FontSize = scale * UpgradesMenuSize;
            TimeMachineMenu.FontSize = scale * TimeMachineMenuSize;

            InfoButton.FontSize = SettingsTitle.FontSize = scale * SettingsTitleSize;
            UISizeText.FontSize = scale * UISizeTextSize;
            OnFullscreenText.FontSize = scale * OnFullscreenTextSize;
            CloseSettingsButton.FontSize = scale * CloseSettingsButtonSize;
            CurrentUpgradeName.FontSize = scale * CurrentUpgradeNameSize;
            CurrentUpgradeDesc.FontSize = scale * CurrentUpgradeDescSize;
            CloseUpgradeBubble.FontSize = scale * CloseUpgradeBubbleSize;
            BuyUpgradeBubble.FontSize = scale * BuyUpgradeBubbleSize;
            LOG.FontSize = scale * LOGSize;

            const double Machine1Size = 50;
            const double Machine2Size = 25;
            const double Machine3Size = 40;
            const double Machine4Size = 30;

            const double TimeMachine1Size = 45;
            const double TimeMachine2Size = 25;
            const double TimeMachine3Size = 40;
            const double TimeMachine4Size = 30;

            Machine1Name.FontSize = Machine2Name.FontSize 
                = Machine3Name.FontSize = Machine4Name.FontSize = Machine1Size * scale;
            Machine1Count.FontSize = Machine2Count.FontSize 
                = Machine3Count.FontSize = Machine4Count.FontSize = Machine2Size * scale;
            BuyMachine1.FontSize = BuyMachine2.FontSize 
                = BuyMachine3.FontSize = BuyMachine4.FontSize = Machine3Size * scale;
            Machine1Power.FontSize = Machine2Power.FontSize 
                = Machine3Power.FontSize = Machine4Power.FontSize = Machine4Size * scale;

            TimeMachine1Name.FontSize = TimeMachine2Name.FontSize
                = TimeMachine3Name.FontSize = TimeMachine4Name.FontSize = TimeMachine1Size * scale;
            TimeMachine1Count.FontSize = TimeMachine2Count.FontSize 
                = TimeMachine3Count.FontSize = TimeMachine4Count.FontSize = TimeMachine2Size * scale;
            BuyTimeMachine1.FontSize = BuyTimeMachine2.FontSize 
                = BuyTimeMachine3.FontSize = BuyTimeMachine4.FontSize = TimeMachine3Size * scale;
            TimeMachine1Power.FontSize = TimeMachine2Power.FontSize 
                = TimeMachine3Power.FontSize = TimeMachine4Power.FontSize = TimeMachine4Size * scale;

            const double PageButtonFont = 35;
            const double LabelPageFont = 39;
            const double TextPageFont = 28;
            const double PageNumFont = 35;
            const double CloseManualButtonFont = 35;

            NextPageButton.FontSize = PrevPageButton.FontSize = PageButtonFont * scale;
            LabelPage1.FontSize = LabelPage2.FontSize = LabelPageFont * scale;
            PageNum1.FontSize = PageNum2.FontSize = PageNumFont * scale;
            TextPage1.FontSize = TextPage2.FontSize = TextPageFont * scale;
            CloseManualButton.FontSize = CloseManualButtonFont * scale;

            const double SaveSettingsTextFont = 30;
            const double SaveButtonFont = 25;
            const double DeleteSaveButtonFont = 25;
            const double ExitGameButtonFont = 30;

            SaveSettingsText.FontSize = SaveSettingsTextFont * scale;
            SaveButton.FontSize = SaveButtonFont * scale;
            DeleteSaveButton.FontSize = DeleteSaveButtonFont * scale;
            ExitGameButton.FontSize = ExitGameButtonFont * scale;
        }

        private void SetMarginScale(FrameworkElement obj, Thickness baseMargin, double scale)
        {
            Thickness newMargin = new(baseMargin.Left * scale, baseMargin.Top * scale, 
                baseMargin.Right * scale, baseMargin.Bottom * scale);
            obj.Margin = newMargin;
        }

        private void SetUIScale(double scale)
        {
            Save.save.UIScale = scale;

            SetTextScale(scale);
            ScalePercent.Text = Convert.ToInt32(scale * 100.0).ToString() + "%";

            //const double ReallyMainGridHeight = 750;
            const double ReallyMainGridWidth = 1000;

            const double UpgradeBubbleHeight = 300;
            const double UpgradeBubbleWidth = 600;

            const double UpgradeBubbleStackSize = 140;
            const double CurrentUpgradeNameWidth = 410;

            const double SettingsBubbleHeight = 530;
            const double SettingsBubbleWidth = 600;
            const double DTDialogBoxHeight = 210;
            const double DTDialogBoxWidth = 750;
            const double ResetButtonBackHeight = 150;
            const double ResetButtonBackWidth = 300;

            const double DTButtonHeight = 110;
            const double DTButtonWidth = 170;

            //ReallyMainGrid.Height = ReallyMainGridHeight * scale;
            ReallyMainGrid.Width = ReallyMainGridWidth * scale;
            UpgradeBubble.Height = UpgradeBubbleHeight * scale;
            UpgradeBubble.Width = UpgradeBubbleWidth * scale;
            UpgradeBubbleStack.Height = UpgradeBubbleStack.Width = UpgradeBubbleStackSize * scale;
            CurrentUpgradeName.Width = CurrentUpgradeNameWidth * scale;
            SettingsBubble.Height = SettingsBubbleHeight * scale;
            SettingsBubble.Width = SettingsBubbleWidth * scale;
            DTDialogBox.Height = DTDialogBoxHeight * scale;
            DTDialogBox.Width = DTDialogBoxWidth * scale;
            ResetButtonBack.Height = ResetButtonBackHeight * scale;
            ResetButtonBack.Width = ResetButtonBackWidth * scale;
            
            DTGain.Height = DTButtonHeight * scale;
            DTGain.Width = DTButtonWidth * scale;

            const double TimeGridScalableWidth = 1000;

            TimeGridScalable.Width = TimeGridScalableWidth * scale;

            const double ManualPagesWidth = 800;
            const double ManualPagesHeight = 500;

            const double PageGridWidth = 360;
            const double PageGridHeight = 480;

            ManualPages.Width = ManualPagesWidth * scale;
            ManualPages.Height = ManualPagesHeight * scale;

            LeftPageGrid.Width = RightPageGrid.Width = PageGridWidth * scale;
            LeftPageGrid.Height = RightPageGrid.Height = PageGridHeight * scale;
            SetMarginScale(LeftPageGrid, new(10, 0, 0, 0), scale);
            SetMarginScale(RightPageGrid, new(0, 0, 10, 0), scale);
            SetMarginScale(UpgradesStack, new(50, 0, 50, 0), scale);
            

            ReloadCurrentUpgradePage();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsBubble.Visibility = Visibility.Visible;
            SetUIScale(UIScale);
            DarkerScreen.Visibility = Visibility.Visible;
        }

        private void ShowManualPage(int num)
        {
            if (manualPages.Count == 0) return;

            if (num < 0) currentManualPage = 0;
            else if (num >= manualPages.Count) currentManualPage = manualPages.Count - 2;
            else currentManualPage = num;

            PageNum1.Text = (currentManualPage + 1).ToString();
            LabelPage1.Text = manualPages[currentManualPage].Label;
            TextPage1.Text = manualPages[currentManualPage].Text;
            
            PageNum2.Text = (currentManualPage + 2).ToString();
            LabelPage2.Text = manualPages[currentManualPage + 1].Label;
            TextPage2.Text = manualPages[currentManualPage + 1].Text;

            Save.save.ManualPagesReadedMax = Math.Max(Save.save.ManualPagesReadedMax, currentManualPage + 1);
        }

        private void UIless_Click(object sender, RoutedEventArgs e)
        {
            if (UIScale > 0.51) UIScale -= 0.05;
            SetUIScale(UIScale);
        }

        private void UIbigger_Click(object sender, RoutedEventArgs e)
        {
            if (UIScale < 1.99) UIScale += 0.05;
            SetUIScale(UIScale);
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsBubble.Visibility = Visibility.Hidden;
            DarkerScreen.Visibility = Visibility.Hidden;
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            Fullscreen();
            if (FullscreenButton.Content.ToString() == "выкл")
                FullscreenButton.Content = "вкл";
            else
                FullscreenButton.Content = "выкл";
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            ManualGrid.Visibility = Visibility.Visible;
            ShowManualPage(currentManualPage);
            DarkerScreen.Visibility = Visibility.Visible;
        }

        private void CloseManualButton_Click(object sender, RoutedEventArgs e)
        {
            ManualGrid.Visibility = Visibility.Hidden;
            DarkerScreen.Visibility = Visibility.Hidden;
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowManualPage(currentManualPage - 2);
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowManualPage(currentManualPage + 2);
        }

        private bool confirmClosing = false;
        public void Close(bool confirm)
        {
            confirmClosing = confirm;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!confirmClosing)
                e.Cancel = false;
            else
            {
                var ans = MessageBox.Show("Всё закончится так?", "????", MessageBoxButton.YesNo);
                if (ans == MessageBoxResult.No)
                    e.Cancel = true;
                else
                    File.Delete("Saves/s0.txt");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save.DoSave(LOG);
        }

        private void DeleteSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var ans = MessageBox.Show("Удалить сохранение?", "????", MessageBoxButton.YesNo);
            if (ans == MessageBoxResult.Yes)
            {
                File.Delete("Saves/s0.txt");
                Close(false);
            }
        }

        private void ExitGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!confirmClosing)
                Save.DoSave(LOG);
            Close(confirmClosing);
        }
    }
}
