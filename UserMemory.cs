namespace ST10439055_PROG_POE_WPF
{
    public class UserMemory
    {
        public string UserName { get; set; }
        public string CurrentMood { get; set; }
        private string lastMood = string.Empty;
        private List<string> interests = new List<string>();
        private List<string> mentionedInterests = new List<string>();
        private List<string> conversationHistory = new List<string>();
        private string lastTopic = string.Empty;
        private const int MaxHistoryItems = 10;

        public UserMemory()
        {
            UserName = "User";
            CurrentMood = string.Empty;
        }

        public void AddInterest(string interest)
        {
            if (!string.IsNullOrEmpty(interest) && !interests.Contains(interest))
            {
                interests.Add(interest);
            }
        }

        public bool IsNewInterest(string interest)
        {
            if (string.IsNullOrEmpty(interest)) return false;

            if (!mentionedInterests.Contains(interest))
            {
                mentionedInterests.Add(interest);
                return true;
            }
            return false;
        }

        public bool IsNewMood(string mood)
        {

            if (string.IsNullOrEmpty(mood)) return false;

            if (lastMood != mood)
            {
                lastMood = mood;
                return true;
            }
            return false;
        }

        public List<string> GetInterests()
        {
            return interests;
        }

        public void AddToConversationHistory(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                conversationHistory.Add(message);

                if (conversationHistory.Count > MaxHistoryItems)
                {
                    conversationHistory.RemoveAt(0);
                }
            }
        }

        public List<string> GetConversationHistory()
        {
            return conversationHistory;
        }

        public void SetLastTopic(string topic)
        {
            lastTopic = topic;
        }

        public string GetLastTopic()
        {
            return lastTopic;
        }
    }
}