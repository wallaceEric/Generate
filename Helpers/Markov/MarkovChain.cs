using System.Text.Json;
using generate.Entities;
using generate.Helpers.Errors;
using WMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;

namespace generate.Helpers.MarkovChain
{

    /// <summary>
    /// 
    /// </summary>
    public class MarkovChain
    {
        private readonly WMap _wordMap = new();
        private readonly Random _random = new Random();

        private int _markovOrder { get; }
        private string _trainingDataFile { get; }

        /// <summary>
        /// Create the Markov chain word list from configured training data.
        /// </summary>
        /// <param name="markovOrder"></param>
        /// <param name="trainingDataFile"></param>
        public MarkovChain (int markovOrder, string trainingDataFile)
        {
            if (markovOrder is < 1 or > 5)
                throw new ArgumentOutOfRangeException($"{nameof(markovOrder)} must be between 1 and 5.");
            
            _markovOrder = markovOrder;
            _trainingDataFile = trainingDataFile;
            Train();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private void Train()
        {
            var splitSpec = new char[] { ' ' };
            _wordMap.Clear();

            try
            {
                List<Review> source = ReadTrainingData();
                foreach (Review review in source)
                {
                    string normalizedReview = MLNormalizeText.NormalizeText(review.reviewText);

                    if (string.IsNullOrEmpty(normalizedReview)) continue;

                    string[] thisBlock = normalizedReview.Split(splitSpec, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    string prevWord = string.Empty;

                    for (int i = 0; i < thisBlock.Length - (_markovOrder - 1); i++)
                    {
                        string thisWord = String.Join(" ", thisBlock.Skip(i).Take(_markovOrder));

                        if (!_wordMap.ContainsKey(prevWord))
                        {
                            _wordMap.Add(prevWord, new Dictionary<string, uint>() { { thisWord, 1 } });
                        }
                        else
                        {
                            var nextWordsMap = _wordMap[prevWord];
                            if (!nextWordsMap.ContainsKey(thisWord))
                            {
                                nextWordsMap.Add(thisWord, 1);
                            }
                            else
                            {
                                nextWordsMap[thisWord] += 1;
                            }
                        }
                        prevWord = thisWord;
                    }
                }
            } catch (Exception)
            {
                // Log
                throw;
            }
        }

        private List<Review> ReadTrainingData()
        {
            List<Review> source;
            if (String.IsNullOrEmpty(_trainingDataFile))
                throw new ArgumentException($"{nameof(_trainingDataFile)} was not provided.");

            if (!File.Exists(_trainingDataFile))
                throw new FileNotFoundException($"File [{_trainingDataFile}] was not found.");

            using (StreamReader r = new(_trainingDataFile))
            {
                string json = r.ReadToEnd();
                source = JsonSerializer.Deserialize<List<Review>>(json);
            }
            return source;
        }

        public String GetNextWord(string currentWord, out bool phraseTerminated)
        {
            phraseTerminated = false;

            // Get the next possible words for the current word. If 
            // none exist, indicate end of sentence / start of new sentence
            if (!_wordMap.TryGetValue(currentWord, out var nextWords))
            {
                nextWords = _wordMap[string.Empty];
                phraseTerminated = true;
            }

            long totalWords = nextWords.Sum(x => x.Value);
            long i = 0;
            while (true)
            {
                // Choose one possible word candidate randomly
                var thisWord = nextWords.ElementAt(_random.Next(0, nextWords.Count - 1));

                // the number of times this word occurs relative to the total number is the probability
                // of choosing this word
                double thisWordProbablity = (double)thisWord.Value / (double)totalWords;

                // If a randomly generated percentage is within this word's probability,
                // choose this word
                if (_random.NextDouble() <= thisWordProbablity)
                {
                    // System.Diagnostics.Debug.WriteLine($"{i} tries to match total {totalWords}, unique {nextWords.Count}");
                    return thisWord.Key;
                }
                i++;
            }
        }
    }
}
