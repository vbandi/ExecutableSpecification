# ExecutableSpecification

This project is a proof of concept to see if a written specification can be used to create a simple, working WPF app.

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
The app should also have a button in the lower left corner that toggles the language between English and Hungarian. Pressing this will change every text in the app to the other language (including button labels).
The app should launch in Hungarian.
```

![image](https://user-images.githubusercontent.com/1344888/231562619-39f78e62-732e-46fb-94d4-fe3387b964f7.png)


The "app" gets created in about 10-15 seconds from scratch. Every interaction takes about 2 seconds to complete. 

## Internals 

The created app follows the MVVM model, but with a twist:
- The UI is created in the now more or less standard way -> Using ChatGPT to turn text to XAML. 
- However, there is no actual code being created and ran on the client. The application's inner state is specified by a JSON string, and the AI creates this JSON string every time the user interacts with the application.
- The structure of this JSON is also determined by the AI, based on the specification.

## Limitations

- The demo currently only handles buttons as an input, but it's pretty trivial to extend it to other controls (text fields, sliders, etc).
- Due to the random nature of GPT, the app's behavior sometimes doesn't follow the specification. 
- A minimal self-reflection is built in to make sure that XAML code that doesn't compile (or uses constructs we don't support, despite the prompt asking it not to do so) don't get used.



