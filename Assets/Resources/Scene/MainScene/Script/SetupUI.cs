using UnityEngine;
using System.Collections;

public class SetupUI : MonoBehaviour {

    int showIndex = 0;
    string[] panelName = { "Panel_Options", "Panel_StatisticalData", "Panel_Credits" };
    string[] btnName = { "Options", "StatisticalData", "Credits", "Home"};
    string[] opBtnName = { "Reset", "Music", "Effect" };

	// Use this for initialization
	void Start () 
    {
        Transform find = transform;
        UIEventListener eventListener = null;

        for (int i = 0; i < opBtnName.Length; i++)
        {
            find = transform.Find("Panel_Options/Buttons/" + opBtnName[i]);
            if (find == null) continue;
            eventListener = find.GetComponent<UIEventListener>();
            if (eventListener == null)
            {
                eventListener = find.gameObject.AddComponent<UIEventListener>();
            }

            eventListener.onClick = onButtonClick;
        }
        for (int i = 0; i < btnName.Length; i++)
        {
            find = transform.Find("Buttons/" + btnName[i]);
            if (find == null) continue;
            eventListener = find.GetComponent<UIEventListener>();
            if (eventListener == null)
            {
                eventListener = find.gameObject.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onButtonClick;
        }

        checkSetupState();

        setButtonVisible(0);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    /// <summary>
    /// 设置Panel显示状态;
    /// </summary>
    void changePanelVisible()
    {
        for (int i = 0; i < btnName.Length - 1; i++)
        {
            LuoboTool.changeState(transform, panelName[i], (i == showIndex) ? true : false);
        }
    }


    /// <summary>
    /// 按钮响应函数;
    /// </summary>
    /// <param name="go"></param>
    void onButtonClick(GameObject go)
    {
        if (go.name.Equals("Music") || go.name.Equals("Effect"))
        {
            bool isMusic = go.name.Equals("Music");
            bool effectPlay = AudioManager.getInstance().EffectPlay;
            bool musicPlay = AudioManager.getInstance().MusicPlay;

            effectPlay = (!isMusic) ? !effectPlay : effectPlay;
            musicPlay = (isMusic) ? !musicPlay : musicPlay;

            AudioManager.getInstance().EffectPlay = effectPlay;
            AudioManager.getInstance().MusicPlay = musicPlay;
            checkSetupState();

            if(isMusic)
            {
                //如果是背景音乐;
                AudioManager.getInstance().Play("Main/BGMusic.mp3", true, musicPlay);
            }
        }
        else if(go.name.Equals("Home"))
        {
            //设置主菜单界面显示;
            LuoboTool.changeState(transform.parent, "MainScene", true);
            //获取TweenPosition组件;
            Transform ui = transform.parent;
            TweenPosition tp = ui.GetComponent<TweenPosition>();
            if (tp == null)
            {
                tp = ui.gameObject.AddComponent<TweenPosition>();
            }

            tp.to = Vector3.zero;
            tp.from = new Vector3(0, -640, 0);
            tp.ignoreTimeScale = true;
            tp.duration = 0.3f;
            EventDelegate.Add(tp.onFinished, tweenFinished);
            tp.enabled = true;
        }
        else if (go.name.Equals("Reset"))
        {

        }
        else
        {
            for (int i = 0; i < btnName.Length - 1; i++)
            {
                if (go.name.Equals(btnName[i]))
                {
                    if (showIndex != i)
                    {
                        setButtonVisible(i);
                    }
                    break;
                }
            }
        }

        //播放音效;
        AudioManager.getInstance().Play("Main/Select.mp3", false);
    }


    /// <summary>
    /// Tween结束处理;
    /// </summary>
    void tweenFinished()
    {
        //隐藏其他界面;
        LuoboTool.changeState(transform.parent, "Setup", false);
        LuoboTool.changeState(transform.parent, "Help", false);

        //移除Tween组件结束响应;
        Transform ui = transform.parent;
        TweenPosition tp = ui.GetComponent<TweenPosition>();
        if(tp != null)
        {
            EventDelegate.Remove(tp.onFinished, tweenFinished);
        }
        DestroyImmediate(tp);
        tp = null;
    }


    void checkSetupState()
    {
        bool effectPlay = AudioManager.getInstance().EffectPlay;
        bool musicPlay = AudioManager.getInstance().MusicPlay;

        Transform find = transform.Find("Panel_Options/Buttons/Music");
        LuoboTool.changeState(find, "Background", musicPlay);
        LuoboTool.changeState(find, "CheckMark", !musicPlay);
        find = transform.Find("Panel_Options/Buttons/Effect");
        LuoboTool.changeState(find, "Background", effectPlay);
        LuoboTool.changeState(find, "CheckMark", !effectPlay);
    }

    void setButtonVisible(int index)
    {
        Transform t = null;
        UIButton button = null;
        if (showIndex != index)
        {
            t = LuoboTool.getTransform(transform, "Buttons/" + btnName[showIndex]);
            if (t == null) return;
            button = t.GetComponent<UIButton>();
            switchState(button);
        }

        showIndex = index;

        t = LuoboTool.getTransform(transform, "Buttons/" + btnName[showIndex]);
        if (t == null) return;
        button = t.GetComponent<UIButton>();
        switchState(button);

        changePanelVisible();
    }

    void switchState(UIButton button)
    {
        if (button == null) return;
        string normal = button.normalSprite;
        string press = button.pressedSprite;
        button.normalSprite = press;
        button.pressedSprite = normal;
    }
}
