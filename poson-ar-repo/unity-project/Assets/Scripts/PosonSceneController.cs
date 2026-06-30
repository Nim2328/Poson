using UnityEngine;
using UnityEngine.Playables;

// Drives the historical Timeline (deer -> hunt -> meeting -> blessing) plus narration audio.
public class PosonSceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioClip narrationClip;

    public void BeginExperience()
    {
        timelineDirector.time = 0;
        timelineDirector.Play();

        if (narrationClip != null)
        {
            narrationSource.clip = narrationClip;
            narrationSource.Play();
        }
    }

    public void PauseExperience()
    {
        timelineDirector.Pause();
        narrationSource.Pause();
    }

    public void ResumeExperience()
    {
        timelineDirector.Resume();
        narrationSource.UnPause();
    }

    public void RestartExperience()
    {
        timelineDirector.time = 0;
        timelineDirector.Play();
        narrationSource.Stop();
        narrationSource.Play();
    }
}
