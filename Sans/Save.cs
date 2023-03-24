using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sans
{
    public class Save
    {
        public string CurDialog { get; set; } = "";
        public string LastDialog { get; set; } = "";
        public int RunAwayTimes { get; set; } = 0;
        public bool TutorialPassed { get; set; } = false;
    }
}
