// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.CreateOutputFolder(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.DirectoryExists(System.String)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.GetFilename(System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.IO.FileManager.ReturnCorrectWordFilePath(System.String,System.String)~System.String")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.ScryfallService.HandleDualSideCards(Library.Models.DTO.Scryfall.CardDataDTO,System.Collections.Generic.HashSet{Library.Models.DTO.CardSideDTO})")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.ScryfallService.HandleArtCards(Library.Models.DTO.CardEntryDTO,System.Collections.Generic.HashSet{Library.Models.DTO.CardSideDTO})")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.ScryfallService.HandleRelatedTokens(Library.Models.DTO.CardEntryDTO,Library.Models.DTO.Scryfall.CardDataDTO,System.Int32)")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods", Scope = "member", Target = "~M:Library.Services.ArchidektService.ParseCardsToDeck(System.Collections.Generic.ICollection{Library.Models.DTO.Archidekt.DeckCardDTO})~System.Collections.Generic.List{Library.Models.DTO.CardEntryDTO}")]
