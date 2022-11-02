namespace generate.Entities;
public class Review
    {
        public string? reviewerID { get; set; }
        public string? asin { get; set; }
        public string? reviewerName { get; set; }
        public int[]? helpful { get; set; }
        public string? reviewText { get; set; }
        public Single overall { get; set; }
        public string? summary { get; set; }
        public long unixReviewTime { get; set; }
        public string? reviewTime { get; set; }
    }  