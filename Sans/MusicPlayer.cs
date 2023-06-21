using Sans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Determination
{
    public enum MusicEnable
    {
        im_here,
    }

    public class MusicPlayer
    {
        private static readonly Dictionary<MusicEnable, string> pathes = new()
        {
            { MusicEnable.im_here, "Music/ost1 - I'm here.mp3" },
        };

        private MediaPlayer musicPlayer = new();
        public bool isBackPlaying = false;

        public double Volume { get { return musicPlayer.Volume; } 
            set { if (musicPlayer.Volume >= 0.0 && musicPlayer.Volume <= 1.0) musicPlayer.Volume = value; } }

        public void StartBackgroundMusic(MusicEnable music)
        {
            musicPlayer.Open(new(pathes[music], UriKind.Relative));
            musicPlayer.Position = TimeSpan.Zero;
            musicPlayer.Play();

            isBackPlaying = true;

            musicPlayer.MediaEnded += Media_Ended;
        }
        public void BackgroundMusicStop()
        {
            musicPlayer.Stop();
        }

        public static void Media_Ended(object? sender, EventArgs e)
        {
            MediaPlayer player = sender as MediaPlayer;
            player.Position = TimeSpan.Zero;
            player.Play();
        }
    }
}
