# ExecutableSpecification

This project is a proof of concept to see if a written specification can be used to create a simple, throwaway working WPF app. The app doesn't have any running code (XAML is not code!), the business logic is entirely ran by the AI interpreting the specification after every interaction.

The proof of concept can handle the following simple app:

```
The application is called "Counter".
The application window is resizable.
There is a button labeled "Click Me" in the middle of the window.
Below the button, there is a text that says "Not clicked yet"
When the user clicks the Button, the text should change to "Clicked 1 times"
If the user clicks the button again, the text should change to indicate the number of times the button has been clicked.
If the user has clicked the button 3 times, the button should be disabled, and the text should change to "That's enough!"

The app supports multiple languages.
The app should also have a button in the lower left corner that toggles the language between English and Hungarian.
Pressing this will change every text in the app to the other language (including button labels).
The app should launch in Hungarian.
```

![image](https://user-images.githubusercontent.com/1344888/231562619-39f78e62-732e-46fb-94d4-fe3387b964f7.png)

The "app" gets created in about 10-15 seconds from scratch. Every interaction takes about 2 seconds to complete. 

Note how the addition of multiple languages. If this was a real project, supporting multiple languages would triple the complexity (and cost) of this app. 

## Installation / Running the app
- Clone / download the Github repo
- Open the solution in Visual Studio 
- Open ```OpenAIHelper.cs```
- Uncomment the ```GetOpenAiOptions``` method and follow the instructions to set up OpenAI or Azure service (note: GPT4 works much better).
- Run the app and enjoy.

## Internals 

Internally, the generated app follows the MVVM model, but with a twist:
- The UI is created in the now more or less standard way -> Using ChatGPT to turn text to XAML. 
- However, there is no actual code being created and ran on the client. The application's inner state is specified by a JSON string, and the AI creates this JSON string every time the user interacts with the application. **So, the business logic runs "On the AI"!**
- The structure of this JSON is also determined by the AI, based on the specification.

## Limitations

- The demo currently only handles buttons as an input, but it's pretty trivial to extend it to other controls (text fields, sliders, etc).
- Due to the random nature of GPT, the app's behavior sometimes doesn't follow the specification. The more complex the specification gets, the more tries it takes to get things right. 
- A minimal self-reflection is built in to make sure that XAML code that doesn't compile (or uses constructs we don't support, despite the prompt asking it not to do so) don't get used.

## Enhancements
- A chat-like interface to tweak "almost right" solutions can be introduced. This will eliminate the guesswork by allowing to make small edits instead of restarting the process from scratch
- Automatic tests could be created based on the specification
