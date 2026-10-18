using System.Globalization;
using System.Text.RegularExpressions;
using WhatsForDinner.Models;

namespace WhatsForDinner.UI;

public partial class ConsoleUI
{
    public const int MinimumCookingMinutes = 15;
    private const int MaximumCookingMinutes = 1440;

    private static string Read(string prompt)
    {
        Console.Write(prompt);
        // Preserve the difference between pressing Enter and entering only spaces.
        return Console.ReadLine() ?? throw new OperationCanceledException();
    }

    private static string Normalize(string text) => Regex.Replace(text.Trim().Normalize(), @"\s+", " ");

    private static bool IsName(string text, bool food)
    {
        if (text.Length is 0 or > 80 || !text.Any(char.IsLetter)) return false;
        // Allow international letters and everyday food names like 5-spice and 2% milk.
        var pattern = food ? @"^[\p{L}\p{M}0-9 '\u2019\- %]+$" : @"^[\p{L}\p{M} '\u2019\-]+$";
        return Regex.IsMatch(text, pattern)
            && !Regex.IsMatch(text, @"(?<![0-9])%")
            && !Regex.IsMatch(text, @"['\u2019\-]{2,}")
            && char.IsLetterOrDigit(text[0]) && (char.IsLetterOrDigit(text[^1]) || char.GetUnicodeCategory(text[^1]) == UnicodeCategory.NonSpacingMark);
    }

    private static List<string> ReadItems(string prompt, bool allowNone = false)
    {
        while (true)
        {
            var text = Read(prompt);
            if (allowNone && text.Trim().Equals("none", StringComparison.OrdinalIgnoreCase)) return [];
            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine(allowNone ? "Enter your seasonings, or type none if you have none." : "Please enter at least one ingredient. Blank input is not allowed.");
                continue;
            }
            if (text.Length > 1000) { Console.WriteLine("Please keep the list to 1000 characters or fewer."); continue; }
            var items = text.Split(',').Select(Normalize).ToList();
            if (items.Count > 50) { Console.WriteLine("Please enter no more than 50 items."); continue; }
            if (items.Any(item => !IsName(item, true) || item.Equals("none", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Enter food names separated by commas, with no empty items. Use letters and spaces; hyphens, apostrophes and numbers in names such as 5-spice or 2% milk are allowed. Other symbols and numbers alone are not.");
                continue;
            }
            return items.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    public CookingRequest ReadCookingRequest()
    {
        Console.WriteLine("\nWhat's in your kitchen? Separate items with commas.");
        var request = new CookingRequest
        {
            Ingredients = ReadItems("Ingredients: "),
            Seasonings = ReadItems("Sauces, spices, oil, salt, etc. (type none if you have none): ", true)
        };
        request.Cuisine = ReadCuisine("Any");
        request.Servings = ReadNumber("Servings (press Enter for 2 servings): ", 2, 100);
        request.MaxTotalMinutes = ReadTime(null);
        Console.WriteLine("Recipes may leave out ingredients that don't fit the dish.");
        Console.WriteLine("Tries your ingredients first, then suggests a closest match with up to 3 extras if needed.");
        request.UseOnlyAvailableIngredients = ReadPreference(true);
        return request;
    }

    private static string ReadCuisine(string current)
    {
        while (true)
        {
            var raw = Read($"Cuisine (any cuisine in the world; press Enter or type Any for no preference): ");
            if (raw.Length == 0) return current;
            var cuisine = Normalize(raw);
            if (IsName(cuisine, false)) return cuisine;
            Console.WriteLine("Enter a cuisine name using letters, spaces, hyphens or apostrophes (up to 80 characters), or press Enter to keep the current value. Type Any for no preference. Spaces alone are not valid.");
        }
    }

    private static int? ReadTime(int? current)
    {
        while (true)
        {
            var time = Read($"Maximum cooking time(minutes) (press Enter to keep {current?.ToString() ?? "no limit"}; or type none for no limit): ");
            if (time.Length == 0) return current;
            if (time.Trim().Equals("none", StringComparison.OrdinalIgnoreCase)) return null;
            if (!TryPositiveDigits(time, out var minutes))
            {
                Console.WriteLine("Enter whole minutes using digits only, press Enter to keep the current value, or type none for no limit. Spaces alone, signs and decimals are not valid.");
                continue;
            }
            if (minutes < MinimumCookingMinutes)
            {
                Console.WriteLine($"Cooking time can't be lower than {MinimumCookingMinutes} minutes.");
                continue;
            }
            if (minutes > MaximumCookingMinutes)
            {
                Console.WriteLine($"Max cooking time can't be higher than {MaximumCookingMinutes} minutes.");
                continue;
            }
            return minutes;
        }
    }

    private static bool ReadPreference(bool current)
    {
        while (true)
        {
            var raw = Read($"Prefer recipes using only ingredients I have? Y/Yes or N/No (Enter to keep {(current ? "Y" : "N")}): ");
            var only = raw.Trim().ToLowerInvariant();
            if (raw.Length == 0) return current;
            if (only is "y" or "yes") return true;
            if (only is "n" or "no") return false;
            Console.WriteLine("Please enter Y, Yes, N or No, or press Enter to keep the current value. Spaces alone and symbols are not valid.");
        }
    }

    private static bool TryPositiveDigits(string text, out int value) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value);

    private static int ReadNumber(string prompt, int defaultValue, int maximum)
    {
        while (true)
        {
            var text = Read(prompt);
            if (text.Length == 0) return defaultValue;
            if (TryPositiveDigits(text, out var value) && value > 0 && value <= maximum) return value;
            Console.WriteLine($"Enter a whole number from 1 to {maximum} using digits only, or press Enter for {defaultValue}. Spaces alone, signs and decimals are not valid.");
        }
    }


    public void DisplayResult(RecipeResult result)
    {
        if (result.Recipe is not { } recipe)
        {
            Console.WriteLine($"\nNo matching recipe: {result.Explanation}");
            Console.WriteLine("Try changing your ingredients, cuisine, or time limit.");
            return;
        }
        if (result.IsClosestMatch)
            Console.WriteLine("\nCLOSEST MATCH: No exact ingredient match was found. This option needs a few extras.");
        Console.WriteLine($"\n{recipe.Name}\n{recipe.Description}");
        Console.WriteLine($"Serves {recipe.Servings} | Estimated total: {recipe.TotalMinutes} minutes");
        Console.WriteLine($"Preparation: {recipe.PreparationMinutes} min | Cooking: {recipe.CookingMinutes} min");
        Console.WriteLine("\nIngredients:");
        foreach (var item in recipe.Ingredients)
        {
            Console.WriteLine($"- {item.Quantity:0.##} {item.Unit} {item.Name}{(item.IsMissing ? " [MISSING - needed before cooking]" : "")}");
            if (!item.IsMissing && !string.IsNullOrWhiteSpace(item.AvailableIngredientName)
                && !string.Equals(item.Name.Trim(), item.AvailableIngredientName.Trim(), StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"  Matched to your entry: {item.AvailableIngredientName}");
        }
        Console.WriteLine("\nInstructions:");
        for (var i = 0; i < recipe.Instructions.Count; i++) Console.WriteLine($"{i + 1}. {recipe.Instructions[i]}");
        if (!string.IsNullOrWhiteSpace(result.Explanation)) Console.WriteLine($"\n{result.Explanation}");
    }

    public string ReadNextAction()
    {
        while (true)
        {
            var action = Read("\n1: Try another recipe | 2: Edit my preferences | 3: Exit: ").Trim();
            if (action is "1" or "2" or "3") return action;
            Console.WriteLine("Choose 1, 2, or 3.");
        }
    }
}

