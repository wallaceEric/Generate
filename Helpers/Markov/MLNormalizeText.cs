using Microsoft.ML;

namespace generate.Helpers.MarkovChain
{
    internal static class MLNormalizeText
    {
        private static PredictionEngine<Input, Output> _predictionEngine;

        static MLNormalizeText()
        {
            var context = new MLContext();
            var emptyData = context.Data.LoadFromEnumerable(new List<Input>());
            var normalizedPipeline = context.Transforms.Text.NormalizeText("NormalizedText", "Text",
                Microsoft.ML.Transforms.Text.TextNormalizingEstimator.CaseMode.Lower,
                keepDiacritics: false,
                keepPunctuations: true,
                keepNumbers: true);

            var normalizeTransformer = normalizedPipeline.Fit(emptyData);
            _predictionEngine = context.Model.CreatePredictionEngine<Input, Output>(normalizeTransformer);
        }

        public static string NormalizeText(string preNormalizedText)
        {
            var text = new Input { Text = preNormalizedText };
            var normalizedText = _predictionEngine.Predict(text);
            return normalizedText?.NormalizedText;
        }
    
        private class Input
        {
            public string? Text { get; set; }
        }

        private class Output
        {
            public string? NormalizedText { get; set; }
        }
    }
}