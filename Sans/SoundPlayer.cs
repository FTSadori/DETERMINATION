using Sans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Determination
{
    public enum SoundEnable
    {

    }

    public class SoundPlayer
    {
        private static readonly Dictionary<SoundEnable, string> pathes = new()
        {
        };

        public double Volume
        {
            get { return soundPlayer.Volume; }
            set { if (soundPlayer.Volume >= 0.0 && soundPlayer.Volume <= 1.0) soundPlayer.Volume = value; }
        }

        private MediaPlayer soundPlayer = new();
        
        public void PlaySound(SoundEnable sound)
        {
            soundPlayer.Open(new(pathes[sound], UriKind.Relative));
            soundPlayer.Play();
        }
    }
}
