using System.Text.RegularExpressions;

namespace ST10439055_PROG_POE_WPF
{
    public class BasicQuestionsHandler : UserInteraction
    {
        public BasicQuestionsHandler(UserMemory memory) : base(memory) { }

        public override bool HandleInput(string input)
        {
            string lowerInput = input.ToLower();
            bool handled = true;

            if (lowerInput.Contains("how are you") || lowerInput.Contains("how you doing"))
            {
                string[] responses = {
                    $"I'm functioning optimally! How can I help you with cybersecurity today, {userMemory.UserName}?",
                    $"I'm just a bot, but I'm here and ready to help you stay safe online, {userMemory.UserName}.",
                    $"I'm always ready to discuss cybersecurity with you, {userMemory.UserName}! What would you like to know?"
                };
                Console.WriteLine($"Stitch: {responses[random.Next(responses.Length)]}");
            }
            else if (lowerInput.Contains("purpose") || lowerInput.Contains("why do you exist"))
            {
                string[] responses = {
                    "I'm here to educate you about cybersecurity and help you avoid online threats.",
                    $"My purpose is to make cybersecurity accessible for you, {userMemory.UserName}.",
                    "I exist to help people navigate online security with confidence!"
                };
                Console.WriteLine($"Stitch: {responses[random.Next(responses.Length)]}");
                userMemory.SetLastTopic("purpose");
            }
            else if (lowerInput.Contains("ask") || lowerInput.Contains("questions") || lowerInput.Contains("help with"))
            {
                string response = $"You can ask me about:\n- Password safety\n- Phishing\n- Safe browsing\n- VPNs\n- Password managers\n- Cyber threats";
                Console.WriteLine($"Stitch: {response}");
                userMemory.SetLastTopic("topics");
            }
            else if (Regex.IsMatch(lowerInput, @"\b(hello|hi|greetings)\b"))
            {
                GreetBasedOnTime();
            }
            else if (lowerInput.Contains("thank") || lowerInput.Contains("appreciate"))
            {
                string[] responses = {
                    $"You're welcome, {userMemory.UserName}!",
                    "Glad I could help!",
                    "Happy to assist!"
                };
                Console.WriteLine($"Stitch: {responses[random.Next(responses.Length)]}");
            }
            else if (lowerInput.Contains("quit") || lowerInput.Contains("goodbye") || lowerInput.Contains("end"))
            {
                string[] responses = {
                    $"Goodbye, {userMemory.UserName}! Stay secure!",
                    $"Take care, {userMemory.UserName}!",
                    $"Until next time!"
                };
                Console.WriteLine($"Stitch: {responses[random.Next(responses.Length)]}");
            }
            else
            {
                handled = false;
            }

            return handled;
        }
    }
}