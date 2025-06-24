namespace ST10439055_PROG_POE_WPF
{
    public class SecurityQuestionsHandler : UserInteraction
    {
        private Dictionary<string, List<string>> securityResponses;

        public SecurityQuestionsHandler(UserMemory memory) : base(memory)
        {
            InitializeSecurityResponses();
        }

        private void InitializeSecurityResponses()
        {
            securityResponses = new Dictionary<string, List<string>>
            {
                { "password_general", new List<string> {
                    "For password security:\n- Use unique passwords for each account\n- Change them regularly\n- Never share them",
                    "Your passwords are your first line of defense. Make them unique and complex.",
                    "Think of passwords like keys to your digital home - each should be unique."
                }},
                { "password_strength", new List<string> {
                    "Strong passwords should:\n- Be at least 12 characters\n- Mix letters, numbers, and symbols\n- Avoid personal information",
                    "Create passwords that are long passphrases - easy for you to remember but hard to guess.",
                    "The best passwords combine random words with numbers and symbols."
                }},
                { "password_manager", new List<string> {
                    "Password managers securely store and generate strong passwords for all your accounts.",
                    "A password manager acts like a digital vault for all your login credentials.",
                    "Using a password manager means you only need to remember one master password."
                }},
                { "phishing_general", new List<string> {
                    "Phishing is when attackers pretend to be trustworthy to steal your data.",
                    "Phishing scams trick you into revealing sensitive information through fake communications.",
                    "Phishing attacks often use urgency or fear to pressure victims."
                }},
                { "phishing_identification", new List<string> {
                    "Spot phishing by checking:\n- Sender addresses\n- Urgent language\n- Requests for sensitive data",
                    "Look for poor grammar, generic greetings, and mismatched URLs in suspicious messages.",
                    "Legitimate organizations rarely ask for sensitive info via email."
                }},
                { "browsing_general", new List<string> {
                    "Safe browsing tips:\n- Look for HTTPS\n- Avoid public Wi-Fi for sensitive activities\n- Keep browsers updated",
                    "HTTPS encrypts your connection to websites, protecting your data.",
                    "Be cautious about downloads and keep software updated for safer browsing."
                }},
                { "vpn", new List<string> {
                    "A VPN encrypts your internet connection, protecting your privacy online.",
                    "VPNs create secure tunnels for your internet traffic, especially useful on public networks.",
                    "Using a VPN helps hide your online activities and location from prying eyes."
                }},
                { "two_factor", new List<string> {
                    "Two-factor authentication adds an extra security layer beyond just passwords.",
                    "2FA requires both something you know (password) and something you have (like your phone).",
                    "Enable two-factor authentication whenever possible for better account security."
                }},
                { "updates", new List<string> {
                    "Software updates often include critical security patches for known vulnerabilities.",
                    "Keeping software updated is like getting vaccines for your digital devices.",
                    "Enable automatic updates to ensure you're always protected."
                }},
                { "social_engineering", new List<string> {
                    "Social engineering exploits human psychology rather than technical vulnerabilities.",
                    "Be wary of unexpected requests, even from known contacts.",
                    "Social engineering manipulates people into compromising security."
                }},
                { "backup", new List<string> {
                    "Regular backups protect against ransomware and data loss.",
                    "Follow the 3-2-1 backup rule: 3 copies, 2 media types, 1 off-site.",
                    "Automated backups ensure you don't forget to protect your data."
                }},
                { "malware", new List<string> {
                    "Malware includes viruses, ransomware, and spyware that can harm your devices.",
                    "Protect against malware with updated antivirus software and safe browsing habits.",
                    "Be cautious with downloads and email attachments to avoid malware infections."
                }}
            };
        }

        public override bool HandleInput(string input)
        {
            string lowerInput = input.ToLower();
            bool handled = true;

            if (MatchesKeyword(lowerInput, new[] { "password", "passwords" }))
            {
                HandlePasswordQuestions(input);
                userMemory.SetLastTopic("password");
            }
            else if (MatchesKeyword(lowerInput, new[] { "phish", "phishing", "scam", "scams" }))
            {
                HandlePhishingQuestions(input);
                userMemory.SetLastTopic("phishing");
            }
            else if (MatchesKeyword(lowerInput, new[] { "browsing", "browser", "internet", "web" }))
            {
                HandleBrowsingQuestions(input);
                userMemory.SetLastTopic("browsing");
            }
            else if (MatchesKeyword(lowerInput, new[] { "vpn", "virtual private network" }))
            {
                HandleVPNQuestion();
                userMemory.SetLastTopic("vpn");
            }
            else if (MatchesKeyword(lowerInput, new[] { "two factor", "2fa", "mfa" }))
            {
                RespondWithTopic("two_factor");
                userMemory.SetLastTopic("two_factor");
            }
            else if (MatchesKeyword(lowerInput, new[] { "update", "updates", "patch" }))
            {
                RespondWithTopic("updates");
                userMemory.SetLastTopic("updates");
            }
            else if (MatchesKeyword(lowerInput, new[] { "social engineering" }))
            {
                RespondWithTopic("social_engineering");
                userMemory.SetLastTopic("social_engineering");
            }
            else if (MatchesKeyword(lowerInput, new[] { "backup", "backups" }))
            {
                RespondWithTopic("backup");
                userMemory.SetLastTopic("backup");
            }
            else if (MatchesKeyword(lowerInput, new[] { "malware", "virus", "ransomware" }))
            {
                RespondWithTopic("malware");
                userMemory.SetLastTopic("malware");
            }
            else if (MatchesKeyword(lowerInput, new[] { "identify", "spot", "recognize" }))
            {
                string relevantTopic = DetermineRelevantThreatTopic();
                if (relevantTopic == "phishing")
                {
                    RespondWithTopic("phishing_identification");
                }
                else
                {
                    HandlePhishingIdentification();
                }
                userMemory.SetLastTopic("identification");
            }
            else if (MatchesKeyword(lowerInput, new[] { "manager", "password manager" }))
            {
                HandlePasswordManagerQuestion();
                userMemory.SetLastTopic("password_manager");
            }
            else if (MatchesKeyword(lowerInput, new[] { "strong", "strength", "secure password" }))
            {
                HandlePasswordStrengthQuestion();
                userMemory.SetLastTopic("password_strength");
            }
            else if (MatchesKeyword(lowerInput, new[] { "privacy", "private", "data protection" }))
            {
                userMemory.AddInterest("privacy");
                RespondWithTopic("privacy");
                userMemory.SetLastTopic("privacy");
            }
            else
            {
                handled = false;
            }

            return handled;
        }

        private bool MatchesKeyword(string input, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        private string DetermineRelevantThreatTopic()
        {
            // Look at recent conversation history to determine context
            var history = userMemory.GetConversationHistory();
            foreach (var message in history)
            {
                if (message.Contains("phish") || message.Contains("scam") || message.Contains("email"))
                {
                    return "phishing";
                }
                else if (message.Contains("malware") || message.Contains("virus"))
                {
                    return "malware";
                }
                else if (message.Contains("social engineering"))
                {
                    return "social_engineering";
                }
            }

            // Default to phishing if no context
            return "phishing";
        }

        private void HandlePasswordQuestions(string question)
        {
            question = question.ToLower();

            if (question.Contains("strong") || question.Contains("create"))
            {
                HandlePasswordStrengthQuestion();
            }
            else if (question.Contains("manager"))
            {
                HandlePasswordManagerQuestion();
            }
            else
            {
                RespondWithTopic("password_general");
            }
        }

        private void HandlePhishingQuestions(string question)
        {
            question = question.ToLower();

            if (question.Contains("identify") || question.Contains("spot") ||
                question.Contains("recognize") || question.Contains("detect"))
            {
                HandlePhishingIdentification();
            }
            else
            {
                RespondWithTopic("phishing_general");
            }
        }

        private void HandleBrowsingQuestions(string question)
        {
            question = question.ToLower();

            if (question.Contains("vpn"))
            {
                HandleVPNQuestion();
            }
            else
            {
                RespondWithTopic("browsing_general");
            }
        }

        private void HandleVPNQuestion()
        {
            RespondWithTopic("vpn");
        }

        private void HandlePhishingIdentification()
        {
            RespondWithTopic("phishing_identification");
        }

        private void HandlePasswordManagerQuestion()
        {
            RespondWithTopic("password_manager");
        }

        private void HandlePasswordStrengthQuestion()
        {
            RespondWithTopic("password_strength");
        }

        private void RespondWithTopic(string topicKey)
        {
            if (securityResponses.ContainsKey(topicKey))
            {
                var responses = securityResponses[topicKey];
                string response = responses[random.Next(responses.Count)];

                // Add personalized touch based on user memory
                AddPersonalisedTouch(ref response);

                Console.WriteLine($"Stitch: {response}");
            }
            else
            {
                Console.WriteLine("Stitch: I don't have specific information on that topic yet, but I'm learning!");
            }
        }
    }
}