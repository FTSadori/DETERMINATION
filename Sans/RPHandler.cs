using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sans
{
    public class DTHandler
    {
        private TextBlock DTCounter;
        private TextBlock DTPSCounter;

        private Thread? DTPSThread;
        private Thread? DTNumberChecker;
        private Thread? ClickNumberChecker;
        private Thread? MachineNumberChecker;

        public double DTPSValue = 0.0;
        public bool DoDTPS = true;
        public bool DoCheck = true;

        public bool IsStopped = false;



        public void Reset()
        {
            ResetVal = new bool[4] { true, true, true, true };

            Dictionary<double, string> TimeSpaceUpgradesEvent = new()
            {
                { 2, "[Вечное] Перемещение во времени и пространстве 1" },
                { 3, "[Вечное] Перемещение во времени и пространстве 2" },
                { 4, "[Вечное] Перемещение во времени и пространстве 3" },
                { 5, "[Вечное] Перемещение во времени и пространстве 4" },
                { 7, "[Вечное] Перемещение во времени и пространстве 5" },
                { 10, "[Вечное] Перемещение во времени и пространстве 6" },
            };

            foreach (var pair in TimeSpaceUpgradesEvent)
            {
                if (Save.save.Resets >= pair.Key && 
                    !Save.save.OpenedUpgrades.ContainsKey(pair.Value))
                {
                    Save.save.OpenedUpgrades[pair.Value] = UpgradeMode.Opened;
                    MainWindow.This?.ReloadCurrentUpgradePage();
                }
            }

            IsStopped = false;
        }

        public bool[] ResetVal = new bool[4] { false, false, false, false };

        public DTHandler(TextBlock dtcounter, TextBlock dtpscounter)
        {
            DTCounter = dtcounter;
            DTPSCounter = dtpscounter;

            DTPSThread = new Thread(AddDTPS);
            DTPSThread.Start();
            DTPSThread.IsBackground = true;

            DTNumberChecker = new Thread(CheckDTNumber);
            DTNumberChecker.Start();
            DTNumberChecker.IsBackground = true;

            ClickNumberChecker = new Thread(CheckClickNumber);
            ClickNumberChecker.Start();
            ClickNumberChecker.IsBackground = true;

            MachineNumberChecker = new Thread(CheckMachineNumber);
            MachineNumberChecker.Start();
            MachineNumberChecker.IsBackground = true;
        }

        private void CheckMachineNumber()
        {
            while (DoCheck)
            {
                List<Dictionary<string, double>> OpenMachineUpgradesEvent = new()
                {
                    new()
                    {
                        { "Улучшенный I корпус", 6 },
                        { "Стальной I корпус", 16 },
                        { "Мощный I корпус", 26 },
                        { "Титаниевый I корпус", 56 },
                        { "Прочный I корпус", 86 },
                        { "Переработанный I корпус", 136 },
                        { "Межвременной I корпус 1", 206 },
                        { "Межвременной I корпус 2", 286 },
                        { "Межвременной I корпус 3", 406 }
                    },
                    new()
                    {
                        { "Улучшенный II корпус", 6 },
                        { "Стальной II корпус", 16 },
                        { "Мощный II корпус", 26 },
                        { "Титаниевый II корпус", 56 },
                        { "Прочный II корпус", 86 },
                        { "Переработанный II корпус", 116 },
                        { "Межвременной II корпус 1", 166 },
                        { "Межвременной II корпус 2", 206 },
                        { "Межвременной II корпус 3", 256 }
                    },
                    new()
                    {
                        { "Улучшенный III корпус", 6 },
                        { "Стальной III корпус", 16 },
                        { "Мощный III корпус", 26 },
                        { "Титаниевый III корпус", 56 },
                        { "Прочный III корпус", 76 },
                        { "Переработанный III корпус", 96 },
                        { "Межвременной III корпус 1", 116 },
                        { "Межвременной III корпус 2", 136 },
                        { "Межвременной III корпус 3", 166 }
                    },
                    new()
                    {
                        { "Улучшенный IV корпус", 6 },
                        { "Стальной IV корпус", 16 },
                        { "Мощный IV корпус", 26 },
                        { "Титаниевый IV корпус", 36 },
                        { "Прочный IV корпус", 46 },
                        { "Переработанный IV корпус", 56 },
                        { "Межвременной IV корпус 1", 66 },
                        { "Межвременной IV корпус 2", 86 },
                        { "Межвременной IV корпус 3", 106 }
                    }
                };
                
                List<Dictionary<string, double>> OpenTimeMachineUpgradesEvent = new()
                {
                    new()
                    {
                        { "[Вечное] Улучшенный I корпус времени 1", 1 },
                        { "[Вечное] Улучшенный I корпус времени 2", 6 },
                        { "[Вечное] Улучшенный I корпус времени 3", 12 },
                        { "[Вечное] Улучшенный I корпус времени 4", 18 },
                        { "[Вечное] Улучшенный I корпус времени 5", 24 },
                        { "[Вечное] Улучшенный I корпус времени 6", 30 },
                    },
                    new()
                    {
                        { "[Вечное] Улучшенный II корпус времени 1", 1 },
                        { "[Вечное] Улучшенный II корпус времени 2", 4 },
                        { "[Вечное] Улучшенный II корпус времени 3", 7 },
                        { "[Вечное] Улучшенный II корпус времени 4", 10 },
                        { "[Вечное] Улучшенный II корпус времени 5", 13 },
                        { "[Вечное] Улучшенный II корпус времени 6", 16 },
                    },
                    new()
                    {
                        { "[Вечное] Улучшенный III корпус времени 1", 1 },
                        { "[Вечное] Улучшенный III корпус времени 2", 2 },
                        { "[Вечное] Улучшенный III корпус времени 3", 4 },
                        { "[Вечное] Улучшенный III корпус времени 4", 6 },
                        { "[Вечное] Улучшенный III корпус времени 5", 8 },
                        { "[Вечное] Улучшенный III корпус времени 6", 10 },
                    },
                    new()
                    {
                        { "[Вечное] Улучшенный IV корпус времени 1", 1 },
                        { "[Вечное] Улучшенный IV корпус времени 2", 2 },
                        { "[Вечное] Улучшенный IV корпус времени 3", 3 },
                        { "[Вечное] Улучшенный IV корпус времени 4", 4 },
                        { "[Вечное] Улучшенный IV корпус времени 5", 5 },
                        { "[Вечное] Улучшенный IV корпус времени 6", 6 },
                    }
                };
                
                while (true)
                {
                    if (ResetVal[0]) { Thread.Sleep(220); ResetVal[0] = false; break; }
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        for (int i = 0; i < OpenMachineUpgradesEvent.Count; ++i)
                        {
                            OpenMachineUpgradesEvent[i].Where(p => p.Value <= Save.save.MachineCount[i]).ToList()
                            .ForEach(p =>
                            {
                                if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                {
                                    Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                    MainWindow.This?.ReloadCurrentUpgradePage();

                                    if (p.Key == "Межвременной I корпус 3")
                                    { MainWindow.This.Machine1Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[0] = true; }
                                    if (p.Key == "Межвременной II корпус 3")
                                    { MainWindow.This.Machine2Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[1] = true; }
                                    if (p.Key == "Межвременной III корпус 3")
                                    { MainWindow.This.Machine3Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[2] = true; }
                                    if (p.Key == "Межвременной IV корпус 3")
                                    { MainWindow.This.Machine4Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[3] = true; }
                                }
                            });
                        }
                        for (int i = 0; i < OpenTimeMachineUpgradesEvent.Count; ++i)
                        {
                            OpenTimeMachineUpgradesEvent[i].Where(p => p.Value <= Save.save.TimeMachineCount[i]).ToList()
                            .ForEach(p =>
                            {
                                if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                {
                                    Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                    MainWindow.This?.ReloadCurrentUpgradePage();

                                    if (p.Key == "[Вечное] Улучшенный I корпус времени 6")
                                    { MainWindow.This.TimeMachine1Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[0] = true; }
                                    if (p.Key == "[Вечное] Улучшенный II корпус времени 6")
                                    { MainWindow.This.TimeMachine2Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[1] = true; }
                                    if (p.Key == "[Вечное] Улучшенный III корпус времени 6")
                                    { MainWindow.This.TimeMachine3Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[2] = true; }
                                    if (p.Key == "[Вечное] Улучшенный IV корпус времени 6")
                                    { MainWindow.This.TimeMachine4Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[3] = true; }
                                }
                            });
                        }
                    });
                    Thread.Sleep(1000);
                }
            }
        }

        private void CheckClickNumber()
        {
            while (DoCheck)
            {
                Dictionary<string, double> OpenUpgradesEvent = new() { 
                    { "Вторая игла", 10 }, //2
                    { "Четвертая игла", 30 }, //3
                    { "Повышенное давление", 70 }, //4
                    { "Восьмая игла", 120 }, //5
                    { "Повышенное давление 2", 200 }, //6
                    { "16 игл", 300 }, //7
                    { "32 иглы", 500 }, //8
                    { "Повышенное давление 3", 800 }, //9
                    { "Пятый корпус", 1200 }, //10
                    { "64 иглы", 1700 }, //11
                    { "128 иглы", 2500 }, //12
                    { "Шестой корпус", 3500 }, //13
                    { "Повышенное давление 4", 5000 }, //14
                    { "256 иглы", 7000 }, //15
                    { "512 иглы", 10000 }, //16
                    { "Седьмой корпус", 15000 }, //17
                    { "Тысяча игл", 25000 }, //18
                    { "Повышенное давление 5", 50000 }, //19
                    { "Восьмой корпус", 99999 }, //20
                };

                while (true)
                {
                    if (ResetVal[1]) { ResetVal[1] = false; Thread.Sleep(200); break; }
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        if (Save.save.End == Endings.Insane && Save.save.InsaneLevel == 1)
                        {
                            if (Math.Max(Save.save.TotalDT, Save.save.DT) >= 6.6e25 && Save.save.MaxClicks < 3500)
                                Save.save.End = Endings.Neutral;
                            else if (Save.save.MaxClicks >= 3500)
                            {
                                Save.save.InsaneLevel = 2;
                                MainWindow.This.SetHallAnimation(1);
                            }
                        }
                        else if (Save.save.End == Endings.Insane && Save.save.InsaneLevel == 2)
                        {
                            if (Math.Max(Save.save.TotalDT, Save.save.DT) >= 6.6e35 && Save.save.MaxClicks < 15000)
                                Save.save.End = Endings.Neutral;
                            else if (Save.save.MaxClicks >= 15000)
                            {
                                Save.save.InsaneLevel = 3;
                                MainWindow.This.SetHallAnimation(2);
                            }
                        }
                        else if (Save.save.End == Endings.Insane && Save.save.InsaneLevel == 3)
                        {
                            if (Math.Max(Save.save.TotalDT, Save.save.DT) >= 6.6e45 && Save.save.MaxClicks < 99999)
                                Save.save.End = Endings.Neutral;
                            else if (Save.save.MaxClicks >= 99999)
                            {
                                Save.save.OpenedUpgrades["[Вечное] Последний корпус"] = UpgradeMode.Opened;
                                Save.save.TryAddPages(new List<int>() { 18, 19 });
                                MainWindow.This.SetHallAnimation(3);
                                Save.save.InsaneLevel = 4;
                            }
                        }

                        if (Save.save.End == Endings.Pre)
                        {
                            if (Math.Max(Save.save.TotalDT, Save.save.DT) <= 6.6e9 && Save.save.MaxClicks > 1200)
                            {
                                Save.save.End = Endings.Insane;
                                Save.save.InsaneLevel = 1;
                                MainWindow.This?.ReloadCurrentUpgradePage();
                            }
                            else if (Math.Max(Save.save.TotalDT, Save.save.DT) >= 1e25 && Save.save.MaxClicks == 0)
                                Save.save.End = Endings.Save;
                            else if (Math.Max(Save.save.TotalDT, Save.save.DT) >= 1e25 && Save.save.MaxClicks > 0 && Save.save.MaxClicks < 99999)
                                Save.save.End = Endings.Neutral;
                        }

                        OpenUpgradesEvent.Where(p => p.Value <= Save.save.MaxClicks).ToList()
                            .ForEach(p =>
                            {
                                if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                {
                                    if (p.Key == "Пятый корпус")
                                    {
                                        Save.save.TryAddPages(new List<int>() { 14, 15 });
                                    }
                                    Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                    MainWindow.This?.ReloadCurrentUpgradePage();
                                }
                            });
                    });
                    Thread.Sleep(1000);
                }
            }
        }

        private void CheckDTNumber()
        {
            Thread.Sleep(100);
            while (DoCheck)
            {
                List<ConditionDialog> DialogsEvent = new()
                {
                    new ConditionDialog(Condition.Determination, 1e3, "_second_machine", Endings.Any),
                    new ConditionDialog(Condition.Determination, 1e6, "_reset_av", Endings.Any),
                    new ConditionDialog(Condition.AmalgametWins, 1, "_amalgam", Endings.Any),
                    new ConditionDialog(Condition.Determination, 6.7e66, "_6.6e66_n1", Endings.Neutral),
                    new ConditionDialog(Condition.Determination, 6.7e66, "_6.6e66_i1", Endings.Insane),
                    new ConditionDialog(Condition.Determination, 9.9e39, "_im_here", Endings.Save),
                    new ConditionDialog(Condition.Determination, 9.9e49, "_he_notice", Endings.Save),
                };

                Dictionary<double, int> OpenMachinesEvent = new() { { 1e3, 1 }, { 1e7, 2 }, { 1e14, 3 } };
                List<double> TabOpenEvent = new() { 1e6 };
                Dictionary<string, double> OpenUpgradesEventKiller = new()
                {
                    { "Сила 1", 1e30 },
                    { "Сила 2", 1e35 },
                };
                Dictionary<string, double> OpenUpgradesEventSave = new()
                {
                    { "Действие 1", 1e28 },
                    { "Действие 2", 1e34 },
                    { "Действие 3", 1e38 },
                };
                Dictionary<string, double> OpenUpgradesEventNeutral = new()
                {
                    { "[Вечное] Бесконечность 1", 1e25 },
                    { "[Вечное] Бесконечность 2", 1e35 },
                    { "[Вечное] Бесконечность 3", 1e50 },
                };
                Dictionary<string, double> OpenUpgradesEvent = new()
                {
                    { "Повышенная мощность корпусов 1", 256 },
                    { "Повышенная мощность корпусов 2", 2.56e3 },
                    { "Повышенная мощность корпусов 3", 2.56e5 },
                    { "Повышенная мощность корпусов 4", 2.56e8 },
                    { "Повышенная мощность корпусов 5", 2.56e15 },
                    { "Повышенная мощность корпусов 6", 2.56e25 },
                    { "Усмирение 1", 1e7 },
                    { "Усмирение 2", 1e17 },
                    { "Усмирение 3", 1e27 },
                    { "Усмирение 4", 1e37 },
                    { "Усмирение 5", 1e47 },
                    { "[Вечное] Искривление пространства 1", 1e13 },
                    { "[Вечное] Искривление пространства 2", 1e23 },
                    { "[Вечное] Искривление пространства 3", 1e33 },
                    { "[Вечное] Разлом времени 1", 1e33 },
                    { "[Вечное] Разлом времени 2", 1e39 },
                    { "[Вечное] Разлом времени 3", 1e43 },
                };

                while (true)
                {
                    if (ResetVal[2]) { ResetVal[2] = false; Thread.Sleep(200); break; }
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        if (!MainWindow.This.dialogClass.InDialog && !MainWindow.This.dialogClass.Occupied)
                        {
                            var list = DialogsEvent.Where(d => d.CheckCondition() && !Save.save.DialogsWas.Contains(d.DialogId))
                                .ToList();
                            if (list.Count > 0)
                            {
                                var el = list[0];
                                MainWindow.This?.BeginNewDialog(el.DialogId, 0);
                                Save.save.DialogsWas.Add(el.DialogId);
                                DialogsEvent.Remove(el);
                            }
                        }

                        if (Save.save.TotalDT > 10000 && !Save.save.StatButton && !MainWindow.This.dialogClass.InDialog && !MainWindow.This.dialogClass.Occupied)
                        {
                            MainWindow.This?.BeginNewDialog("_stat1", 500);
                            Save.save.StatButton = true;
                            MainWindow.This.Stat.Visibility = Visibility.Visible;
                        }
                        if (Save.save.TotalDT > 100000 && !Save.save.RoomButton && !MainWindow.This.dialogClass.InDialog && !MainWindow.This.dialogClass.Occupied)
                        {
                            MainWindow.This?.BeginNewDialog("_room1", 500);
                            Save.save.RoomButton = true;
                            MainWindow.This.Room.Visibility = Visibility.Visible;
                            Save.save.TryAddPages(new List<int>() { 4, 5 });
                        }
                        OpenMachinesEvent.Where(p => p.Key <= Save.save.TotalDT).ToList()
                            .ForEach(p =>
                            {
                                Save.save.MachineOpened[p.Value] = true;
                                OpenMachinesEvent.Remove(p.Key);
                                MainWindow.This?.ReloadMachineTab();
                            });
                        switch (Save.save.End)
                        {
                            case Endings.Insane:
                                OpenUpgradesEventKiller.Where(p => p.Value <= Save.save.TotalDT).ToList()
                                .ForEach(p =>
                                {
                                    if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                    {
                                        Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                        OpenUpgradesEvent.Remove(p.Key);
                                        MainWindow.This?.ReloadCurrentUpgradePage();
                                    }
                                });
                                break;
                            case Endings.Save:
                                OpenUpgradesEventSave.Where(p => p.Value <= Save.save.TotalDT).ToList()
                                .ForEach(p =>
                                {
                                    if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                    {
                                        Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                        OpenUpgradesEvent.Remove(p.Key);
                                        MainWindow.This?.ReloadCurrentUpgradePage();
                                    }
                                }); break;
                            case Endings.Neutral:
                                OpenUpgradesEventNeutral.Where(p => p.Value <= Save.save.TotalDT).ToList()
                                .ForEach(p =>
                                {
                                    if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                    {
                                        Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                        OpenUpgradesEvent.Remove(p.Key);
                                        MainWindow.This?.ReloadCurrentUpgradePage();
                                        Save.save.TryAddPages(new List<int>() { 16, 17 });
                                    }
                                }); break;
                        }

                        TabOpenEvent.Where(n => n <= Save.save.TotalDT).ToList()
                            .ForEach(n =>
                            {
                                if (Save.save.IsFullscreen)
                                    MainWindow.This.ResetMenu.Width = MainWindow.This.menuTabsSize * Save.save.UIFullscreenScale;
                                else
                                    MainWindow.This.ResetMenu.Width = MainWindow.This.menuTabsSize * Save.save.UIScale;
                                TabOpenEvent.Remove(n);
                                Save.save.TryAddPages(new List<int>() { 6, 7 });
                            });
                        OpenUpgradesEvent.Where(p => p.Value <= Save.save.TotalDT).ToList()
                            .ForEach(p =>
                            {
                                if (!Save.save.OpenedUpgrades.ContainsKey(p.Key))
                                {
                                    Save.save.OpenedUpgrades[p.Key] = UpgradeMode.Opened;
                                    OpenUpgradesEvent.Remove(p.Key);
                                    MainWindow.This?.ReloadCurrentUpgradePage();
                                }
                            });
                    });
                    Thread.Sleep(1000);
                }
            }
        }

        private void AddDTPS()
        {
            const int gap = 100;

            while (DoDTPS)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                if (ResetVal[0])
                {
                    Thread.Sleep(200); 
                    ResetVal[0] = false;
                    Save.save.DT = Save.save.TotalDT = 0.0;
                }
                else ChangeDT(DTPSValue / (1000.0 / gap));
                });
                Thread.Sleep(gap);
            }
        }

        public void ReloadCounter() => ChangeDT(0);

        public void ChangeDT(double num)
        {
            if (Save.save.OnLimit || IsStopped) return;

            Save.save.DT += num;
            Save.save.DT = Math.Max(0, Save.save.DT);
            Save.save.TotalDT += Math.Max(0, num);
            DTCounter.Text = BigNumsConverter.GetInPrettyENotation(Save.save.DT) + " Решимости";
            MainWindow.rphandler?.ReloadPRGainCounter();
        }

        public void ReloadDTPS(List<Machine> machines)
        {
            double dtps = 1.0;
            for (int i = 0; i < machines.Count; ++i)
            {
                dtps *= ((!Save.save.MachineOpened[i]) 
                    ? 1.0
                    : (((i == 0) ? 0.0 : 1.0) + Save.save.MachineCount[i]) * machines[i].Multipicator);
            }
            dtps *= 1.0 + Save.save.TimeMachinePowers[0];

            DTPSCounter.Text = BigNumsConverter.GetInPrettyENotation(dtps) + " Решимости в секунду";
            DTPSValue = dtps;
        }

        static public bool IfEnough(double price) => (Math.Round(Save.save.DT) >= Math.Round(price));
    }
    public class RPHandler
    {
        private TextBlock RPCounter;
        private TextBlock RPGainCounter;

        public double RPGainOnReset = 0.0;
        public double RPGainMultiplier = 1.0;

        public RPHandler(TextBlock rpcounter, TextBlock rpgaincounter)
        {
            RPCounter = rpcounter;
            RPGainCounter = rpgaincounter;
        }

        public void ReloadCounter() => ChangeRP(0);

        public void ChangeRP(double num)
        {
            Save.save.RP += num;
            Save.save.RP = Math.Max(Save.save.RP, 0.0);
            RPCounter.Text = BigNumsConverter.GetInPrettyENotation(Save.save.RP) + " Очков сброса";
        }

        public void ReloadPRGainCounter()
        {
            RPGainCounter.Text = "+ " +
                BigNumsConverter.GetInPrettyENotation(Math.Max(RPGainOnReset = CalculateResetPoints(Save.save.TotalDT + 1), 0.0))
                + " ОС";
        }

        static public bool IfEnough(double price) => (Math.Round(Save.save.RP) >= Math.Round(price));

        public double CalculateResetPoints(double dt) => RPGainMultiplier * Math.Pow(Math.Max(0.0, Math.Log10(dt) - 4.5), 2.0);
    }

    public class TickHandler
    {
        public TextBlock TickCounter;

        private Thread? TickThread;

        public bool DoTickCycle = true;
        public double TimeSpeed = 1.0;
        public TickHandler(TextBlock tickcounter)
        {
            TickCounter = tickcounter;

            TickThread = new Thread(TickCycle);
            TickThread.Start();
            TickThread.IsBackground = true;
        }

        public void TickCycle()
        {
            const int gap = 1000;

            while (DoTickCycle)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    for (int i = Save.save.TimeMachinePowers.Length - 1; i > 0; --i)
                        Save.save.TimeMachinePowers[i - 1] += MainWindow.machinesHandler.TimeMachines[i].Multipicator * Save.save.TimeMachinePowers[i];
                    ReloadCounter();
                    MainWindow.dthandler?.ReloadDTPS(MainWindow.machinesHandler.Machines);
                    MainWindow.machinesHandler.ReloadTimeMachines();
                });
                Thread.Sleep(Convert.ToInt32(gap / TimeSpeed));
            }
        }

        public void ReloadCounter()
        {
            TickCounter.Text = BigNumsConverter.GetInPrettyENotation(1.0 + Save.save.TimeMachinePowers[0] * MainWindow.machinesHandler.TimeMachines[0].Multipicator) + " Тиков в секунду";
        }
    }
}
