using Azure.AI.OpenAI;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            OpenAIAPI api = new OpenAIAPI("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");
            List<string> actions = new List<string> { "mở", "khởi động" };
            List<string> forms = new List<string> { "Form1", "Form2", "SettingsForm", "UserForm" };
            string userInput = "khởi chạy form 1";
            Console.WriteLine("Please enter your command:");
            string prompt = $"Xác định hành động và đối tượng từ câu lệnh sau: \"{userInput}\"\nHành động bao gồm: {actions}\nCác đối tượng bao gồm: {forms}\nNhãn hành động cần trả về có dạng 'open_' theo sau là tên của form.";
            MessageBox.Show((await api.Chat.CreateChatCompletionAsync(prompt)).ToString());
        }
    }
}
