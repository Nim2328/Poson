using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARRaycastManager))]
public class ARScenePlacer : MonoBehaviour
{
    [SerializeField] private GameObject mihintaleScenePrefab;
    [SerializeField] private GameObject placementIndicator;

    private ARRaycastManager raycastManager;
    private GameObject spawnedScene;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (spawnedScene != null) return; // already placed, stop scanning

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceScene(hitPose);
            }
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void PlaceScene(Pose pose)
    {
        spawnedScene = Instantiate(mihintaleScenePrefab, pose.position, pose.rotation);
        placementIndicator.SetActive(false);

        var sceneController = spawnedScene.GetComponent<PosonSceneController>();
        sceneController?.BeginExperience();
    }
}
