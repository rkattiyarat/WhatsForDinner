namespace WhatsForDinner.Models;

public class CookingRequest
{
    public List<string> Ingredients { get; set; } = [];
    public List<string> Seasonings { get; set; } = [];
    public string Cuisine { get; set; } = "Any";
    public int Servings { get; set; } = 2;
    public int? MaxTotalMinutes { get; set; }
    // Prefer an exact pantry match first; allow a closest-match fallback if none is found.
    public bool UseOnlyAvailableIngredients { get; set; } = true;
}
