using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wikitude;
using UnityEngine.EventSystems;

public class SpeakModeController : MonoBehaviour {
    public InstantTracker tracker;
    public Button stateButton;
    public Text stateText;
    public Text messageBox;

    GridRenderer grid;
    InstantTrackable trackable;

    InstantTrackingState trackerState = InstantTrackingState.Initializing;
    bool isChanging = false;

    bool isTracking = false;

    //AR Object
    public GameObject arObject;

    // Worm Hole
    public GameObject wormHole;

    void Awake()
    {
        grid = this.gameObject.GetComponent<GridRenderer>();
        grid.enabled = true;
        trackable = tracker.GetComponentInChildren<InstantTrackable>();
        wormHole.SetActive(false);
    }
	// Use this for initialization
	void Start () {
        messageBox.text = "Starting the SDK";
	}

    [Header("Wit AI Variables")]
    public Wit3D wit3D;
    public Button recordButton;

	// Update is called once per frame
	void Update () {

		if(trackerState == InstantTrackingState.Initializing)
        {
            if (tracker.CanStartTracking())
            {
                grid.TargetColor = Color.green;
            }
            else
            {
                grid.TargetColor = GridRenderer.DefaultTargetColor;
            }
        }
        else
        {
            grid.TargetColor = GridRenderer.DefaultTargetColor;
        }
	}

    public void StateButtonPressed()
    {
        if (!isChanging)
        {
            if(trackerState == InstantTrackingState.Initializing)
            {
                if (tracker.CanStartTracking())
                {
                    stateText.text = "Switching State...";
                    isChanging = true;
                    tracker.SetState(InstantTrackingState.Tracking);
                }
            }
            else
            {
                stateText.text = "Switching State...";
                isChanging = true;
                tracker.SetState(InstantTrackingState.Initializing);
            }
        }
    }

    public void OnStateChanged(InstantTrackingState newState)
    {
        trackerState = newState;
        if(trackerState == InstantTrackingState.Initializing)
        {
            stateText.text = "Start Tracking";
            messageBox.text = "Not Tracking";

            // Turn off the Wormhole
            wormHole.SetActive(false);
        }
        else
        {
            stateText.text = "Stop Tracking";
            messageBox.text = "Tracking";
            wormHole.SetActive(true);
            wormHole.transform.position = Camera.main.transform.forward * 10;
        }

        isChanging = false;
    }

    public void OnInitializationStarted(InstantTarget target)
    {
        SetSceneEnabled(true);
    }

    public void OnInitializationStopped(InstantTarget target)
    {
        SetSceneEnabled(false);
    }

    public void OnSceneRecognized(InstantTarget target)
    {
        SetSceneEnabled(true);
        isTracking = true;
        messageBox.text = "Scene Found";
    }

    public void OnSceneLost(InstantTarget target)
    {
        SetSceneEnabled(false);
        isTracking = false;
        messageBox.text = "Scene Lost";
    }

    void SetSceneEnabled(bool enable)
    {
        grid.enabled = enable;
        GameObject[] gos = GameObject.FindGameObjectsWithTag("3DModel");
        foreach(GameObject g in gos)
        {
            Renderer[] rends = g.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
                r.enabled = enable;
        }
    }
    public void OnHeightValueChanged(float newHeightValue)
    {
        tracker.DeviceHeightAboveGround = newHeightValue;
    }

    public void RecordButtonClicked()
    {
        recordButton.interactable = false;
        wit3D.commandClip = Microphone.Start(null, false, 3, 16000);
        StartCoroutine(Recording());
    }

    IEnumerator Recording()
    {
        yield return new WaitForSeconds(3);
        wit3D.RecordingForWit();
        recordButton.interactable = true;
    }

    public void SpawnARObject(string arObjectName)
    {
        ModelingController modelCtr = gameObject.GetComponent<ModelingController>();
        Vector3 position = Camera.main.transform.forward * 10;
        arObject = modelCtr.modelingObj[arObjectName];
        GameObject newAR = Instantiate(arObject, position, Quaternion.identity);
        newAR.transform.parent = trackable.transform;
        wormHole.SetActive(false);
    }
}
