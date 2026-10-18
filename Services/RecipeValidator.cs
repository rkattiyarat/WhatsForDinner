using WhatsForDinner.Models;

namespace WhatsForDinner.Services;

public static class RecipeValidator
{
    public static List<string> Validate(RecipeResult result, CookingRequest request, bool allowExtraIngredients = false)
    {
        var errors = new List<string>();
        var recipe = result.Recipe;
        if (recipe is null)
        {
            if (string.IsNullOrWhiteSpace(result.Explanation))
                errors.Add("The response did not explain why no recipe was found.");
            return errors;
        }
        if (string.IsNullOrWhiteSpace(recipe.Name) || string.IsNullOrWhiteSpace(recipe.Description))
            errors.Add("The recipe name or description is missing.");
        if (recipe.Servings != request.Servings) errors.Add("The serving count does not match.");
        if (recipe.PreparationMinutes < 0 || recipe.CookingMinutes < 0 || recipe.TotalMinutes <= 0)
            errors.Add("The estimated times are invalid.");
        if (request.MaxTotalMinutes is int limit && recipe.TotalMinutes > limit)
            errors.Add("The recipe exceeds your time limit.");
        if (recipe.Instructions is null || recipe.Instructions.Count == 0 || recipe.Instructions.Any(string.IsNullOrWhiteSpace))
            errors.Add("Cooking instructions are missing.");
        if (recipe.Ingredients is null || recipe.Ingredients.Count == 0)
        {
            errors.Add("The recipe has no ingredients.");
            return errors;
        }
        var available = request.Ingredients.Concat(request.Seasonings)
            .Select(name => name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient is null || string.IsNullOrWhiteSpace(ingredient.Name) || ingredient.Quantity <= 0 || string.IsNullOrWhiteSpace(ingredient.Unit))
            {
                errors.Add("An ingredient has an invalid name, quantity, or unit.");
                continue;
            }
            // An exact name is sufficient evidence that the ingredient is available.
            // Discard a conflicting AI reference so the UI cannot show a false mapping.
            var sourceName = ingredient.AvailableIngredientName?.Trim();
            if (available.Contains(ingredient.Name.Trim()))
            {
                ingredient.IsMissing = false;
                ingredient.AvailableIngredientName = null;
            }
            else
            {
                // Only consult the original inventory reference for corrected names.
                // No edit-distance guessing: similar-looking foods can be different ingredients.
                if (sourceName is not null && !available.Contains(sourceName))
                    errors.Add($"The ingredient '{ingredient.Name}' refers to an unknown inventory entry.");
                ingredient.IsMissing = sourceName is null || !available.Contains(sourceName);
            }
            if (request.UseOnlyAvailableIngredients && !allowExtraIngredients && ingredient.IsMissing)
                errors.Add($"The recipe requires an unlisted ingredient: {ingredient.Name}.");
        }
        if (recipe.Ingredients.Where(i => i is not null && i.IsMissing)
            .Select(i => i.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 3)
            errors.Add("The recipe requires more than three extra ingredients.");
        if (!recipe.Ingredients.Any(i => i is not null && !i.IsMissing))
            errors.Add("The recipe must use at least one of your available ingredients.");
        return errors;
    }
}
