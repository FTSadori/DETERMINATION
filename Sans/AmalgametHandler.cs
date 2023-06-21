using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sans
{
    public class AmalgametHandler
    {
        public static Random random = new Random();

        private Thread? AmlgThread;
        public int AmalgamPlace = 0;

        public double AmalgamWalk = 0.0;
        public int AmalgamWalkTime = 10000;

        static public bool DESTROYED = false;

        static public bool IsActive = false;

        public const int AvgSpawnTime = 10;//5 * 60 * 1000;

        static public double AmalgametTimeIncreaser = 1.0;

        private int GetAverageTimeToSpawn()
        {
            int avgTime = AvgSpawnTime;
            var mult = Math.Max(Math.Sqrt(Math.Log10(Save.save.TotalDT + 1)), 1.0);
            return Convert.ToInt32(avgTime * AmalgametTimeIncreaser / mult);
        }

        public AmalgametHandler()
        {
            random.NextDouble();

            AmlgThread = new(FindAmalgamet);
            AmlgThread.Start();
            AmlgThread.IsBackground = true;
        }

        public void Reset()
        {
            AmalgamPlace = 0;
            IsActive = true;
        }

        private void FindAmalgamet()
        {
            bool found = false;

            while (true)
            {
                if (!IsActive || MainWindow.This.dialogClass.InDialog)
                {
                    Thread.Sleep(500);
                }
                else if (AmalgamPlace != 0)        
                {
                    AmalgamWalk += 50.0 / AmalgamWalkTime;
                    Thread.Sleep(50);

                    if (AmalgamWalk > 1.0 && !DESTROYED)
                    {
                        DESTROYED = true;
                        Save.save.BreakingTimes += 1;
                        MainWindow.This.canLookAtRoom = false;
                        MainWindow.This.BeginNewDialog($"_destroyed_{Math.Min(4, Save.save.BreakingTimes)}", 0);
                    }
                }
                else if (found)
                {
                    found = false;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        random.NextDouble();
                        AmalgamPlace = (random.NextDouble() >= 0.5 ? 1 : -1);
                        AmalgamWalk = 0.0;
                    });

                    Thread.Sleep(50);
                }
                else
                {
                    Thread.Sleep(GetAverageTimeToSpawn());
                    found = true;
                }
            }
        }
    }
}
