using UnityEngine;

// Attach TapInfoTarget to any object you want users to be able to tap for info
// (e.g. the King, Mahinda Thero, a stone pillar, a thorana decoration).
public class TapInfoTarget : MonoBehaviour
{
    [TextArea] public string infoTextEnglish;
    [TextArea] public string infoTextSinhala;
    public AudioClip infoAudioClip;
}

// Attach TouchInteractionManager once to a manager object in the AR scene.
public class TouchInteractionManager : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private InfoPanelUI infoPanel;
    [SerializeField] private AudioSource sfxSource;

    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        Ray ray = arCamera.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            var target = hit.collider.GetComponent<TapInfoTarget>();
            if (target != null)
            {
                infoPanel.ShowInfo(target.infoTextEnglish, target.infoTextSinhala);
                if (target.infoAudioClip != null)
                {
                    sfxSource.clip = target.infoAudioClip;
                    sfxSource.Play();
                }
            }
        }
    }
}
