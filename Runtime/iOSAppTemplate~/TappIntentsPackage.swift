//
//  TappIntentsPackage.swift
//  Compiled into the host APP target — not the extension.
//
//  GENERATED — do not edit in your Xcode project. Re-staged from the Tapp package on every build.
//
//  An interactive widget's tap that OPENS the app — a payload's CLAIM button — is an App Intent the app
//  performs, and iOS only routes it to an app that declares the SDK's intents package. Omit this and that
//  tap does nothing at all: no launch, no log, nothing to tell it apart from a tap that landed on no
//  control. The widget extension declares the same package for the taps it performs itself.
//
//  Nothing to configure and no tokens: declaring the package is the whole registration.
//

import AppIntents
import TappWidgets

// Gated to the SDK's runtime floor, not the app's deployment target: the app links at iOS 15.0, where neither
// `AppIntentsPackage` nor the SDK's package exists. Below 17.2 the SDK renders no widget, so there is no tap
// to route and nothing is lost by the declaration not existing there.
@available(iOS 17.2, *)
struct TappIntentsPackage: AppIntentsPackage {
    static var includedPackages: [any AppIntentsPackage.Type] { [TappWidgetIntentsPackage.self] }
}
