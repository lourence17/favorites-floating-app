# Favorite Apps and Folders Manager

## Project Description
This Windows Forms application allows users to add, manage, and launch their frequently used applications and folders as favorites. It features a rounded corner, design that starts in the top-right corner of the screen. Favorites are displayed as buttons with custom application icons, making it easy to add, remove, or open them.

## Features
- **Rounded Corner Design:** Offers an aesthetic and modern user interface.
- **Favorite Management:** Applications (`.exe`) and folders can be added as favorites.
- **Favorite Removal:** Favorites can be easily removed via a right-click context menu.
- **Exit Option:** The application can be closed from the context menu of the "Add" button.
- **Automatic Positioning:** The form starts in the top-right corner of the screen (100% width, 5% height).
- **Close on Mouse Click Outside:** The form shrinks to 50x50 pixels when clicked outside (e.g., on the desktop or another application).
- **Performance Optimization:** Includes `DoubleBuffered` and `SetStyle` optimizations for smooth animations and interactions.

## Prerequisites
- **.NET Framework or .NET Runtime:** Ensure the target .NET version (e.g., .NET Framework 4.8 or .NET 6.0) is installed on the user's machine. Check the project properties in Visual Studio to confirm the target framework.

## Installation
1. Clone or download this repository to your local machine.
2. Open the solution file (`.sln`) in Visual Studio.
3. Build the solution in **Release** mode for an optimized executable:
   - Go to **Build > Configuration Manager**, set "Active solution configuration" to "Release".
   - Select **Build > Rebuild Solution**.
4. Locate the `.exe` file in the `bin\Release` folder (e.g., `Favorites.exe`).
5. Run the `.exe` file to launch the application.

## Usage
- **Add Favorites:** Right-click the "Add" button, select "Add Application" to add an `.exe` file or "Add Folder" to add a directory. Favorites appear as 50x40 pixel buttons below the "Add" button.
- **Launch Favorites:** Click a favorite button to open the associated application or folder.
- **Remove Favorites:** Right-click a favorite button and select "Remove" to delete it from the list.
- **Close the Form:** Click outside the form (e.g., on the desktop) to shrink it back to 50x50 pixels, or use the "Exit" option from the "Add" button’s context menu to close the application.

## Development
- **Tools:** Developed using Visual Studio with C# and Windows Forms.
- **Dependencies:** No external libraries are required beyond the standard .NET Framework or .NET Runtime.

## Contributing
Feel free to fork this repository, make improvements, and submit pull requests. Issues and feature requests are welcome via GitHub Issues.

## License
This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details.

## Acknowledgments
- Inspired by the need for a simple, customizable desktop favorite manager.
- Thanks to the Windows Forms community for providing robust tools and libraries.
