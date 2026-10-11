# Changelog

All notable changes to `com.tapp.go.widgets`. Follows [Semantic Versioning](https://semver.org). Every Tapp package is
released at one version, and needs Tapp Core (`com.tapp.go`) at that same version.

## [2.1.1] — 2026-10-11

Resolves iOS `tappgo/tapp-ios` **2.2.2** and Android `com.tappgo` **2.1.1**, as pinned by Tapp Core. No C# API,
settings or bridge change: everything below comes from the two native releases. **One thing to do on iOS** if your
game reads widget links in `Application.deepLinkActivated` (the first entry).

- **iOS: a widget link in your game's own URL scheme waits in `TappWidgets.PendingLink()`** instead of arriving
  through `Application.deepLinkActivated`. A Collect tap opened `tapp://collect?sender=tapp&token=…` through iOS, which hands a scheme to *any*
  installed app claiming it: with two Tapp games on one phone the tap could open the other game and give it the
  claim token. **What to change:** read `PendingLink()` in `OnApplicationFocus(true)` and once more about half a
  second later (iOS parks the link a few tens of milliseconds after the game becomes active), and keep the scheme
  registered under *Player Settings → iOS → Supported URL schemes*. Android is unchanged: every widget link
  already waits there.
- **`ValidateToken` no longer refuses a real win from your game's second widget**, on both platforms. A claim link
  does not say which widget it came from, so a game with two widget ids could pass the other one's id, and the
  token was answered invalid. The SDK now asks under the widget on this device that minted the token, whichever
  of your ids you pass. Nothing changes for a game with one widget.
- **A widget drawn while offline fills in its pictures once the network is back**, on both platforms. It used to
  keep empty frames where its pictures go until the game was opened or the widget's regular update came round.
  It now tries again a few minutes later, up to three times; on Android the game's next open also fills it in.
- **Android 8 to 11: widgets work.** A campaign with platform-font text or a turned node did not load, and a
  wheel's spin and collect button took no taps. Widgets on those versions now load, take taps and look as they
  do on Android 12 and later.
- **Android 8 to 12: a widget with many small pictures no longer stops every widget on the launcher**, such as a
  slot animation drawn in a small widget. The update is now kept within what those versions can deliver.
- **Android: a spin turns every placed copy of its campaign**, as on iOS. A small and a large copy of one campaign
  now turn together; before, the other copy jumped to the result.
- **Android: a scratch card's second play starts covered.** It showed its prize from the first frame.
- **iOS: a widget's background no longer leaves a thin line along two edges** when the widget's shape differs very
  slightly from its design's canvas.

## [2.1.0] — 2026-10-05

Resolves iOS `tappgo/tapp-ios` **2.2.1** and Android `com.tappgo` **2.1.0**, as pinned by Tapp Core.

- **First release.** Home-screen widgets on iOS and Android, Picture in Picture on iOS and widget pinning on
  Android are new in 2.1.0, in a package of their own installed beside Tapp Core at the same version. A project
  that does not install this package does not link the Widgets module on either platform.
- **Installed by Git URL** from the generated public repo [`tappgo/tapp-widgets-unity`](https://github.com/tappgo/tapp-widgets-unity),
  beside Tapp Core from `tappgo/tapp-unity` at the same tag. Signed with Unity's package signature,
  attributed to the Tapp organization.
- **`TappWidgets`** is the whole API: the widget commands (`ShowNextEntry`, `MoveToEntry`, `SetText`,
  `SetCountdownDuration`), `IsInstalled`, the claim (`PendingLink`, `ValidateToken`), Picture in Picture
  (`StartPictureInPicture`, `StopPictureInPicture`) and pinning on Android (`IsPinSupported`, `RequestPin`,
  `PlacedCount`). One API on both platforms: Picture in Picture answers `pictureInPictureUnsupported` on
  Android, and the pin calls answer `false`/`0` on iOS.
- Settings of its own (`TappWidgetsSettings.asset`: widget ids, *Picture in Picture*, the Android widget's
  label, description and update period), created from Project Settings → Tapp. Every field has a default.
- iOS: links `TappWidgets`, stages the app's App Intents package, renders `TappWidgets.all` in the widget
  extension, and adds `audio` to `UIBackgroundModes` only with Picture in Picture on. The extension gets
  Keychain Sharing under the App Group beside the App Group itself: a widget authenticates on its own and
  decrypts its cache with a key kept there.
- Android: declares `com.tappgo:tapp-widgets`, creates one home-screen widget per id, and runs the widgets
  module's own `configure` after core's — without it `IsInstalled` answers `false` for a placed widget.
- **Picture in Picture alone needs no widget ids.** With the list empty and Picture in Picture on, the iOS
  build links the module into the app and asks nothing of the widget extension, and the Android build stages
  no widget. With the list empty and Picture in Picture off, the settings page and both builds refuse.
- An iOS build is refused with more than eight widget ids — the most one widget extension serves — instead of
  passing and rendering placeholders on a device.
