# Menu image proportions

MainMenu now fits the credits panel to the cookbook’s 3:2 aspect ratio, with all text and controls following the fitted panel. The pie and lasagna images preserve their source proportions. The rules board uses nine-slice borders instead of stretching the corner decorations, and the menu Canvas has a valid reference pixel density.

Unity preview renders checked the main menu, rules, and credits at 1024×768, 1280×800, 1920×1080, and 2560×1080. All 96 label checks passed with no overflow or offscreen rectangles. The 1280×800 previews were visually inspected. Existing malformed meta-file GUID warnings occurred during asset refresh; the layout checks completed.

Preview rendering does not replace a gameplay or player-build test.
