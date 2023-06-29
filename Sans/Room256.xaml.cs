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
using System.Text.Json;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Runtime.CompilerServices;
using Determination;

namespace Sans
{
    public class Enemy
    {
        public string Name { get; set; } = "SmthR";
        public int HP { get; set; } = 2;
        public int MaxHP { get; set; } = 2;
        public int DMG { get; set; } = 1;

        public double difficulty { get; set; } = 2.0;
        public double speed { get; set; } = 40.0;
        public double spawnSpeed { get; set; } = 30.0;
    }
    /// <summary>
    /// Логика взаимодействия для Room256.xaml
    /// </summary>
    public partial class Room256 : Window, IDialogWindow
    {
        Thread? bgThread;
        Thread? ObstaclesThread;
        Thread? ObstaclesMoveThread;
        Thread? ActThread;
        Thread? WinThread;
        Thread? GameOverThread;
        Thread? StartBattleThread;
        Thread? AmalgMoveThread;

        public static AmalgametHandler? amHandler;

        public Room256()
        {
            InitializeComponent();

            if (amHandler == null)
            { amHandler = new(); AmalgametHandler.IsActive = true; }

            bgThread = new(DoBGAnimation);
            bgThread.IsBackground = true; 
            bgThread.Start();

            AHAHAH = false;

            AmalgMoveThread = new(Checker);
            AmalgMoveThread.IsBackground = true;
            AmalgMoveThread.Start();

            int level = Math.Min(3, Save.save.InsaneLevel) - 1;
            if (level > 0)
            {
                H1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/RoomHall{level}1.png"));
                H2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/RoomHall{level}2.png"));
                H3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/RoomHall{level}3.png"));

                ActButtonText.Text = "[A] Attack";
            }

            CalmingText.Text = Translator.Dictionary[TWord.Room_Calming];
        }

        private void Checker()
        {
            while (!IsClosed)
            {
                MainWindow.DoCmd(delegate ()
                {
                    LA.Visibility = LAmalgamet.Visibility = Visibility.Hidden;
                    RA.Visibility = RAmalgamet.Visibility = Visibility.Hidden;

                    if (amHandler?.AmalgamPlace != 0)
                    {
                        Warning.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Warning.Visibility = Visibility.Hidden;
                    }

                    if (amHandler?.AmalgamPlace == 1)
                    {
                        LA.Visibility = LAmalgamet.Visibility = Visibility.Visible;
                        if (amHandler.AmalgamWalk > 1.0)
                            Close();
                        var path = amHandler.AmalgamWalk * 430;
                        LA.Margin = LAmalgamet.Margin = new Thickness(Math.Min(180.0, path), 70.0, 0.0, 0.0);
                        path -= 180.0;
                        if (path > 0.0)
                        {
                            LA.Margin = LAmalgamet.Margin = new Thickness(180.0, 70.0 - Math.Min(200.0, path), 0.0, 0.0);
                            path -= 200.0;
                            if (path > 0.0)
                            {
                                LA.Margin = LAmalgamet.Margin = new Thickness(180.0 + Math.Min(50.0, path), -130, 0.0, 0.0);
                            }
                        }
                    }
                    else if (amHandler?.AmalgamPlace == -1)
                    {
                        RA.Visibility = RAmalgamet.Visibility = Visibility.Visible;
                        if (amHandler.AmalgamWalk > 1.0)
                            Close();
                        var path = amHandler.AmalgamWalk * 430;
                        RA.Margin = RAmalgamet.Margin = new Thickness(0.0, 70.0, Math.Min(180.0, path), 0.0);
                        path -= 180.0;
                        if (path > 0.0)
                        {
                            RA.Margin = RAmalgamet.Margin = new Thickness(0.0, 70.0 - Math.Min(200.0, path), 180.0, 0.0);
                            path -= 200.0;
                            if (path > 0.0)
                            {
                                RA.Margin = RAmalgamet.Margin = new Thickness(0.0, -130, 180.0 + Math.Min(50.0, path), 0.0);
                            }
                        }
                    }
                });

                Thread.Sleep(20);
            }
        }

        bool doBGAnimation = true;
        int bgAnimationSpeed = 200;

        bool IsClosed = false;

        int currentSoulState = 2;

        Enemy currEnemy = new();
        double MaxCalmLevel = 350.0;
        bool GasterDialogWas = false;

        Thread? dialogThread;

        public void SetSoulInColumn(int num)
        {
            if (num >= 5 || num < 0) return;

            currentSoulState = num;
            Soul.Margin = new Thickness(10.0 + 80.0 * num, 0.0, 0.0, 0.0);
        }
        DialogClass? DialogClass;
        public void StartFight(Enemy en)
        {
            InFight = true;

            CalmingLevel.Width = 0;
            currEnemy = en;
            en.HP = en.MaxHP;
            Save.save.MaxHP = Save.save.HP;

            Idling = 0;
            
            IsBreakNow = false;
            AmalgametHandler.IsActive = false;

            NotInGameOver1.Visibility = Visibility.Visible;
            NotInGameOver2.Visibility = Visibility.Visible;
            Obstacles.Visibility = Visibility.Visible;
            Field.Background = Brushes.White;

            if (en.Name == "Gaster")
            {
                F1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing1.png"));
                F2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing2.png"));
                F3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing3.png"));

                Field.Margin = new Thickness(50, 250, 0, 0);
                NotInGameOver1.Margin = new Thickness(80, 20, 20, 20);
                Obstacles.Margin = new Thickness(22, 0, 0, 0);
                Hit.Margin = new Thickness(22, 0, 0, 0);

                DialogClass = new DialogClass(DialogBoxText, DialogBox);

                CharaText1.Text = CharaText2.Text = CharaText3.Text =
                CharaText4.Text = CharaText5.Text = CharaText6.Text = "";
                No.IsEnabled = false;
                No.Content = "";
                gameoverspeed = 50;

                if (!GasterDialogWas)
                {
                    BeginNewDialog("_gaster1", 500);
                    GasterDialogWas = true;
                }
                else
                    DialogEnds("_gaster1");

                HeNormal.Visibility = Visibility.Visible;
            }
            else if (en.Name == "AlphaGaster")
            {
                Field.Margin = new Thickness(50, 250, 0, 0);
                //NotInGameOver1.Margin = new Thickness(80, 20, 20, 20);
                NotInGameOver1.Visibility = Visibility.Hidden;
                Obstacles.Margin = new Thickness(22, 0, 0, 0);
                Hit.Margin = new Thickness(22, 0, 0, 0);

                Field.Visibility = Visibility.Visible;
                Field.Background = Brushes.Transparent;
                PreField.Background = Brushes.Transparent;

                F1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing1.png"));
                F2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing2.png"));
                F3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/nothing3.png"));
            }
            else
            {
                F1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{en.Name}1.png"));
                F2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{en.Name}2.png"));
                F3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/{en.Name}3.png"));
            }

            IsBattleClosed = false;
            Obstacles.Children.Clear();
            Obstacles.Opacity = 1.0;

            FightingScreen.Visibility = Visibility.Visible;
            SetSoulInColumn(currentSoulState);

            if (en.Name != "Gaster")
            {
                ObstaclesThread = new Thread(delegate () { ObstaclesCreatingCycle(en); });
                ObstaclesThread.Start();
                ObstaclesThread.IsBackground = true;

                ObstaclesMoveThread = new Thread(delegate () { ObstaclesMovingCycle(en); });
                ObstaclesMoveThread.Start();
                ObstaclesMoveThread.IsBackground = true;
            }
        }
        public void BeginNewDialog(string key, int timegap)
        {
            if (dialogThread != null)
                dialogThread.Interrupt();
            dialogThread = new Thread((ThreadStart)delegate ()
            {
                Thread.Sleep(timegap);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    DialogClass?.StartNewDialogs(key, this);
                });
            });

            dialogThread.Start();
            dialogThread.IsBackground = true;
        }
        public void Win(bool force = false)
        {
            IsBattleClosed = true;

            Obstacles.Background = Brushes.Black;

            MainWindow.This?.musicPlayer.StartBackgroundMusic(MusicEnable.im_here);

            WinThread = new Thread(delegate ()
            {
                for (int i = 49; i >= 0; --i)
                {
                    MainWindow.DoCmd(delegate () { Obstacles.Opacity = 1.0 - i / 100.0; });
                    Thread.Sleep(10);
                }
                InFight = false;
                MainWindow.DoCmd(delegate () { Obstacles.Background = Brushes.Transparent; });
                MainWindow.DoCmd(delegate () { 
                    FightingScreen.Visibility = Visibility.Hidden; 
                    Obstacles.Visibility = Visibility.Hidden;

                    if (!force)
                    {
                        Save.save.AmalgamWin++;
                        if (Save.save.AmalgamWin == 1)
                        {
                            Close();
                        }
                    }
                    else Close();
                });
            });
            WinThread.Start();
            WinThread.IsBackground = true;

            amHandler.AmalgamPlace = 0;
            LAmalgamet.Visibility = Visibility.Hidden;
            RAmalgamet.Visibility = Visibility.Hidden;

            AmalgametHandler.IsActive = true;
        }

        int gameoverspeed = 100;

        public void GameOver()
        {
            if (currEnemy.Name == "AlphaGaster")
            {
                Close();
                MainWindow.This.Close(false);
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate ()
            {
                MainWindow.This.musicPlayer.BackgroundMusicStop();

                Field.Background = Brushes.Black;
                NotInGameOver1.Visibility = Visibility.Hidden;
                NotInGameOver2.Visibility = Visibility.Hidden;
                ActButton.Visibility = Visibility.Hidden;
                Obstacles.Visibility = Visibility.Hidden;
                IsBattleClosed = true;

                S1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken1.png"));
                S2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken2.png"));
                S3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken3.png"));

                GameOverThread = new Thread(delegate ()
                {
                    List<TextBlock> list1 = new() { CharaText1, CharaText2, CharaText3, CharaText4 };
                    for (int i = 0; i < list1.Count; ++i)
                    {
                        Thread.Sleep(4 * gameoverspeed);
                        MainWindow.DoCmd(delegate () { list1[i].Visibility = Visibility.Visible; });
                        Thread.Sleep(gameoverspeed);
                    }
                    for (int i = 0; i < list1.Count; ++i)
                    {
                        Thread.Sleep(gameoverspeed);
                        MainWindow.DoCmd(delegate () { list1[i].Visibility = Visibility.Hidden; });
                        Thread.Sleep(gameoverspeed);
                    }
                    Thread.Sleep(5 * gameoverspeed);
                    MainWindow.DoCmd(delegate () { CharaText5.Visibility = Visibility.Visible; });
                    Thread.Sleep(5 * gameoverspeed);
                    MainWindow.DoCmd(delegate () { CharaText6.Visibility = Visibility.Visible; });
                    Thread.Sleep(10 * gameoverspeed);
                    MainWindow.DoCmd(delegate () {
                        CharaText5.Visibility = Visibility.Hidden;
                        CharaText6.Visibility = Visibility.Hidden;
                    });
                    Thread.Sleep(10 * gameoverspeed);
                    MainWindow.DoCmd(delegate () { No.Visibility = Visibility.Visible; });
                    Thread.Sleep(6 * gameoverspeed);
                    MainWindow.DoCmd(delegate () { Yes.Visibility = Visibility.Visible; });
                });

                GameOverThread.Start();
                GameOverThread.IsBackground = true;
            });
        }

        volatile bool fuckingkurwachecker = false;

        public void ObstaclesMovingCycle(Enemy en)
        {
            while (FightingScreen.Visibility == Visibility.Visible && !IsBattleClosed)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    fuckingkurwachecker = false;

                    if (Obstacles.Children.Count > 0)
                    {
                        List<Image> toRemove = new();
                        for (int i = 0; i < Obstacles.Children.Count; ++i)
                        {
                            if (Obstacles.Children[i] is Image o)
                            {
                                // ow fuck dont touch that anymore
                                o.Margin = new Thickness(o.Margin.Left, o.Margin.Top + (260.0 / en.speed) / 1.5, 0, 0);
                                if (currEnemy.Name == "AlphaGaster")
                                    o.Opacity = Math.Max(0.0, o.Opacity - 0.035);
                                if (o.Margin.Top > 369.0)
                                    toRemove.Add(o);
                                if (o.Margin.Top > 300.0 && o.Margin.Top < 350.0
                                    && currentSoulState == Convert.ToInt32(o.Margin.Left - 254.0) / 80.0)
                                {
                                    toRemove.Add(o);
                                    Save.save.HP = Math.Max(0, Save.save.HP - en.DMG);
                                    if (Save.save.HP == 0)
                                        GameOver();
                                }
                            }
                        }
                        toRemove.ForEach(i => Obstacles.Children.Remove(i));
                    }

                    fuckingkurwachecker = true;
                });

                while (!fuckingkurwachecker) {
                };
                
                Thread.Sleep(20);
            }
        }

        private bool IsBattleClosed = false;

        public static bool InFight = false;

        private int roundLenght = 6000;
        private int breakLenght = 3000;

        private void ObstaclesCreatingCycle(Enemy en)
        {
            Thread.Sleep(4000);

            while (FightingScreen.Visibility == Visibility.Visible && !IsBattleClosed && ! gasterBreak)
            {
                for (int i = 0; i < roundLenght / (35 * en.speed) && !IsBattleClosed; ++i)
                {
                    List<int> places = new() { 0, 1, 2, 3, 4 };
                    List<int> spawnIn = new();

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        for (int i = 0; i < Math.Min(en.difficulty, 5); ++i)
                        {
                            var col = places[AmalgametHandler.random.Next(places.Count)];
                            places.Remove(col);
                            spawnIn.Add(col);
                        }
                        spawnIn.ForEach(n => Obstacles.Children.Add(CreateObstacle(n)));
                    });

                    if (currEnemy.Name == "Gaster")
                        GasterGlitch(0.5);
                    else if (currEnemy.Name == "AlphaGaster")
                        AlphaGasterGlitch();
                    Thread.Sleep(Convert.ToInt32(en.spawnSpeed * 35));
                }

                if (IsBattleClosed) return;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        IsBreakNow = true;
                        ActButton.Visibility = Visibility.Visible;
                    });
                
                for (int bi = 0; bi < breakLenght / 1000 && !IsBattleClosed; ++bi)
                    Thread.Sleep(1000);
                if (IsBattleClosed || gasterBreak) return;

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        if (ActButton.Visibility == Visibility.Visible)
                        {
                            Idling += 1;

                            if (Idling == 3 && !AHAHAH)
                            {
                                AHAHAH = true;
                                Idling = 0;
                                Save.save.IdlingTimes += 1;
                                MainWindow.This.BeginNewDialog($"_idling_{Math.Min(4, Save.save.IdlingTimes)}", 0);
                                MainWindow.This.canLookAtRoom = false;
                                Close();
                            }
                        }

                        ActButton.Visibility = Visibility.Hidden;

                        IsBreakNow = false;
                    });
            }
        }

        static bool AHAHAH = false;

        int Idling = 0;

        bool IsBreakNow = false;        

        public Image CreateObstacle(int col)
        {
            Image i = new();
            i.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Block" +
                $"{((currEnemy.Name == "Gaster") ? AmalgametHandler.random.Next(1, 4) : AmalgametHandler.random.Next(0, 3)) }.png"));
            i.Width = 80.0;
            i.Height = 80.0;
            i.HorizontalAlignment = HorizontalAlignment.Left;
            i.VerticalAlignment = VerticalAlignment.Top;
            i.Margin = new Thickness(254.0 + 80.0 * col, 110.0, 0.0, 0.0);
            return i;
        }

        private void DoBGAnimation()
        {
            int i = 0;

            while (doBGAnimation)
            {
                i++;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    GA1.Visibility = H1.Visibility = F1.Visibility = RA1.Visibility = LA1.Visibility = S1.Visibility = BG1.Visibility = Visibility.Hidden;
                    GA2.Visibility = H2.Visibility = F2.Visibility = RA2.Visibility = LA2.Visibility = S2.Visibility = BG2.Visibility = Visibility.Hidden;
                    GA3.Visibility = H3.Visibility = F3.Visibility = RA3.Visibility = LA3.Visibility = S3.Visibility = BG3.Visibility = Visibility.Hidden;
                    if (i == 0)
                        GA1.Visibility = H1.Visibility = F1.Visibility = RA1.Visibility = LA1.Visibility = S1.Visibility = BG1.Visibility = Visibility.Visible;
                    else if (i == 1)
                        GA2.Visibility = H2.Visibility = F2.Visibility = RA2.Visibility = LA2.Visibility = S2.Visibility = BG2.Visibility = Visibility.Visible;
                    else
                    {
                        GA3.Visibility = H3.Visibility = F3.Visibility = RA3.Visibility = LA3.Visibility = S3.Visibility = BG3.Visibility = Visibility.Visible;
                        i = -1;
                    }
                });

                Thread.Sleep(bgAnimationSpeed);
            }
        }

        public void LastBattle()
        {
            FightingScreen.Visibility = Visibility.Visible;
            NotInGameOver1.Visibility = Visibility.Hidden;
            NotInGameOver2.Visibility = Visibility.Hidden;
            Field.Visibility = Visibility.Hidden;

            DialogClass = new DialogClass(DialogBoxText, DialogBox);
            
            lastBattle = new(delegate ()
            {
                Thread.Sleep(1000);
                MainWindow.DoCmd(delegate ()
                {
                    AHead.Visibility = Visibility.Visible;
                    BeginNewDialog($"_alpha{Math.Min(Save.save.LastBattleTimes + 1, 5)}", 1000);
                });
            });

            lastBattle.IsBackground = true;
            lastBattle.Start();
        }

        private void AlphaGasterGlitch()
        {
            gasterGlitchThread = new(delegate ()
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    DoubleAnimation backgroundAnimation = new(0.5, 0.0, new Duration(TimeSpan.FromSeconds(1.0)));
                    DoubleAnimation hallOpacityAnimation = new(0.5, 0.0, new Duration(TimeSpan.FromSeconds(1.5)));
                    DoubleAnimation soulAnimation = new(0.0, 1.0, new Duration(TimeSpan.FromSeconds(1.0)));

                    HallG.Visibility = Visibility.Visible;
                    BattleGridBack.BeginAnimation(OpacityProperty, backgroundAnimation);
                    HallG.BeginAnimation(OpacityProperty, hallOpacityAnimation);
                    BattleGridFront.BeginAnimation(OpacityProperty, backgroundAnimation);
                    BlackS.BeginAnimation(OpacityProperty, soulAnimation);
                });
            });

            gasterGlitchThread.IsBackground = true;
            gasterGlitchThread.Start();
        }

        Thread? lastBattle;
        Thread? gasterGlitchThread;

        private void GasterGlitch(double power = 1.0)
        {
            gasterGlitchThread = new(delegate ()
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    DoubleAnimation backgroundAnimation = new(power, 0.0, new Duration(TimeSpan.FromSeconds(1.0)));
                    DoubleAnimation hallOpacityAnimation = new(power, 0.0, new Duration(TimeSpan.FromSeconds(1.5)));

                    HallG.Visibility = Visibility.Visible;
                    BattleGridBack.BeginAnimation(OpacityProperty, backgroundAnimation);
                    HallG.BeginAnimation(OpacityProperty, hallOpacityAnimation);
                    BattleGridFront.BeginAnimation(OpacityProperty, backgroundAnimation);
                    HeNormalG1.Visibility = Visibility.Visible;

                    HeNormalG1.Opacity = power;
                    HeNormalG2.Opacity = power;
                    HeNormalG3.Opacity = power;

                    S1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken1.png"));
                    S2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken2.png"));
                    S3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/broken3.png"));
                });
                Thread.Sleep(100);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    HeNormalG1.Visibility = Visibility.Hidden;
                    HeNormalG2.Visibility = Visibility.Visible;
                });
                Thread.Sleep(100);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    HeNormalG1.Visibility = Visibility.Visible;
                    HeNormalG2.Visibility = Visibility.Hidden;
                    HeNormalG3.Visibility = Visibility.Visible;
                });
                Thread.Sleep(100);
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    HeNormalG1.Visibility = Visibility.Hidden;
                    HeNormalG2.Visibility = Visibility.Hidden;
                    HeNormalG3.Visibility = Visibility.Hidden;

                    S1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul1.png"));
                    S2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul2.png"));
                    S3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul3.png"));
                });
            });

            gasterGlitchThread.IsBackground = true;
            gasterGlitchThread.Start();
        }

        Thread? WindowGlitchThread;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DialogClass != null)
                if (DialogClass.InDialog)
                {
                    switch (e.Key)
                    {
                        case Key.Z:
                            DialogClass.NextDialog(this);
                            // Gaster glitches
                            if (currEnemy.Name == "Gaster" && !DialogClass.IsPrinting)
                            {
                                GasterGlitch();
                            }
                            break;
                    }
                }
            if (FightingScreen.Visibility == Visibility.Visible)
            {
                if (NotInGameOver1.Visibility == Visibility.Visible || Save.save.IsLastBattle)
                    switch (e.Key)
                    {
                        case Key.Left:
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                SetSoulInColumn(currentSoulState - 1);
                            });
                            break;
                        case Key.Right:
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (ThreadStart)delegate ()
                            {
                                SetSoulInColumn(currentSoulState + 1);
                            });
                            break;
                        case Key.A:
                            if (IsBreakNow)
                            {
                                IsBreakNow = false;
                                ActButton.Visibility = Visibility.Hidden;
                                Hit.Visibility = Visibility.Visible;

                                if (currEnemy.Name == "Gaster")
                                {
                                    gasterBreak = true;
                                    BeginNewDialog("_gaster2", 500);
                                    GasterGlitch();
                                    HisHead2.Visibility = Visibility.Visible;
                                }
                                if (currEnemy.Name == "AlphaGaster")
                                {
                                    AlphaGasterGlitch();
                                    currEnemy.difficulty += 1.0;

                                    WindowGlitchThread = new(delegate ()
                                    {
                                        for (int i = 9; i >= 0; --i)
                                        {
                                            MainWindow.DoCmd(delegate () {
                                                Left *= 1.0 + 0.05 * (i / 10.0);
                                            });
                                            Thread.Sleep(100);
                                            MainWindow.DoCmd(delegate () {
                                                Left /= 1.0 + 0.05 * (i / 10.0);
                                            });
                                            Thread.Sleep(100);
                                        }
                                    });
                                    WindowGlitchThread.IsBackground = true;
                                    WindowGlitchThread.Start();
                                }

                                ActThread = new Thread(delegate ()
                                {
                                    for (int i = 0; i < 3; ++i)
                                    {
                                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate ()
                                        {
                                            Hit.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Hit{i + 1}.png"));
                                        });
                                        Thread.Sleep(100);
                                    }
                                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (ThreadStart)delegate ()
                                    {
                                        Hit.Visibility = Visibility.Hidden;

                                        currEnemy.HP = Math.Max(0, currEnemy.HP - MainWindow.This.DMG);
                                        if (currEnemy.HP == 0)
                                            Win();
                                        CalmingLevel.Width = (currEnemy.MaxHP - currEnemy.HP) * MaxCalmLevel / currEnemy.MaxHP;
                                    });
                                });

                                ActThread.Start();
                                ActThread.IsBackground = true;
                            }
                            break;
                    }
            }
        }

        public bool gasterBreak = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (IsGasterHere)
            {
                File.Delete("Saves/s0.txt");
                MainWindow.This.Close(false);
                Close();
            }

            if (FightingScreen.Visibility == Visibility.Visible)
            {
                if (!AHAHAH)
                {
                    AHAHAH = true;
                    Save.save.IdlingTimes += 1;
                    MainWindow.This.canLookAtRoom = false;
                    MainWindow.This.BeginNewDialog($"_idling_{Math.Min(4, Save.save.IdlingTimes)}", 0);
                }
            }
            AmalgametHandler.IsActive = true;

            IsClosed = true;
            IsBattleClosed = true;
            FightingScreen.Visibility = Visibility.Hidden;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            S1.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul1.png"));
            S2.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul2.png"));
            S3.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/SSoul3.png"));

            No.Visibility = Visibility.Hidden;
            Yes.Visibility = Visibility.Hidden;
            MainWindow.This.musicPlayer.StartBackgroundMusic(MusicEnable.he_must_die);
            StartFight(currEnemy);
        }

        public bool IsGasterHere = false;

        public void GasterHere()
        {
            IsGasterHere = true;
            amHandler.AmalgamPlace = 0;
            Gaster.Visibility = Visibility.Visible;
            GA.Visibility = Visibility.Visible;

            GasterWalking = new Thread(delegate ()
            {
                for (int i = 0; i < 35; ++i)
                {
                    MainWindow.DoCmd(delegate () {
                        Gaster.Margin = new Thickness(0, 0, 0, Gaster.Margin.Bottom + 2.5);
                    });
                    Thread.Sleep(100);
                }
                if (!clickedOnGaster)
                MainWindow.DoCmd(delegate () { Close(); });
            });
            GasterWalking.Start();
            GasterWalking.IsBackground = true;
        }

        public bool clickedOnGaster = false;
        public Thread? GasterWalking;
        public double gasterWalk = 0;

        private void No_Click(object sender, RoutedEventArgs e)
        {
            No.Visibility = Visibility.Hidden;
            Yes.Visibility = Visibility.Hidden;
            Close();
        }

        private void GA_Click(object sender, RoutedEventArgs e)
        {
            clickedOnGaster = true;

            MainWindow.This?.musicPlayer.StartBackgroundMusic(MusicEnable.you_must_die);
            Enemy en = new();
            en.Name = "Gaster";
            en.difficulty = 3.0;
            en.speed = 25;
            en.MaxHP = en.HP = 66;

            StartBattleThread = new Thread(delegate ()
            {
                Fade();
                MainWindow.DoCmd(delegate () { StartFight(en); });
            });
            StartBattleThread.Start();
            StartBattleThread.IsBackground = true;
        }

        private void Fade(bool _out = true)
        {
            MainWindow.DoCmd(delegate () { Obstacles.Background = Brushes.Black; Obstacles.Visibility = Visibility.Visible; });
            for (int i = 49; i >= 0; --i)
            {
                MainWindow.DoCmd(delegate () { Obstacles.Opacity = (_out ? 1.0 - i / 100.0 : 1 / 100.0); });
                Thread.Sleep(10);
            }
            MainWindow.DoCmd(delegate () { Obstacles.Background = Brushes.Transparent; });
        }

        private void LA_Click(object sender, RoutedEventArgs e)
        {
            Enemy en = new();
            en.Name = "SmthL";
            en.difficulty = 2.0 + Math.Min((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 7), 2);
            en.speed = 40.0 - Math.Min((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 3.0), 10.0);
            en.MaxHP = en.HP = Math.Max((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 20), 0) + 2;
            
            MainWindow.This?.musicPlayer.StartBackgroundMusic(MusicEnable.he_must_die);

            StartBattleThread = new Thread(delegate ()
            {
                Fade();
                MainWindow.DoCmd(delegate () { StartFight(en); });
            });
            StartBattleThread.Start();
            StartBattleThread.IsBackground = true;
        }
        /*
        private void Log(string log)
        {
            StreamWriter sw = new("log.txt", true);
            sw.WriteLine($"[{DateTime.Now}] " + log);
            sw.Close();
        }
        */
        private void RA_Click(object sender, RoutedEventArgs e)
        {
            Enemy en = new();
            en.Name = "SmthR";
            en.difficulty = 2.0 + Math.Min((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 7), 2);
            en.speed = 40.0 - Math.Min((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 3.0), 10.0);
            en.MaxHP = en.HP = Math.Max((Convert.ToInt32(Math.Log10(Save.save.TotalDT + 1)) / 20), 0) + 2;
            
            MainWindow.This?.musicPlayer.StartBackgroundMusic(MusicEnable.he_must_die);

            StartBattleThread = new Thread(delegate ()
            {
                Fade();
                MainWindow.DoCmd(delegate () { StartFight(en); });
            });
            StartBattleThread.Start();
            StartBattleThread.IsBackground = true;
        }

        Thread? AGasterShow;
        Thread? AGasterAnimation;

        Thread? HeadAlpha;
        Thread? BodyAlpha;
        Thread? Body2Alpha;
        Thread? WingAlpha;
        Thread? Wing1Alpha;
        Thread? HandsAlpha;

        Thread? AHighLayerAnimation;

        public void DialogEnds(string key)
        {
            switch (key)
            {
                case "_gaster1":
                    MainWindow.DoCmd(delegate ()
                    {
                        ObstaclesThread = new Thread(delegate () { ObstaclesCreatingCycle(currEnemy); });
                        ObstaclesThread.Start();
                        ObstaclesThread.IsBackground = true;

                        ObstaclesMoveThread = new Thread(delegate () { ObstaclesMovingCycle(currEnemy); });
                        ObstaclesMoveThread.Start();
                        ObstaclesMoveThread.IsBackground = true;
                    });
                    break;
                case "_gaster2":
                    IsGasterHere = false;
                    Save.save.IsLastBattle = true;
                    Save.DoSave(MainWindow.This.LOG);
                    Close();
                    MainWindow.This.Close(false);
                    break;
                case "_alpha1":
                case "_alpha2":
                case "_alpha3":
                case "_alpha4":
                case "_alpha5":
                    BlackS.Visibility = Visibility.Visible;

                    AGasterShow = new(delegate ()
                    {
                        MainWindow.DoCmd(
                            delegate ()
                            {
                                ABlackBG.Visibility = Visibility.Visible;
                                
                                AHands.Visibility = ABody1.Visibility = ABody2.Visibility = AWing2.Visibility 
                                = AWing1.Visibility = ATimeBG.Visibility = Visibility.Visible;
                                
                                AWing1.Opacity = AWing2.Opacity = AHands.Opacity
                                = ABody1.Opacity = ABody2.Opacity = ATimeBG.Opacity = 0.0;
                            });
                        for (int i = 0; i < 20; ++i)
                        {
                            MainWindow.DoCmd(delegate () { ATimeBG.Opacity = i / 20.0; });
                            Thread.Sleep(100 / (Save.save.LastBattleTimes + 1));
                        }
                        for (int i = 0; i < 20; ++i)
                        {
                            MainWindow.DoCmd(delegate () { AWing1.Opacity = i / 20.0; });
                            MainWindow.DoCmd(delegate () { AWing2.Opacity = i / 20.0; });
                            Thread.Sleep(100 / (Save.save.LastBattleTimes + 1));
                        }
                        for (int i = 0; i < 20; ++i)
                        {
                            MainWindow.DoCmd(delegate () { ABody1.Opacity = i / 20.0; });
                            MainWindow.DoCmd(delegate () { ABody2.Opacity = i / 20.0; });
                            Thread.Sleep(100 / (Save.save.LastBattleTimes + 1));
                        }
                        for (int i = 0; i < 20; ++i)
                        {
                            MainWindow.DoCmd(delegate () { AHands.Opacity = i / 20.0; });
                            Thread.Sleep(100 / (Save.save.LastBattleTimes + 1));
                        }
                        Thread.Sleep(100);

                        Enemy en = new();
                        en.speed = 27;
                        en.difficulty = Math.Min(5, 2 + Save.save.LastBattleTimes / 2);
                        en.Name = "AlphaGaster";
                        en.MaxHP = en.HP = 666;
                        MainWindow.DoCmd(delegate () {
                            MainWindow.This?.musicPlayer.StartBackgroundMusic(MusicEnable.alpha);
                            Save.save.LastBattleTimes++; Save.DoSave(MainWindow.This.LOG); StartFight(en); });

                        void AlphaCycle(Image part, double startspeed = 1.0)
                        {
                            const int gap = 50;
                            const double coef = 1.1;

                            double opacity = 0;

                            MainWindow.DoCmd(delegate ()
                            {
                                opacity = part.Opacity;
                            });

                            while (true)
                            {
                                while (opacity > 0)
                                {
                                    MainWindow.DoCmd(delegate ()
                                    {
                                        opacity = part.Opacity -= gap / 1000.0 * startspeed;
                                    });
                                    Thread.Sleep(gap);
                                }

                                startspeed *= coef;

                                while (opacity < 1)
                                {
                                    MainWindow.DoCmd(delegate ()
                                    {
                                        opacity = part.Opacity += gap / 1000.0 * startspeed;
                                    });
                                    Thread.Sleep(gap);
                                }

                                startspeed *= coef;
                                if (startspeed > 2) startspeed = 1;
                            }
                        }

                        void StartThread(Thread? thread, ThreadStart del)
                        {
                            thread = new(del);
                            thread.Start();
                            thread.IsBackground = true;
                        }

                        MainWindow.DoCmd(delegate () {
                            AGasterAnimation = new(delegate ()
                            {
                                MainWindow.DoCmd(delegate () {
                                    StartThread(HeadAlpha, new(delegate ()
                                    {
                                        AlphaCycle(AHead);
                                    }));
                                    StartThread(BodyAlpha, new(delegate ()
                                    {
                                        AlphaCycle(ABody1, 1.3);
                                    }));
                                    StartThread(Body2Alpha, new(delegate ()
                                    {
                                        AlphaCycle(ABody2, 1.1);
                                    })); 
                                    StartThread(HandsAlpha, new(delegate ()
                                    {
                                        AlphaCycle(AHands, 1.4);
                                    }));
                                    StartThread(Wing1Alpha, new(delegate ()
                                    {
                                        AlphaCycle(AWing2, 1.2);
                                    }));
                                    StartThread(Wing1Alpha, new(delegate ()
                                    {
                                        AlphaCycle(AWing1, 1.5);
                                    }));
                                    StartThread(AHighLayerAnimation, new(delegate ()
                                    {
                                        while (true)
                                        {
                                            MainWindow.DoCmd(delegate () { 
                                                LightL1.Visibility = Visibility.Visible;
                                                LightL3.Visibility = Visibility.Hidden;
                                            });
                                            Thread.Sleep(200);
                                            MainWindow.DoCmd(delegate () { 
                                                LightR1.Visibility = Visibility.Visible;
                                                LightR3.Visibility = Visibility.Hidden;
                                            });
                                            Thread.Sleep(200);
                                            MainWindow.DoCmd(delegate () { 
                                                LightL1.Visibility = Visibility.Hidden;
                                                LightL2.Visibility = Visibility.Visible;
                                            });
                                            Thread.Sleep(200);
                                            MainWindow.DoCmd(delegate () {
                                                LightR1.Visibility = Visibility.Hidden;
                                                LightR2.Visibility = Visibility.Visible;
                                            });
                                            Thread.Sleep(200);
                                            MainWindow.DoCmd(delegate () {
                                                LightL2.Visibility = Visibility.Hidden;
                                                LightL3.Visibility = Visibility.Visible;
                                            });
                                            Thread.Sleep(200);
                                            MainWindow.DoCmd(delegate () {
                                                LightR2.Visibility = Visibility.Hidden;
                                                LightR3.Visibility = Visibility.Visible;
                                            });
                                            Thread.Sleep(200);
                                        }
                                    }));
                                });

                                while (true)
                                {
                                    MainWindow.DoCmd(delegate () { AHands.Margin = new Thickness(0, 2, 0, 0); });
                                    MainWindow.DoCmd(delegate () { ABlackBG.Opacity = 0.9; });
                                    MainWindow.DoCmd(delegate () { 
                                        AGlitch1.Visibility = Visibility.Visible;
                                        AGlitch2.Visibility = Visibility.Visible;
                                        AGlitch3.Visibility = Visibility.Hidden;
                                    });
                                    Thread.Sleep(100);
                                    MainWindow.DoCmd(delegate () { AHands.Margin = new Thickness(0, 1, 0, 0); });
                                    MainWindow.DoCmd(delegate () { ABlackBG.Opacity = 0.8; });
                                    MainWindow.DoCmd(delegate () {
                                        AGlitch1.Visibility = Visibility.Visible;
                                        AGlitch2.Visibility = Visibility.Hidden;
                                        AGlitch3.Visibility = Visibility.Visible;
                                    });
                                    Thread.Sleep(100);
                                    MainWindow.DoCmd(delegate () { AHands.Margin = new Thickness(0, 0, 0, 0); });
                                    MainWindow.DoCmd(delegate () { ABlackBG.Opacity = 0.85; });
                                    MainWindow.DoCmd(delegate () {
                                        AGlitch1.Visibility = Visibility.Hidden;
                                        AGlitch2.Visibility = Visibility.Visible;
                                        AGlitch3.Visibility = Visibility.Visible;
                                    });
                                    Thread.Sleep(100);
                                    MainWindow.DoCmd(delegate () { AHands.Margin = new Thickness(0, 1, 0, 0); });
                                    MainWindow.DoCmd(delegate () { ABlackBG.Opacity = 0.95; });
                                    MainWindow.DoCmd(delegate () {
                                        AGlitch1.Visibility = Visibility.Hidden;
                                        AGlitch2.Visibility = Visibility.Hidden;
                                        AGlitch3.Visibility = Visibility.Hidden;
                                    });
                                    Thread.Sleep(100);
                                }
                            });
                            AGasterAnimation.Start();
                            AGasterAnimation.IsBackground = true;
                        });
                    });
                    AGasterShow.Start();
                    AGasterShow.IsBackground = true;

                    break;
            }
        }
    }
}
