# Slice 4: PasteBin Integration

- **Type:** AFK
- **Blocked by:** Slice 2
- **User stories:** #4, #7, #9, #10, #19, #20, #21, #22

## What to build

Add PasteBin as a third transport channel. Users configure their PasteBin API key and paste expiration in Settings, then export/import via PasteBin URLs.

PasteBinService handles HTTP interaction with the PasteBin API. Exports upload the base64-encoded Export Format text and place the returned URL on the clipboard. Imports detect `https://pastebin.com/*` URLs in the clipboard, fetch the paste content, base64-decode, and import as usual.

PasteBin export options are greyed out (disabled) in all menus until the user configures their API key. PasteBin import is always available (PasteBin links are public).

## Acceptance criteria

- [ ] UserSettings has `PasteBinApiKey` (string, empty default) and `PasteBinExpiration` (string, default "1M")
- [ ] SettingsDialog has a text input for PasteBinApiKey and a dropdown for PasteBinExpiration with human-readable labels ("10 Minutes"→"10M", "1 Month"→"1M", "1 Year"→"1Y", etc.)
- [ ] PasteBin export menu items in all context menus are disabled (greyed out) when PasteBinApiKey is empty
- [ ] PasteBin export menu items are enabled when API key is configured
- [ ] Exporting to PasteBin uploads the base64-encoded payload via PasteBin API and places the returned URL on the user's clipboard
- [ ] PasteBin pastes use visibility=Unlisted (accessible by URL, not listed on public feed)
- [ ] Ctrl+V detects `https://pastebin.com/*` in clipboard, fetches the raw content, base64-decodes, and imports
- [ ] PasteBin import works without an API key (public links)
- [ ] PasteBinService handles HTTP errors gracefully with appropriate user-facing errors
- [ ] All new functionality has passing unit tests (PasteBinService with mocked HttpClient)
