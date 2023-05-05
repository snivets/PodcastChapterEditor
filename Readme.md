# Audio Chapter Editor

This is an audio chapter editor tool for Windows, built using WinUI 3. It's straightforward, minimal, (hopefully) good-looking, and useful if you're looking for a tool to embed podcast chapters in audio files on Windows. Such a tool exists out there for Mac, and Descript and other such tools can embed chapter marks in files they create, but if you want to edit these frames thereafter, this is the tool for you!

**[Link to the app on the Windows Store ($4.99)](https://www.microsoft.com/store/productId/9P0N6801VP3N)**

![Audio chapter editor screenshot](/Resources/screenshot.png?raw=true "Screenshot of Nate's audio chapter editor")

## Features:
* Changing cover art
* Editing timestamps through a variety of friendly formats - you can type things like 0455 in the timestamp textbox and it will be converted to 00:04:55.000, or 024933 for 02:49:33.000.
* Editing chapter title text
* Adding and removing chapters (including creating chapters in files with none)
* Rearranging chapters by moving a given chapter up one index at a time

## Possible future features:
* Ways to paste timestamp shorthands from clipboard
* Editing images per-chapter - feedback requested if useful
* Export chapters as Spotify- and YouTube-friendly text format
* ?? (Email me for feature suggestions!)

_This tool is majorly indebted to the [ATL.NET audio library](https://github.com/Zeugma440/atldotnet), which is wonderful and makes the backend of this rather straightforward._