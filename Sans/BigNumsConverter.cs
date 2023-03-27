using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Sans
{
    public class DTHandler
    {
        private TextBlock DTCounter;
        private TextBlock DTPSCounter;

        private Thread? DTPSThread;

        public double DTPSValue = 0.0;
        public bool DoDTPS = true;

        public DTHandler(TextBlock dtcounter, TextBlock dtpscounter)
        {
            DTCounter = dtcounter;
            DTPSCounter = dtpscounter;

            DTPSThread = new Thread(AddDTPS);
            DTPSThread.Start();
            DTPSThread.IsBackground = true;
        }

        private void AddDTPS()
        {
            const int gap = 100;
            
            while (DoDTPS)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    ChangeDT(DTPSValue / (1000.0 / gap));
                });
                Thread.Sleep(gap);
            }
        }

        public void ReloadCounter() => ChangeDT(0);

        public void ChangeDT(double num)
        {
            Save.save.DT += num;
            Save.save.TotalDT += Math.Max(0, num);
            DTCounter.Text = BigNumsConverter.GetInPrettyENotation(Save.save.DT) + " Решимости";
        }

        public void ReloadDTPS(List<Machine> machines)
        {
            double dtps = 1.0;
            for (int i = 0; i < machines.Count; ++i)
            {
                dtps *= (((i == 0) ? 0.0 : 1.0) + Save.save.MachineCount[i]) * machines[i].Multipicator;
            }
            DTPSCounter.Text = BigNumsConverter.GetInPrettyENotation(dtps) + " Решимости в секунду";
            DTPSValue = dtps;
        }

        static public bool IfEnough(double price) => (Math.Round(Save.save.DT) >= Math.Round(price));
    } 

    internal class BigNumsConverter
    {
        public const double Infinity = 6.66e66;

        public static string GetInPrettyENotation(double num, double beginFrom = 1e6)
        {
            if (num < beginFrom) return num.ToString("0");
            if (num > Infinity) return Double.PositiveInfinity.ToString();
            return num.ToString("0.##e0");
        }
    }
}
