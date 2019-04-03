// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using BungieNetResponseTypes;
//
//    var owlSchedule = OwlSchedule.FromJson(jsonString);

namespace OverwatchLeagueApiResponseTypes
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class OwlSchedule
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("stages")]
        public Stage[] Stages { get; set; }
    }

    public partial class Stage
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tournaments")]
        public TournamentElement[] Tournaments { get; set; }

        [JsonProperty("matches")]
        public Match[] Matches { get; set; }

        [JsonProperty("weeks")]
        public Week[] Weeks { get; set; }

        [JsonProperty("stageIdMap", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> StageIdMap { get; set; }

        [JsonProperty("bracketIdMap", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> BracketIdMap { get; set; }
    }

    public partial class Match
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("competitors")]
        public Competitor[] Competitors { get; set; }

        [JsonProperty("scores")]
        public Score[] Scores { get; set; }

        [JsonProperty("conclusionValue")]
        public long ConclusionValue { get; set; }

        [JsonProperty("conclusionStrategy")]
        public string ConclusionStrategy { get; set; }

        [JsonProperty("winner", NullValueHandling = NullValueHandling.Ignore)]
        public Competitor Winner { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusReason")]
        public string StatusReason { get; set; }

        [JsonProperty("attributes")]
        public MatchAttributes Attributes { get; set; }

        [JsonProperty("games")]
        public Game[] Games { get; set; }

        [JsonProperty("clientHints")]
        public object[] ClientHints { get; set; }

        [JsonProperty("dateCreated")]
        public long DateCreated { get; set; }

        [JsonProperty("flags")]
        public object[] Flags { get; set; }

        [JsonProperty("handle", NullValueHandling = NullValueHandling.Ignore)]
        public string Handle { get; set; }

        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("showStartTime")]
        public bool ShowStartTime { get; set; }

        [JsonProperty("showEndTime")]
        public bool ShowEndTime { get; set; }

        [JsonProperty("startDateTS")]
        public long StartDateTs { get; set; }

        [JsonProperty("endDateTS")]
        public long EndDateTs { get; set; }

        [JsonProperty("youtubeId")]
        public string YoutubeId { get; set; }

        [JsonProperty("wins")]
        public long[] Wins { get; set; }

        [JsonProperty("ties")]
        public long[] Ties { get; set; }

        [JsonProperty("losses")]
        public long[] Losses { get; set; }

        [JsonProperty("videos")]
        public Video[] Videos { get; set; }

        [JsonProperty("tournament")]
        public MatchTournament Tournament { get; set; }

        [JsonProperty("round", NullValueHandling = NullValueHandling.Ignore)]
        public long? Round { get; set; }

        [JsonProperty("ordinal", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ordinal { get; set; }

        [JsonProperty("winnersNextMatch", NullValueHandling = NullValueHandling.Ignore)]
        public long? WinnersNextMatch { get; set; }

        [JsonProperty("winnerRound", NullValueHandling = NullValueHandling.Ignore)]
        public long? WinnerRound { get; set; }

        [JsonProperty("winnerOrdinal", NullValueHandling = NullValueHandling.Ignore)]
        public long? WinnerOrdinal { get; set; }

        [JsonProperty("bestOf", NullValueHandling = NullValueHandling.Ignore)]
        public long? BestOf { get; set; }
    }

    public partial class MatchAttributes
    {
    }

    public partial class Competitor
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("availableLanguages")]
        public string[] AvailableLanguages { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("homeLocation")]
        public string HomeLocation { get; set; }

        [JsonProperty("primaryColor")]
        public string PrimaryColor { get; set; }

        [JsonProperty("secondaryColor")]
        public string SecondaryColor { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }

        [JsonProperty("abbreviatedName")]
        public string AbbreviatedName { get; set; }

        [JsonProperty("addressCountry")]
        public string AddressCountry { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("secondaryPhoto")]
        public string SecondaryPhoto { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Game
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("points", NullValueHandling = NullValueHandling.Ignore)]
        public long[] Points { get; set; }

        [JsonProperty("attributes")]
        public GameAttributes Attributes { get; set; }

        [JsonProperty("attributesVersion")]
        public string AttributesVersion { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusReason")]
        public string StatusReason { get; set; }

        [JsonProperty("stats")]
        public object Stats { get; set; }

        [JsonProperty("handle", NullValueHandling = NullValueHandling.Ignore)]
        public string Handle { get; set; }
    }

    public partial class GameAttributes
    {
        [JsonProperty("instanceID")]
        public string InstanceId { get; set; }

        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("mapScore", NullValueHandling = NullValueHandling.Ignore)]
        public MapScore MapScore { get; set; }

        [JsonProperty("mapGuid", NullValueHandling = NullValueHandling.Ignore)]
        public string MapGuid { get; set; }
    }

    public partial class MapScore
    {
        [JsonProperty("team1")]
        public long Team1 { get; set; }

        [JsonProperty("team2")]
        public long Team2 { get; set; }
    }

    public partial class Score
    {
        [JsonProperty("value")]
        public long Value { get; set; }
    }

    public partial class MatchTournament
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; set; }

        [JsonProperty("prize", NullValueHandling = NullValueHandling.Ignore)]
        public string Prize { get; set; }
    }

    public partial class Video
    {
        [JsonProperty("name")]
        public long Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("vodLink")]
        public string VodLink { get; set; }

        [JsonProperty("youtubeId")]
        public string YoutubeId { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }

    public partial class TournamentElement
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Week
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("startDate")]
        public long StartDate { get; set; }

        [JsonProperty("endDate")]
        public long EndDate { get; set; }

        [JsonProperty("matches")]
        public Match[] Matches { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("strings")]
        public Dictionary<string, string> Strings { get; set; }
    }

    public partial class OwlSchedule
    {
        public static OwlSchedule FromJson(string json) => JsonConvert.DeserializeObject<OwlSchedule>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this OwlSchedule self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
