using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using System.IO;
using Pose = UnityEngine.Pose;

public class MyOVRAnchorManager : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public GameObject prefab;
    public InputActionProperty input;
    private int numAnchors = 0;
    private List<OVRSpatialAnchor> savedAnchors = new();
    


    // Serializable class to track name and persistent id of OVR anchor
    [System.Serializable]
    public class AnchorData
    {
        public string id;
        public string Uuid;
        public AnchorData(string name, string uuid)
        {
            id = name;
            Uuid = uuid;
        }
    }

    // Serializable class to store multiple anchor objects
    [System.Serializable]
    public class AnchorList
    {
        public List<AnchorData> anchorList;
        public AnchorList (List<AnchorData> list)
        {
            anchorList = list;
        }
        // This part was added automatically, don't know what it is
        public static explicit operator List<object>(AnchorList v)
        {
            throw new NotImplementedException();
        }
    }

    private void Start()
    {
        // Check if there are any saved anchors
        string anchorPath = Path.Combine(Application.persistentDataPath, "anchors.json");
        if (File.Exists(anchorPath))
        {
            // Load and spawn all stored OVR Anchors
            spawnSavedAnchors(File.ReadAllText(anchorPath));
        }   
    }

    private void Update()
    {
        if (input.action.WasPressedThisFrame())
        {
            SpawnAnchor();
        }
    }

    private void OnApplicationQuit()
    {
        saveAnchorData();
    }

    public void SpawnAnchor()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Store hitpoint of raycast as a Pose
            Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            Pose hitPose = new(hit.point, rotation);

            // Instantiate the prefab at the hit pose position and rotation
            GameObject spawned = Instantiate(prefab, hitPose.position, hitPose.rotation);
            spawned.transform.Rotate(90, 0, 0);

            // Add OVR Anchor component
            OVRSpatialAnchor anchor = spawned.AddComponent<OVRSpatialAnchor>();

            // Add anchor to session's arrayList
            savedAnchors.Add(anchor);
        }
    }
    

    // Save UUIDs of anchors and serialize into json string to store in persistent directory
    public void saveAnchorData()
    {
        // Save/Persist all UUIDs in session array list
        OVRSpatialAnchor.SaveAnchorsAsync(savedAnchors); // ADD AWAIT() AND ERROR HANDLING HERE

        // Serialize each anchor in session array list
        List<AnchorData> serialAnchorList = new();
        foreach (OVRSpatialAnchor anchor in savedAnchors) {
            serialAnchorList.Add(extractAnchorData(anchor));
        }

        // Serialize array object to json string
        string jsonString = JsonUtility.ToJson(serialAnchorList);

        // Get file path for persistent anchor directory
        string path = Path.Combine(Application.persistentDataPath, "anchors.json");

        // Store all Anchor Data objects in persistent directory
        File.WriteAllText(path, jsonString);
    }

    // Take in spawned spatial anchor, return json serializable object
    public AnchorData extractAnchorData(OVRSpatialAnchor anchor)
    {
        // Name anchor with unique number
        string id = $"Anchor{++numAnchors}";
        string uuid = anchor.Uuid.ToString() ;
        // Return serializable object for storing anchor position and orientation
        return new(id, uuid);
    }

    // Retrieve all spatial anchors from stored json, spawn according to loaded/associated UUID
    public async void spawnSavedAnchors(string json)
    {
        // Convert json to list of AnchorData objects
        List<AnchorData> anchorList = JsonUtility.FromJson<AnchorList>(json).anchorList;

        // Extract UUIDS into list and UUIDs+Names into dictionary
        List<Guid> uuids = new();
        Dictionary<Guid, string> anchorIdPairs = new();
        foreach (AnchorData ad in anchorList)
        {
            Guid uuid = Guid.Parse(ad.Uuid);
            uuids.Add(uuid);
            anchorIdPairs[uuid] = ad.id;
        }

        // Load all persistent OVR Anchor info
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids,_unboundAnchors);

        // If loaded successfully, localize all unbound anchors, bind to newly instantiated OVRAnchor
        if (result.Success) {
            foreach (var unbound in result.Value)
            {
                var localized = await unbound.LocalizeAsync();
                if (localized)
                {
                    OVRSpatialAnchor anchor = new GameObject(anchorIdPairs[unbound.Uuid]).AddComponent<OVRSpatialAnchor>();
                    unbound.BindTo(anchor);
                }
                else { Debug.Log($"Localization Failed for: {unbound.Uuid}");}
            }
        }
        else { Debug.Log("Load Failed");}
    }
}