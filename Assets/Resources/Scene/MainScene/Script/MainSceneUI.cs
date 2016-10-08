using UnityEngine;
using System.Collections;

public class MainSceneUI : MonoBehaviour {

    int showIndex = 1;
    string[] buttonUnion = {"BottomButton", "CenterButton", "TopButton" };
	// Use this for initialization
	void Start () 
    {
        Transform find = transform;

        //绑定所有按钮响应事件处理函数;
        for (int i = 0; i < buttonUnion.Length; i++ )
        {
            find = transform.Find("Buttons/" + buttonUnion[i]);
            if (find == null) continue;
            for(int j = 0; j < find.childCount; j++)
            {
                Transform child = find.GetChild(j);
                UIEventListener eventListener = child.GetComponent<UIEventListener>();
                if(eventListener == null)
                {
                    eventListener = child.gameObject.AddComponent<UIEventListener>();
                }

                eventListener.onClick = onButtonClick;
            }
        }

        find = transform.Find("Luobo");
        if(find != null)
        {
            find.localScale = Vector3.one * 0.01f;
            TweenScale ts = find.GetComponent<TweenScale>();
            if(ts != null)
            {
                ts.enabled = true;
            }
        }

        AudioManager.getInstance().Play("Main/BGMusic.mp3");
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LuoboTool.Log("Application Quit!");
            Application.Quit();
        }
	}


    void onButtonClick(GameObject go)
    {
        //播放音效;
        AudioManager.getInstance().Play("Main/Select.mp3", false);

        if(go.name.Equals("Advanture"))
        {//点击冒险模式按钮;
            
            SceneManager.getInstance().openWindow("Scene/SelectStage/UI", 0);
            Resources.UnloadUnusedAssets();
            return;
        }
        if (go.name.Equals("Setup") || go.name.Equals("Help"))
        {//设置、帮助按钮响应;
            LuoboTool.changeState(transform.parent, go.name, true);//显示界面;

            Transform ui = transform.parent;
            TweenPosition tp = ui.GetComponent<TweenPosition>();
            if (tp == null)
            {
                tp = ui.gameObject.AddComponent<TweenPosition>();
            }

            showIndex = (go.name.Equals("Setup")) ? 0 : 2;//设置索引;

            tp.from = Vector3.zero;
            tp.to = new Vector3(0, (go.name.Equals("Setup")) ? -640 : 640, 0);
            tp.ignoreTimeScale = true;
            tp.duration = 0.3f;
            EventDelegate.Add(tp.onFinished, tweenFinished);
            tp.enabled = true;
            return;
        }
    }

    void tweenFinished()
    {
        LuoboTool.changeState(transform.parent, "MainScene", false);//隐藏其他界面;
        LuoboTool.changeState(transform.parent, (showIndex == 0) ? "Help" : "Setup", false);
        //LuoboTool.changeState(transform.parent, (showIndex == 0) ? "Setup" : "Help", true);
        Transform ui = transform.parent;
        TweenPosition tp = ui.GetComponent<TweenPosition>();
        if (tp != null)
        {
            EventDelegate.Remove(tp.onFinished, tweenFinished);
        }
        DestroyImmediate(tp);
        tp = null;
    }
}
