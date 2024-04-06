# ArchidektProxyPrinter

It is console application to generate printable and editable document with cards from a deck stored in Archidekt or exported to text file.

## Features

- Download deck list straight from archidekt via url or deck id
- Get deck list from exported file
- Add cards number of times per number of quantity
- Print dual side cards
- Save cards resized and adjusted for printing in editable Word .docx format
- Option to store original images alongside created document
- Logs in separate file showing errors in receiving data
- Show % progress status in console app

## Plans

- Download cards from specified expansion or specific version
- Produce PDF document instead of Word
- Create Web version in ASP.NET API

## Usage

Call ArchidektProxyPrinter from command liner with one of the following options:

    -- deck-file-path  <String>
    -- deck-id  <String>
    -- deck-url  <String>

List of all parameters:
```
Options:
  --deck-file-path <String>      Filepath to exported deck from Archidekt
  --deck-id <Int32>              ID of the deck in Archidekt
  --deck-url <String>            URL link to deck in Archidekt
  --output-path <String>         Directory path to output file(s)
  --output-file-name <String>    Filename of the output word file
  --store-original-images        Flag to store original images in the same folder as output file
  -h, --help                     Show help message
  --version                      Show version
  ```

## Installation

TODO