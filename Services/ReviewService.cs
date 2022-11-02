using Microsoft.Extensions.Options;
using generate.Entities;
using generate.Helpers.MarkovChain;
using generate.Helpers.Settings;

namespace generate.Services;

/// <summary>
/// Interface for ReviewService
/// </summary>
public interface IReviewService
{
    public Review Generate();
}


/// <summary>
/// 
/// </summary>
public class ReviewService: IReviewService
{
    private readonly AppSettings _appSettings;
    private readonly MarkovChain _markovChain;
    
    /// <summary>
    /// Service to generate Review
    /// </summary>
    /// <param name="appSettings"></param>
    public ReviewService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
        _markovChain = new MarkovChain(_appSettings.MarkovOrder, _appSettings.TrainingDataFile);
    }

     public Review Generate()
    {
        int wordCount = Random.Shared.Next(
            _appSettings.WordCountMin, 
            _appSettings.WordCountMax);

        return GenerateReview(wordCount);      
    }


    /// <summary>
    /// Creates a review from the trained MarkovChain
    /// </summary>
    /// <param name="wordCount">
    /// Number of words in the review
    /// </param>
    /// <returns></returns>
    public Review GenerateReview(int wordCount)
    {
        bool phraseTerminated = false;
        List<String> wordList = new();
        string word = _markovChain.GetNextWord(string.Empty, out phraseTerminated);
        
        for (int i = 0; i < wordCount; i++)
        {
            word = _markovChain.GetNextWord(word, out phraseTerminated);
            wordList.Add((phraseTerminated ? ". " : "") + word.Split(" ").Last());
        }

        return AssembleReview(wordList);
    }

    /// <summary>
    /// Create the final Review with some cleanup
    /// </summary>
    /// <param name="wordList">
    /// Review text generated from MarkovChain
    /// </param>
    /// <returns></returns>
    private Review AssembleReview(List<string> wordList)
    {
        // a little cleanup
        string reviewText = (String.Join(' ', wordList) + ".").Trim();
        reviewText = reviewText.Replace(" .", ".");
        reviewText = reviewText.Replace("dnt ", "dn't ");
        reviewText = reviewText.Replace("yre ", "y're ");
        reviewText = reviewText.Replace(" im ", " i'm ");
        reviewText = reviewText.Replace(" i'm ", " I'm ");
        reviewText = reviewText.Replace(" i ", " I ");
        reviewText = CapitalizeFirst(reviewText);
        
        return new Review()
        {
            reviewerID = "337737",
            reviewerName = "Markov",
            reviewText = reviewText,
            overall = Random.Shared.Next(1,5),
            reviewTime = DateTime.UtcNow.ToString(),
            asin = "",
            summary = "",
            unixReviewTime = 0
        };
    }

    /// <summary>
    /// Capitalizes the first word of each sentence in the string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private static string CapitalizeFirst(string s)
    {
        bool isSentence = true;
        var result = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            if (isSentence && char.IsLetter(s[i]))
            {
                result.Append (char.ToUpper (s[i]));
                isSentence = false;
            }
            else
                result.Append (s[i]);
            if (s[i] == '!' || s[i] == '?' || s[i] == '.')
            {
                isSentence = true;
            }
        }
        return result.ToString();
    }
}

