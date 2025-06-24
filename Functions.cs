using NAudio.Wave; // playing audio 
using ST10439055_PROG_POE_WPF;
using System.IO;
using System.Text.RegularExpressions;
class Functions
{
    private UserMemory userMemory = new UserMemory();  // Stores user info, interests, and conversation history
    private Random random = new Random();              // Used to generate random responses

    public Functions(UserMemory userMemory)
    {
        this.userMemory = userMemory;
    }

    public void PlayVoice()
    {
        string filePath = @"C:\Users\nicho\Desktop\PROG_POE_WPF\ST10439055_PROG_POE_WPF\StitchVoice.mp3";

        // Check if file exists before trying to play it
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Error: Voice file not found.");
            return;
        }

        try
        {
            // Play the audio file
            using (var reader = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(reader);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    // Wait for the sound
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing voice file: {ex.Message}");
        }
    }

    // Display a welcome message based on the time of day
    public void WelcomeMessage()
    {
        DateTime now = DateTime.Now;
        int hour = now.Hour;

        
        if (hour >= 5 && hour < 12)
        {
            Console.WriteLine("Good morning. Welcome to Stitch your personal cyber security chatbot.");
        }
        else if (hour >= 12 && hour < 17)
        {
            Console.WriteLine("Good afternoon. Welcome to Stitch your personal cyber security chatbot.");
        }
        else if (hour >= 17 && hour < 22)
        {
            Console.WriteLine("Good evening. Welcome to Stitch your personal cyber security chatbot.");
        }
        else
        {
            Console.WriteLine("Good night. Welcome to Stitch your personal cyber security chatbot.");
        }

        
        Console.WriteLine("\r\n   ▄████████     ███      ▄█      ███      ▄████████    ▄█    █▄    \r\n  ███    ███ ▀█████████▄ ███  ▀█████████▄ ███    ███   ███    ███   \r\n  ███    █▀     ▀███▀▀██ ███▌    ▀███▀▀██ ███    █▀    ███    ███   \r\n  ███            ███   ▀ ███▌     ███   ▀ ███         ▄███▄▄▄▄███▄▄ \r\n▀███████████     ███     ███▌     ███     ███        ▀▀███▀▀▀▀███▀  \r\n         ███     ███     ███      ███     ███    █▄    ███    ███   \r\n   ▄█    ███     ███     ███      ███     ███    ███   ███    ███   \r\n ▄████████▀     ▄████▀   █▀      ▄████▀   ████████▀    ███    █▀    \r\n                                                                    \r\n");
    }

    // ask for name
    public string GreetUser()
    {
        Console.WriteLine("");
        Console.WriteLine("What is your name?");
        string userName = Console.ReadLine();

        // Ask again if name is empty or whitespace
        while (string.IsNullOrWhiteSpace(userName))
        {
            Console.WriteLine("Please enter a valid name.");
            userName = Console.ReadLine();
        }

        // Save name in memory
        userMemory.UserName = userName;

        Console.WriteLine("");
        Console.WriteLine($"Hello {userName}, I am Stitch. You can ask me about password safety, phishing, and safe browsing." +
            $"\nI'm here to help you stay secure online!" +
            $"\nI also know about VPNs, password managers, and how to identify cyber threats." +
            $"\nType in 'Bye', Quit', 'End' or 'Exit' to leave the ChatBot");
        return userName;
    }

    // Process user input and route it to the appropriate handler
    public void HandleUserInput(string input)
    {
        userMemory.AddToConversationHistory(input);  // Save the input in history

        // user's mood
        SentimentDetector sentimentDetector = new SentimentDetector();
        string detectedSentiment = sentimentDetector.DetectSentiment(input);
        if (!string.IsNullOrEmpty(detectedSentiment))
        {
            userMemory.CurrentMood = detectedSentiment;
        }

        // Detect topics of interest
        InterestDetector interestDetector = new InterestDetector();
        string detectedInterest = interestDetector.DetectInterest(input);
        if (!string.IsNullOrEmpty(detectedInterest))
        {
            userMemory.AddInterest(detectedInterest);
        }

        
        UserInteraction basicHandler = new BasicQuestionsHandler(userMemory);
        UserInteraction securityHandler = new SecurityQuestionsHandler(userMemory);

        // Try to handle input with basic questions first
        bool handledBasic = TryHandleBasicQuestions(input, basicHandler);

        // If not handled, try more specific security-related questions
        if (!handledBasic)
        {
            bool handled = securityHandler.HandleInput(input);

            // If still not handled, check if it's a follow-up question
            if (!handled)
            {
                if (CheckForFollowUpQuestion(input))
                {
                    string lastTopic = userMemory.GetLastTopic();

                    // Try answering by using context from the last topic
                    if (!string.IsNullOrEmpty(lastTopic))
                    {
                        securityHandler.HandleInput(lastTopic + " " + input);
                    }
                    else
                    {
                        ProvideDefaultResponse();  // Fallback
                    }
                }
                else
                {
                    ProvideDefaultResponse();  // Fallback
                }
            }
        }
    }

    // Check if the user input is a general/basic type of question
    private bool TryHandleBasicQuestions(string input, UserInteraction handler)
    {
        string lowerInput = input.ToLower();

        // Match common basic question patterns
        if (lowerInput.Contains("how are you") ||
            lowerInput.Contains("how you doing") ||
            lowerInput.Contains("purpose") ||
            lowerInput.Contains("why do you exist") ||
            lowerInput.Contains("ask") ||
            lowerInput.Contains("questions") ||
            lowerInput.Contains("thank") ||
            lowerInput.Contains("appreciate") ||
            Regex.IsMatch(lowerInput, @"\b(hello|hi|greetings)\b") ||
            lowerInput.Contains("bye") ||
            lowerInput.Contains("goodbye"))
        {
            return handler.HandleInput(input);
        }

        return false;
    }

    // Determine if a user's message is likely a follow-up question
    private bool CheckForFollowUpQuestion(string input)
    {
        string[] followUpPhrases = new string[]
        {
            "tell me more",
            "more info",
            "explain",
            "what about",
            "can you explain",
            "please continue",
            "how does that work",
            "what does that mean",
            "why",
            "how"
        };

        string lowerInput = input.ToLower();

        // If it contains common follow-up phrases
        foreach (var phrase in followUpPhrases)
        {
            if (lowerInput.Contains(phrase))
                return true;
        }

        // Or if it's a very short and vague question
        return lowerInput.Split(' ').Length <= 3 &&
               (lowerInput.Contains("?") || lowerInput.EndsWith("though") || lowerInput.EndsWith("then"));
    }

    // Default fallback response when input isn't understood
    private void ProvideDefaultResponse()
    {
        string[] defaultResponses = new string[]
        {
            "I'm not quite sure I understand. Could you rephrase that? I'm best at discussing cybersecurity topics like passwords, phishing, and safe browsing.",
            "I didn't catch that. Can you try asking about cybersecurity in a different way?",
            "I'm still learning! Can you ask me about password security, phishing, or safe browsing instead?",
            $"Sorry {userMemory.UserName}, I didn't understand your question. " +
            $"\nAsk me about cybersecurity topics like passwords, phishing scams, or internet safety."
        };

        Console.WriteLine($"Stitch: {defaultResponses[random.Next(defaultResponses.Length)]}");
    }

    internal object PlayVoiceAsync()
    {
        throw new NotImplementedException();
    }
}
