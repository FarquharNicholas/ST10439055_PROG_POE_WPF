using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ST10439055_PROG_POE_WPF
{
    public class QuizClass
    {
        private readonly MainWindow mainWindow;
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizIndex = 0;
        private int quizScore = 0;
        private bool isQuizActive = false;

        public QuizClass(MainWindow window)
        {
            mainWindow = window;
            InitializeQuizQuestions();
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions.Add(new QuizQuestion("What should you do if you receive an email asking for your password?",
                new[] { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                "C", "Reporting phishing emails helps prevent scams."));
            quizQuestions.Add(new QuizQuestion("True or False: A strong password should include personal information like your birthdate.",
                new[] { "True", "False" },
                "False", "Personal information makes passwords easier to guess."));
            quizQuestions.Add(new QuizQuestion("Which of these is a safe browsing practice?",
                new[] { "A) Clicking on unknown links", "B) Using HTTPS websites", "C) Downloading files from untrusted sites", "D) Ignoring software updates" },
                "B", "HTTPS encrypts your connection, protecting your data."));
            quizQuestions.Add(new QuizQuestion("What is a common sign of a phishing attempt?",
                new[] { "A) Professional email design", "B) Urgent language", "C) Verified sender address", "D) Clear company logo" },
                "B", "Urgent language is often used to pressure victims."));
            quizQuestions.Add(new QuizQuestion("True or False: Social engineering relies on technical hacking.",
                new[] { "True", "False" },
                "False", "Social engineering exploits human psychology, not just technical methods."));
            quizQuestions.Add(new QuizQuestion("Which password is the strongest?",
                new[] { "A) Password123", "B) Tr0ub4dor&3", "C) MyName2023", "D) 12345678" },
                "B", "A mix of letters, numbers, and symbols with no personal info is strongest."));
            quizQuestions.Add(new QuizQuestion("What should you do to protect against malware?",
                new[] { "A) Open all email attachments", "B) Use updated antivirus software", "C) Disable firewall", "D) Share passwords" },
                "B", "Antivirus software helps detect and remove malware."));
            quizQuestions.Add(new QuizQuestion("True or False: Public Wi-Fi is safe for online banking.",
                new[] { "True", "False" },
                "False", "Public Wi-Fi can be insecure; use a VPN instead."));
            quizQuestions.Add(new QuizQuestion("Which is a safe way to spot phishing emails?",
                new[] { "A) Checking the sender’s email address", "B) Clicking all links", "C) Replying to verify", "D) Ignoring grammar errors" },
                "A", "Verifying the sender’s address helps identify fakes."));
            quizQuestions.Add(new QuizQuestion("What is a benefit of two-factor authentication (2FA)?",
                new[] { "A) Easier password creation", "B) Extra security layer", "C) Faster logins", "D) Less device usage" },
                "B", "2FA adds an additional step to verify your identity."));
        }

        public void StartQuiz()
        {
            if (isQuizActive) return; // Prevent starting a new quiz if one is active
            currentQuizIndex = 0;
            quizScore = 0;
            isQuizActive = true;
            mainWindow.QuizPopup.Visibility = System.Windows.Visibility.Visible;
            LoadNextQuizQuestion();
            mainWindow.AddBotMessage("Starting quiz! Select an answer and submit to begin.");
        }

        public void LoadNextQuizQuestion()
        {
            if (mainWindow.QuizQuestionText == null || mainWindow.QuizOptionsList == null || mainWindow.QuizScoreText == null || mainWindow.QuizFeedbackText == null) return;
            if (currentQuizIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }
            mainWindow.QuizQuestionText.Text = quizQuestions[currentQuizIndex].Question;
            mainWindow.QuizOptionsList.Items.Clear();
            foreach (var option in quizQuestions[currentQuizIndex].Options) mainWindow.QuizOptionsList.Items.Add(option);
            mainWindow.QuizFeedbackText.Text = "";
            mainWindow.QuizScoreText.Text = $"Score: {quizScore}/10";
            mainWindow.QuizOptionsList.SelectedIndex = -1; 
        }

        public void SubmitAnswer()
        {
            if (mainWindow.QuizOptionsList == null || mainWindow.QuizFeedbackText == null || !isQuizActive) return;

            if (currentQuizIndex >= quizQuestions.Count)
            {
                mainWindow.QuizFeedbackText.Text = "Quiz has ended. Please start a new one.";
                return;
            }

            if (mainWindow.QuizOptionsList.SelectedItem == null)
            {
                mainWindow.QuizFeedbackText.Text = "Please select an answer.";
                return;
            }

            string selectedAnswer = mainWindow.QuizOptionsList.SelectedItem.ToString();
            QuizQuestion currentQuestion = quizQuestions[currentQuizIndex];
            bool isCorrect = selectedAnswer == currentQuestion.CorrectAnswer;

            if (isCorrect) quizScore++;
            mainWindow.QuizFeedbackText.Text = isCorrect
                ? $"Correct! {currentQuestion.Explanation}"
                : $"Wrong. The correct answer is {currentQuestion.CorrectAnswer}. {currentQuestion.Explanation}";
            currentQuizIndex++;
            mainWindow.LogActivity($"Quiz question {currentQuizIndex} answered: {isCorrect}");

            if (currentQuizIndex < quizQuestions.Count)
            {
                LoadNextQuizQuestion();
            }
            else
            {
                EndQuiz();
            }
        }

        public void EndQuiz()
        {
            if (!isQuizActive || mainWindow.QuizPopup == null) return;
            isQuizActive = false;
            string feedback = quizScore >= 7 ? "Great job!" : "Keep learning!";
            mainWindow.AddBotMessage($"Quiz ended. Your score: {quizScore}/{quizQuestions.Count}. {feedback}");
            mainWindow.QuizPopup.Visibility = System.Windows.Visibility.Collapsed;
            mainWindow.LogActivity($"Quiz ended with score: {quizScore}/{quizQuestions.Count}");
            currentQuizIndex = 0;
            quizScore = 0;
        }
    }
}