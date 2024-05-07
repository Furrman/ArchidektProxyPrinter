# ArchidektProxyPrinter

Have you ever thought about building your deck in Magic The Gathering? Have you ever found yourself missing some cards for your newly created deck, but still wanted to give it a try? Do you need some card replacements until your new cards arrive? Or perhaps you want to build a deck before spending real money on it?

If so, then this application is for you! It allows you to generate a printable and editable document with cards from a deck stored in Archidekt or exported to a text file. With this application, you can easily print your previously created deck and try it out at the table with your friend(s)!

## Features

- Download deck list straight from archidekt via url or deck id
- Get deck list from exported file
- Add cards number of times per number of quantity
- Print dual side cards
- Download cards from specified expansion and specific card version
- Support art cards
- Save cards resized and adjusted for printing in editable Word .docx format
- Option to download all cards in specific language (cards not found in given language will be replaced with default english language)
- Option to store original images alongside created document
- Logs in separate file showing errors in receiving data
- Show % progress status in console app

## Plans

- Options to add tokens that are generated from cards in the deck
- Produce read-only PDF document instead of Word
- Create Web version in ASP.NET API with Blazor
- Host Web version via Github Pages
- Build Web version via Github Actions
- Support to other tools like Moxfield, EDHREC, MTGArena or MTGGoldfish

## Limitation

- Download tokens not supported (Archidekt is not supporting tokens https://archidekt.com/forum/thread/884003/1 )
- Download selected cards in specific language not supported (Archidekt does not support card language selection)
- Custom cards are not supported (Archidekt does not support unofficial cards)
- Foil version of non unique card arts are not supported (Scryfall API that provide high quality foil card images except etched foil and unique foiled arts)
- Import based on file exported from Archidekt does not support specific card version (missing *card number* information in exported file)
- Supports only cards in english or unique version of cards (Archidekt is not supporting different card language versions https://archidekt.com/forum/thread/2627536/1 )

## Usage

Call ArchidektProxyPrinter file (can have .exe extension) from command liner with one of the following options:

    -- deck-file-path  <String>
    -- deck-id  <String>
    -- deck-url  <String>

List of all parameters:
```
Usage: ArchidektProxyP [--deck-file-path <String>] [--deck-id <Int32>] [--deck-url <String>] [--language-code <String>] [--token-copies <Int32>] [--print-all-tokens] [--output-path <String>] [--output-file-name <String>] [--store-original-images] [--help] [--version]

ArchidektProxyPrinter

Options:
  --deck-file-path <String>      Filepath to exported deck from Archidekt
  --deck-id <Int32>              ID of the deck in Archidekt
  --deck-url <String>            URL link to deck in Archidekt
  --language-code <String>       Set language for all cards to print
  --token-copies <Int32>         Number of copy for each token (Default: 0)
  --print-all-tokens             Print all tokens or reduce different version of the same token
  --output-path <String>         Directory path to output file(s)
  --output-file-name <String>    Filename of the output word file
  --store-original-images        Flag to store original images in the same folder as output file
  -h, --help                     Show help message
  --version                      Show version
  ```

## Installation

Download existing file from Github Releases section of this project https://github.com/Furrman/ArchidektProxyPrinter and put it anywhere you want. You can add path to this console app in your PATH environment variable.

## Instruction

You need to have .NET8 SDK installed to build this solution. You can use Visual Studio, Visual Studio Code IDE or just simple terminal. The command to run the build is:

`dotnet build`

To publish your application use:

`dotnet publish --configuration Release`
