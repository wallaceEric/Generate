using Microsoft.Extensions.Options;
using generate.Helpers.MarkovChain;
using generate.Helpers.Settings;

namespace generate.Services;

/// <summary>
/// Interface for ReviewService
/// </summary>
public interface IMarkovService
{
    public String GetNextWord(String currentWord);
}

/// <summary>
/// Markov review service
/// </summary>
public class MarkovService: IMarkovService
{
    private readonly AppSettings _appSettings;
    private readonly MarkovChain _markovChain;
    
    /// <summary>
    /// Service for Markov chain
    /// </summary>
    /// <param name="appSettings"></param>
    public MarkovService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
        _markovChain = new MarkovChain(_appSettings.MarkovOrder, _appSettings.TrainingDataFile);
    }

     public String GetNextWord(String currentWord)
    {
        return _markovChain.GetNextWord(currentWord);
    }
}

