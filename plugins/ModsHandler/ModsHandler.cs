using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CollectionManager.DataTypes;
using CollectionManager.Enums;
using StreamCompanionTypes.DataTypes;
using StreamCompanionTypes.Interfaces;
using Beatmap = StreamCompanionTypes.DataTypes.Beatmap;

namespace ModsHandler
{
    public class ModsHandler : IPlugin, IModParser, ISettingsProvider, IDifficultyCalculator, IMapDataReplacements
    {
        private readonly ModParser _modParser = new ModParser();
        private readonly DifficultyCalculator _difficultyCalculator = new DifficultyCalculator();
        public bool Started { get; set; }


        public string Description { get; } = "";
        public string Name { get; } = nameof(ModsHandler);
        public string Author { get; } = "Piotrekol";
        public string Url { get; } = "";
        public string UpdateUrl { get; } = "";

        public void Start(ILogger logger)
        {
            Started = true;
        }

        public Mods GetMods(int modsEnum)
        {
            _modParser.Start(null);
            return (Mods)modsEnum;
        }
        public string GetModsFromEnum(int modsEnum)
        {
            return _modParser.GetModsFromEnum(modsEnum);
        }

        public string GetModsFromEnum(int modsEnum, bool shortMod = false)
        {
            return _modParser.GetModsFromEnum(modsEnum, shortMod);
        }

        public void SetSettingsHandle(ISettingsHandler settings)
        {
            _modParser.SetSettingsHandle(settings);
        }

        public string SettingGroup => _modParser.SettingGroup;

        public void Free()
        {
            _modParser.Free();
        }

        public UserControl GetUiSettings()
        {
            return _modParser.GetUiSettings();
        }

        public Beatmap ApplyMods(Beatmap map, Mods mods)
        {
            var c = _difficultyCalculator.ApplyMods(map, mods);
            var retMap = (Beatmap)map.Clone();
            retMap.ApproachRate = c["AR"];
            retMap.CircleSize = c["CS"];
            retMap.OverallDifficulty = c["OD"];
            return retMap;
        }

        public Dictionary<string, string> GetMapReplacements(MapSearchResult map)
        {
            Dictionary<string, string> dict;
            if (map.FoundBeatmaps)
            {
                var foundMap = map.BeatmapsFound[0];
                var mods = map.Mods?.Item1 ?? Mods.Omod;
                var c = _difficultyCalculator.ApplyMods(foundMap, mods);

                if ((mods & Mods.Nc) != 0)
                {
                    mods -= Mods.Nc;
                    mods |= Mods.Dt;
                }

                var bpm = Math.Abs(c["MinBpm"] - c["MaxBpm"]) < 0.95 ? c["MinBpm"].ToString("0") : string.Format("{0:0}-{1:0}", c["MinBpm"], c["MaxBpm"]);
                dict = new Dictionary<string, string>()
                {
                    {"!mods!", map.Mods?.Item2 ?? string.Empty },
                    {"!mAR!", Math.Round(c["AR"], 1).ToString(System.Globalization.CultureInfo.InvariantCulture)},
                    {"!mCS!", Math.Round(c["CS"], 1).ToString(System.Globalization.CultureInfo.InvariantCulture)},
                    {"!mOD!", Math.Round(c["OD"], 1).ToString(System.Globalization.CultureInfo.InvariantCulture)},
                    {"!mHP!",string.Format("{0:0.##}", c["HP"]) },
                    { "!mStars!", Math.Round(foundMap.Stars(PlayMode.Osu, mods), 2).ToString(System.Globalization.CultureInfo.InvariantCulture)},
                    {"!mBpm!",bpm },
                    {"!mMaxBpm!",string.Format("{0:0}", c["MaxBpm"]) },
                    {"!mMinBpm!",string.Format("{0:0}", c["MinBpm"]) },
                };
            }
            else
            {
                dict = new Dictionary<string, string>()
                {
                    {"!mods!", string.Empty },
                    {"!mAR!", string.Empty},
                    {"!mCS!", string.Empty},
                    {"!mOD!", string.Empty},
                    {"!mHP!", string.Empty},
                    {"!mStars!", string.Empty},
                    {"!mBpm!", string.Empty},
                    {"!mMaxBpm!", string.Empty},
                    {"!mMinBpm!", string.Empty}

                };
            }
            return dict;
        }
    }
}