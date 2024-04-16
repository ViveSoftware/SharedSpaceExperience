# Shared Space Experience

Copyright 2023 - 2024, HTC Corporation. All rights reserved.

## About

Shared Space Experience is an MR co-location example for VIVE XR Elite. This example shows how to allow users to interact in the same physical and virtual space.

In this project, we demonstrate how to align the space between two VIVE XR Elites by [Trackable Marker](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRTrackableMarker.html) and [Spatial Anchor](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRScenePerception.html#spatial-anchor).

## Requirements

- Hardware
  - 2 or more VIVE XR Elites
    - System version: 1.0.999.624 or newer
  - 1 pair of VIVE Controllers for XR Series for each VIVE XR Elite
  - An ArUco Marker (dictionary: 4x4, ID: 0 ~ 99) for Trackable Marker
- Software
  - Unity Editor 2022.3.1f1 or newer
  - Unity Packages:
    - [VIVE Wave XR Plugin](https://github.com/ViveSoftware/VIVE-Wave/tree/master) 6.0.0-r.14 or newer
    - [Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.7/manual/index.html): 1.7.0 or newer
    - [Netcode for GameObjects Community Extensions](https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/com.community.netcode.extensions): 1.0.1 or newer
    - Unity [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.6/manual/index.html): 1.6.3 or newer
    - [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html): 3.0.6 or newer
  - Unity Assets:
    - [KF Magic Effets Free](https://assetstore.unity.com/packages/vfx/particles/spells/ky-magic-effects-free-21927): 1.1.1 or newer

## Settings & Build Setup

1. Create a new Unity 3D project and switch build platform to `Android`.

2. Install and setup WaveXR Plugin.

    1. Follow [Wave Unity SDK -- Getting Started](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRGettingStart.html) to setup WaveXR in Unity.
    2. Follow [Wave Unity SDK -- Unity Marker](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRTrackableMarker.html) to setup Trackable Marker.
    3. Follow [Wave Unity SDK -- Unity Scene Perception](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRScenePerception.html) to setup Spatial Anchor.

3. Install following packages and assets:

    - [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/1.7.1/installation/)
    - [Netcode for GameObjects Community Extensions](https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/com.community.netcode.extensions)
    - [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.6/manual/index.html)
    - [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)
    - [KF Magic Effets Free](https://assetstore.unity.com/packages/vfx/particles/spells/ky-magic-effects-free-21927)

4. Import this project and setup build settings.
    - Add the scenes [`Launcher`](Assets/SharedSpaceExperience/Launcher/Scenes/Launcher.unity) and [`ShootingGame`](Assets/SharedSpaceExperience/Apps/ShootingGame/Scenes/ShootingGame.unity) to the `Scenes In Build`, and set `Launcher` to be the entry scene.
    - If the `NetworkPrefabsLists` in `NetworkManager` component is missing, assign it with `Assets/DefaultNetworkPrefabs`.

## How to Play with VIVE XR Elites

1. Environment Setup
    1. Do `Room Setup` and make sure both VIVE XR Elites have the same floor height.
    2. Place the ArUco marker in a fixed place.
2. Install the App
    - Build the project and install the APK on VIVE XR Elites by [Android Debug Bridge](https://developer.android.com/tools/adb).
3. Play the Game
    1. Matching
        - In the launcher, you can choose to be a host or join others as a client. Maker sure all the VIVE XR Elites connect to the same local network.
    2. Aligning Space
        - The host can decide which align method to use.
            - Trackable Marker:
              1. The host have to scan the markers and select one for alignment.
              2. The clients scans the same marker to align the space.
            - Spatial Anchor:
              1. The host first create a spatial anchor with the right controller.
              2. The client then download the spatial anchor. The anchor will relocate and align the space automatically.
        - If the menu is not in a proper place, hold grip button of the left controller to adjust the menu position.
        - Once the alignment is completed, players can see each other's virtual controller models and check whether the space has been properly aligned.
    3. Playing
        - The host can start the game once all the players are aligned and ready.
        - The game is a simple PvP shooting game that the player wins by shooting down all the health bricks of other players.
            - Attack: Press the trigger on the right controller to shoot magic orbs. Press the grip button can change orb type.
            - Defense: Use the shield attached on the left controller to protect the health bricks.

## Developer Guidelines

- Co-location -- Trackable Marker and Spatial Anchor:
  - Scripts under [`Assets/SharedSpaceExperience/Alignment`](Assets/SharedSpaceExperience/Alignment) demonstrate how to align the space by Trackable Marker and Spatial Anchor.
    - [`MarkerManager.cs`](Assets/SharedSpaceExperience/Alignment/Scripts/TrackableMarker/MarkerManager.cs): Demonstrates how to detect markers and retrieve their information.
    - [`SpatialManager.cs`](Assets/SharedSpaceExperience/Alignment/Scripts/SpatialAnchor/SpatialManager.cs): Demonstrates how to create and import/export spatial anchor.
    - [`AlignmentManager.cs`](Assets/SharedSpaceExperience/Alignment/Scripts/AlignManager.cs): Demonstrates how to align the space between the host and other players.
  - See [Wave Unity SDK -- Unity Marker](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRTrackableMarker.html) and [Wave Unity SDK -- Unity Scene Perception](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRScenePerception.html) for more information about Trackable Markers and Spatial Anchor.

- MR Pass through:
  - [`PassThrough.cs`](Assets/SharedSpaceExperience/Launcher/Scripts/PassThrough.cs): Demonstrates how to enable pass through mode.
  - See [Wave Unity SDK -- Passthrough](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRPassthrough.html) for more information.

## Third Party Assets

The third part packages and assets are not included in this repo.

- [Netcode for GameObjects](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@1.7/manual/index.html)
- [Netcode for GameObjects Community Extensions](https://github.com/Unity-Technologies/multiplayer-community-contributions/tree/main/com.community.netcode.extensions)
- [KF Magic Effets Free](https://assetstore.unity.com/packages/vfx/particles/spells/ky-magic-effects-free-21927)

## License

See [LICENSE.pdf](/LICENSE.pdf) for more information.

## Contacts Us

Email: <ViveSoftware@htc.com>
