using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NAudio.Wave;
using System.IO;
using System.Windows.Threading;


namespace ST10439055_PROG_POE_WPF
{

    public partial class MainWindow : Window
    {
        private Functions? functions;
        private UserMemory? userMemory;
        private UserInteraction? basicHandler;
        private UserInteraction? securityHandler;
        private SentimentDetector? sentimentDetector;
        private InterestDetector? interestDetector;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();
            SetWelcomeMessage();
        }

        private void InitializeComponents()
        {
            userMemory = new UserMemory();
            functions = new Functions(userMemory); // Pass userMemory here
            basicHandler = new BasicQuestionsHandler(userMemory);
            securityHandler = new SecurityQuestionsHandler(userMemory);
            sentimentDetector = new SentimentDetector();
            interestDetector = new InterestDetector();
        }

        private void SetWelcomeMessage()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;

            string timeGreeting;
            if (hour >= 5 && hour < 12)
                timeGreeting = "Good morning. Welcome to Stitch your personal cyber security chatbot.";
            else if (hour >= 12 && hour < 17)
                timeGreeting = "Good afternoon. Welcome to Stitch your personal cyber security chatbot.";
            else if (hour >= 17 && hour < 22)
                timeGreeting = "Good evening. Welcome to Stitch your personal cyber security chatbot.";
            else
                timeGreeting = "Good night. Welcome to Stitch your personal cyber security chatbot.";

            WelcomeText.Text = timeGreeting;
        }

        private void NameSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessNameInput();
        }

        private void NameInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessNameInput();
            }
        }

        private void ProcessNameInput()
        {
            string userName = NameInputBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please enter a valid name.", "Invalid Name",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save name in memory
            userMemory.UserName = userName;
            UserNameDisplay.Text = userName;

            // Update greeting text
            UserGreetingText.Text = $"Hello {userName}, I am Stitch. You can ask me about password safety, phishing, and safe browsing.\n" +
                                   "I'm here to help you stay secure online!\n" +
                                   "I also know about VPNs, password managers, and how to identify cyber threats.";

            // Hide name dialog and enable chat
            NameInputDialog.Visibility = Visibility.Collapsed;
            UserInputBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            UserInputBox.Focus();

            // Add welcome message to chat
            AddBotMessage($"Hello {userName}! I'm Stitch, your cybersecurity assistant. How can I help you stay secure online today?");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserInput();
        }

        private void UserInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                ProcessUserInput();
            }
        }

        private void ProcessUserInput()
        {
            string userInput = UserInputBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                MessageBox.Show("Please type something!", "Empty Message",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check for exit commands
            if (IsExitCommand(userInput))
            {
                var result = MessageBox.Show($"Goodbye {userMemory.UserName}! Stay secure!\n\nDo you want to close the application?",
                                           "Goodbye", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Close();
                }
                return;
            }

            // Add user message to chat
            AddUserMessage(userInput);

            // Clear input box
            UserInputBox.Clear();

            // Process the input (simulate your existing logic)
            ProcessChatInput(userInput);
        }

        private bool IsExitCommand(string input)
        {
            string lowerInput = input.ToLower();
            return lowerInput.Contains("bye") || lowerInput.Contains("exit") ||
                   lowerInput.Contains("end") || lowerInput.Contains("quit");
        }

        private void ProcessChatInput(string input)
        {
            // Add to conversation history
            userMemory.AddToConversationHistory(input);

            // Detect sentiment
            string detectedSentiment = sentimentDetector.DetectSentiment(input);
            if (!string.IsNullOrEmpty(detectedSentiment))
            {
                userMemory.CurrentMood = detectedSentiment;
                UpdateMoodDisplay();
            }

            // Detect interests
            string detectedInterest = interestDetector.DetectInterest(input);
            if (!string.IsNullOrEmpty(detectedInterest))
            {
                userMemory.AddInterest(detectedInterest);
                UpdateInterestsDisplay();
            }

            // Handle the input using existing logic
            bool handledBasic = TryHandleBasicQuestions(input);

            if (!handledBasic)
            {
                bool handled = HandleSecurityQuestion(input);

                if (!handled)
                {
                    if (CheckForFollowUpQuestion(input))
                    {
                        string lastTopic = userMemory.GetLastTopic();
                        if (!string.IsNullOrEmpty(lastTopic))
                        {
                            HandleSecurityQuestion(lastTopic + " " + input);
                        }
                        else
                        {
                            ProvideDefaultResponse();
                        }
                    }
                    else
                    {
                        ProvideDefaultResponse();
                    }
                }
            }
        }

        private bool TryHandleBasicQuestions(string input)
        {
            string lowerInput = input.ToLower();

            if (lowerInput.Contains("how are you") ||
                lowerInput.Contains("how you doing") ||
                lowerInput.Contains("purpose") ||
                lowerInput.Contains("why do you exist") ||
                lowerInput.Contains("ask") ||
                lowerInput.Contains("questions") ||
                lowerInput.Contains("thank") ||
                lowerInput.Contains("appreciate") ||
                lowerInput.Contains("hello") ||
                lowerInput.Contains("hi") ||
                lowerInput.Contains("greetings"))
            {
                return HandleBasicQuestion(input);
            }

            return false;
        }

        private Random random = new Random();
        private bool HandleBasicQuestion(string input)
        {

            string lowerInput = input.ToLower();
            string response = "";

            if (lowerInput.Contains("how are you") || lowerInput.Contains("how you doing"))
            {
                string[] responses = {
                    $"I'm functioning optimally! How can I help you with cybersecurity today, {userMemory.UserName}?",
                    $"I'm just a bot, but I'm here and ready to help you stay safe online, {userMemory.UserName}.",
                    $"I'm always ready to discuss cybersecurity with you, {userMemory.UserName}! What would you like to know?"
                };
                response = responses[random.Next(responses.Length)];
            }
            else if (lowerInput.Contains("purpose") || lowerInput.Contains("why do you exist"))
            {
                string[] responses = {
                    "I'm here to educate you about cybersecurity and help you avoid online threats.",
                    $"My purpose is to make cybersecurity accessible for you, {userMemory.UserName}.",
                    "I exist to help people navigate online security with confidence!"
                };
                response = responses[random.Next(responses.Length)];
                userMemory.SetLastTopic("purpose");
            }
            else if (lowerInput.Contains("ask") || lowerInput.Contains("questions"))
            {
                response = "You can ask me about:\n• Password safety\n• Phishing\n• Safe browsing\n• VPNs\n• Password managers\n• Cyber threats";
                userMemory.SetLastTopic("topics");
            }
            else if (lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("greetings"))
            {
                response = GetTimeBasedGreeting();
            }
            else if (lowerInput.Contains("thank") || lowerInput.Contains("appreciate"))
            {
                string[] responses = {
                    $"You're welcome, {userMemory.UserName}!",
                    "Glad I could help!",
                    "Happy to assist!"
                };
                response = responses[random.Next(responses.Length)];
            }

            if (!string.IsNullOrEmpty(response))
            {
                AddBotMessage(response);
                return true;
            }

            return false;
        }

        private string GetTimeBasedGreeting()
        {
            int hour = DateTime.Now.Hour;
            string timeGreeting = hour switch
            {
                >= 5 and < 12 => "Good morning",
                >= 12 and < 17 => "Good afternoon",
                >= 17 and < 22 => "Good evening",
                _ => "Good night"
            };
            return $"{timeGreeting}, {userMemory.UserName}. How can I help you with cybersecurity today?";
        }

        private bool HandleSecurityQuestion(string input)
        {
            // This would integrate with your existing SecurityQuestionsHandler
            // For now, simplified version with key topics
            string lowerInput = input.ToLower();

            if (lowerInput.Contains("password"))
            {
                HandlePasswordQuestion(input);
                return true;
            }
            else if (lowerInput.Contains("phish") || lowerInput.Contains("scam"))
            {
                HandlePhishingQuestion(input);
                return true;
            }
            else if (lowerInput.Contains("vpn"))
            {
                HandleVPNQuestion();
                return true;
            }
            else if (lowerInput.Contains("malware") || lowerInput.Contains("virus"))
            {
                HandleMalwareQuestion();
                return true;
            }
            // Add more security topics as needed

            return false;
        }

        private void HandlePasswordQuestion(string input)
        {
            string response;
            if (input.ToLower().Contains("strong") || input.ToLower().Contains("create"))
            {
                response = "Strong passwords should:\n• Be at least 12 characters\n• Mix letters, numbers, and symbols\n• Avoid personal information\n• Use unique passwords for each account";
            }
            else if (input.ToLower().Contains("manager"))
            {
                response = "Password managers securely store and generate strong passwords for all your accounts. They act like a digital vault for all your login credentials.";
            }
            else
            {
                response = "For password security:\n• Use unique passwords for each account\n• Change them regularly\n• Never share them\n• Consider using a password manager";
            }

            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("password");
        }

        private void HandlePhishingQuestion(string input)
        {
            string response;
            if (input.ToLower().Contains("identify") || input.ToLower().Contains("spot"))
            {
                response = "Spot phishing by checking:\n• Sender addresses\n• Urgent language\n• Requests for sensitive data\n• Poor grammar and generic greetings\n• Mismatched URLs";
            }
            else
            {
                response = "Phishing is when attackers pretend to be trustworthy to steal your data. They often use urgency or fear to pressure victims into revealing sensitive information.";
            }

            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("phishing");
        }

        private void HandleVPNQuestion()
        {
            string response = "A VPN encrypts your internet connection, protecting your privacy online. VPNs create secure tunnels for your internet traffic, especially useful on public networks.";

            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("vpn");
        }

        private void HandleMalwareQuestion()
        {
            string response = "Malware includes viruses, ransomware, and spyware that can harm your devices. Protect against malware with updated antivirus software and safe browsing habits.";

            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("malware");
        }

        private void AddPersonalizedResponse(ref string response)
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
        }

        private bool CheckForFollowUpQuestion(string input)
        {
            string[] followUpPhrases = {
                "tell me more", "more info", "explain", "what about", "can you explain",
                "please continue", "how does that work", "what does that mean", "why", "how"
            };

            string lowerInput = input.ToLower();
            return followUpPhrases.Any(phrase => lowerInput.Contains(phrase)) ||
                   (lowerInput.Split(' ').Length <= 3 && (lowerInput.Contains("?") || lowerInput.EndsWith("though")));
        }

        private void ProvideDefaultResponse()
        {
            string[] defaultResponses = {
                "I'm not quite sure I understand. Could you rephrase that? I'm best at discussing cybersecurity topics like passwords, phishing, and safe browsing.",
                "I didn't catch that. Can you try asking about cybersecurity in a different way?",
                "I'm still learning! Can you ask me about password security, phishing, or safe browsing instead?",
                $"Sorry {userMemory.UserName}, I didn't understand your question. Ask me about cybersecurity topics like passwords, phishing scams, or internet safety."
            };

            AddBotMessage(defaultResponses[random.Next(defaultResponses.Length)]);
        }

        private void UpdateMoodDisplay()
        {
            MoodDisplay.Text = string.IsNullOrEmpty(userMemory.CurrentMood) ? "Unknown" :
                              char.ToUpper(userMemory.CurrentMood[0]) + userMemory.CurrentMood.Substring(1);
        }

        private void UpdateInterestsDisplay()
        {
            var interests = userMemory.GetInterests();
            InterestsDisplay.Text = interests.Count == 0 ? "None" : string.Join(", ", interests);
        }

        private void AddUserMessage(string message)
        {
            var border = new Border();
            border.Style = (Style)FindResource("ChatBubbleUser");

            var textBlock = new TextBlock();
            textBlock.Style = (Style)FindResource("ChatText");
            textBlock.Text = message;

            border.Child = textBlock;
            ChatPanel.Children.Add(border);

            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            var border = new Border();
            border.Style = (Style)FindResource("ChatBubbleBot");

            var textBlock = new TextBlock();
            textBlock.Style = (Style)FindResource("ChatText");
            textBlock.Text = message;

            border.Child = textBlock;
            ChatPanel.Children.Add(border);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ChatScrollViewer.ScrollToBottom();
            }));
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            PlayVoice();
        }

        private void PlayVoice()
        {
            string filePath = @"C:\Users\nicho\Desktop\PROG_POE_WFP\ST10439055_PROG_POE\StitchVoice.mp3";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("Voice file not found.", "Audio Error",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var reader = new AudioFileReader(filePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(reader);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing voice file: {ex.Message}", "Audio Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}