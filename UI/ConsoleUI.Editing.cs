using WhatsForDinner.Models;

namespace WhatsForDinner.UI;

public partial class ConsoleUI
{
    // Edit a copy so Back or end-of-input cannot partially change the active request.
    public CookingRequest? EditPreferences(CookingRequest current)
    {
        var draft = new CookingRequest
        {
            Ingredients = [.. current.Ingredients], Seasonings = [.. current.Seasonings],
            Cuisine = current.Cuisine, Servings = current.Servings,
            MaxTotalMinutes = current.MaxTotalMinutes,
            UseOnlyAvailableIngredients = current.UseOnlyAvailableIngredients
        };
        while (true)
        {
            Console.WriteLine("\nYour preferences (editing):");
            Console.WriteLine($"1. Ingredients: {string.Join(", ", draft.Ingredients)}");
            Console.WriteLine($"2. Seasonings: {(draft.Seasonings.Count == 0 ? "None" : string.Join(", ", draft.Seasonings))}");
            Console.WriteLine($"3. Cuisine: {draft.Cuisine}\n4. Servings: {draft.Servings}");
            Console.WriteLine($"5. Maximum cooking time: {draft.MaxTotalMinutes?.ToString() ?? "No limit"}");
            Console.WriteLine($"6. Prefer available ingredients: {(draft.UseOnlyAvailableIngredients ? "Yes" : "No")}");
            Console.WriteLine("7. Find a recipe with these preferences\n8. Back (discard edits)");
            switch (Read("Choose 1-8: ").Trim())
            {
                case "1": draft.Ingredients = EditItems(draft.Ingredients, false); break;
                case "2": draft.Seasonings = EditItems(draft.Seasonings, true); break;
                case "3": draft.Cuisine = ReadCuisine(draft.Cuisine); break;
                case "4": draft.Servings = ReadNumber($"Servings (Enter to keep {draft.Servings}): ", draft.Servings, 100); break;
                case "5": draft.MaxTotalMinutes = ReadTime(draft.MaxTotalMinutes); break;
                case "6": draft.UseOnlyAvailableIngredients = ReadPreference(draft.UseOnlyAvailableIngredients); break;
                case "7": return draft;
                case "8": return null;
                default: Console.WriteLine("Please choose a number from 1 to 8."); break;
            }
        }
    }

    private static List<string> EditItems(List<string> original, bool seasonings)
    {
        var items = new List<string>(original);
        while (true)
        {
            Console.WriteLine(seasonings ? "\nYour seasonings:" : "\nYour ingredients:");
            if (items.Count == 0) Console.WriteLine("None");
            for (var i = 0; i < items.Count; i++) Console.WriteLine($"{i + 1}. {items[i]}");
            Console.WriteLine("A. Add items\nD. Delete items\nR. Replace the entire list\nB. Back (keep this list)");
            if (seasonings) Console.WriteLine("C. Clear all seasonings");
            switch (Read("Choose an action: ").Trim().ToUpperInvariant())
            {
                case "A":
                    var additions = ReadItems("Items to add, separated by commas: ");
                    var merged = items.Concat(additions).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    if (merged.Count > 50 || string.Join(", ", merged).Length > 1000)
                    {
                        Console.WriteLine("That would exceed 50 items or 1000 characters. Nothing was added.");
                        break;
                    }
                    Console.WriteLine($"Added {merged.Count - items.Count} item(s). Duplicates were skipped.");
                    items = merged;
                    break;
                case "D":
                    if (items.Count == 0) { Console.WriteLine("There are no items to delete."); break; }
                    var raw = Read("Item numbers to delete, separated by commas (Enter to cancel): ");
                    if (raw.Length == 0) break;
                    var indices = new HashSet<int>();
                    var valid = true;
                    foreach (var part in raw.Split(','))
                    {
                        if (!TryPositiveDigits(part, out var number) || number < 1 || number > items.Count)
                        { valid = false; break; }
                        indices.Add(number - 1);
                    }
                    if (!valid) { Console.WriteLine("Invalid item number. Nothing was deleted."); break; }
                    if (!seasonings && indices.Count == items.Count)
                    { Console.WriteLine("Keep at least one ingredient. Nothing was deleted."); break; }
                    items = items.Where((_, index) => !indices.Contains(index)).ToList();
                    break;
                case "R":
                    items = ReadItems(seasonings ? "Replacement seasonings (or none): " : "Replacement ingredients: ", seasonings);
                    break;
                case "C" when seasonings: items.Clear(); break;
                case "B": return items;
                default: Console.WriteLine(seasonings ? "Choose A, D, R, C or B." : "Choose A, D, R or B."); break;
            }
        }
    }
}
