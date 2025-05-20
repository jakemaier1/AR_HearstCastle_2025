# AR_HearstCastle_2025
Repository for code from Hearst Castle AR Research Project, being featured in future exhibit

## Files
### SpawnSpatialAnchor.cs
#### Features
- C# file containing all logic for spatial anchors
- Ability to spawn anchors using ray interactor
    - Game object previewing orientation and position of anchor placement
    - Behind the scenes management of children components
- Storing and retrieving anchors as JSON object to persist them across sessions
#### Functions
- SpawnAnchor() - On button press, instantiate prefab at raycast hit. Add Rigidbody and AR Anchor components to object, add anchor to cross-session directory.
- anchorToJson() - Converts anchor into serializable object, then converts to JSON string format to be stored
- Update() [C# Default] - Check if button was pressed once per frame, spawn anchor if pressed
- Start() [C# Default] - Retrieve cross-session directory data, spawn any stored/persisted anchors
- jsonToAnchor() - Extracts data from json string, instantiates saved AR Anchors
#### Future Additions
- Store all spawned anchors in an array, only serialize and flush to persistentDataPath on application close
- Add functionality to delete and edit the position of anchors (much easier with above alteration)
