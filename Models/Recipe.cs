using System.Text.Json.Serialization;

namespace WhatsForDinner.Models;

public class Recipe
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Cuisine { get; set; } = "";
    public int Servings { get; set; }
    public int PreparationMinutes { get; set; }
    public int CookingMinutes { get; set; }
    [JsonIgnore]
    public long TotalMinutes => (long)PreparationMinutes + CookingMinutes;
    public List<RecipeIngredient> Ingredients { get; set; } = [];
    public List<string> Instructions { get; set; } = [];
}
