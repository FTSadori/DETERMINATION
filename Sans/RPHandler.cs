using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Determination;
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
                { 2, Upgrades.GetName("timespace_1") },
                { 3, Upgrades.GetName("timespace_2") },
                { 4, Upgrades.GetName("timespace_3") },
                { 5, Upgrades.GetName("timespace_4") },
                { 7, Upgrades.GetName("timespace_5") },
                { 10, Upgrades.GetName("timespace_6") },
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
                        { Upgrades.GetName("machine_I1"), 6 },
                        { Upgrades.GetName("machine_I2"), 16 },
                        { Upgrades.GetName("machine_I3"), 26 },
                        { Upgrades.GetName("machine_I4"), 56 },
                        { Upgrades.GetName("machine_I5"), 86 },
                        { Upgrades.GetName("machine_I6"), 136 },
                        { Upgrades.GetName("machine_It1"), 206 },
                        { Upgrades.GetName("machine_It2"), 286 },
                        { Upgrades.GetName("machine_It3"), 406 }
                    },
                    new()
                    {
                        { Upgrades.GetName("machine_II1"), 6 },
                        { Upgrades.GetName("machine_II2"), 16 },
                        { Upgrades.GetName("machine_II3"), 26 },
                        { Upgrades.GetName("machine_II4"), 56 },
                        { Upgrades.GetName("machine_II5"), 86 },
                        { Upgrades.GetName("machine_II6"), 116 },
                        { Upgrades.GetName("machine_IIt1"), 166 },
                        { Upgrades.GetName("machine_IIt2"), 206 },
                        { Upgrades.GetName("machine_IIt3"), 256 }
                    },
                    new()
                    {
                        { Upgrades.GetName("machine_III1"), 6 },
                        { Upgrades.GetName("machine_III2"), 16 },
                        { Upgrades.GetName("machine_III3"), 26 },
                        { Upgrades.GetName("machine_III4"), 56 },
                        { Upgrades.GetName("machine_III5"), 76 },
                        { Upgrades.GetName("machine_III6"), 96 },
                        { Upgrades.GetName("machine_IIIt1"), 116 },
                        { Upgrades.GetName("machine_IIIt2"), 136 },
                        { Upgrades.GetName("machine_IIIt3"), 166 }
                    },
                    new()
                    {
                        { Upgrades.GetName("machine_IV1"), 6 },
                        { Upgrades.GetName("machine_IV2"), 16 },
                        { Upgrades.GetName("machine_IV3"), 26 },
                        { Upgrades.GetName("machine_IV4"), 36 },
                        { Upgrades.GetName("machine_IV5"), 46 },
                        { Upgrades.GetName("machine_IV6"), 56 },
                        { Upgrades.GetName("machine_IVt1"), 66 },
                        { Upgrades.GetName("machine_IVt2"), 86 },
                        { Upgrades.GetName("machine_IVt3"), 106 }
                    }
                };
                
                List<Dictionary<string, double>> OpenTimeMachineUpgradesEvent = new()
                {
                    new()
                    {
                        { Upgrades.GetName("timemachine_I1"), 1 },
                        { Upgrades.GetName("timemachine_I2"), 6 },
                        { Upgrades.GetName("timemachine_I3"), 12 },
                        { Upgrades.GetName("timemachine_I4"), 18 },
                        { Upgrades.GetName("timemachine_I5"), 24 },
                        { Upgrades.GetName("timemachine_I6"), 30 },
                    },
                    new()
                    {
                        { Upgrades.GetName("timemachine_II1"), 1 },
                        { Upgrades.GetName("timemachine_II2"), 4 },
                        { Upgrades.GetName("timemachine_II3"), 7 },
                        { Upgrades.GetName("timemachine_II4"), 10 },
                        { Upgrades.GetName("timemachine_II5"), 13 },
                        { Upgrades.GetName("timemachine_II6"), 16 },
                    },
                    new()
                    {
                        { Upgrades.GetName("timemachine_III1"), 1 },
                        { Upgrades.GetName("timemachine_III2"), 2 },
                        { Upgrades.GetName("timemachine_III3"), 4 },
                        { Upgrades.GetName("timemachine_III4"), 6 },
                        { Upgrades.GetName("timemachine_III5"), 8 },
                        { Upgrades.GetName("timemachine_III6"), 10 },
                    },
                    new()
                    {
                        { Upgrades.GetName("timemachine_IV1"), 1 },
                        { Upgrades.GetName("timemachine_IV2"), 2 },
                        { Upgrades.GetName("timemachine_IV3"), 3 },
                        { Upgrades.GetName("timemachine_IV4"), 4 },
                        { Upgrades.GetName("timemachine_IV5"), 5 },
                        { Upgrades.GetName("timemachine_IV6"), 6 },
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

                                    if (p.Key == Upgrades.GetName("machine_It3"))
                                    { MainWindow.This.Machine1Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[0] = true; }
                                    if (p.Key == Upgrades.GetName("machine_IIt3"))
                                    { MainWindow.This.Machine2Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[1] = true; }
                                    if (p.Key == Upgrades.GetName("machine_IIIt3"))
                                    { MainWindow.This.Machine3Name.Foreground = Brushes.Yellow; Save.save.MachinesFull[2] = true; }
                                    if (p.Key == Upgrades.GetName("machine_IVt3"))
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

                                    if (p.Key == Upgrades.GetName("timemachine_I6"))
                                    { MainWindow.This.TimeMachine1Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[0] = true; }
                                    if (p.Key == Upgrades.GetName("timemachine_II6"))
                                    { MainWindow.This.TimeMachine2Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[1] = true; }
                                    if (p.Key == Upgrades.GetName("timemachine_III6"))
                                    { MainWindow.This.TimeMachine3Name.Foreground = Brushes.DarkOrange; Save.save.TimeMachinesFull[2] = true; }
                                    if (p.Key == Upgrades.GetName("timemachine_IV6"))
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
                    { Upgrades.GetName("needle_1"), 10 }, //2
                    { Upgrades.GetName("needle_2"), 30 }, //3
                    { Upgrades.GetName("press_1"), 70 }, //4
                    { Upgrades.GetName("needle_3"), 120 }, //5
                    { Upgrades.GetName("press_2"), 200 }, //6
                    { Upgrades.GetName("needle_4"), 300 }, //7
                    { Upgrades.GetName("needle_5"), 500 }, //8
                    { Upgrades.GetName("press_3"), 800 }, //9
                    { Upgrades.GetName("machine_5"), 1200 }, //10
                    { Upgrades.GetName("needle_6"), 1700 }, //11
                    { Upgrades.GetName("needle_7"), 2500 }, //12
                    { Upgrades.GetName("machine_6"), 3500 }, //13
                    { Upgrades.GetName("press_4"), 5000 }, //14
                    { Upgrades.GetName("needle_8"), 7000 }, //15
                    { Upgrades.GetName("needle_9"), 10000 }, //16
                    { Upgrades.GetName("machine_7"), 15000 }, //17
                    { Upgrades.GetName("needle_10"), 25000 }, //18
                    { Upgrades.GetName("press_5"), 50000 }, //19
                    { Upgrades.GetName("machine_8"), 99999 }, //20
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
                                Save.save.OpenedUpgrades[Upgrades.GetName("lastmachine")] = UpgradeMode.Opened;
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
                                    if (p.Key == Upgrades.GetName("machine_5"))
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
                    { Upgrades.GetName("knife_1"), 1e30 },
                    { Upgrades.GetName("knife_2"), 1e35 },
                };
                Dictionary<string, double> OpenUpgradesEventSave = new()
                {
                    { Upgrades.GetName("act_1"), 1e28 },
                    { Upgrades.GetName("act_2"), 1e34 },
                    { Upgrades.GetName("act_3"), 1e38 },
                };
                Dictionary<string, double> OpenUpgradesEventNeutral = new()
                {
                    { Upgrades.GetName("infinity_1"), 1e25 },
                    { Upgrades.GetName("infinity_2"), 1e35 },
                    { Upgrades.GetName("infinity_3"), 1e50 },
                };
                Dictionary<string, double> OpenUpgradesEvent = new()
                {
                    { Upgrades.GetName("machine_A1"), 256 },
                    { Upgrades.GetName("machine_A2"), 2.56e3 },
                    { Upgrades.GetName("machine_A3"), 2.56e5 },
                    { Upgrades.GetName("machine_A4"), 2.56e8 },
                    { Upgrades.GetName("machine_A5"), 2.56e15 },
                    { Upgrades.GetName("machine_A6"), 2.56e25 },
                    { Upgrades.GetName("amalg_1"), 1e7 },
                    { Upgrades.GetName("amalg_2"), 1e17 },
                    { Upgrades.GetName("amalg_3"), 1e27 },
                    { Upgrades.GetName("amalg_4"), 1e37 },
                    { Upgrades.GetName("amalg_5"), 1e47 },
                    { Upgrades.GetName("machine_A1E"), 1e13 },
                    { Upgrades.GetName("machine_A2E"), 1e23 },
                    { Upgrades.GetName("machine_A3E"), 1e33 },
                    { Upgrades.GetName("breakingtime_1"), 1e33 },
                    { Upgrades.GetName("breakingtime_2"), 1e39 },
                    { Upgrades.GetName("breakingtime_3"), 1e43 },
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
            DTCounter.Text = BigNumsConverter.GetInPrettyENotation(Save.save.DT) + $" {Translator.Dictionary[TWord.C_Determination]}";
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

            DTPSCounter.Text = BigNumsConverter.GetInPrettyENotation(dtps) + $" {Translator.Dictionary[TWord.C_DTPerSecond]}";
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
            RPCounter.Text = BigNumsConverter.GetInPrettyENotation(Save.save.RP) + $" {Translator.Dictionary[TWord.C_ResetPoints]}";
        }

        public void ReloadPRGainCounter()
        {
            RPGainCounter.Text = "+ " +
                BigNumsConverter.GetInPrettyENotation(Math.Max(RPGainOnReset = CalculateResetPoints(Save.save.TotalDT + 1), 0.0))
                + $" {Translator.Dictionary[TWord.RPPrice]}";
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
            TickCounter.Text = BigNumsConverter.GetInPrettyENotation(1.0 + Save.save.TimeMachinePowers[0] * MainWindow.machinesHandler.TimeMachines[0].Multipicator) + $" {Translator.Dictionary[TWord.C_TicksPerSecond]}";
        }
    }
}
