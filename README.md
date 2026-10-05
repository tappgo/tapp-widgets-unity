# Tapp for Unity — Widgets

Server-configured **home-screen widgets** on iOS and Android, and **Picture in Picture** on iOS, from Unity —
without shipping a new app build.

One of Tapp's product packages. It needs **Tapp Core** (`com.tapp.go`) at the same version, and nothing else:
install it without the Live Activities package and your app does not carry that library at all.

## Install

```json
{
  "dependencies": {
    "com.tapp.go": "https://github.com/tappgo/tapp-unity.git#v2.1.0",
    "com.tapp.go.widgets": "https://github.com/tappgo/tapp-widgets-unity.git#v2.1.0"
  }
}
```

Both lines, on the **same tag** — core checks, and fails the build on a mixed set. Requirements, the OS floors
and what to expect in the Editor and on the Simulator are in
[core's README](https://github.com/tappgo/tapp-unity#readme).

## Configure

**Project Settings → Tapp → Widgets.** Add your **widget ids**, one per campaign, from the Tapp back office.
Stored at `Assets/Resources/TappWidgetsSettings.asset` — commit it.

Then call `Tapp.Configure()` once at startup, as core's README shows. Tapp does not configure itself,
and the calls below need it.

**Widget ids are shared by both platforms, and their order is a contract**: on iOS a placed widget is known by
its position in the list, so change an id in place to move that slot to a new campaign, append to add one, and
never reorder. One iOS widget extension serves up to eight widgets, so an iOS build is refused with more ids
than that.

Installing this package is what makes the iOS build link the `TappWidgets` module and put the widgets into the
widget extension Tapp creates, and the Android build declare `com.tappgo:tapp-widgets` and create one
home-screen widget per id. *Picture in Picture* is off by default: turning it on adds `audio` to
`UIBackgroundModes`, which App Review asks about.

**Picture in Picture alone needs no widget ids.** Turn it on and leave the list empty: the iOS build links the
module into your app and adds no widget — and, with no other product drawing in one, no widget extension at
all — and the Android build stages no home-screen widget. With the list empty and Picture in Picture off the
package has nothing to do, and the settings page and both builds say so.

## Use

```csharp
using TappGo;

TappWidgets.ShowNextEntry();                                   // every widget turns to its next screen
TappWidgets.SetText(widgetId, "screen_1", "title", "3 spins left!");
TappWidgets.SetCountdownDuration(widgetId, "screen_1", "Countdown", 90);
if (!TappWidgets.IsInstalled().Value) ShowAddWidgetTutorial();

// A widget tap brought the app forward: read the link once, validate the token it carries
var link = TappWidgets.PendingLink().Value;
if (link != null)
    TappWidgets.ValidateToken(TokenIn(link), widgetId, result => { if (result.Value.IsValid) Grant(result.Value.Prize); });
```

**One API on both platforms.** Picture in Picture answers `pictureInPictureUnsupported` on Android; the
pin-widget calls answer `false` on iOS. Neither needs an `#if`. A widget's link does not come through
`Tapp.HandleUrl`: it waits in `TappWidgets.PendingLink()`, once — with one exception on iOS, below.

**One case on iOS arrives as a deep link instead.** A claim link authored *with a URL scheme*
(`mygame://claim?token=…`) is opened by iOS itself, so it reaches your game through
`Application.deepLinkActivated` — and `Application.absoluteURL` on a cold start — and never appears in
`PendingLink()`. Register the scheme under *Player Settings → iOS → Supported URL schemes* and read the token
from that URL the same way. A link with no scheme waits in `PendingLink()`; on Android every link does.

## Errors

| Code | What to do |
|---|---|
| `pictureInPictureUnsupported` | This device or platform cannot present one. Carry on silently. |
| `pictureInPictureBackgroundAudioMissing` | Turn on *Picture in Picture* in Project Settings → Tapp → Widgets and rebuild. |
| `pictureInPictureVideoUnavailable` / `pictureInPictureStartFailed` | The message carries the reason. |

## Links

- [Documentation](https://documentation.tappgo.com/v2/unity/overview) · [Changelog](CHANGELOG.md)
