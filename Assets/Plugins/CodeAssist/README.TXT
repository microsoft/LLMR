Code Assist improves your coding experience with Visual Studio. Coding for Unity projects becomes easier and more efficient with fewer bugs.
Connect your game editor and IDE, let Unity and Visual Studio share data between each other. Enrich your coding experience with the new data available.



Contact:
merryyellow@outlook.com

Website:
https://unitycodeassist.netlify.app/

Discord:
https://discord.gg/2CgKHDq

Forum:
https://forum.unity.com/threads/code-assist-empower-your-ide.1384731/

GitHub:
https://github.com/merryyellow/Unity-Code-Assist

Asset Store:
http://u3d.as/2N2H

Visual Studio Marketplace:
https://marketplace.visualstudio.com/items?itemName=MerryYellow.UCA-Lite



Restrictions:
* Unity 2020.3 and newer.
* Visual Studio 2022. Does not work with Visual Studio Code, Visual Studio for Mac, Rider or any other IDE or text editor.



How to Setup:
* Import the asset under folder "Assets"
* Install Meryel.UnityCodeAssist.VSIX.vsix under folder "Assets/Plugins/CodeAssist" (You can skip this step if you have installed it for another project)
* Open your Unity project with Visual Studio. Code Assist loads in background with low priority, so it will be ready in a minute when Visual Studio first starts up.
* To check if setup is successfull, in Unity, choose "Tools"->"Code Assist"->"Status" from menu. The windows should display "Code Assist is working!".



How to Use:
Code Assist is always active while you are coding. The following samples will give you a glimpse of Code Assist.
* Code Completion: In a class which derives MonoBehaviour, type "CompareTag(". When you press double quotes("), code completion popup should appear with the available tags of the scene.
* Inline Visuals: Type a color in hex code with quotation marks, like "#FF8800". A box with the matching color should appear left of the color code.
* Code Analyzers: Define a Vector3 (let's say vec3). When used like "if (vec3.magnitude < 10)", a warning will appear on that line. Press Alt+Enter to convert it to "if (vec3.sqrMagnitude < 10 * 10)", which has better performance.



Generative AI:
Generative AI is one of the features of Code Assist. If you have any ethical or legal concerns, you can use Code Assist without this feature by disabling generative AI in the options.

You need an OpenAI account and provide it's API key to this asset.
https://platform.openai.com/account/api-keys

Provide key by one of the below
* Create an environment variable called OPENAI_API_KEY with the API key
* Create a file with name .openai under user profile directory with the API key
* Set API key in Visual Studio options, Tools->Options->Unity Code Assist->Generative AI->Authentication

This asset has one-time purchase fee, but you may need to pay extra for OpenAI services to use Generative AI feature.
https://openai.com/pricing

To use generative AI with your comments:
* Type "//gpt "
* Type your prompt
* Press Enter key



Troubleshooting:
* Make sure in Unity, "Tools"->"Code Assist"->"Status" is available from the menu bar. If not, reimport the asset.
* Make sure in Visual Studio, "Extensions"->"Unity Code Assist"->Status is available from the menu bar. If not, reinstall the UnityCodeAssistVSIX.vsix file.
* If Visual Studio has just been started, just wait; it usually takes around one minute to fully load Code Assist.
* If still not working, in Unity try "Tools"->"Code Assist"->"Synchronize" from the menu bar. This will attempt a new connection with Visual Studio.
* If none works above, try restarting both Unity and Visual Studio; sometimes an unclosed background thread blocks communication channels (network ports).



Error Reporting:
* In Unity, choose "Tools"->"Code Assist"->"Report error" from the menu bar, to open feedback window.
* Or in Visual Studio, choose "Extensions"->"Unity Code Assist"->"Report error" from the menu bar, to open feedback window.
* In the feedback window, you can view the most recent logs, or view full log of Unity or Visual Studio.
* You can report the error via email ( merryyellow@outlook.com ) or Discord ( https://discord.gg/2CgKHDq ) or GitHub ( https://github.com/merryyellow/Unity-Code-Assist/issues/new )
* Attaching full logs is recommended when reporting an error.



Thank you for using Code Assist. We hope this will help you in your crazy journey called game development :)