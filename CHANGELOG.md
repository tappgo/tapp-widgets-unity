# Changelog

All notable changes to `com.tapp.go.widgets`. Follows [Semantic Versioning](https://semver.org). Every Tapp package is
released at one version, and needs Tapp Core (`com.tapp.go`) at that same version.

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
