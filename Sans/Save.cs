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

namespace Sans
{
    public class Save
    {
        public static Save save = new Save();

        public static bool DoSave(TextBlock log)
        {
            try
            {
                StreamWriter sw = new("Saves/s0.txt");
                sw.WriteLine(JsonSerializer.Serialize<Save>(save));
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
                    var s = JsonSerializer.Deserialize<Save>(sr.ReadToEnd());
                    save = s ?? new Save();
                    sr.Close(); 
                    if (s == null) return false;
                    return true;
                }
                catch (Exception) { }
            }
            return false;
        }

        public string CurDialog { get; set; } = "";
        public string LastDialog { get; set; } = "";
        public int RunAwayTimes { get; set; } = 0;
        public bool TutorialPassed { get; set; } = false;
        public double DT { get; set; } = 0;
        public double TotalDT { get; set; } = 0;

        public int[] MachineCount { get; set; } = new int[4] {0, 0, 0, 0};
        public bool[] MachineOpened { get; set; } = new bool[4] { true, false, false, false };
        public int Clicks { get; set; } = 0;
    }
}
