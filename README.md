# MoonlightAppImport
A [Playnite](https://github.com/JosefNemec/Playnite) Plugin to import games from a [Sunshine](https://github.com/LizardByte/Sunshine) or [Apollo](https://github.com/ClassicOldSong/Apollo) server to Playnite and open them directly with [Moonlight](https://github.com/moonlight-stream/moonlight-qt).
With using this plugin there is no need to open the Moonlight UI ever again, except for configuring the stream quality.
For example you could have a Mini-PC as your client in your living room and open Playnite in Fullscreen Mode at launch.
Then you could navigate to your favourite game using the controller only and just launch it. If you are done playing, exit the game normally. The Moonlight stream will end and Playnite will be back to focus.
From here you can choose to open another game or shut down the PC.
All from Playnite! Offering you the true Console Experience!
## Features
- Works with Sunshine and Apollo hosts
- Import games from Sunshine and open them directly in Moonlight. There is no need for using the Moonlight UI anymore!
- Stores the Sunshine password encrypted
## Installation
1. Download and install the [MoonlightAppImport_60ea0079-bf4b-417c-a1f3-d5470ec5e96b_X.X.pext](https://github.com/SolemnDucc/MoonlightAppImport/releases)
2. Go to "Library" > "Configure Integrations" > "Moonlight App Import"
3. Enter the full path to the Moonlight.exe (C:\Program Files\Moonlight Game Streaming\Moonlight.exe)
4. Enter the host address of the Sunshine server (192.168.1.69)
5. Enter the username of the Sunshine server
6. Enter the password for the Sunshine server (it is encrypted using your Windows User)
7. Check if you are using Sunshine or the fork Apollo
8. Check if you need to skip the certificate validation because the sunshine server does not have a valid certificate
9. Start the update with "Update Game Library" > "Moonlight App Import"
## Best Practices
I suggest the following setup:
- A Server with [Apollo](https://github.com/ClassicOldSong/Apollo), [Playnite](https://github.com/JosefNemec/Playnite) and the plugins [SunshineAppExport](https://github.com/MichaelMKenny/SunshineAppExport) and [PlayNiteWatcher](https://github.com/Nonary/PlayNiteWatcher).
- A Client with [Playnite](https://github.com/JosefNemec/Playnite) and [MoonlightAppImport](https://github.com/SolemnDucc/MoonlightAppImport/)
Then you would do the following if you want to play a game in Moonlight:
1. Download and install the game on the server.
2. Add the game to Playnite.
3. Add the game to Apollo using the SunshineAppExport script.
4. On the client: Add the game to Playnite using this plugin.
5. Start the game thru Playnite on your client.