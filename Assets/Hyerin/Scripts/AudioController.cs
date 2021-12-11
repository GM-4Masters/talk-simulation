using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioClip[] bgmClip = new AudioClip[12];
    [SerializeField] private AudioClip[] effectClip = new AudioClip[5];
    [SerializeField] private AudioSource bgmAudio;
    [SerializeField] private AudioSource effectAudio;
    public enum BGM { MAIN, EP1, GBS1, EP2, GBS2, EP3_1, GBS3, EP3_2, ED0, ED1, ED2, ED3 }
    public enum EFFECT { ALERT, START, TOUCH, TYPING, POP, }

    private BGM ingameBgm = BGM.MAIN;                   // �ΰ��� �����(���̺�)
    private float bgmVolume = 1f;                        // ����� ����(���̺�)
    private float effectVolume = 1f;                        // ����� ����(���̺�)

    private void Awake()
    {
        //bgmAudio = transform.GetComponent<AudioSource>();
        //effectAudio = transform.GetChild(0).GetComponent<AudioSource>();
        bgmAudio.loop = true;
    }

    public void LoadVolume()
    {
        bgmVolume = PlayerPrefs.GetFloat("VolumeBgm");
        effectVolume = PlayerPrefs.GetFloat("VolumeEffect");
        if (!PlayerPrefs.HasKey("VolumeBgm")) bgmVolume = 0.2f;
        if (!PlayerPrefs.HasKey("VolumeEffect")) effectVolume = 0.2f;
        bgmAudio.volume = bgmVolume;
        effectAudio.volume = effectVolume;
    }

    public void Pause()
    {
        bgmAudio.Pause();
        effectAudio.Pause();
    }

    public void Resume()
    {
        bgmAudio.Play();
        effectAudio.Play();
    }


    #region ����� ===============================================================

    // ���� ���� �� ����
    public void SetBgmVolume(float value)
    {
        bgmVolume = value;
        bgmAudio.volume = bgmVolume;
        PlayerPrefs.SetFloat("VolumeBgm", bgmVolume);
    }

    public float GetBgmVolume()
    {
        return bgmVolume;
    }

    // �ΰ��� ����� ���̺�
    public void SaveIngameBGM()
    {
        PlayerPrefs.SetInt("BGM", (int)ingameBgm);
    }

    public void LoadIngameBGM()
    {
        ingameBgm = (BGM)PlayerPrefs.GetInt("BGM");
    }

    public void ResetIngameBGM()
    {
        ingameBgm = BGM.MAIN;
    }

    public void PlayBGM(BGM audio)
    {
        //if (bgmAudio.clip != null && bgmAudio.clip.Equals(bgmClip[(int)audio])) return;
        if (bgmAudio.isPlaying) bgmAudio.Stop();
        bgmAudio.clip = bgmClip[(int)audio];
        bgmAudio.Play();
    }

    public void PlayIngameBGM()
    {
        PlayBGM(ingameBgm);
    }

    public void StopBGM()
    {
        bgmAudio.clip = null;
        bgmAudio.Stop();
    }

    // ���� ���Ǽҵ� ����� ���� �� ���̺�
    public void SetNextIngameBGM(int episodeIndex)
    {
        ingameBgm = (BGM)System.Enum.Parse(typeof(BGM), "EP" + (episodeIndex + 1));
        SaveIngameBGM();
    }

    // ���ڱ� ������ ������
    public void ChangeMood(int episodeIndex)
    {
        ingameBgm = (BGM)(episodeIndex + 1 + (int)BGM.ED0);
        PlayBGM(ingameBgm);
        SaveIngameBGM();
    }
    #endregion ===================================================================










    #region ����Ʈ ===============================================================

    public void SetEffectVolume(float value)
    {
        effectVolume = value;
        effectAudio.volume = effectVolume;
        PlayerPrefs.SetFloat("VolumeEffect", effectVolume);
    }
    public float GetEffectVolume()
    {
        return effectVolume;
    }
    public void PlayEffect(EFFECT audio)
    {
        effectAudio.PlayOneShot(effectClip[(int)audio]);
    }

    public void StopEffect()
    {
        effectAudio.Stop();
    }
    #endregion ===================================================================
}
