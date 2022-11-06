using System.Text.Json;
using generate.Entities;
using WMap = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;

namespace generate.Helpers.MarkovChain
{
    /// <summary>
    /// Markov chaing for machine learning reivew generation
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
            try 
            {
                if (markovOrder is < 1 or > 5)
                    throw new ArgumentOutOfRangeException($"{nameof(markovOrder)} must be between 1 and 5.");
                
                _markovOrder = markovOrder;
                _trainingDataFile = trainingDataFile;
                
                Train();
            }
            catch (Exception ex)
            {
                //Log
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Train the Markov chain words/probabilities with _markovOrder for order.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private void Train()
        {
            var splitSpec = new char[] { ' ' };
            _wordMap.Clear();

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
        }

        /// <summary>
        /// Load Markov training data
        /// </summary>
        private List<Review> ReadTrainingData()
        {
            List<Review> source = null;
            
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

        /// <summary>
        /// Get next work in Markov chain based on previous word
        /// </summary>
        public String GetNextWord(string currentWord)
        {
            
            // Get the next possible words for the current word. If 
            // none exist, indicate end of sentence / start of new sentence
            if (!_wordMap.TryGetValue(currentWord, out var nextWords))
            {
                nextWords = _wordMap[string.Empty];
            }
           
            // select random word occurence based on
            // total count of words' occurrences
            long randomOccurenceIndex = _random.NextInt64(1, nextWords.Sum(x => x.Value) + 1);

            // return the word occurence matching the random value
            uint occurenceIndex = 0;
            string word = string.Empty;
            foreach (var w in nextWords)
            {
                occurenceIndex += w.Value;

                if (occurenceIndex >= randomOccurenceIndex)
                {
                    word = w.Key;
                    break;
                }
            }
            
            return word;
        }
    }
}
