# Changelog

All notable changes to `com.tapp.go.widgets`. Follows [Semantic Versioning](https://semver.org). Every Tapp package is
released at one version, and needs Tapp Core (`com.tapp.go`) at that same version.

## [2.2.0] — 2026-10-19

Resolves iOS `tappgo/tapp-ios` **2.3.0** and Android `com.tappgo` **2.2.0**, as pinned by Tapp Core, both released on
2026-10-19. **No C# API, settings or bridge change**: everything below comes from the two
native releases, and the one call involved is Tapp Core's.

- **Pools.** An image or text node the back office publishes with several candidates shows one of them at random once
  per refresh of the widget's content — a scheduled refresh, a publish, a tap that moves to another screen. A spin, a
  countdown tick or a picture retry keeps the pick. Drawn natively; nothing to call and nothing to configure.
- **A widget published in several languages follows `Tapp.SetLanguage`** (new in Tapp Core 2.2.0; `null` follows the
  device again). Without a call the widget asks for the device's language and gets the closest the campaign is
  published in, or its base language. After a switch the campaign is re-fetched in the background and the widget
  redraws once the new content is cached. A translated screen is a screen of its own, so **`SetText` and
  `SetCountdownDuration` edits are not carried into it — set them again after a switch**, as after a publish.
- **A widget config cached by an earlier SDK is fetched once more after the upgrade**, on both platforms: the cache now
  remembers the language it was requested in, and a cache written before 2.2.0 names none. One extra request per
  widget, nothing to do.
- **Android: a widget on a Unity host no longer sits on the loading card after a reboot** until the app draws it. The
  launcher keeps no picture across a reboot and asks the provider for an update in a process where the host has not
  configured yet; the SDK now restores the last configuration in that callback, as a tap already did. Fixed in the
  Android SDK 2.2.0 (Tapp-Android D-167), found with this package's demo. No change on iOS.

## [2.1.2] — 2026-10-15

Resolves iOS `tappgo/tapp-ios` **2.2.3** and Android `com.tappgo` **2.1.1**, as pinned by Tapp Core. One new
setting; no bridge change. Neither native release in this version touched widgets: iOS 2.2.3 fixes a Live
Activity call order (Tapp Live Activities' changelog) and rebuilds the widgets framework unchanged.

- **iOS: choose the widget sizes the gallery offers** — *Widget sizes (iOS)* in Project Settings → Tapp → Widgets
  (`TappWidgetsSettings.iosWidgetSizes`, a `TappWidgetSizes` flags field): Small, Medium or both, for every widget
  id. Owner ruling of 2026-10-14: hosts can choose the iOS widget sizes, the docs lead with one small widget, and
  existing settings keep both. **Nothing changes for an existing project**: an asset without the field reads both
  sizes and generates `TappWidgets.all` exactly as before. A Widgets asset created from now on starts with Small
  alone, unless it is carried over from a pre-split project that had widget ids. One size generates one
  `TappSmallWidget` / `TappMediumWidget` per id, in id order. iOS knows a placed widget by its slot and its size,
  so dropping a size after release removes the widgets players placed at that size and keeps the rest. An iOS
  build with widget ids and no size is refused. Android has no counterpart.

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
