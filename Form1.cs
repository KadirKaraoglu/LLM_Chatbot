using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LLM_Chatbot
{
    public partial class Form1 : Form
    {
        // Using a single HttpClient instance throughout the application improves performance
        private static readonly HttpClient client = new HttpClient();

        // ATTENTION: Paste your Google AI Studio API key here
        private const string apiKey = "Api_key";

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Leave empty
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Leave empty
        }

        // Asynchronous method to send a request to the Gemini 1.5 Flash model
        private async Task<string> AskLLMAsync(string prompt)
        {
            // Gemini 1.5 Flash API URL
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            // Prepare the request body in JSON format
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request to the API and wait for the response
                HttpResponseMessage response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                // Read the complex JSON response
                string responseJson = await response.Content.ReadAsStringAsync();

                // Extract only the generated text from the JSON structure
                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    var root = doc.RootElement;

                    string aiResponse = root.GetProperty("candidates")[0]
                                            .GetProperty("content")
                                            .GetProperty("parts")[0]
                                            .GetProperty("text")
                                            .GetString();

                    return aiResponse;
                }
            }
            catch (Exception ex)
            {
                return $"[Connection Error]: {ex.Message}";
            }
        }

        // Event handler for the "Send" button click
        private async void button1_Click(object sender, EventArgs e)
        {
            // Get the text from the input box and remove leading/trailing whitespaces
            string userText = textBox2.Text.Trim();

            // If the input is empty or just spaces, do nothing
            if (string.IsNullOrEmpty(userText)) return;

            // 1. Display the user's message in the main chat history box
            textBox1.Text += $"You: {userText}\r\n";

            // 2. Clear the input box for the next message
            textBox2.Clear();

            // 3. Disable the send button while waiting for the API to prevent spam clicks
            button1.Enabled = false;

            // 4. Send the message to the API and await the bot's reply
            string botReply = await AskLLMAsync(userText);

            // 5. Display the bot's reply in the main chat history box (with an extra blank line)
            textBox1.Text += $"Bot: {botReply}\r\n\r\n";

            // Automatically scroll to the bottom as the chat fills up
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();

            // 6. Re-enable the send button
            button1.Enabled = true;
        }
    }
}