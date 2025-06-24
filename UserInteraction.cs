namespace ST10439055_PROG_POE_WPF
{
    public abstract class UserInteraction
    {
        protected UserMemory userMemory;
        protected Random random = new Random();

        public UserInteraction(UserMemory memory)
        {
            userMemory = memory;
        }

        public abstract bool HandleInput(string input);

        protected void GreetBasedOnTime()
        {
            int hour = DateTime.Now.Hour;
            string timeGreeting = hour switch
            {
                >= 5 and < 12 => "Stitch: Good morning",
                >= 12 and < 17 => "Stitch: Good afternoon",
                >= 17 and < 22 => "Stitch: Good evening",
                _ => "Stitch: Good night"
            };
            Console.WriteLine($"{timeGreeting}, {userMemory.UserName}. How can I help you with cybersecurity today?");
        }

        protected void AddPersonalisedTouch(ref string response)
        {
           
            if (!string.IsNullOrEmpty(userMemory.CurrentMood))
            {
                switch (userMemory.CurrentMood.ToLower())
                {
                    case "worried":
                        response = $"I understand this can be concerning. {response}";
                        break;
                    case "curious":
                        response = $"I appreciate your curiosity. {response}";
                        break;
                    case "frustrated":
                        response = $"Let me help clarify this for you. {response}";
                        break;
                    case "confused":
                        response = $"Let me explain this clearly. {response}";
                        break;
                }
            }

            // Reference interests naturally in some responses
            var interests = userMemory.GetInterests();
            if (interests.Count > 0 && random.Next(10) < 2) // 20% chance to reference interests
            {
                string interest = interests[random.Next(interests.Count)];
                response += $"\n\nBy the way, since you asked about {interest}, remember that good {interest} practices are important for overall security.";
            }
        }
    }
}