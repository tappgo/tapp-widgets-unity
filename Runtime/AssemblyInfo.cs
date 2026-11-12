using System.Runtime.CompilerServices;

// The Editor and test assemblies are separate compilation units, so `internal` doesn't reach them without
// this. Nothing here widens the *public* surface: adding a name to this list is not an API decision; adding
// a `public` member is.
[assembly: InternalsVisibleTo("TappGo.Widgets.Editor")]
[assembly: InternalsVisibleTo("TappGo.Widgets.Tests")]
[assembly: InternalsVisibleTo("TappGo.Widgets.Editor.Tests")]
