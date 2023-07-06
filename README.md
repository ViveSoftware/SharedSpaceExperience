# Shared Space Experience

Copyright 2023, HTC Corporation. All rights reserved.

## About

Shared Space Experience is an MR co-location example for VIVE XR Elite. It shows how to allow users to interact in the same physical and virtual space.

In this project, we demonstrate how to align the space between two VIVE XR Elites by ArUco markers.

## Requirements

-   Hardware
    -   2 VIVE XR Elites
        -   System version: 1.0.999.334 or newer
    -   4 VIVE Controllers for XR Series (2 per VIVE XR Elite)
    -   An ArUco Marker (dictionary: 4x4, ID: 0~99)
-   Software
    -   Unity Editor 2020.3.36f1 or newer
    -   Unity Packages:
        -   VIVE Wave XR Plugin 5.3.1-r.2 or newer
        -   Unity [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html) 1.3.0 or newer
        -   [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html) 3.0.6 or newer
    -   Unity Assets:
        -   [Photon](https://www.photonengine.com/) account and [Photon Pun 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) asset 2.42.0 or newer
        -   [KY Magic Effects Free](https://assetstore.unity.com/packages/vfx/particles/spells/) 1.1.1 or newer

## Settings & Build Setup

1. Create a new Unity 3D project and switch build platform to `Android`.

2. Install and setup WaveXR Plugin.

    1. Follow [Wave XR Plugin -- Getting Started](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRGettingStart.html) to setup WaveXR in Unity.
    2. Follow this [Wave XR Plugin -- Unity Marker](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRTrackableMarker.html) to setup Trackable Marker.

3. Install following packages:

    - [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/manual/index.html)
    - [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)

4. Install following assets:

    - [Photon Pun 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922).  
       Note that you have to use your own Photon `App ID`. See [Photon Pun -- Setup and Connect](https://doc.photonengine.com/pun/current/getting-started/initial-setup) for more information.
    - [KY Magic Effects Free](https://assetstore.unity.com/packages/vfx/particles/spells/)

5. Import this project and setup build settings.
    - Add the scenes in `Assets/SharedSpaceExperience/Scenes` to the `Scenes In Build`, and set `Lobby` scene as the entry scene.

## How to Play with VIVE XR Elites

1. Environment Setup
    1. Do `Room Setup` and make sure both VIVE XR Elites have the same floor height.
    2. Place the ArUco marker in a fixed place.
2. Install the App
    - Build the project and install the APK on VIVE XR Elites by [Android Debug Bridge](https://developer.android.com/tools/adb).
3. Play the Game
    1. Matching
        1. Launch the app. In the lobby, you can select the server with desired region in the top-right dropdown list. Make sure both players are in the same server.
        2. Click `Start` Button to start matching. The player who join the match first will be the host.
    2. Aligning Space
        1. The host have to scan and specify a marker for space alignment.
        2. After the host selects a marker, the other player has to scan and select the same one to align the space. Once the alignment is completed, players can see each other's virtual controller models and check whether the space has been properly aligned.
    3. Playing
        1. The game starts once the alignment process is completed.
            - Attack: Press the trigger on the right controller to shoot magic orbs. Press the grip button to switch orb type.
            - Defense: Use the shield attached on the left controller to prevent the health bricks from damage.
        2. The game ends when the time's up or one of the players loses all the health. All players will return to the lobby.

## Developer Guidelines

-   Co-location and Markers:

    -   In the folder `Assets/SharedSpaceExperience/Scripts/Alignment`
        -   `MarkerManager.cs`: Demonstrates the process of marker detection and how to convert ArUco marker into trackable markers.
        -   `AlignmentManager.cs`: Demonstrates how to align the space between the host and other players.
    -   See [Wave XR Plugin -- Unity Marker](https://hub.vive.com/storage/app/doc/en-us/UnityXR/UnityXRTrackableMarker.html) for more information about trackable markers.

-   MR Pass through:
    -   `Assets/SharedSpaceExperience/Scripts/Utils/PassThrough.cs`: Demonstrates how to enable pass through mode.
    -   See [Wave XR Plugin -- Passthrough](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRPassthrough.html) for more information.
-   Debug:
    -   Press the menu button on the left controller (or `D` key when testing in Unity Editor) to enable debug mode.

## Third Party Assets

-   [Photon Pun 2](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) (not included in the project)
-   [KY Magic Effects Free](https://assetstore.unity.com/packages/vfx/particles/spells/ky-magic-effects-free-21927) (not included in the project)
-   [Unity Built-in Shaders](https://unity.com/releases/editor/archive) 2020.3.36f1

## License

See [LICENSE.pdf](/LICENSE.pdf) for more information.

## Contacts Us

Email: ViveSoftware@htc.com
