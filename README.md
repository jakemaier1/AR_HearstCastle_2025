# AR_HearstCastle_2025
Repository for code from Hearst Castle AR Research Project, being featured in future exhibit

## Files
*IMPORTANT DIFFERENCE:* 
SpawnSpatialAnchor.cs is before and MyOVRAnchorManager.cs is after MRUK/OVR Refactor
### <u>MyOVRAnchorManager.cs<u>
#### Features
- C# file containing all logic for spatial anchors
- Ability to spawn anchors using ray interactor
    - Game object previewing orientation and position of anchor placement
    - Behind the scenes management of children components
- Storing and retrieving anchors as JSON object to persist them across sessions
#### Functions
- Start() [C# Default] - Retrieve cross-session directory data, spawn any stored/persisted anchors
- Update() [C# Default] - Check if button was pressed once per frame, spawn anchor if pressed
- OnApplicationQuit() [C# Default] - Flush/save anchor data to persistent directory when application closes
- SpawnAnchor()
    - On button press, instantiate prefab at raycast hit
    - Add OVRAnchor component to object
    - Add to session's array list of anchor objects
- extractAnchorData()
    - Take in spawned spatial anchor, create serializable object with name and uuid
- saveAnchorData()
    - Save each anchor's positional data to headset
    - Serialize unique identifiers into json string
    - Store in persistent directory
- spawnSavedAnchors()
    - Retrieve and convert saved json data into unbound anchors
    - Localize them to the OVRSpace and bind to OVRSpatialAnchor objects
    - Add all anchors to session array list
#### Future Additions
- Add functionality to delete and edit the position of anchors


### <u>SpawnSpatialAnchor.cs<u>
#### Features
- C# file containing all logic for spatial anchors
- Ability to spawn anchors using ray interactor
    - Game object previewing orientation and position of anchor placement
    - Behind the scenes management of children components
- Storing and retrieving anchors as JSON object to persist them across sessions
#### Functions
- Start() [C# Default] - Retrieve cross-session directory data, spawn any stored/persisted anchors
- Update() [C# Default] - Check if button was pressed once per frame, spawn anchor if pressed
- OnApplicationQuit() [C# Default] - Flush/save anchor data to persistent directory when application closes
- SpawnAnchor()
    - On button press, instantiate prefab at raycast hit
    - Add Rigidbody and AR Anchor components to object
    - Find anchor's Scene Anchor parent
    - Convert anchor to serializable object
    - Add to session's array of anchor objects
- extractAnchorData() - Take in spawned spatial anchor and parent Scene Anchor, create json serializable object
- saveAnchorData() - Serialize array list of saved anchors into json string and store in persistent directory
- spawnSavedAnchors() - Retrieve all spatial anchors from stored json, spawn relative to their associated Scene Anchor
#### Future Additions
- When stored anchors are spawned, add them to this session's anchor list
- Add functionality to delete and edit the position of anchors
