//
//  TappGoShim.Widgets.swift
//  com.tapp.go.widgets
//
//  The widget and Picture in Picture C entry points Unity's `DllImport("__Internal")` resolves against.
//
//  Marshalling and nothing else, exactly as core's `TappGoShim.swift` — read it for why a shim is a source
//  file, and for the three helpers used here (`tappGoPayload`, `tappGoHandOff`, `TappGoReplyTarget`), which
//  it declares. Unity compiles every Tapp shim into the same module, UnityFramework, so they are in scope.
//
//  Imports `Tapp` and nothing else, on purpose: the whole `TappBridge` facade lives in the core framework,
//  which finds the Widgets module by its Obj-C class name at run time. So UnityFramework links one native
//  product however many Tapp packages are installed — the module itself is linked into the app, by core's
//  build hook, because this package asked for it.
//
//  The symbol names are a published contract — see docs/NATIVE-CONTRACT.md in the core repo.
//

import Foundation
import Tapp

// MARK: - Widgets

@_cdecl("TappGo_ValidateToken")
public func TappGo_ValidateToken(
    _ json: UnsafePointer<CChar>?,
    _ context: UnsafeMutableRawPointer?,
    _ reply: TappGoReply?
) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.validateToken(json: tappGoPayload(json)) { target.send($0) }
}

/// Takes no payload, by design — see `TappGo_Logout`.
@_cdecl("TappGo_ShowNextWidgetEntry")
public func TappGo_ShowNextWidgetEntry() -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.showNextWidgetEntry())
}

@_cdecl("TappGo_MoveToWidgetEntry")
public func TappGo_MoveToWidgetEntry(_ json: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.moveToWidgetEntry(json: tappGoPayload(json)))
}

@_cdecl("TappGo_SetWidgetText")
public func TappGo_SetWidgetText(_ json: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.setWidgetText(json: tappGoPayload(json)))
}

@_cdecl("TappGo_SetWidgetCountdownDuration")
public func TappGo_SetWidgetCountdownDuration(_ json: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.setWidgetCountdownDuration(json: tappGoPayload(json)))
}

/// `{}` is the ordinary payload here — "any declared widget" — so this one does take JSON.
@_cdecl("TappGo_IsWidgetInstalled")
public func TappGo_IsWidgetInstalled(_ json: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.isWidgetInstalled(json: tappGoPayload(json)))
}

@_cdecl("TappGo_PendingWidgetLink")
public func TappGo_PendingWidgetLink() -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.pendingWidgetLink())
}

// MARK: - Picture in Picture

@_cdecl("TappGo_StartPictureInPicture")
public func TappGo_StartPictureInPicture(
    _ json: UnsafePointer<CChar>?,
    _ context: UnsafeMutableRawPointer?,
    _ reply: TappGoReply?
) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.startPictureInPicture(json: tappGoPayload(json)) { target.send($0) }
}

/// Takes no payload, by design — see `TappGo_Logout`.
@_cdecl("TappGo_StopPictureInPicture")
public func TappGo_StopPictureInPicture(_ context: UnsafeMutableRawPointer?, _ reply: TappGoReply?) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.stopPictureInPicture { target.send($0) }
}
