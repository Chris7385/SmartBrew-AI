# SmartBrew Coffee Machine — Easy Testing Edition

A C# WPF graphical coffee-machine prototype for Lab 3 Task B. Open `SmartBrewGUI.csproj` in Visual Studio 2022 (Windows, .NET 8 Desktop Development workload) and press **Ctrl+F5**.

## Fast demo
- Click **Account / Login → Quick Demo Login**, or use username `demo`, password `demo`.
- **Coffee Menu** has 12 drinks. Customize size, strength, milk, sugar, syrup and temperature.
- Enter a recipe name, click **Save Recipe**, then go to **My Recipes** to edit, order or delete it.
- Checkout uses simulated payment, an animated brewing progress bar and a receipt.
- **Recommendation** picks a random drink each click.
- **Top Today** shows a random drink; click Randomize to change it.
- Sidebar weather is randomly chosen at startup; use **Demo Guide → Randomize demo weather** to change it.
- **Technician** PIN is `2468`; refill stock, empty waste, simulate low stock and view error logs.
- **Offline demo** accepts only simulated Prepaid Employee Card payment.

## Scope and limitations
No actual coffee machine hardware, AI, camera, LLM, NFC, real payment, or weather API. Randomized recommendations, weather and Top Today are labeled as demo data. All accounts, recipes, orders, and inventory are in-memory only, reset at restart; passwords are stored in memory as plain text **for demonstration only**. Never enter real passwords. Not production ready.

## Build
1. Install Visual Studio 2022 with **.NET desktop development** and .NET 8 SDK.
2. Extract the ZIP to a new folder (don't mix files with the old project).
3. Open `SmartBrewGUI.csproj`, then Build → Rebuild Solution.
4. Press Ctrl+F5.

The project was statically reviewed but could not be compiled in this environment (Windows/.NET SDK unavailable). If Visual Studio reports errors, send the first errors from Error List.

## Ten-feature verification checklist

Perform the following tests after a successful Windows build. Record pass/fail results for submission.

1. **Login:** Open Account / Login and use Quick Demo Login. Confirm the signed-in user changes to `demo`.
2. **Coffee menu:** Open Coffee Menu and confirm all 12 beverages are available.
3. **Customization:** Select a beverage and change size, strength, milk, sugar, syrup, and temperature.
4. **Custom recipes:** Save a named recipe, open My Recipes, then edit, reorder, and delete it.
5. **Recommendations:** Open Recommendation and refresh to show a demo beverage suggestion.
6. **Top Today:** Open Top Today and randomize the displayed beverage.
7. **Weather:** Open Demo Guide and randomize demo weather; confirm the status display changes.
8. **Ordering and payment:** Customize a beverage, proceed to checkout, and complete a simulated payment.
9. **Brewing and receipt:** Observe the brewing progress and inspect the generated receipt.
10. **Technician tools:** Open Technician with PIN `2468`; refill ingredients, empty waste, and inspect simulated error logs.

**Important:** This is a proposed manual test plan, not evidence that the tests passed. The original project has not been compiled or executed in this environment.

## Source code documentation

The C# methods in `MainWindow.xaml.cs` have XML documentation comments describing purpose, inputs, and return values. Event handlers and `void` methods explicitly state that they return no value.

## Repository submission

Upload the extracted project files (including `SmartBrewGUI.csproj`) to the root of your GitHub repository. Do not upload generated `bin/`, `obj/`, or `.vs/` folders. Include the repository URL in the Canvas submission.
