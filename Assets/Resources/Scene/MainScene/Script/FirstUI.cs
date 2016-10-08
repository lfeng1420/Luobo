using UnityEngine;
using System.Collections;

public class FirstUI : MonoBehaviour 
{
    void Start()
    {
        //检查音乐和音效是否允许播放;
        int check = PlayerPrefs.GetInt("Music", 1);
        AudioManager.getInstance().MusicPlay = (check == 1) ? true : false;
        check = PlayerPrefs.GetInt("Effect", 1);
        AudioManager.getInstance().EffectPlay = (check == 1) ? true : false;

        SceneManager.getInstance().openWindow("Scene/MainScene/UI", 3, true, false);
        //StartCoroutine(SceneManager.getInstance().openWindow("Scene/MainScene/ui", 3, false, true));

        DataManager.getInstance().loadData("XML/Data.xml");
    }
    
}
