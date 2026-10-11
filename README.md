# Tapp for Unity — Widgets

Server-configured **home-screen widgets** on iOS and Android, and **Picture in Picture** on iOS, from Unity —
without shipping a new app build.

One of Tapp's product packages. It needs **Tapp Core** (`com.tapp.go`) at the same version, and nothing else:
install it without the Live Activities package and your app does not carry that library at all.

## Install

```json
{
  "dependencies": {
    "com.tapp.go": "https://github.com/tappgo/tapp-unity.git#v2.1.1",
    "com.tapp.go.widgets": "https://github.com/tappgo/tapp-widgets-unity.git#v2.1.1"
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
`Tapp.HandleUrl`: it waits in `TappWidgets.PendingLink()`, once.

**That includes a link in your own URL scheme**, from 2.1.1, which pins the iOS SDK at 2.2.2; through 2.1.0
such a link arrived through `Application.deepLinkActivated`. A claim
link like `tapp://collect?…&token=…` is parked rather than opened: iOS would hand it to whichever installed app
claims `tapp`, so with two Tapp games on one phone the other game could open and receive your token. It no
longer fires `Application.deepLinkActivated`. Two things to do:

- **Keep the scheme registered** under *Player Settings → iOS → Supported URL schemes*. iOS recognises a link as
  your game's own only by a scheme it declares; an undeclared one is still opened by iOS, for whichever app
  claims it.
- **Read on focus, and once more a moment later.** On iOS the link is parked a few tens of milliseconds after
  your game becomes active, which `OnApplicationFocus(true)` can beat. A read that finds nothing consumes
  nothing, so the second read is safe; what loses a claim is dropping a link a read returned.

```csharp
void OnApplicationFocus(bool focused) { if (focused) StartCoroutine(CollectWidgetLink()); }

IEnumerator CollectWidgetLink()
{
    if (Claim(TappWidgets.PendingLink().Value)) yield break;   // Claim: validate the token; false for null
    yield return new WaitForSecondsRealtime(0.5f);
    Claim(TappWidgets.PendingLink().Value);
}
```

On iOS a widget link in *another* app's scheme, or https, is opened by iOS and never appears in `PendingLink()`.
On Android every widget link waits there, whatever its scheme.

## Errors

| Code | What to do |
|---|---|
| `pictureInPictureUnsupported` | This device or platform cannot present one. Carry on silently. |
| `pictureInPictureBackgroundAudioMissing` | Turn on *Picture in Picture* in Project Settings → Tapp → Widgets and rebuild. |
| `pictureInPictureVideoUnavailable` / `pictureInPictureStartFailed` | The message carries the reason. |

## Links

- [Documentation](https://documentation.tappgo.com/v2/unity/overview) · [Changelog](CHANGELOG.md)
