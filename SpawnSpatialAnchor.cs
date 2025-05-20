using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem;
using System.IO;
using UnityEngine.XR.OpenXR.Input;
using Pose = UnityEngine.Pose;

public class SpawnAnchorFromRayInteract : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public ARAnchorManager anchorManager;
    public GameObject prefab;
    public InputActionProperty input;
    private int numAnchors = 0;


    // Serializable class to track name, position and rotation of spawned anchor, will be stored in persistentDataPath
    [System.Serializable]
    public class AnchorData
    {
        public string id;
        public float posX, posY, posZ, rotX, rotY, rotZ,rotW;
        public AnchorData(string name, float pX, float pY, float pZ, float rX, float rY, float rZ, float rW)
        {
            id = name; posX = pX; posY = pY; posZ = pZ; rotX = rX; rotY = rY; rotZ = rZ; rotW = rW;
        }
    }

    private void Start()
    {
        // Convert 
        File.ReadAllText(Path.Combine(Application.persistentDataPath, "anchors.json"));
        if () //check if any anchors exist in persistentDataPath
        {
             // Use this somehow?
        }
    }

    private void Update()
    {
        if (input.action.WasPressedThisFrame())
        {
            SpawnAnchor();
        }
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

            // Put position and rotation of anchor into json format
            string jsonString = anchorToJson("ReferencePoint",spawned.GetComponent<ARAnchor>().transform);

            // Get file path for session and reboot transcendent file directory
            string path = Path.Combine(Application.persistentDataPath,"anchors.json");

            // Store anchor position and rotation in directory
            File.WriteAllText(path,jsonString); 
        }
    }

    public string anchorToJson(string name, Transform transform)
    {
        // Var tracking num of anchors created, helps with naming
        name += ++numAnchors;

        // Create serializable object for storing anchor position and orientation
        AnchorData anchor = new(name+numAnchors, transform.position.x, transform.position.y, transform.position.z, transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);

        // Serialize to JSON
        return JsonUtility.ToJson(anchor,true);
    }
}
