# Firebase SDK Installation Guide

**Project**: When I'm Cleaning Windows  
**Current Status**: Ready for Firebase installation  
**Date**: February 2026

---

## Overview

The codebase is prepared for Firebase integration. All Firebase-dependent code is wrapped with `#if FIREBASE_ENABLED` preprocessor directives, allowing the project to run without Firebase while maintaining support for it when installed.

---

## Prerequisites

1. Firebase Console Account (https://console.firebase.google.com/)
2. Unity 6.3 LTS (already installed)
3. External Dependency Manager (EDM) - Already in project at `Assets/ExternalDependencyManager/`

---

## Installation Steps

### Step 1: Download Firebase Unity SDK

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create a new project or select existing project
3. Download **Firebase Unity SDK** for your platform:
   - [Firebase Unity SDK Download](https://firebase.google.com/download/unity)
4. Extract the downloaded file to a temporary location

### Step 2: Import Firebase into Unity

1. Open the project in Unity 6.3 LTS
2. **Assets → Import Package → Custom Package**
3. Navigate to the extracted Firebase SDK folder
4. Select ALL Firebase packages you need:
   - `FirebaseAnalytics.unitypackage`
   - `FirebaseAuth.unitypackage`
   - `FirebaseRemoteConfig.unitypackage`
   - `FirebaseStorage.unitypackage`
   - `FirebaseCrashlytics.unitypackage`
5. Click **Import** and wait for compilation

### Step 3: Enable FIREBASE_ENABLED Preprocessor

1. **File → Build Settings**
2. Select your target platform (Android/iOS)
3. **Player Settings → Other Settings → Scripting Define Symbols**
4. Add: `FIREBASE_ENABLED`
5. Click **Apply**

### Step 4: Configure Firebase Project

1. Download `google-services.json` from Firebase Console
2. Place in `Assets/` folder (Already present in project)
3. Download `GoogleService-Info.plist` (iOS)
4. Place in `Assets/` folder (Already present in project)

### Step 5: Set Up Service Accounts

1. In Firebase Console, go to **Project Settings**
2. Download **Service Account Key** (JSON file)
3. Save to: `Assets/Resources/firebase-key.json`

### Step 6: Initialize in Editor

1. **Window → External Dependency Manager → Android Resolver** (if Android)
2. Click **Force Resolve** (EDM will resolve all Firebase dependencies)
3. Wait for compilation

### Step 7: Test Firebase Connection

1. In Editor, open **Window → Firebase → Remote Config Manager**
2. Verify connection shows "✓ Firebase Connected"
3. Check Console for: `[Firebase] ✓ Services Initialized`

---

## Code Integration

Once Firebase is installed, these features activate automatically:

### Firebase Analytics
- **File**: `Assets/Scripts/Analytics/FirebaseManager.cs`
- **Features**:
  - Crash reporting via Crashlytics
  - Custom event logging
  - User tracking
- **Check Conditional**: Lines 1-15 show `#if FIREBASE_ENABLED`

### Remote Config
- **File**: `Assets/Scripts/Analytics/RemoteConfigManager.cs`
- **Features**:
  - Live parameter tuning without builds
  - A/B testing support
  - Feature flags
- **Usage**: `RemoteConfigManager.Instance.GetInt("key", defaultValue)`

### Cloud Save
- **File**: `Assets/Scripts/CloudSave/CloudSaveManager.cs`
- **Features**:
  - Save game data to Firebase Storage
  - Auto-backup on level complete
  - Conflict resolution
- **Usage**: `CloudSaveManager.Instance.SaveToCloud()`

---

## Preprocessor Flags Reference

**`#if FIREBASE_ENABLED`** wraps:

### FirebaseManager.cs
```csharp
#if FIREBASE_ENABLED
    private DependencyStatus dependencyStatus;
    private Firebase.Crashlytics.FirebaseCrashlytics crashlytics;
    // ... Firebase initialization code
#else
    // Stub implementations
#endif
```

### RemoteConfigManager.cs
```csharp
#if FIREBASE_ENABLED
    var result = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(...);
#else
    // Uses default values from dictionary
#endif
```

### CloudSaveManager.cs
```csharp
#if FIREBASE_ENABLED
    storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
    // ... Cloud save implementation
#else
    Debug.LogWarning("[CloudSave] Firebase not installed.");
#endif
```

---

## Build Configurations

### Development Build (No Firebase)
```
Scripting Define Symbols: (empty or without FIREBASE_ENABLED)
Result: Game runs with default values, no Firebase features
```

### Production Build (With Firebase)
```
Scripting Define Symbols: FIREBASE_ENABLED
Result: Full Firebase integration active
```

### Testing Build (Firebase Optional)
```
Scripting Define Symbols: FIREBASE_ENABLED
Result: Can toggle between Firebase and local testing
```

---

## Troubleshooting

### Issue: "Firebase not found" after import

**Solution**: 
1. Close Unity completely
2. Delete `Library/` folder
3. Reopen project (will recompile)
4. Reimport Firebase packages

### Issue: EDM not finding Firebase dependencies

**Solution**:
1. **Assets → External Dependency Manager → Android Resolver → Force Resolve**
2. Wait for gradle sync
3. Check `Assets/Firebase/` folder created

### Issue: "Service not initialized" at runtime

**Solution**:
1. Verify `google-services.json` in `Assets/`
2. Verify `FIREBASE_ENABLED` in Build Settings
3. Check Firebase project has correct bundle ID
4. View Console for specific error messages

### Issue: Firestore connection timeout

**Solution**:
1. Check internet connection
2. Verify Firebase project is active
3. Check Firebase quotas not exceeded
4. Verify authentication rules allow access

---

## Performance Considerations

Firebase SDK adds ~2-3MB to APK size. To minimize:

1. Use only needed Firebase modules (don't import all)
2. Lazy load Firebase features (InitializeOnDemand)
3. Use Remote Config to disable features on low-end devices

---

## Security Best Practices

1. **Never commit** `firebase-key.json` to public repos
2. Use **Firestore Security Rules** to restrict access
3. Enable **reCAPTCHA** for authentication
4. Set up **API restrictions** in GCP Console
5. Use **service account roles** with minimal permissions

---

## Next Steps

After Firebase is installed:

1. ✅ Run game in editor to verify connection
2. ✅ Test cloud save feature
3. ✅ Monitor Firebase Console for events
4. ✅ Build and test on device
5. ✅ Deploy to production with monitoring

---

## Support Resources

- [Firebase Documentation](https://firebase.google.com/docs)
- [Firebase Unity SDK](https://github.com/firebase/firebase-unity-sdk)
- [External Dependency Manager](https://github.com/googlesamples/unity-jar-resolver)
- [Project Technical Spec](TECHNICAL_SPEC.md)
