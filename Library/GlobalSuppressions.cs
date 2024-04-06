// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods with complex logic", Scope = "member", Target = "~M:Library.Services.DeckService.TryExtractDeckIdFromUrl(System.String,System.Int32@)~System.Boolean")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Avoid using static methods with complex logic", Scope = "member", Target = "~M:Library.Services.DeckService.ParseCardsToDeck(System.Collections.Generic.ICollection{Library.Models.DTO.Archidekt.DeckCardDTO})~System.Collections.Generic.Dictionary{System.String,Library.Models.DTO.CardEntryDTO}")]
