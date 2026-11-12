//
//  __TAPP_PROVIDER_CLASS__.java
//  Tapp home-screen widget for the campaign __TAPP_WIDGET_ID__
//
//  GENERATED — do not edit in your Gradle project.
//
//  Unity regenerates the Gradle project on every build, and the Tapp build hook re-stages this file from
//  the package each time — once per widget id in Project Settings → Tapp, with the class named after the
//  id. Anything you change here is lost on the next build.
//
//  The whole of a host's widget class is the empty subclass: fetching, conditions, layout and taps all
//  happen inside the SDK. The manifest receiver the hook writes beside this names the class and carries
//  the widget id as `com.tappgo.sdk.WIDGET_ID`, which is how the SDK knows which campaign this serves.
//
//  Java rather than Kotlin because Unity's `unityLibrary` module applies no Kotlin plugin; extending the
//  SDK's Kotlin base class from Java needs only the Kotlin stdlib the AAR already depends on.
//
//  Tokens substituted at stage time (the hook fails the build if any survives):
//    __TAPP_PROVIDER_CLASS__   the class name, derived from the widget id
//    __TAPP_WIDGET_ID__        the campaign this provider serves
//

package com.tappgo.unity;

public final class __TAPP_PROVIDER_CLASS__ extends com.tappgo.sdk.TappWidgetProvider {
}
