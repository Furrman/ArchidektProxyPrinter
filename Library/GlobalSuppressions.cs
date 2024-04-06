// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.MagicCardService.TryExtractDeckIdFromUrl(System.String,System.Int32@)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.MagicCardService.ParseCardsToDeck(System.Collections.Generic.ICollection{Library.Models.DTO.Archidekt.DeckCardDTO})~System.Collections.Generic.Dictionary{System.String,Library.Models.DTO.CardEntryDTO}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.CardListFileParser.ParseLine(System.String)~System.Nullable{System.ValueTuple{System.String,System.Int32}}")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.CreateOutputFolder(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.DirectoryExists(System.String)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.GetFilename(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.ReturnCorrectWordFilePath(System.String,System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.WordGeneratorService.ExtractPattern(System.String,System.Int32@,System.String@)")]
