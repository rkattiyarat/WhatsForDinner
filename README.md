# What's for Dinner?

A C# .NET 10 console app that asks OpenAI to generate recipes from your ingredients.
There is no built-in recipe list. Recipe generation requires internet access and an OpenAI API key.

## Setup

1. Open WhatsForDinner.slnx in Visual Studio.
2. Create an API key in your OpenAI Platform account: https://platform.openai.com/api-keys
3. In Windows, search for **Edit environment variables for your account**.
4. Add a user variable named `OPENAI_API_KEY` with your API key as its value.
5. Close and reopen Visual Studio so it receives the variable.
6. Press Ctrl+F5 to build and run.

API usage requires API billing/credits separately from a ChatGPT subscription.
Never put the key in a C# file, upload it, or commit it to GitHub.
Optional: set `OPENAI_MODEL` to an accessible Responses API model supporting structured outputs.
The default is `gpt-4o-mini`. Model access depends on your API account.

## Using the app

Enter comma-separated ingredients, then sauces and seasonings (include oil, salt and water if needed).
Choose cuisine, servings, an optional time limit, and whether to prefer your available ingredients.
Ingredients and seasonings cannot be blank or spaces alone. Type `none` for no seasonings.
Use comma-separated names without empty items (up to 50 items, 80 characters per item and 1000
characters per list). International letters, spaces, apostrophes and hyphens are supported;
food names such as `5-spice` and `2% milk` are accepted, but numbers alone and unrelated symbols are not.
Cuisine accepts any written cuisine name, without a fixed list; Enter defaults to Any.
Servings must be digits representing 1-100; Enter defaults to 2. Time is optional, but a supplied
limit must be 15-1440 whole minutes. The 15-minute minimum is an app rule, not a recipe calculation.
Y/Yes and N/No are case insensitive and trimmed; Enter defaults to Y. At all defaultable prompts,
spaces alone are rejected, while pressing Enter without typing uses the default.
With Y (the default), the app first asks for a recipe using only your ingredients. If AI reports no
match, including after previous suggestions, it makes one additional request allowing up to three
extra ingredients. That result is labeled CLOSEST MATCH when extras are needed, with every missing
ingredient marked. With N, up to three extras are allowed from the first request.
Recipes can omit any supplied ingredients that do not suit the dish. Servings and the time limit
still apply to fallback recipes; the AI is instructed to retain the requested cuisine.
The app displays quantities for those servings, estimated times, missing items, and instructions.
Choose another recipe, change inputs, or exit. Each generation makes one API request, or two when
the exact-match attempt returns no recipe. Errors do not trigger an automatic fallback request.

## Code map

After a result, choose **2: Edit my preferences** to change selected settings. Ingredients and
seasonings support add, delete by item number, replace, and back. Duplicate additions are skipped;
invalid deletion lists change nothing. Keep at least one ingredient; seasonings may be cleared.
Enter keeps the current value when editing cuisine, servings, time or the Y/N preference.
Type `none` to remove the time limit, or `Any` to remove the cuisine preference.
Choose 7 to apply edits and generate a recipe; choose 8 to discard edits and return without an API call.
Edits last only for the current run. Nothing is saved to a pantry yet.

- `Program.cs`: coordinates input, generation, errors, and the repeat menu.
- `Models/`: cooking preferences and the structured recipe result.
- `UI/ConsoleUI.cs`: console prompts, input validation and display.
- `Services/AiRecipeService.cs`: authenticated OpenAI Responses API request and response parsing.
- `Services/RecipeValidator.cs`: checks servings, times, ingredients and required recipe content.
- `Prompts/recipe-schema.json`: the JSON structure requested from OpenAI.

Uses built-in .NET HTTP and JSON libraries; no external NuGet packages are required.
API reference: https://developers.openai.com/api/docs/guides/structured-outputs?api-mode=responses

## Limits

AI-generated recipes and times are estimates. Validation cannot prove cooking safety or detect every
ingredient hidden in instruction text. The AI supplies a corrected ingredient name plus the original
inventory entry in `availableIngredientName` (for example, soy sauce linked to soysauce). The validator
checks that the original entry exists and recalculates missing status. Corrected names are displayed
alongside the user's original entry. Unlinked, unlisted ingredients remain missing; invented inventory
references are rejected. Typo and synonym interpretation still depends on AI and can be mistaken;
the prompt forbids ambiguous guesses and ingredient substitutions. There is no blanket fuzzy matching.
The app relies on AI to judge whether an exact match exists; it does not search
an exhaustive recipe database. A closest match must use at least one supplied ingredient.
Input records ingredient names, not pantry quantities, so ensure you have the displayed amounts.
Time estimates add preparation and cooking sequentially. The app does not save recipes between runs.
