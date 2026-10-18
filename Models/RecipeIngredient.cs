namespace WhatsForDinner.Models;

public class RecipeIngredient
{
    public string Name { get; set; } = "";
    // Original user entry supporting this ingredient; null when it is an extra.
    public string? AvailableIngredientName { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "";
    public bool IsMissing { get; set; }
}
