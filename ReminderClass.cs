using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ST10439055_PROG_POE_WPF
{
    public class ReminderClass
    {
        private readonly MainWindow mainWindow;
        private List<TaskItem> tasks;

        public ReminderClass(MainWindow window, List<TaskItem> taskList)
        {
            mainWindow = window;
            tasks = taskList;
        }

        public void SetReminderFromInput(string input)
        {
            if (mainWindow.UserMemory == null) return;
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
            mainWindow.AddBotMessage($"Reminder set for '{title}' on {task.Reminder.Value.ToShortDateString()}.");
            mainWindow.LogActivity($"Reminder set: {title} for {days} days");
        }

        public void AddTask(string title, string description, DateTime? reminder)
        {
            var task = new TaskItem { Title = title, Description = description, Reminder = reminder };
            tasks.Add(task);
            UpdateTaskList();
            mainWindow.AddBotMessage($"Task added: '{task.Title}' with reminder on {task.Reminder?.ToShortDateString() ?? "none"}.");
            mainWindow.LogActivity($"Task added: {task.Title}");
        }

        public void UpdateTaskList()
        {
            if (mainWindow.TaskListBox == null) return;
            mainWindow.TaskListBox.Items.Clear();
            foreach (var task in tasks)
            {
                mainWindow.TaskListBox.Items.Add($"{task.Title} - {task.Description} (Reminder: {task.Reminder?.ToShortDateString() ?? "None"}) - {(task.IsComplete ? "Completed" : "Pending")}");
            }
        }

        public void DeleteTask(int index)
        {
            if (mainWindow.TaskListBox == null) return;
            if (index >= 0 && index < tasks.Count)
            {
                tasks.RemoveAt(index);
                UpdateTaskList();
                mainWindow.AddBotMessage("Task deleted.");
                mainWindow.LogActivity("Task deleted");
            }
        }

        public void CompleteTask(int index)
        {
            if (mainWindow.TaskListBox == null) return;
            if (index >= 0 && index < tasks.Count)
            {
                tasks[index].IsComplete = true;
                UpdateTaskList();
                mainWindow.AddBotMessage("Task marked as complete.");
                mainWindow.LogActivity($"Task completed: {tasks[index].Title}");
            }
        }

        public List<TaskItem> GetPendingTasks()
        {
            return tasks.Where(t => !t.IsComplete).ToList();
        }
    }
}