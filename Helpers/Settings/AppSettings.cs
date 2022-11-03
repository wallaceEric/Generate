namespace generate.Helpers.Settings;

/// <summary>
/// Application settings for review generation
/// </summary>
public class AppSettings
{
    public string TrainingDataFile { get; set; } = string.Empty;
    public int MarkovOrder { get; set; } = 2;
    public int WordCountMin {get; set;} = 12;
    public int WordCountMax {get; set;} = 36;
}