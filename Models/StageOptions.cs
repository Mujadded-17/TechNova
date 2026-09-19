namespace TechNova.Models
{
    /// <summary>
    /// The one list of business/funding stages used by registration, profile
    /// editing and the investor filters. Three different vocabularies used to
    /// exist, so a startup registered as "MVP" would find no matching option
    /// on Edit Profile and silently lose its stage on save.
    /// </summary>
    public static class StageOptions
    {
        public static readonly string[] BusinessStages =
        {
            "Idea",
            "Prototype",
            "MVP",
            "Pre-Seed",
            "Seed",
            "Series A",
            "Series B",
            "Series C",
            "Growth",
            "Established"
        };
    }
}
