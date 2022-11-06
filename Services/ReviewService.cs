using Microsoft.Extensions.Options;
using generate.Entities;
using generate.Helpers.MarkovChain;
using generate.Helpers.Settings;
using System.Text.RegularExpressions;

namespace generate.Services;

/// <summary>
/// Interface for ReviewService
/// </summary>
public interface IReviewService
{
    public Review Generate();
}

/// <summary>
/// Generate review service
/// </summary>
public class ReviewService: IReviewService
{
    private readonly AppSettings _appSettings;
    private readonly IMarkovService _markovService;
    
    /// <summary>
    /// Service to generate Review
    /// </summary>
    /// <param name="appSettings"></param>
    public ReviewService(IOptions<AppSettings> appSettings, IMarkovService markovService)
    {
        _appSettings = appSettings.Value;
        _markovService = markovService;
    }

     public Review Generate()
    {
        int wordCount = Random.Shared.Next(
            _appSettings.WordCountMin, 
            _appSettings.WordCountMax + 1);

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
        List<String> wordList = new();
        
        string word = string.Empty;
        while (true)
        {
            word = _markovService.GetNextWord(word);
            wordList.Add(word.Split(" ").Last());
            if (wordList.Count >= wordCount && wordList.Last().TrimEnd().EndsWith("."))
                break;
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
        string reviewText = (String.Join(' ', wordList)).Trim();
        reviewText = reviewText.Replace(" .", ".");
        reviewText = Regex.Replace(reviewText, @"\.(?! |$)", ". ");
        reviewText = Regex.Replace(reviewText, @"&#\w+;", "");
        reviewText = reviewText.Replace("\"", "");
        reviewText = reviewText.Replace("dnt ", "dn't ");
        reviewText = reviewText.Replace("yre ", "y're ");
        reviewText = reviewText.Replace(" im ", " i'm ");
        reviewText = reviewText.Replace(" i'm ", " I'm ");
        reviewText = reviewText.Replace(" i ", " I ");
        reviewText = CapitalizeFirst(reviewText);
        
        return new Review()
        {
            reviewerID = "337737",
            reviewerName = "Markov, Andrey",
            reviewText = reviewText,
            overall = Random.Shared.Next(1,6),
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

