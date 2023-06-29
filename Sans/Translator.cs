using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace Determination
{
    public enum TWord
    {
        Start,
        S_Settings,
        S_Saving,
        S_Save,
        S_DeleteSave,
        S_DeleteSaveConfirm,
        S_Interface,
        S_UISize,
        S_Fullscreen,
        S_On,
        S_Off,
        S_MusicAndSounds,
        S_MusicVolume,
        S_SoundsVolume,
        S_ExitGame,
        Back,
        Log_Saving,
        C_Determination,
        C_DTPerSecond,
        C_ResetPoints,
        C_TicksPerSecond,
        Menu_Machine,
        Menu_Upgrades,
        Menu_Reset,
        Menu_TimeMachine,
        DTPrice,
        Machine_MachineName,
        Machine_Upgrades,
        Machine_DTperSecond,
        TimeMachine_MachineName,
        RPPrice,
        TicksPerSecond,
        TimeMachine_TM,
        TimeMachine_PerSecond,

        UpgradesBubble_Buy,
        BuyMaxUpgrades,
        Room_Calming,

        C_Limit,
        ResetButton,

        Stat_Sans,
        Stat_LV,
        Stat_HP,
        Stat_DMG,
        Stat_EXP,
        Stat_DT,
        Stat_DPC,
        Stat_RP,

        RM_Phrase1,
        RM_Phrase2,
        RM_Phrase3,
        RM_Phrase4,
        RM_Phrase5,
        RM_Phrase6,
        RM_Phrase7,
        RM_Phrase8,

        She,

        DontLeave,

        CharaTxt,
        SansTxt,
        RM_Delete,

        CharaWarning,

        Eternal,
    }

    internal static class Translator
    {
        const string Path = $"pack://application:,,,/Text/translate.txt";
        public static Dictionary<TWord, string> Dictionary { private set; get; } = new();

        static public void LoadTranslates()
        {
            StreamReader sr = new(Application.GetResourceStream(new Uri(Path)).Stream);
            Dictionary = JsonSerializer.Deserialize<Dictionary<TWord, string>>(sr.ReadToEnd()) ?? new();
            sr.Close();
        }

        static private void SaveTranslates()
        {
            StreamWriter sw = new("text.txt");
            Dictionary<TWord, string> yes = new();
            foreach (TWord name in Enum.GetValues(typeof(TWord))){
                yes.Add(name, "-");
            }
            sw.WriteLine(JsonSerializer.Serialize(yes));
        }
    }
}
