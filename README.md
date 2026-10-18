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
