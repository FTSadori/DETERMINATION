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
using Determination;

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
    public class MachinesHandler
    {
        public List<Machine> Machines { get; private set; } = new();
        public List<Machine> TimeMachines { get; private set; } = new();

        private const double PriceRaiseCoef = 0.2;

        public void AddMachine(Machine newMachine) => Machines.Add(newMachine);
        public void AddTimeMachine(Machine newMachine) => TimeMachines.Add(newMachine);

        public static double PriceFunction(double start, int x, double lvl, bool startFromZero = true)
            => start * (Math.Pow(1.3 + lvl * PriceRaiseCoef, x) - (startFromZero ? 1.0 : 0.0));

        public void ReloadMachine(int num)
        {
            Machine m = Machines[num];

            m.BuyButton.Content = BigNumsConverter.GetInPrettyENotation(
                PriceFunction(m.BasePrice, Save.save.MachineCount[num], num)
                ) + " " + Translator.Dictionary[TWord.DTPrice];
            m.Count.Text = Save.save.MachineCount[num] + " " + Translator.Dictionary[TWord.Machine_Upgrades];
            m.Power.Text = $"P{num + 1}: "
                + BigNumsConverter.GetInPrettyENotation(Save.save.MachineCount[num] * m.Multipicator, 999)
                + ((num == 0) ? $" {Translator.Dictionary[TWord.Machine_DTperSecond]}" : $"P{num}");

            MainWindow.dthandler?.ReloadDTPS(Machines);
        }

        public void ReloadTimeMachine(int num)
        {
            Machine m = TimeMachines[num];

            m.BuyButton.Content = BigNumsConverter.GetInPrettyENotation(
                PriceFunction(m.BasePrice, Save.save.TimeMachineCount[num], num, false)
                ) + " " + Translator.Dictionary[TWord.RPPrice];
            m.Count.Text = Save.save.TimeMachineCount[num] + " " + Translator.Dictionary[TWord.Machine_Upgrades];
            m.Power.Text = $"P{num + 1}: "
                + BigNumsConverter.GetInPrettyENotation(Save.save.TimeMachinePowers[num] * TimeMachines[num].Multipicator, 999)
                + ((num == 0) ? $" {Translator.Dictionary[TWord.TicksPerSecond]}" : $" {Translator.Dictionary[TWord.TimeMachine_TM]}{num}{Translator.Dictionary[TWord.TimeMachine_PerSecond]}");

            MainWindow.dthandler?.ReloadDTPS(Machines);
        }

        public void ReloadMachines()
        {
            for (int i = 0; i < Machines.Count; ++i)
                ReloadMachine(i);
        }

        public void ReloadTimeMachines()
        {
            for (int i = 0; i < TimeMachines.Count; ++i)
                ReloadTimeMachine(i);
        }
        public void BuyMachine(int num, bool buyMax, bool playsound = true)
        {
            double price = Math.Round(PriceFunction(Machines[num].BasePrice, Save.save.MachineCount[num], num));
            if (DTHandler.IfEnough(price) && price < BigNumsConverter.Infinity)
            {
                MainWindow.dthandler?.ChangeDT(-price);
                ++Save.save.MachineCount[num];
                ReloadMachine(num);
                MainWindow.dthandler?.ReloadDTPS(Machines);

                if (playsound) MainWindow.This.soundPlayer.PlaySound(SoundEnable.buy_sound);
                if (buyMax) BuyMachine(num, buyMax, false);
            }
            else if (playsound) MainWindow.This.soundPlayer.PlaySound(SoundEnable.no);
        }

        public void BuyTimeMachine(int num, bool buyMax, bool playsound = true)
        {
            double price = Math.Round(PriceFunction(TimeMachines[num].BasePrice, Save.save.TimeMachineCount[num], num, false));
            if (RPHandler.IfEnough(price) && price < BigNumsConverter.Infinity)
            {
                MainWindow.rphandler?.ChangeRP(-price);
                ++Save.save.TimeMachineCount[num];
                ++Save.save.TimeMachinePowers[num];
                ReloadTimeMachine(num);
                MainWindow.dthandler?.ReloadDTPS(Machines);
                MainWindow.tickhandler?.ReloadCounter();

                if (playsound) MainWindow.This.soundPlayer.PlaySound(SoundEnable.buy_sound);
                if (buyMax) BuyTimeMachine(num, buyMax, false);
            }
            else if (playsound) MainWindow.This.soundPlayer.PlaySound(SoundEnable.no);
        }
    }
}
