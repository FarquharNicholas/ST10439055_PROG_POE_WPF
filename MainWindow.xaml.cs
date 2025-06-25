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
        private List<TaskItem> tasks = new List<TaskItem>();
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizIndex = 0;
        private int quizScore = 0;
        private List<ActivityLogEntry> activityLog = new List<ActivityLogEntry>();
        private const int MaxLogEntries = 10;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComponents();
            InitializeQuizQuestions();
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
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions.Add(new QuizQuestion("What should you do if you receive an email asking for your password?", 
                new[] { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" }, "C", "Reporting phishing emails helps prevent scams."));
            quizQuestions.Add(new QuizQuestion("True or False: A strong password should include personal information like your birthdate.",
                new[] { "True", "False" }, "False", "Personal information makes passwords easier to guess."));
            quizQuestions.Add(new QuizQuestion("Which of these is a safe browsing practice?", 
                new[] { "A) Clicking on unknown links", "B) Using HTTPS websites", "C) Downloading files from untrusted sites", "D) Ignoring software updates" }, "B", "HTTPS encrypts your connection, protecting your data."));
            quizQuestions.Add(new QuizQuestion("What is a common sign of a phishing attempt?",
                new[] { "A) Professional email design", "B) Urgent language", "C) Verified sender address", "D) Clear company logo" }, "B", "Urgent language is often used to pressure victims."));
            quizQuestions.Add(new QuizQuestion("True or False: Social engineering relies on technical hacking.",
                new[] { "True", "False" }, "False", "Social engineering exploits human psychology, not just technical methods."));
            quizQuestions.Add(new QuizQuestion("Which password is the strongest?", 
                new[] { "A) Password123", "B) Tr0ub4dor&3", "C) MyName2023", "D) 12345678" }, "B", "A mix of letters, numbers, and symbols with no personal info is strongest."));
            quizQuestions.Add(new QuizQuestion("What should you do to protect against malware?", 
                new[] { "A) Open all email attachments", "B) Use updated antivirus software", "C) Disable firewall", "D) Share passwords" }, "B", "Antivirus software helps detect and remove malware."));
            quizQuestions.Add(new QuizQuestion("True or False: Public Wi-Fi is safe for online banking.", 
                new[] { "True", "False" }, "False", "Public Wi-Fi can be insecure; use a VPN instead."));
            quizQuestions.Add(new QuizQuestion("Which is a safe way to spot phishing emails?",
                new[] { "A) Checking the sender’s email address", "B) Clicking all links", "C) Replying to verify", "D) Ignoring grammar errors" }, "A", "Verifying the sender’s address helps identify fakes."));
            quizQuestions.Add(new QuizQuestion("What is a benefit of two-factor authentication (2FA)?", 
                new[] { "A) Easier password creation", "B) Extra security layer", "C) Faster logins", "D) Less device usage" }, "B", "2FA adds an additional step to verify your identity."));
        }

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
            if (string.IsNullOrWhiteSpace(UserInputBox.Text)) UserInputBox.Text = "";
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
                    var result = MessageBox.Show($"Goodbye {userMemory.UserName}! Stay secure!\nDo you want to close the application?", "Goodbye", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            if (userMemory == null || sentimentDetector == null || interestDetector == null || QuizPopup == null) return;
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
            // Enhanced quiz trigger
            string lowerInput = input.ToLower();
            string[] quizTriggers = { "quiz", "game", "play", "test", "challenge" };
            if (quizTriggers.Any(trigger => lowerInput.Contains(trigger)))
            {
                currentQuizIndex = 0;
                quizScore = 0;
                QuizPopup.Visibility = Visibility.Visible;
                LoadNextQuizQuestion();
                AddBotMessage("Starting quiz! Select an answer and submit to begin.");
            }
        }

        private bool HandleNLPInput(string input)
        {
            if (userMemory == null) return false;
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("add task") || lowerInput.Contains("create reminder") || lowerInput.Contains("add reminder"))
            {
                AddTaskFromInput(input);
                return true;
            }
            else if (lowerInput.Contains("show activity log"))
            {
                ShowActivityLog();
                return true;
            }
            else if (lowerInput.Contains("remind me to") || lowerInput.Contains("set reminder for"))
            {
                SetReminderFromInput(input);
                return true;
            }
            return false;
        }

        private void AddTaskFromInput(string input)
        {
            if (userMemory == null) return;
            string? title = "New Task";
            string? description = "No description";
            DateTime? reminder = null;
            string[] words = input.Split(' ');
            for (int i = 1; i < words.Length; i++)
            {
                if (words[i].ToLower() == "to") title = string.Join(" ", words.Skip(i + 1));
                else if (words[i].ToLower().Contains("remind")) reminder = DateTime.Now.AddDays(1);
            }
            var task = new TaskItem { Title = title, Description = description, Reminder = reminder };
            tasks.Add(task);
            AddBotMessage($"Task added: '{title}'. Would you like to set a reminder?");
            LogActivity($"Task added: {title}");
        }

        private void SetReminderFromInput(string input)
        {
            if (userMemory == null) return;
            string? title = input.Contains("to") ? input.Split(new[] { "to" }, StringSplitOptions.None)[1].Trim() : "Update Security";
            int days = 1;
            if (input.ToLower().Contains("tomorrow")) days = 1;
            else if (input.ToLower().Contains("days"))
            {
                string[] parts = input.Split(new[] { "days" }, StringSplitOptions.None);
                if (parts.Length > 1) int.TryParse(parts[0].Split().Last(), out days);
            }
            var task = new TaskItem { Title = title, Description = "Reminder set", Reminder = DateTime.Now.AddDays(days) };
            tasks.Add(task);
            AddBotMessage($"Reminder set for '{title}' on {task.Reminder.Value.ToShortDateString()}.");
            LogActivity($"Reminder set: {title} for {days} days");
        }

        private bool TryHandleBasicQuestions(string input)
        {
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("how are you") || lowerInput.Contains("how you doing") || lowerInput.Contains("purpose") || lowerInput.Contains("why do you exist") || lowerInput.Contains("ask") || lowerInput.Contains("questions") || lowerInput.Contains("thank") || lowerInput.Contains("appreciate") || lowerInput.Contains("hello") || lowerInput.Contains("hi") || lowerInput.Contains("greetings"))
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
                string[] responses = { $"I'm functioning optimally! How can I help you with cybersecurity today, {userMemory.UserName}?", $"I'm just a bot, but I'm here and ready to help you stay safe online, {userMemory.UserName}.", $"I'm always ready to discuss cybersecurity with you, {userMemory.UserName}! What would you like to know?" };
                response = responses[random.Next(responses.Length)];
            }
            else if (lowerInput.Contains("purpose") || lowerInput.Contains("why do you exist"))
            {
                string[] responses = { "I'm here to educate you about cybersecurity and help you avoid online threats.", $"My purpose is to make cybersecurity accessible for you, {userMemory.UserName}.", "I exist to help people navigate online security with confidence!" };
                response = responses[random.Next(responses.Length)];
                userMemory.SetLastTopic("purpose");
            }
            else if (lowerInput.Contains("ask") || lowerInput.Contains("questions"))
            {
                response = "You can ask me about:\n• Password safety\n• Phishing\n• Safe browsing\n• VPNs\n• Password managers\n• Cyber threats\nOr start a quiz with 'quiz'!";
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
            if (userMemory == null) return;
            string? response;
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
            if (userMemory == null) return;
            string? response = "A VPN encrypts your internet connection, protecting your privacy online. VPNs create secure tunnels for your internet traffic, especially useful on public networks.";
            AddPersonalizedResponse(ref response);
            AddBotMessage(response);
            userMemory.SetLastTopic("vpn");
        }

        private void HandleMalwareQuestion()
        {
            if (userMemory == null) return;
            string? response = "Malware includes viruses, ransomware, and spyware that can harm your devices. Protect against malware with updated antivirus software and safe browsing habits.";
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
            string[] followUpPhrases = { "tell me more", "more info", "explain", "what about", "can you explain", "please continue", "how does that work", "what does that mean", "why", "how" };
            string lowerInput = input.ToLower();
            return followUpPhrases.Any(phrase => lowerInput.Contains(phrase)) || (lowerInput.Split(' ').Length <= 3 && (lowerInput.Contains("?") || lowerInput.EndsWith("though")));
        }

        private void ProvideDefaultResponse()
        {
            if (userMemory == null) return;
            string[] defaultResponses = { "I'm not quite sure I understand. Could you rephrase that? I'm best at discussing cybersecurity topics like passwords, phishing, and safe browsing.", "I didn't catch that. Can you try asking about cybersecurity in a different way?", "I'm still learning! Can you ask me about password security, phishing, or safe browsing instead?", $"Sorry {userMemory.UserName}, I didn't understand your question. Ask me about cybersecurity topics like passwords, phishing scams, or internet safety." };
            AddBotMessage(defaultResponses[random.Next(defaultResponses.Length)]);
        }

        private void AddUserMessage(string message)
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

        private void AddBotMessage(string? message)
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
            UpdateTaskList();
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
            var task = new TaskItem { Title = TaskTitleBox.Text.Trim(), Description = TaskDescriptionBox.Text.Trim(), Reminder = TaskReminderDate.SelectedDate };
            tasks.Add(task);
            UpdateTaskList();
            AddBotMessage($"Task added: '{task.Title}' with reminder on {task.Reminder?.ToShortDateString() ?? "none"}.");
            LogActivity($"Task added: {task.Title}");
            TaskTitleBox.Text = "Enter task title...";
            TaskDescriptionBox.Text = "Enter description...";
            TaskReminderDate.SelectedDate = null;
        }

        private void UpdateTaskList()
        {
            if (TaskListBox == null) return;
            TaskListBox.Items.Clear();
            foreach (var task in tasks)
            {
                TaskListBox.Items.Add($"{task.Title} - {task.Description} (Reminder: {task.Reminder?.ToShortDateString() ?? "None"}) - {(task.IsComplete ? "Completed" : "Pending")}");
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox == null) return;
            if (TaskListBox.SelectedIndex >= 0)
            {
                tasks.RemoveAt(TaskListBox.SelectedIndex);
                UpdateTaskList();
                AddBotMessage("Task deleted.");
                LogActivity("Task deleted");
            }
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox == null) return;
            if (TaskListBox.SelectedIndex >= 0)
            {
                tasks[TaskListBox.SelectedIndex].IsComplete = true;
                UpdateTaskList();
                AddBotMessage("Task marked as complete.");
                LogActivity($"Task completed: {tasks[TaskListBox.SelectedIndex].Title}");
            }
        }

        private void CloseTaskPopupButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskPopup != null) TaskPopup.Visibility = Visibility.Collapsed;
        }

        private void LoadNextQuizQuestion()
        {
            if (QuizQuestionText == null || QuizOptionsList == null || QuizScoreText == null || QuizFeedbackText == null) return;
            if (currentQuizIndex >= quizQuestions.Count)
            {
                EndQuizButton_Click(null, null);
                return;
            }
            QuizQuestionText.Text = quizQuestions[currentQuizIndex].Question;
            QuizOptionsList.Items.Clear();
            foreach (var option in quizQuestions[currentQuizIndex].Options) QuizOptionsList.Items.Add(option);
            QuizFeedbackText.Text = "";
            QuizScoreText.Text = $"Score: {quizScore}/{quizQuestions.Count}";
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizOptionsList == null || QuizFeedbackText == null) return;
            if (QuizOptionsList.SelectedItem == null)
            {
                QuizFeedbackText.Text = "Please select an answer.";
                return;
            }
            string selectedAnswer = QuizOptionsList.SelectedItem.ToString();
            bool isCorrect = selectedAnswer == quizQuestions[currentQuizIndex].CorrectAnswer;
            if (isCorrect) quizScore++;
            QuizFeedbackText.Text = isCorrect ? "Correct! " + quizQuestions[currentQuizIndex].Explanation : "Wrong. " + quizQuestions[currentQuizIndex].Explanation;
            currentQuizIndex++;
            LoadNextQuizQuestion();
            LogActivity($"Quiz question {currentQuizIndex} answered: {isCorrect}");
        }

        private void EndQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizPopup != null)
            {
                string feedback = quizScore >= 7 ? "Great job!" : "Keep learning!";
                AddBotMessage($"Quiz ended. Your score: {quizScore}/10. {feedback}");
                QuizPopup.Visibility = Visibility.Collapsed;
                LogActivity($"Quiz ended with score: {quizScore}/10");
                currentQuizIndex = 0;
                quizScore = 0;
            }
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

            
            var pendingTasks = tasks.Where(t => !t.IsComplete).ToList();
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

        private void LogActivity(string description)
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