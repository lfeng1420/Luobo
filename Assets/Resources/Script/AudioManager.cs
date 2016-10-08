using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour 
{
    private static AudioManager _audioManager = null;
    private static GameObject _audioHolder = null;

    private bool effectPlay = true;
    private bool musicPlay = true;

    public bool EffectPlay
    {
        set
        {
            effectPlay = value;
            PlayerPrefs.SetInt("Effect", (effectPlay) ? 1 : 0);//保存设置;
        }
        get
        {
            return effectPlay;
        }
    }

    public bool MusicPlay
    {
        set
        {
            musicPlay = value;
            PlayerPrefs.SetInt("Music", (musicPlay) ? 1 : 0);//保存设置;
        }
        get
        {
            return musicPlay;
        }
    }

    public static AudioManager getInstance()
    {
        if(_audioHolder == null)
        {
            _audioHolder = LuoboTool.getGameObject("AudioManager", true);
        }
        if(_audioManager == null)
        {
            _audioManager = _audioHolder.GetComponent<AudioManager>();
            if(_audioManager == null)
            {
                _audioManager = _audioHolder.AddComponent<AudioManager>();
            }
        }

        return _audioManager;
    }

    /// <summary>
    /// 检查背景音乐是否存在;
    /// 1:正在播放   0:停止    -1:缺少AudioSource组件   -2:不存在该GameObject;
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    int checkAudioState(string name)
    {
        Transform t = _audioHolder.transform.Find("BGMusic");
        if (t == null) return -2;
        AudioSource audioSource = t.GetComponent<AudioSource>();
        if (audioSource == null) return -1;
        return (audioSource.enabled) ? 1 : 0;
    }


    /// <summary>
    /// 播放音效;
    /// </summary>
    public void Play(string fileName, bool isMusic = true, 
                        bool turnOnOrOff = true, bool loop = false)
    {
        //替换文件名中的后缀和/;
        string name = (fileName.Substring(0, fileName.LastIndexOf('.'))).Replace("/", "_");
        AudioSource audioSource = null;
        if(isMusic)
        {
            Transform t = _audioHolder.transform.Find("BGMusic");
            if (t == null)
            {
                GameObject go = new GameObject("BGMusic");
                t = go.transform;
                t.parent = _audioHolder.transform;
            }

            audioSource = t.GetComponent<AudioSource>();
            //如果需要关闭背景音乐;
            if (!turnOnOrOff)
            {
                if (audioSource != null)
                {
                    audioSource.enabled = false;
                }
                return;
            }
            else//否则，如果AudioSource不存在则需要新建;
            {
                if (!musicPlay)
                {//如果不允许播放;
                    return;
                }

                if (audioSource == null)
                {
                    audioSource = t.gameObject.AddComponent<AudioSource>();
                }
            }
        }
        else
        {
            if (!effectPlay)
            {//如果不允许播放;
                return;
            }
            Transform t = _audioHolder.transform.Find("Effect");
            if (t == null)
            {
                GameObject go = new GameObject("Effect");
                t = go.transform;
                t.parent = _audioHolder.transform;
            }

            audioSource = t.gameObject.AddComponent<AudioSource>();
        }
        //加载资源;
        AudioClip clip = Resources.Load("Audio/" + fileName.Split('.')[0],
                                 typeof(AudioClip)) as AudioClip;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.enabled = true;
        audioSource.Play();
        if(!isMusic)
        {
            Destroy(audioSource, clip.length);
        }
    }
}
