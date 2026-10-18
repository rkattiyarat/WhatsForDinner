using System.Text.Json.Serialization;

namespace WhatsForDinner.Models;

public class RecipeResult
{
    // Null means the AI could not find a reasonable recipe for this request.
    public Recipe? Recipe { get; set; }
    public string Explanation { get; set; } = "";
    // Set by the service after validation, rather than trusting an AI-provided label.
    [JsonIgnore]
    public bool IsClosestMatch { get; set; }
}
