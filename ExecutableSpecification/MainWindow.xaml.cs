using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Newtonsoft.Json;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace ExecutableSpecification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenAIService _service;
        List<ChatMessage> _messages;
        Window? _window;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetupWindowButtons()
        {
            // every button in the window should invoke Button_Click
            foreach (var btn in FindElementsInWindow<Button>(_window))
            {
                btn.Click += Button_Clicked;
            }
        }

        private static IEnumerable<T> FindElementsInWindow<T>(FrameworkElement frameworkElement) where T : FrameworkElement
        {
            var result = new List<T>();

            foreach (var child in LogicalTreeHelper.GetChildren(frameworkElement))
            {
                if (child is T c)
                    result.Add(c);

                if (child is FrameworkElement fe)
                    result.AddRange(FindElementsInWindow<T>(fe));
            }
            return result;
        }


        private async Task<dynamic?> GetStateFromAI(string prompt)
        {
            tb_Status.Text = "Waiting for state...";
            var json = await GetAnswer(prompt);

            if (json == null)
                return null;

            dynamic result = JsonConvert.DeserializeObject<ExpandoObject>(json);
            tb_Status.Text = "";

            return result;
        }

        private DateTime _lastCallTime = DateTime.MinValue;
        private readonly TimeSpan _minimumCallInterval = TimeSpan.FromSeconds(2);

        private async Task<string?> GetAnswer(string userPrompt)
        {
            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeSinceLastCall = currentTime - _lastCallTime;

            if (timeSinceLastCall < _minimumCallInterval)
            {
                TimeSpan timeToWait = _minimumCallInterval - timeSinceLastCall;
                await Task.Delay(timeToWait);
            }

            _messages.Add(new ChatMessage("user", userPrompt));

            Console.WriteLine($"USER: {userPrompt}\n");

            ChatCompletionCreateRequest request = new ChatCompletionCreateRequest();
            request.Messages = _messages;

            var answer = await _service.ChatCompletion.CreateCompletion(request, Models.Model.ChatGpt3_5Turbo.EnumToString());
            _lastCallTime = DateTime.UtcNow;

            var msg = answer.Choices[0].Message;
            _messages.Add(msg);

            Console.WriteLine($"\nAI:\n{msg.Content}\n\n");

            return msg.Content;
        }

        private async void Button_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var state = await GetStateFromAI($"The user has clicked button '{btn.Name}'. Please provide me with the updated JSON state");
            _window.DataContext = state;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.Close();
                _window.DataContext = null;
                _window = null;
            }

            OpenAiOptions options = OpenAIHelper.GetOpenAiOptions();

            _service = new(options);
            _messages = new();

            var systemPrompt = @"Here's a specification of a simple WPF application.

--------
<!<!Specification!>!>
--------

The application has the following components:

- The XAML code, which can not be changed while the application is running. The XAML code is a Window. Do not include the Class definition as this Window won't have a codebehind. You can differentiate between different controls (such as multiple buttons) by providing them with an x:Name.

- The DataContext, representing the application's state. This is a JSON data, bound to the root of the application.
The structure of the JSON data never changes while the application is running, but it's the data itself can.
The DataContext should include any internal state needed by the application, and anything on the UI that potentially changes while the application is running. Data will be bound to the UI without any converters. Static text should not be included in the DataContext.

To display information in the application, the JSON DataContext is updated. The XAML uses data binding to display the information from the DataContext.

When the XAML or JSON is requested, do not provide any explanation. Do not enclose the XAML or JSON in a Markup code block, or other characters. Just provide the raw XAML or JSON.

The main loop is as follows:
1. I will let you know if the user has interacted with the UI.
2. You will then generate an updated JSON to reflect the application's state as it reacts to the user's input.";

            systemPrompt = systemPrompt.Replace("<!<!Specification!>!>", tb_Specification.Text);
            _messages.Add(new ChatMessage("system", systemPrompt));

            tb_Status.Text = "Preparing app (Step 1 of 2)";
            var state = await GetStateFromAI("First, provide me with the initial state (DataContext) in JSON format.");

            tb_Status.Text = "Preparing app (Step 2 of 2)";
            var xaml = await GetAnswer("Next, provide me with the XAML, and only the XAML. Do not include an x:Class attribute. Include any xmlns as needed. Do NOT use any converters, event handlers, or commands.");


            while (_window == null)
            {
                try
                {
                    _window = (Window)XamlReader.Parse(xaml);
                    SetupWindowButtons();
                    _window.DataContext = state;
                    _window.Left = this.Left + this.Width / 2 - _window.Width / 2;
                    _window.Top = this.Top + this.Height / 2 - _window.Height / 2;
                    _window.Show();
                }
                catch (Exception exception)
                {
                    tb_Status.Text = $"Error: {exception.Message}... retrying";

                    xaml = await GetAnswer(
                        $"I got an error with that XAML. Let's try again. Do not include an x:Class attribute. Include any xmlns as needed. Do NOT use any converters, event handlers, or commands. The error was: {exception.Message}." +
                        $"Do not apologize or add comments. Just provide me with the fixed XAML.");
                }
            }

            tb_Status.Text = "";
        }
    }
}
