using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using Determination;

namespace Sans
{
    public enum UpgradeMode
    {
        Locked,
        Opened,
        Bought,
    }

    public enum Endings
    {
        Pre,
        Insane,
        Neutral,
        Save,
        Any,
    }

    public class Save
    {
        public static Save save = new Save();

        private static byte[] key = Encoding.UTF8.GetBytes(
            Convert.ToString(1345) + Convert.ToString(4233) + "qDf2" + Convert.ToString(1432));
        private static byte[] iv = Encoding.UTF8.GetBytes(
            Convert.ToString(1232) + "32Dq" + Convert.ToString(1532) + Convert.ToString(3232));

        public static bool DoSave(TextBlock log)
        {
            try
            {
                StreamWriter sw = new("Saves/s0.txt");

                byte[] bytes = AES.EncryptStringToBytes(JsonSerializer.Serialize<Save>(save), key, iv);
                sw.WriteLine(Convert.ToBase64String(bytes));

                sw.Close();
                Thread aaa = new Thread((ThreadStart)delegate () {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate () {
                        log.Text = "Сохранение...";
                    });
                    Thread.Sleep(200);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate () {
                        log.Text = "";
                    });
                });
                aaa.IsBackground = true;
                aaa.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // return true if file already exist and valid
        // else false
        public static bool DoLoad()
        {
            if (File.Exists("Saves/s0.txt"))
            {
                try
                {
                    StreamReader sr = new("Saves/s0.txt");
                    string line = sr.ReadToEnd();
                    byte[] bytes = Convert.FromBase64String(line);

                    var s = JsonSerializer.Deserialize<Save>(AES.DecryptStringFromBytes(bytes, key, iv));
                    save = s ?? new Save();
                    sr.Close();
                    if (s == null) return false;
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }

        public void TryAddPages(List<int> toAdd)
        {
            foreach(int page in toAdd)
            {
                if (!ManualPagesAvailable.Contains(page))
                {
                    ManualPagesAvailable.Add(page);
                    MainWindow.This?.RefreshManual();
                }
            }
        }

        public string CurDialog { get; set; } = "";
        public string LastDialog { get; set; } = "";
        public int RunAwayTimes { get; set; } = 0;
        public bool TutorialPassed { get; set; } = false;
        public bool ResetWas { get; set; } = false;
        public double DT { get; set; } = 0;
        public double TotalDT { get; set; } = 0;
        public double RP { get; set; } = 0;

        public Dictionary<string, UpgradeMode> OpenedUpgrades { get; set; } = new();
        public int[] MachineCount { get; set; } = new int[4] { 0, 0, 0, 0 };
        public bool[] MachineOpened { get; set; } = new bool[4] { true, false, false, false };
        public int[] TimeMachineCount { get; set; } = new int[4] { 0, 0, 0, 0 };
        public double[] TimeMachinePowers { get; set; } = new double[4] { 0, 0, 0, 0 };
        public bool[] TimeMachineOpened { get; set; } = new bool[4] { true, false, false, false };
        public int Strengh { get; set; } = 1;
        public int HP { get; set; } = 1;
        public int MaxHP { get; set; } = 1;
        public int Clicks { get; set; } = 0;
        public int AmalgamWin { get; set; } = 0;
        public int IdlingTimes { get; set; } = 0;
        public int BreakingTimes { get; set; } = 0;
        public bool RoomButton { get; set; } = false;
        public bool StatButton { get; set; } = false;
        public int Resets { get; set; } = 0;
        public double AmalgametTimeIncreaser { get; set; } = 1.0;
        public Endings End { get; set; } = Endings.Pre;
        public bool EndingWas { get; set; } = false;
        public bool InTutorial { get; set; } = false;
        public int InsaneLevel { get; set; } = 0;
        public List<string> DialogsWas { get; set; } = new();
        public bool IsLastBattle { get; set; } = new();
        public int LastBattleTimes { get; set; } = new();
        public double UIScale { get; set; } = 1.0;

        public List<int> ManualPagesAvailable { get; set; } = new() { 0, 1, 2, 3, 12, 13 };
        public int ManualPagesReadedMax { get; set; } = 0;
        public int MaxClicks { get; set; } = 0;

        public int LimitLvl { get; set; } = 0;
        public bool OnLimit { get; set; } = false;
        public bool RanAway { get; set; } = false;
    }
}
