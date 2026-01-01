# MoonlightAppImport

A [Playnite](https://github.com/JosefNemec/Playnite) plugin that imports games from a [Sunshine](https://github.com/LizardByte/Sunshine), [Apollo](https://github.com/ClassicOldSong/Apollo), or [Vibepollo](https://github.com/Nonary/Vibepollo) host and launches them directly with [Moonlight](https://github.com/moonlight-stream/moonlight-qt).

Once configured, you never need to open the Moonlight UI again, except to tweak stream-quality settings.

Picture this: A tiny living-room PC boots straight into Playnite’s Fullscreen mode. From the couch, you browse your library with a controller, hit *Play*, and the Moonlight stream starts automatically. When you quit the game, the stream ends and Playnite regains focus, ready for the next title. It’s a console-style experience entirely within Playnite.

---

## Features
- Supports Sunshine, Apollo, and Vibepollo hosts.
- Imports host apps and launches them directly in Moonlight. No Moonlight UI required.
- Stores your Sunshine password and Vibepollo API key **encrypted** with Windows DPAPI.

---

## Installation

1. Download the latest `MoonlightAppImport_60ea0079-bf4b-417c-a1f3-d5470ec5e96b_<version>.pext` from the releases page.
2. In Playnite open **Library ▸ Configure Integrations ▸ Moonlight App Import**.
3. Set the full path to `Moonlight.exe` (e.g. `C:\Program Files\Moonlight Game Streaming\Moonlight.exe`).
4. Enter the host address (e.g. `192.168.1.69`).
5. Configure your host:
    - For **Sunshine** and **Apollo**, enter your **Username** and **Password**.
    - Also, for **Apollo**, enable the **“Use Apollo instead of Sunshine”** checkbox.
    - For **Vibepollo**, enter your **API Key**. The plugin will automatically use Vibepollo if this field is filled.
6. Enable **“Allow self-signed certificate”** if your host uses the default, self-signed TLS cert (the common case).
7. Enable **"Ping server"** if your host can be pinged and you want to check if it's online before doing an API call. **Note: On Windows 11 hosts ping might be disabled by default. [Either enable it in your firewall](https://superuser.com/questions/1683853/cannot-ping-a-windows-11-machine) or deactivate this checkbox.**
8. Run **Library ▸ Update Game Library ▸ Moonlight App Import**.
9. The plugin fetches all configured apps from the host and adds them to Playnite.

---

## Best-practice setup

I suggest the following setup:
- A Server with [Vibepollo](https://github.com/Nonary/Vibepollo), plus [Playnite](https://github.com/JosefNemec/Playnite).
- A Client with [Moonlight](https://github.com/moonlight-stream/moonlight-qt), [Playnite](https://github.com/JosefNemec/Playnite) and MoonlightAppImport.

Typical workflow:

1. **Install** the game on the server.
2. **Add** it to Playnite (server side).
3. **Export** it to your host via *Vibepollo*.
4. On the **client**, update the library with *MoonlightAppImport*.
5. **Play** the game from Playnite on the client.

Enjoy couch-friendly game streaming without leaving Playnite!
