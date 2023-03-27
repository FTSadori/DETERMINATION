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

namespace Sans
{
    public class Machine
    {
        public double BasePrice;
        public double Multipicator = 1.0;

        public Button BuyButton;
        public TextBlock Count;
        public TextBlock Power;

        public Machine(Button buyButton, TextBlock count, TextBlock power, double basePrice)
        {
            BuyButton = buyButton;
            Count = count;
            Power = power;
            BasePrice = basePrice;
        }
    }
    internal class MachinesHandler
    {
        public List<Machine> Machines { get; private set; } = new List<Machine>();

        private const double PriceRaiseCoef = 0.2;

        public void AddMachine(Machine newMachine) => Machines.Add(newMachine);
        
        public static double PriceFunction(double start, int x, double lvl)
        {
            return start * (Math.Pow(1.5 + lvl * PriceRaiseCoef, x) - 1.0);
        }

        public void ReloadMachine(int num)
        {
            Machine m = Machines[num];

            m.BuyButton.Content = BigNumsConverter.GetInPrettyENotation(
                PriceFunction(m.BasePrice, Save.save.MachineCount[num], num)
                ) + " реш.";
            m.Count.Text = Save.save.MachineCount[num] + " улучшений";
            m.Power.Text = $"P{num + 1}: "
                + BigNumsConverter.GetInPrettyENotation(Save.save.MachineCount[num] * m.Multipicator, 999)
                + ((num == 0) ? " реш/с" : $"P{num}");
        }

        public void ReloadMachines()
        {
            for (int i = 0; i < Machines.Count; ++i)
            {
                ReloadMachine(i);
            }
        }
        public void BuyMachine(int num)
        {
            double price = Math.Round(PriceFunction(Machines[num].BasePrice, Save.save.MachineCount[num], num));
            if (DTHandler.IfEnough(price) && price < BigNumsConverter.Infinity)
            {
                MainWindow.dthandler?.ChangeDT(-price);
                ++Save.save.MachineCount[num];
                ReloadMachine(num);
                MainWindow.dthandler?.ReloadDTPS(Machines);
            }
        }
    }
}
