# Sharing items between users should be easy.

## Feature set

### General
- All import/Exports are a json encoded using base64.
- All import/Exports can be done 
  - through Clipboard using Ctrl-c/Ctrl-v 
  - through Text file using a file dialog
  - through PasteBin link in the clipboard (see https://pastebin.com/doc_api)
    - This feature is not available (greyed out) as long as the user didn't configured his pastbin api key in the tool.
	  - SettingsDialog must have a new input string for this api key.
	  - this limitation exist only for "export to pastbin", a user having a pastbin link can always import (all pastbin links are public).

### Import logic 
- must reject with warning any invalid json data.
- must check for the item space in the target container and warn the user if there is none.
- must 
  - on an empty container : try to restore each items at their original location.
  - on an non empty container : restore each items at first available space.
  
### Import scope
- a single item
- a vault tab content
- a full vault (all tabs) content
  - ask the user for import strategy (Use standard confirm dialog, Dedicated Custom one if too limited)
    - to an existing vault
      - ask user for override if destination is not empty (strong emphasis on data lose risk)
        - if yes clean all tabs before import
    - to a new vault (create and import)
      - Resolve name conflict with a vault name suffix

### Export
- a single item
- a vault tab content
- a full vault (all tabs) content

### Documentation
- Create a documentation file that describe the feature in ./documentation/IMPORT_EXPORT.md

### Git considerations
- use the dedicated branch "optim-share-items".
- Commit at every steps, so it's easy to track/revert and follow intent.


