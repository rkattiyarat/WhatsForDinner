using System.Text.Json;
using WhatsForDinner.Services;
using WhatsForDinner.UI;

Console.WriteLine("Welcome to What's for Dinner!");
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Set the OPENAI_API_KEY environment variable, then restart Visual Studio.");
    Console.WriteLine("See README.md for setup. Do not paste your key into the source code.");
    return;
}
var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");
if (string.IsNullOrWhiteSpace(model)) model = "gpt-4o-mini";
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
var service = new AiRecipeService(http, apiKey, model);
var ui = new ConsoleUI();
var previous = new List<string>();
try
{
    var request = ui.ReadCookingRequest();
    while (!cancellation.IsCancellationRequested)
    {
        Console.WriteLine("\nFinding you a recipe...");
        try
        {
            var result = await service.GenerateRecipeAsync(request, previous, cancellation.Token);
            ui.DisplayResult(result);
            if (result.Recipe is { } recipe) previous.Add(recipe.Name);
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        { Console.WriteLine("The request timed out. Please try again."); }
        catch (HttpRequestException)
        { Console.WriteLine("Couldn't connect to OpenAI. Check your internet connection."); }
        catch (JsonException)
        { Console.WriteLine("OpenAI returned unreadable recipe data. Please try again."); }
        catch (InvalidOperationException ex)
        { Console.WriteLine(ex.Message); }
        catch (IOException)
        { Console.WriteLine("Couldn't read the recipe schema. Rebuild the project and try again."); }
        cancellation.Token.ThrowIfCancellationRequested();
        while (true)
        {
            var next = ui.ReadNextAction();
            if (next == "3") return;
            if (next == "1") break;
            var edited = ui.EditPreferences(request);
            if (edited is null) continue; // Back does not make an API call.
            if (JsonSerializer.Serialize(edited) != JsonSerializer.Serialize(request)) previous.Clear();
            request = edited;
            break;
        }
    }
}
catch (OperationCanceledException) { Console.WriteLine("\nGoodbye!"); }
