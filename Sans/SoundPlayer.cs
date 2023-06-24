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
        buy_sound,
        horn,
        no,
        short_kick,
        start_button_enter,
        start_click,
        vinyl_short,
        voice,
        reset,
    }

    public class SoundPlayer
    {
        private static string GetFullName(SoundEnable sound) => "Sounds/" + sound.ToString() + ".mp3";

        public double Volume
        {
            get { return soundPlayer.Volume; }
            set { if (soundPlayer.Volume >= 0.0 && soundPlayer.Volume <= 1.0) soundPlayer.Volume = value; }
        }

        private MediaPlayer soundPlayer = new();
        
        public void PlaySound(SoundEnable sound)
        {
            //MainWindow.This.LOG.Text = GetFullName(sound) + " " + Volume.ToString() + " " + soundPlayer.Position;
            soundPlayer.Stop();
            soundPlayer.Open(new(GetFullName(sound), UriKind.Relative));
            soundPlayer.Position = TimeSpan.Zero; 
            soundPlayer.Play();
        }
    }
}
