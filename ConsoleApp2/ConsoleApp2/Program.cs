// See https://aka.ms/new-console-template for more information

using Azure.AI.OpenAI;

Console.OutputEncoding = System.Text.Encoding.UTF8;
OpenAIClient client = new OpenAIClient("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");
var actions = new List<string> { "mở", "khởi động" }; // thêm các hành động khác nếu cần
var forms = new List<string> { "Form1", "Form2", "SettingsForm", "UserForm" }; // thêm tên các forms khác nếu cần
string userInput = "khởi chạy form 1";


Console.WriteLine("Please enter your command:");
string prompt = $"Xác định hành động và đối tượng từ câu lệnh sau: \"{userInput}\"\n" +
               $"Hành động bao gồm: {actions}\n" +
               $"Các đối tượng bao gồm: {forms}\n" +
               $"Nhãn hành động cần trả về có dạng 'open_' theo sau là tên của form.";
var result = await client.GetChatCompletionsAsync("gpt-3.5-turbo", new ChatCompletionsOptions
{
    Messages = { new ChatMessage(ChatRole.System, prompt) }
});

foreach (var item in result.Value.Choices)
{
    Console.WriteLine(item.Message.Content);
}
