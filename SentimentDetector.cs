using System.Text.RegularExpressions;

namespace ST10439055_PROG_POE_WPF
{
    public class SentimentDetector
    {
        private Dictionary<string, List<string>> sentimentPatterns;

        public SentimentDetector()
        {
            InitializeSentimentPatterns();
        }

        private void InitializeSentimentPatterns()
        {
            sentimentPatterns = new Dictionary<string, List<string>>
            {
                { "worried", new List<string> {
                    "worried", "concerned", "anxious", "scared", "afraid", "nervous", "stress", "stressed",
                    "panic", "alarmed", "fear", "fearful", "uneasy", "troubled", "distressed", "apprehensive"
                }},
                { "curious", new List<string> {
                    "curious", "intrigued", "fascinating",
                    "learn about", "discover", "explain", "interested in"
                }},
                { "frustrated", new List<string> {
                    "frustrated", "annoyed", "irritated", "upset", "agitated", "angry", "mad", "bothered",
                    "tired of", "fed up", "had enough", "exasperated", "furious", "irritating", "aggravating"
                }},
                { "confused", new List<string> {
                    "confused", "don't understand", "unclear", "lost", "puzzled", "perplexed", "unsure",
                    "not following", "complicated", "confusing", "bewildered", "baffled", "don't get it"
                }}
            };
        }

        public string DetectSentiment(string input)
        {
            string lowerInput = input.ToLower();

            foreach (var sentiment in sentimentPatterns)
            {
                foreach (var pattern in sentiment.Value)
                {
                    if (Regex.IsMatch(lowerInput, $"\\b{pattern}\\b") || lowerInput.Contains(pattern))
                    {
                        return sentiment.Key;
                    }
                }
            }

            return string.Empty; // No sentiment detected
        }
    }

    public class InterestDetector
    {
        private Dictionary<string, List<string>> interestPatterns;

        public InterestDetector()
        {
            InitializeInterestPatterns();
        }

        private void InitializeInterestPatterns()
        {
            interestPatterns = new Dictionary<string, List<string>>
            {
                { "privacy", new List<string> {
                    "privacy", "private", "confidential", "personal data", "data protection", "anonymity", "hidden"
                }},
                { "passwords", new List<string> {
                    "password", "passphrase", "authentication", "login", "credential"
                }},
                { "security", new List<string> {
                    "security", "protection", "defend", "safeguard", "safety", "secure"
                }},
                { "phishing", new List<string> {
                    "phishing", "scam", "fraud", "deception", "fake email", "impersonation"
                }},
                { "malware", new List<string> {
                    "malware", "virus", "trojan", "ransomware", "spyware", "adware", "worm"
                }},
                { "browsing", new List<string> {
                    "browsing", "internet", "web", "surf", "online", "website"
                }},
                { "social media", new List<string> {
                    "social media", "facebook", "twitter", "instagram", "tiktok", "linkedin", "social network"
                }}
            };
        }

        public string DetectInterest(string input)
        {
            string lowerInput = input.ToLower();

            foreach (var interest in interestPatterns)
            {
                foreach (var pattern in interest.Value)
                {
                    if (lowerInput.Contains(pattern))
                    {
                        return interest.Key;
                    }
                }
            }

            return string.Empty; // No interest detected
        }
    }
}