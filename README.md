# Logo Mod

A simple mod that displays a logo/watermark on the screen of your choice. Can be configured to use a `.png` file or your Steam Avatar (default).
Customizable properties: position, spacing, scale, opacity, border thickness, border color (HEX color code).

Approved for use in speedrunning!

## Installation and Usage

1. Download [MelonLoader](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.2) and install it to your `<gamedir>\Neon Boost.exe`.
2. Run the game once. This will create the required folders. You should see a splash screen and a terminal if the modloader was installed correctly.
3. Optionally, download [Melon Preferences Manager](https://github.com/Bluscream/MelonPreferencesManager/releases).
3b. Download the latest version of [UniverseLib](https://github.com/sinai-dev/UniverseLib/releases).
4. Download the `NeonBoostLogoMod.dll` from [the releases page](https://github.com/Hyonk-Tea/NeonBoostEnhancedTimer/releases)
5. Extract all of the downloaded mods from their `.zip` files, and deposit them into your Neon Boost/Mods/ folder.
6. Launch the game!

## Config
Either use [Melon Preferences Manager](https://github.com/Bluscream/MelonPreferencesManager/releases) or edit your `MelonPreferences.cfg` in `<gamedir>\UserData`.
Example config:
``` TOML
[RunnerLogoMod]

LogoScaleFactor = 1.0 # multiplier for scale

LogoSpacing = 0.02 # distance between the logo and the screen edge expressed as percentage of screen size e.g. 0.05 = 5%

LogoBorderSize = 0.05 # percentage of logo size e.g. 0.05 = 5%

ShowLogo = true # if false the logo doesn't display at all

BorderColor = "#00ff22" # HEX code

LogoOpacity = 0.5 # percentage alpha value e.g. 0.5 = 50%

LogoPosition = "TopRight" # possible values = {TopRight, TopLeft, BottomRight, BottomLeft}

UseSteamAvatar = false # if false, the logo is loaded from a "RunnerLogo.png" present in your mods folder
```

## Building
### Requirements
- Visual Studio 2026 w/ .Net Desktop Development
- .NET 4.7.2
### Process
1. Create a new sub-folder in the project directory called `lib`.
2. Copy all Unity `.dll` files and `Assembly-CSharp.dll` from `<gamedir>\NeonBoost_Data` into `lib/`.
3. Copy `0Harmony.dll` & `MelonLoader.dll` from `<gamedir>\MelonLoader\net472\` into `lib/`.
4. Open `NeonBoostLogoMod.slnx` with VS 2026.
5. Build the project as release.

## Additional Notes

Once you've confirmed your MelonLoader install is functional, make sure to add `--melonloader.hideconsole` to your game launch properties (Neon Boost in your Steam library -> properties -> launch options at the bottom of that window). This will help your game launch faster.

