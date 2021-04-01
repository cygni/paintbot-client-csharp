# paintbot-client-csharp

Welcome to the Paintbot C# Client. This is where you write awesome bots for the game Paintbot using C#
## Prerequisites
 - Make sure to have .NET 5 SDK installed. (It can be downloaded here https://dotnet.microsoft.com/download/dotnet/5.0)
## Getting started
 - Clone this repository
 - Open the project in whatever editor you feel comfortable with
 - Navigate to the `MyPaintBot.cs` class. In there, there's a `GetAction` method. This is the *only* method that you'll need to  change. As of now, there's a very simple bot in there which just acts as an example of what a bot could look like. You'll replace the content of this method with your own logic. Your goal is to take an action based on the current state of the board. Take a look at the `Action.cs` enum to get an idea of the possible actions. 
 - Take a look at the `IMapUtils` interface. This contains methods that let's you gather information about the current state of the game board and can be used as a basis of your decision on which action to take. We have created an instance of the `MapUtils` class at the top of the `GetAction` method for you.
 - Start writing your bot! 
## Running a game
 To get feedback on how your bot is faring you can run a game with it. 
There are two game modes for PaintBot.  
 - **Training** This lets you play through one game with your bot against random pre-defined bots. This mode is great to use during development, and it's the default mode whenever you run the application. 
 - **Tournament**.  This mode lets you compete with other bots in a tournament. To run in tournament mode, you'll have to add the commandline argument `tournament`.

### Run training mode through commandline
When using the training mode, you can adjust the game length with a command line argument so that you don't have to sit around and wait for too long. 
1. From the repository root folder, CD into the `Painbot` folder
2. Run `dotnet run training 20`. 
This would run a training match which takes 20 seconds. 

### Run training mode from Visual Studio
1. Right click the `Paintbot` project and choose `Properties`.
2. Go to the `Debug` tab. Add `training 20` in the `Application Arguments`text area. 20 can be replaced with the game length of your choice. 

### Run a tournament through commandline
1. From the repository root folder, CD into the `Painbot` folder
2. Run `dotnet run tournament`

### Run a tournament from Visual Studio
1. Right click the `Paintbot` project and choose `Properties`.
2. Go to the `Debug` tab. Add `tournament` in the `Application Arguments`text area. 
