using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using System.IO;
using Pose = UnityEngine.Pose;
using System;
using Meta.XR.MRUtilityKit;
using OVRSimpleJSON;

public class SpawnAnchorFromRayInteract : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public ARAnchorManager anchorManager;
    public GameObject prefab;
    public InputActionProperty input;
    private int numAnchors = 0;
    private List<AnchorData> savedAnchors = new();
    


    // Serializable class to track name, position and rotation of spawned anchor, will be stored in persistentDataPath
    [System.Serializable]
    public class AnchorData
    {
        public string id;
        public string parent;
        public float posX, posY, posZ, rotX, rotY, rotZ,rotW;
        public AnchorData(string name, string anchor, float pX, float pY, float pZ, float rX, float rY, float rZ, float rW)
        {
            id = name; parent = anchor;
            posX = pX; posY = pY; posZ = pZ;
            rotX = rX; rotY = rY; rotZ = rZ; rotW = rW;
        }
        public AnchorData(string name, string anchor, Vector3 localPos, Quaternion localRot)
        {
            id = name; parent = anchor;
            posX = localPos.x; posY = localPos.y; posZ = localPos.z;
            rotX = localRot.x; rotY = localRot.y; rotZ = localRot.z; rotW = localRot.w;
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
            // Spawn all saved anchors relative to Scene Anchors
            spawnSavedAnchors(GameObject.FindObjectsOfType<MRUKAnchor>(), File.ReadAllText(anchorPath));
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
        saveAnchorData(savedAnchors);
    }

    public void SpawnAnchor()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Adjust the rotation to align with the hit surface normal
            Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);

            // Create a new pose for the hit point with the adjusted rotation
            Pose hitPose = new(hit.point, rotation);

            // Instantiate the prefab at the hit pose position and rotation
            GameObject spawned = Instantiate(prefab, hitPose.position, hitPose.rotation);

            spawned.transform.Rotate(90, 0, 0);

            // Ensure the rigidbody is properly attached if needed
            Rigidbody cubeRigidbody = spawned.GetComponent<Rigidbody>();

            // Add ARAnchor component if necessary
            spawned.AddComponent<ARAnchor>();

            // Find the Meta Room Scan Anchor that the spatial anchor is attached to
            MRUKAnchor parentAnchor = hit.collider.GetComponentInParent<MRUKAnchor>();

            // Convert ARAnchor to serializable object
            AnchorData anchorData = extractAnchorData(spawned, parentAnchor);

            // Add ARAnchor to array list
            savedAnchors.Add(anchorData);
        }
    }

    // Compile all saved anchors into array list and store in persistent directory
    public void saveAnchorData(List<AnchorData> savedAnchors)
    {
        foreach (AnchorData anchor in savedAnchors)
        {
            // Convert list of AnchorData objects to json string
            string jsonString = JsonUtility.ToJson(savedAnchors);

            // Get file path for persistent anchor directory
            string path = Path.Combine(Application.persistentDataPath, "anchors.json");

            // Store all Anchor Data objects in persistent directory
            File.WriteAllText(path, jsonString);
        }
    }

    // Take in spawned spatial anchor and parent Scene Anchor, create json serializable object
    public AnchorData extractAnchorData(GameObject anchor, MRUKAnchor parent)
    {
        // Name anchor with unique number
        string id = $"Anchor{++numAnchors}";

        // Get local transform of spatial anchor in reference to Scene Anchor
        Vector3 localPos = parent.transform.InverseTransformPoint(anchor.transform.position);
        Quaternion localRot = Quaternion.Inverse(parent.transform.rotation) * anchor.transform.rotation;

        // Return serializable object for storing anchor position and orientation
        return new(id, parent.name, localPos, localRot);
    }

    // Spawn all spatial anchors from stored json relative to their associated Scene Anchor
    public void spawnSavedAnchors(MRUKAnchor[] sceneAnchors, string json)
    {
        // Convert json to list of AnchorData objects
        List<AnchorData> anchorList = JsonUtility.FromJson<AnchorList>(json).anchorList;

        // Iterate through AnchorData objects
        foreach (AnchorData anchor in anchorList)
        {
            // Retrieve position and orientation data from stored object
            Vector3 savedPos = new(anchor.posX, anchor.posY, anchor.posZ);
            Quaternion savedRot = new(anchor.rotW, anchor.rotX, anchor.rotY, anchor.rotZ);
            foreach (MRUKAnchor sceneAnchor in sceneAnchors)
            {
                if (sceneAnchor.name == anchor.id)
                {
                    // Instatiate each anchor in list
                    GameObject anchorSaved = Instantiate(prefab, sceneAnchor.transform.TransformPoint(savedPos), sceneAnchor.transform.rotation * savedRot);
                    anchorSaved.name = anchor.id;
                    anchorSaved.GetComponent<ARAnchor>();
                }
            }
        }
    }
}