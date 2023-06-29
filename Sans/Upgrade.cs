using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Windows;

namespace Sans
{
    public class Upgrades
    {
        public static List<Upgrade> UpgradesList = new();
        private static string Path = $"pack://application:,,,/Text/upgrades.txt";

        public static string GetName(string iconName)
        {
            return UpgradesList.Where(u => u.GetIconName() == iconName).First().GetUpgradeName();
        }

        public static void ReadUpgrades()
        {
            StreamReader sr = new(Application.GetResourceStream(new Uri(Path)).Stream);
            string path, name, desk, json;
            while (!sr.EndOfStream)
            {
                path = sr.ReadLine() ?? throw new Exception("Wrong upgrade format");
                name = sr.ReadLine() ?? throw new Exception("Wrong upgrade format");
                desk = sr.ReadLine() ?? throw new Exception("Wrong upgrade format");
                json = sr.ReadLine() ?? throw new Exception("Wrong upgrade format");
                UpgradesList.Add(JsonSerializer.Deserialize<Upgrade>(json) ?? throw new Exception("Wrong upgrade format"));
                UpgradesList.Last().SetValues(path, desk, name);
            }
        }
    }

    public class Upgrade
    {
        public Upgrade(double price)
        {
            Price = price;
        }

        public void SetValues(string iconName = "", string description = "", string upgradeName = "")
        {
            IconName = iconName;
            Description = description;
            UpgradeName = upgradeName;
        }

        private string IconName = "";
        private string Description = "";
        private string UpgradeName = "";

        public string GetIconName() => IconName;
        public string GetDescription() => Description;
        public string GetUpgradeName() => UpgradeName;

        public double Price { get; set; }

        public double[] MachineUpgrades { get; set; } = new double[4] { 1.0, 1.0, 1.0, 1.0 };
        public double[] TimeMachineUpgrades { get; set; } = new double[4] { 1.0, 1.0, 1.0, 1.0 };
        public double CursorUpgrade { get; set; } = 1.0;
        public double ExpUpgrade { get; set; } = 1.0;
        public int DamageUpgrade { get; set; } = 0;
        public double TimePointsIncomeUpgrade { get; set; } = 1.0;
        public double TimeSpeed { get; set; } = 1.0;
        public int AddPercentOfDT { get; set; } = 0;
        public double AmalgametSpawnTimeMult { get; set; } = 1.0;
    }
}
