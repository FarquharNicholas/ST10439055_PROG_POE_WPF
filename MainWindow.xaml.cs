using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NAudio.Wave;
using System.IO;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;

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
        private Random random = new Random();
        private QuizClass? quizManager;
        private ReminderClass? reminderManager;
        private List<TaskItem> tasks = new List<TaskItem>(); // Added tasks list
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private const int MaxLogEntries = 10;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();
            SetWelcomeMessage();
        }

        private void InitializeComponents()
        {
            userMemory = new UserMemory();
            functions = new Functions(userMemory);
            basicHandler = new BasicQuestionsHandler(userMemory);
            securityHandler = new SecurityQuestionsHandler(userMemory);
            sentimentDetector = new SentimentDetector();
            interestDetector = new InterestDetector();
            quizManager = new QuizClass(this); // Pass MainWindow reference for UI updates
            reminderManager = new ReminderClass(this, tasks); // Pass MainWindow and tasks list
        }

        public UserMemory? UserMemory => userMemory; // Public accessor for userMemory

        private void SetWelcomeMessage()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            string timeGreeting = hour switch
            {
                >= 5 and < 12 => "Good morning",
                >= 12 and < 17 => "Good afternoon",
                >= 17 and < 22 => "Good evening",
                _ => "Good night"
            };
            WelcomeText.Text = $"{timeGreeting}. Welcome to Stitch your personal cybersecurity chatbot.";
        }

        private void NameSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessNameInput();
        }

        private void NameInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ProcessNameInput();
        }

        private void ProcessNameInput()
        {
            if (userMemory == null || NameInputBox == null || UserGreetingText == null || NameInputDialog == null || UserInputBox == null || SendButton == null) return;
            string userName = NameInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please enter a valid name.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            userMemory.UserName = userName;
            UserGreetingText.Text = $"Hello {userName}, I am Stitch. Ask me about password safety, phishing, and safe browsing.\nI'm here to help you stay secure online!\nI also know about VPNs, password managers, and cyber threats.";
            NameInputDialog.Visibility = Visibility.Collapsed;
            UserInputBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            UserInputBox.Focus();
            AddBotMessage($"Hello {userName}! I'm Stitch, your cybersecurity assistant. How can I help you stay secure online today?");
            LogActivity($"User {userName} started the chatbot.");
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

        private void UserInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserInputBox == null) return;
        }

        private void ProcessUserInput()
        {
            if (UserInputBox == null) return;
            string userInput = UserInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput))
            {
                MessageBox.Show("Please type something!", "Empty Message", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (IsExitCommand(userInput))
            {
                if (userMemory != null)
                {
                    var result = MessageBox.Show($"Goodbye {userMemory.UserName}! Stay secure!\nDo you want to close the application?", "Goodbye",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes) Close();
                }
                return;
            }
            AddUserMessage(userInput);
            UserInputBox.Text = "";
            ProcessChatInput(userInput);
        }

        private bool IsExitCommand(string input)
        {
            string lowerInput = input.ToLower();
            return lowerInput.Contains("bye") || lowerInput.Contains("exit") || lowerInput.Contains("end") || lowerInput.Contains("quit");
        }

        private void ProcessChatInput(string input)
        {
            if (userMemory == null || sentimentDetector == null || interestDetector == null || QuizPopup == null || quizManager == null) return;
            userMemory.AddToConversationHistory(input);
            LogActivity($"User input: {input}");
            string detectedSentiment = sentimentDetector.DetectSentiment(input);
            if (!string.IsNullOrEmpty(detectedSentiment))
            {
                userMemory.CurrentMood = detectedSentiment;
            }
            string detectedInterest = interestDetector.DetectInterest(input);
            if (!string.IsNullOrEmpty(detectedInterest))
            {
                userMemory.AddInterest(detectedInterest);
            }
            if (HandleNLPInput(input)) return;
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

            string lowerInput = input.ToLower();
            string[] quizTriggers = { "quiz", "game", "play", "test", "challenge" };
            if (quizTriggers.Any(trigger => lowerInput.Contains(trigger)))
            {
                quizManager.StartQuiz();
            }
        }

        private bool HandleNLPInput(string input)
        {
            if (userMemory == null || reminderManager == null) return false;
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("add a task to") || lowerInput.Contains("create me a reminder for") || lowerInput.Contains("add reminder to"))
            {
                reminderManager.SetReminderFromInput(input);
                return true;
            }
            else if (lowerInput.Contains("activity log"))
            {
                ShowActivityLog();
                return true;
            }
            else if (lowerInput.Contains("remind me to") || lowerInput.Contains("set reminder for"))
            {
                reminderManager.SetReminderFromInput(input);
                return true;
            }
            return false;
        }

        private bool TryHandleBasicQuestions(string input)
        {
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how you doing") || lowerInput.Contains("purpose") ||
                lowerInput.Contains("why do you exist") || lowerInput.Contains("ask") || lowerInput.Contains("questions") ||
                lowerInput.Contains("thank") || lowerInput.Contains("appreciate") || lowerInput.Contains("hello") ||
                lowerInput.Contains("hi") || lowerInput.Contains("greetings"))
            {
                return HandleBasicQuestion(input);
            }
            return false;
        }

        private bool HandleBasicQuestion(string input)
        {
            if (userMemory == null) return false;
            string lowerInput = input.ToLower();
            string? response = "";
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how you doing"))
            {
                string[] responses = { $"I'm functioning optimally! How can I help you with cybersecurity today, {userMemory.UserName}?",
                    $"I'm just a bot, but I'm here and ready to help you stay safe online, {userMemory.UserName}.",
                    $"I'm always ready to discuss cybersecurity with you, {userMemory.UserName}! What would you like to know?" };
                response = responses[random.Next(responses.Length)];
            }
            else if (lowerInput.Contains("purpose") || lowerInput.Contains("why do you exist"))
            {
                string[] responses = { "I'm here to educate you about cybersecurity and help you avoid online threats.",
                    $"My purpose is to make cybersecurity accessible for you, {userMemory.UserName}.",
                    "I exist to help people navigate online security with confidence!" };
                response = responses[random.Next(responses.Length)];
                userMemory.SetLastTopic("purpose");
            }
            else if (lowerInput.Contains("ask") || lowerInput.Contains("questions"))
            {
                response = "You can ask me about:\nPassword safety\nPhishing\nSafe browsing\nVPNs\nPassword managers\nCyber threats\nStart a quiz\nSet reminders";
                userMemory.SetLastTopic("topics");
            }
            else if (lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("greetings"))
            {
                response = GetTimeBasedGreeting();
            }
            else if (lowerInput.Contains("thank") || lowerInput.Contains("appreciate"))
            {
                string[] responses = { $"You're welcome, {userMemory.UserName}!", "Glad I could help!", "Happy to assist!" };
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
            if (userMemory == null) return "";
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
            return false;
        }

        private void HandlePasswordQuestion(string input)
        {
            if (userMemory == null) return;
            string? response;
            if (input.ToLower().Contains("strong") || input.ToLower().Contains("create"))
            {
                response = "Strong passwords should:\n• Be at least 12 characters\n• Mix letters, numbers, and symbols\n• " +
                    "Avoid personal information\n• Use unique passwords for each account";
            }
            else if (input.ToLower().Contains("manager"))
            {
                response = "Password managers securely store and generate strong passwords for all your accounts. " +
                    "They act like a digital vault for all your login credentials.";
            }
            else
            {
                response = "For password security:\n• Use unique passwords for each account\n• Change them regularly" +
                    "\n• Never share them\n• Consider using a password manager";
            }
            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("password");
        }

        private void HandlePhishingQuestion(string input)
        {
            if (userMemory == null) return;
            string? response;
            if (input.ToLower().Contains("identify") || input.ToLower().Contains("spot"))
            {
                response = "Spot phishing by checking:\n• Sender addresses\n• Urgent language\n• Requests for sensitive data" +
                    "\n• Poor grammar and generic greetings\n• Mismatched URLs";
            }
            else
            {
                response = "Phishing is when attackers pretend to be trustworthy to steal your data. " +
                    "They often use urgency or fear to pressure victims into revealing sensitive information.";
            }
            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("phishing");
        }

        private void HandleVPNQuestion()
        {
            if (userMemory == null) return;
            string? response = "A VPN encrypts your internet connection, protecting your privacy online. " +
                "VPNs create secure tunnels for your internet traffic, especially useful on public networks.";
            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("vpn");
        }

        private void HandleMalwareQuestion()
        {
            if (userMemory == null) return;
            string? response = "Malware includes viruses, ransomware, and spyware that can harm your devices. " +
                "Protect against malware with updated antivirus software and safe browsing habits.";
            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("malware");
        }

        private void AddPersonalizedResponse(ref string? response)
        {
            if (userMemory != null && !string.IsNullOrEmpty(userMemory.CurrentMood))
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
            string[] followUpPhrases = { "tell me more", "more info", "explain", "what about", "can you explain",
                "please continue", "how does that work", "what does that mean", "why", "how" };
            string lowerInput = input.ToLower();
            return followUpPhrases.Any(phrase => lowerInput.Contains(phrase)) || (lowerInput.Split(' ').Length <= 3 && (lowerInput.Contains("?") ||
                lowerInput.EndsWith("though")));
        }

        private void ProvideDefaultResponse()
        {
            if (userMemory == null) return;
            string[] defaultResponses = { "I'm not quite sure I understand. Could you rephrase that? I'm best at discussing cybersecurity topics like passwords, " +
                    "phishing, and safe browsing.", "I didn't catch that. Can you try asking about cybersecurity in a different way?", "I'm still learning! " +
                    "Can you ask me about password security, phishing, or safe browsing instead?", $"Sorry {userMemory.UserName}, " +
                    $"I didn't understand your question. Ask me about cybersecurity topics like passwords, phishing scams, or internet safety." };
            AddBotMessage(defaultResponses[random.Next(defaultResponses.Length)]);
        }

        public void AddUserMessage(string message) 
        {
            if (ChatPanel == null) return;
            var border = new Border();
            border.Style = (Style)FindResource("ChatBubbleUser");
            var textBlock = new TextBlock();
            textBlock.Style = (Style)FindResource("UserChatText");
            textBlock.Text = message;
            border.Child = textBlock;
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        public void AddBotMessage(string? message)
        {
            if (ChatPanel == null || message == null) return;
            var border = new Border();
            border.Style = (Style)FindResource("ChatBubbleBot");
            var textBlock = new TextBlock();
            textBlock.Style = (Style)FindResource("BotChatText");
            textBlock.Text = message;
            border.Child = textBlock;
            ChatPanel.Children.Add(border);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (ChatScrollViewer == null) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ChatScrollViewer.ScrollToBottom(); }));
        }

        private void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            PlayVoice();
        }

        private void PlayVoice()
        {
            string filePath = @"C:\Users\nicho\Desktop\PROG_POE\ST10439055_PROG_POE\StitchVoice.mp3";
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Voice file not found.", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show($"Error playing voice file: {ex.Message}", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewTasksButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskPopup != null) TaskPopup.Visibility = Visibility.Visible;
            reminderManager?.UpdateTaskList(); 
        }

        private void TaskTitleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TaskTitleBox != null && string.IsNullOrWhiteSpace(TaskTitleBox.Text)) TaskTitleBox.Text = "Enter task title...";
        }

        private void TaskDescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TaskDescriptionBox != null && string.IsNullOrWhiteSpace(TaskDescriptionBox.Text)) TaskDescriptionBox.Text = "Enter description...";
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskTitleBox == null || TaskDescriptionBox == null || TaskReminderDate == null) return;
            if (string.IsNullOrWhiteSpace(TaskTitleBox.Text) || TaskTitleBox.Text == "Enter task title...") return;
            reminderManager?.AddTask(TaskTitleBox.Text.Trim(), TaskDescriptionBox.Text.Trim(), TaskReminderDate.SelectedDate);
            TaskTitleBox.Text = "Enter task title...";
            TaskDescriptionBox.Text = "Enter description...";
            TaskReminderDate.SelectedDate = null;
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox == null) return;
            if (TaskListBox.SelectedIndex >= 0)
            {
                reminderManager?.DeleteTask(TaskListBox.SelectedIndex);
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox == null) return;
            if (TaskListBox.SelectedIndex >= 0)
            {
                reminderManager?.CompleteTask(TaskListBox.SelectedIndex);
            }
        }

        private void CloseTaskPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskPopup != null) TaskPopup.Visibility = Visibility.Collapsed;
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            quizManager?.SubmitAnswer();
        }

        private void EndQuizButton_Click(object sender, RoutedEventArgs e)
        {
            quizManager?.EndQuiz(); 
        }

        private void ShowActivityLog()
        {
            if (userMemory == null) return;
            string log = "Activity Log:\n";

            var quizLogs = activityLog.Where(entry => entry.Description.Contains("Quiz ended with score:") || entry.Description.Contains("Quiz question")).ToList();
            if (quizLogs.Any())
            {
                log += "\nQuizzes:\n";
                foreach (var entry in quizLogs.OrderBy(e => e.Timestamp))
                {
                    if (entry.Description.Contains("Quiz ended with score:"))
                    {
                        string scorePart = entry.Description.Split("Quiz ended with score:")[1].Trim();
                        log += $"- {entry.Timestamp:yyyy-MM-dd HH:mm:ss}: {scorePart}\n";
                    }
                    else if (entry.Description.Contains("Quiz question"))
                    {
                        log += $"- {entry.Timestamp:yyyy-MM-dd HH:mm:ss}: Question answered\n";
                    }
                }
            }

            var addedTasks = activityLog.Where(entry => entry.Description.StartsWith("Task added:")).ToList();
            if (addedTasks.Any())
            {
                log += "\nTasks Added:\n";
                foreach (var entry in addedTasks.OrderBy(e => e.Timestamp))
                {
                    string taskTitle = entry.Description.Substring("Task added: ".Length);
                    log += $"- {entry.Timestamp:yyyy-MM-dd HH:mm:ss}: {taskTitle}\n";
                }
            }

            var completedTasks = activityLog.Where(entry => entry.Description.StartsWith("Task completed:")).ToList();
            if (completedTasks.Any())
            {
                log += "\nTasks Completed:\n";
                foreach (var entry in completedTasks.OrderBy(e => e.Timestamp))
                {
                    string taskTitle = entry.Description.Substring("Task completed: ".Length);
                    log += $"- {entry.Timestamp:yyyy-MM-dd HH:mm:ss}: {taskTitle}\n";
                }
            }

            var pendingTasks = reminderManager?.GetPendingTasks() ?? new List<TaskItem>();
            if (pendingTasks.Any())
            {
                log += "\nTasks Not Done:\n";
                foreach (var task in pendingTasks)
                {
                    log += $"- {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {task.Title} (Pending)\n";
                }
            }

            if (string.IsNullOrEmpty(log.Trim().Substring("Activity Log:\n".Length)))
            {
                log += "No activities recorded.";
            }
            AddBotMessage(log);
        }

        public void LogActivity(string description) 
        {
            if (activityLog.Count >= MaxLogEntries) activityLog.RemoveAt(0);
            activityLog.Add(new ActivityLogEntry { Timestamp = DateTime.Now, Description = description });
        }
    }

    public class TaskItem
    {
        public string? Title { get; set; }
        public string? Description { get; set; } 
        public DateTime? Reminder { get; set; }
        public bool IsComplete { get; set; }
    }

    public class QuizQuestion
    {
        public string Question { get; }
        public string[] Options { get; }
        public string CorrectAnswer { get; }
        public string Explanation { get; }

        public QuizQuestion(string question, string[] options, string correctAnswer, string explanation)
        {
            Question = question ?? "";
            Options = options ?? new string[0];
            CorrectAnswer = correctAnswer ?? "";
            Explanation = explanation ?? "";
        }
    }

    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
    }
}