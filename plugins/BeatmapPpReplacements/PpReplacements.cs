﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CollectionManager.Enums;
using OppaiSharp;
using StreamCompanionTypes;
using StreamCompanionTypes.DataTypes;
using StreamCompanionTypes.Interfaces;
using Beatmap = OppaiSharp.Beatmap;
using Helpers = StreamCompanionTypes.Helpers;

namespace BeatmapPpReplacements
{
    public class PpReplacements : IPlugin, IMapDataReplacements, ISettings
    {
        private readonly SettingNames _names = SettingNames.Instance;

        DiffCalc diffCalculator = new DiffCalc();
        PPv2 _ppCalculator;
        Accuracy _accCalculator = new Accuracy();
        private ISettingsHandler _settings;
        private Mods _lastMods = Mods.NoMod;
        private string _lastModsStr = "None";
        public bool Started { get; set; }

        public string Description { get; } = "";
        public string Name { get; } = nameof(PpReplacements);
        public string Author { get; } = "Piotrekol";
        public string Url { get; } = "";
        public string UpdateUrl { get; } = "";

        public void Start(ILogger logger)
        {
            Started = true;
        }

        public Dictionary<string, string> GetMapReplacements(MapSearchResult map)
        {
            var ret = new Dictionary<string, string>
            {
                {"!MaxCombo!", ""},
                {"!SSPP!", ""},
                {"!99.9PP!", ""},
                {"!99PP!", ""},
                {"!98PP!", ""},
                {"!95PP!", ""},
                {"!90PP!", ""},
                {"!mMod!", ""},
                {"!mSSPP!", ""},
                {"!m99.9PP!", ""},
                {"!m99PP!", ""},
                {"!m98PP!", ""},
                {"!m95PP!", ""},
                {"!m90PP!", ""},
            };
            if (!map.FoundBeatmaps) return ret;
            if (map.BeatmapsFound[0].PlayMode != PlayMode.Osu) return ret;
            var mapLocation = map.BeatmapsFound[0].FullOsuFileLocation(BeatmapHelpers.GetFullSongsLocation(_settings));

            if (!File.Exists(mapLocation)) return ret;
            FileInfo file = new FileInfo(mapLocation);

            if (file.Length == 0) return ret;
            Beatmap beatmap = null;
            try
            {
                beatmap = BeatmapHelpers.GetOppaiSharpBeatmap(mapLocation);
                if (beatmap == null)
                    return ret;

                ret["!MaxCombo!"] = beatmap.GetMaxCombo().ToString(CultureInfo.InvariantCulture);

                ret["!SSPP!"] = GetPp(beatmap, 100d).ToString(CultureInfo.InvariantCulture);
                ret["!99.9PP!"] = GetPp(beatmap, 99.9d).ToString(CultureInfo.InvariantCulture);
                ret["!99PP!"] = GetPp(beatmap, 99d).ToString(CultureInfo.InvariantCulture);
                ret["!98PP!"] = GetPp(beatmap, 98d).ToString(CultureInfo.InvariantCulture);
                ret["!95PP!"] = GetPp(beatmap, 95d).ToString(CultureInfo.InvariantCulture);
                ret["!90PP!"] = GetPp(beatmap, 90d).ToString(CultureInfo.InvariantCulture);

                Mods mods = _lastMods;
                string modsStr = _lastModsStr;
                if (map.Action == OsuStatus.Playing || map.Action == OsuStatus.Watching)
                {
                    var tempMods = (map.Mods?.Item1 ?? CollectionManager.DataTypes.Mods.Omod).Convert();
                    if (tempMods != Mods.NoMod)
                    {
                        modsStr = map.Mods?.Item2 ?? "NM";
                        mods = tempMods;

                        _lastMods = tempMods;
                        _lastModsStr = modsStr;
                    }
                }

                ret["!mMod!"] = modsStr;
                ret["!mSSPP!"] = GetPp(beatmap, 100d, mods).ToString(CultureInfo.InvariantCulture);
                ret["!m99.9PP!"] = GetPp(beatmap, 99.9d, mods).ToString(CultureInfo.InvariantCulture);
                ret["!m99PP!"] = GetPp(beatmap, 99d, mods).ToString(CultureInfo.InvariantCulture);
                ret["!m98PP!"] = GetPp(beatmap, 98d, mods).ToString(CultureInfo.InvariantCulture);
                ret["!m95PP!"] = GetPp(beatmap, 95d, mods).ToString(CultureInfo.InvariantCulture);
                ret["!m90PP!"] = GetPp(beatmap, 90d, mods).ToString(CultureInfo.InvariantCulture);
                return ret;
            }
            catch
            {
                return ret;
            }
        }

        private double GetPp(Beatmap beatmap, double acc, Mods mods = Mods.NoMod)
        {
            _accCalculator = new Accuracy(acc, beatmap.Objects.Count, 0);
            _ppCalculator = new PPv2(new PPv2Parameters(beatmap, _accCalculator.Count100, _accCalculator.Count50, _accCalculator.CountMiss, -1, _accCalculator.Count300, mods));
            return Math.Round(_ppCalculator.Total, 2);
        }


        private string GetOsuDir()
        {
            return _settings.Get<string>(_names.MainOsuDirectory);
        }

        public void SetSettingsHandle(ISettingsHandler settings)
        {
            this._settings = settings;
        }

    }
}