using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private Sprite mutedIcon, unmutedIcon;
    [SerializeField] private Image muteButtonIcon;

    private bool isMuted = false;

    public void ToggleMute()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;
        narrationSource.mute = isMuted;
        muteButtonIcon.sprite = isMuted ? mutedIcon : unmutedIcon;
    }

    public void SetMusicVolume(float v) => musicSource.volume = v;
    public void SetNarrationVolume(float v) => narrationSource.volume = v;
}
