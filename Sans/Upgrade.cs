using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sans
{
    public class Upgrade
    {
        public Upgrade(string iconName, string description, string upgradeName, double price)
        {
            IconName = iconName;
            Description = description;
            UpgradeName = upgradeName;
            Price = price;
        }

        public string IconName { get; set; }
        public string Description { get; set; }
        public string UpgradeName { get; set; }
        public double Price { get; set; }
    }

    public class MachineUpgrade
    {

    }
}
