using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.CompilerServices;

namespace Sans
{
    public enum Condition
    {
        Determination,
        Resets,
        AmalgametWins,
    }

    public class ConditionDialog
    {
        public ConditionDialog(Condition condition, double value, string dialogId, Endings endings = Endings.Pre)
        {
            Condition = condition;
            Value = value;
            DialogId = dialogId;
            Ending = endings;
        }

        public Condition Condition { get; set; }
        public double Value { get; set; }
        public string DialogId { get; set; }
        public Endings Ending { get; set; }

        public bool CheckCondition()
        {
            if (Ending == Save.save.End || Ending == Endings.Any)
            switch (Condition)
            {
                case Condition.Determination:
                    return Save.save.TotalDT >= Value;
                case Condition.Resets:
                    return Save.save.Resets >= Value;
                case Condition.AmalgametWins:
                    return Save.save.AmalgamWin >= Value;
                default:
                    return false;
            }
            else return false;
        }
    }

    public class DialogLine
    {
        public string Text { get; private set; } = "";

        public DialogLine(string _text)
        {
            int index = 0;
            while (index < _text.Length)
            {
                Text += _text[index] switch
                {
                    '&' => '\n',
                    '@' => '"',
                    _ => _text[index],
                };
                ++index;
            }
        }
    }

    public struct Dialog
    {
        public DialogLine text;
        public int speed;
        public VerticalAlignment va;
        public FontFamily ff;

        public Dialog(string _text, FontFamily? _ff = null, int _speed = 50, VerticalAlignment _va = VerticalAlignment.Bottom)
        {
            text = new DialogLine(_text);
            speed = _speed;
            va = _va;
            if (_ff == null)
                ff = (FontFamily?)MainWindow.This?.TryFindResource("DefautFont") ?? new FontFamily();
            else
                ff = _ff;
        }
    }

    public class DialogClass
    {
        private TextBlock dtDialogText;
        private Grid dtDialogBox;

        private Thread? dialog_thread;
        private bool dialog_toend = false;
        public bool IsPrinting { get; private set; } = false;
        public bool InDialog { get; private set; } = false;

        private static Dictionary<string, List<Dialog>> AllDialogs = new();

        static public void LoadDialogs()
        {
            StreamReader streamReader = new("Text/dialogs.txt");
            string key = String.Empty;
            
            foreach (var line in streamReader.ReadToEnd().Split('\n'))
            {
                if (line.Trim().Last() == ':')
                {
                    key = line.Trim()[0..^1];
                    AllDialogs[key] = new List<Dialog>();
                }
                else
                {
                    var arr = line.Split('"');
                    var arr2 = line.Split(' ');
                    string font = "DefaultFont";
                    if (arr2[^1].Trim().Length == 2)
                        if (arr2[^1].Trim()[1] == 's')
                            font = "SansFont";

                    AllDialogs[key].Add(new Dialog(arr[0],
                        (FontFamily?)MainWindow.This?.TryFindResource(font) ?? new FontFamily(),
                        Convert.ToInt32(arr2[^2].Trim()),
                        (arr2[^1].Trim()[0] == '1') ? VerticalAlignment.Top : VerticalAlignment.Bottom
                    ));
                }
            }
        }

        public DialogClass(TextBlock dialogTextBlock, Grid dialogBox)
        {
            this.dtDialogText = dialogTextBlock;
            this.dtDialogBox = dialogBox;
        }

        private List<Dialog> dialogs = new();
        private string currentKey = "";

        public void StartNewDialogs(List<Dialog> _dialogs, IDialogWindow owner, string key)
        {
            if (!InDialog)
            {
                dialogs.Clear();
                dialogs.AddRange(_dialogs);

                dtDialogBox.Visibility = Visibility.Visible;

                InDialog = true;
                currentKey = key;
                NextDialog(owner);
            }
        }

        public void StartNewDialogs(string key, IDialogWindow owner)
        {
            try
            {
                StartNewDialogs(AllDialogs[key], owner, key);
            } 
            catch (Exception)
            {
                StartNewDialogs(new List<Dialog> { new Dialog($"ДИАЛОГА С КЛЮЧОМ {key} НЕ СУЩЕСТВУЕТ") }, owner, "_error");
            }
        }

        public void DialogToEnd()
        {
            if (!IsPrinting) return;

            dialog_toend = true;
        }

        public void NextDialog(IDialogWindow owner)
        {
            if (IsPrinting) return;
           
            if (dialogs.Count == 0)
            {
                InDialog = false; 
                dtDialogBox.Visibility = Visibility.Hidden;
                owner.DialogEnds(currentKey);
                return;
            }
            PrintDialogText(dialogs[0]);
            dialogs.RemoveAt(0);
        }

        private void DoTextPrinting(string line, int ms)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate () {
                this.IsPrinting = true;
            });

            bool pause = false;
            foreach (char c in line)
            {
                if (c == '[') {
                    pause = true;
                    continue;
                }
                else if (pause) {
                    pause = false;
                    Thread.Sleep(100 * (c - '0'));
                    continue;
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate () {
                    dtDialogText.Text += c;
                });
                if (dialog_toend)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        dtDialogText.Text = "";
                        for (int i = 0; i < line.Length; ++i) {
                            if (line[i] == '[') {
                                i += 1;
                                continue;
                            }
                            dtDialogText.Text += line[i];
                        }
                    });
                    break;
                }
                Thread.Sleep(ms);
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            (ThreadStart)delegate () {
                IsPrinting = false;
            });
        }

        private void PrintDialogText(Dialog dialog)
        {
            dtDialogText.Text = "";
            dtDialogBox.VerticalAlignment = dialog.va;
            dtDialogText.FontFamily = dialog.ff;
            dialog_toend = false;

            if (dialog_thread != null)
                dialog_thread.Interrupt();
            dialog_thread = new Thread((ThreadStart)delegate () { DoTextPrinting(dialog.text.Text, dialog.speed); });
            dialog_thread.Start();
            dialog_thread.IsBackground = true;
        }
    }
}
