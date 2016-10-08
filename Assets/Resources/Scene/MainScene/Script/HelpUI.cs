using UnityEngine;
using System.Collections;

public class HelpUI : MonoBehaviour {

    int showIndex = 0;
    string[] panelName = { "Panel_Tips", "Panel_Monster", "Panel_Tower" };
    string[] btnName = { "Tips", "Monster", "Tower", "Home" };

	// Use this for initialization
    void Start()
    {
        UIEventListener eventListener = null;

        //顶部三个按钮切换事件绑定;
        for (int i = 0; i < btnName.Length; i++)
        {
            Transform t = transform.Find("Buttons/" + btnName[i]);
            if (t == null) continue;
            eventListener = t.GetComponent<UIEventListener>();
            if (eventListener == null)
            {
                eventListener = t.gameObject.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onButtonClick;
        }

        for (int i = 0; i < panelName.Length; i++)
        {
            //滑动结束事件绑定;
            Transform childTrans = transform.Find(panelName[i] + "/Panel");
            if (childTrans == null) continue;
            UIScrollView scrollView = childTrans.GetComponent<UIScrollView>();
            scrollView.onStoppedMoving = onMoveFinished;
            scrollView.onDragStarted = playScrollEffect;
        }

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
        for (int i = 0; i < 3; i++)
        {
            LuoboTool.changeState(transform, panelName[i], (i == showIndex) ? true : false);
        }
    }


    /// <summary>
    /// 按钮点击事件响应;
    /// </summary>
    /// <param name="go"></param>
    void onButtonClick(GameObject go)
    {
        //播放音效;
        AudioManager.getInstance().Play("Main/Select.mp3", false);

        if (go.name.Equals("Home"))
        {
            LuoboTool.changeState(transform.parent, "MainScene", true);

            Transform ui = transform.parent;
            TweenPosition tp = ui.GetComponent<TweenPosition>();
            if (tp == null)
            {
                tp = ui.gameObject.AddComponent<TweenPosition>();
            }

            tp.to = Vector3.zero;
            tp.from = new Vector3(0, 640, 0);
            tp.ignoreTimeScale = true;
            tp.duration = 0.3f;
            EventDelegate.Add(tp.onFinished, tweenFinished);
            tp.enabled = true;

            return;
        }


        for (int i = 0; i < btnName.Length - 1; i++)
        {
            if (go.name.Equals(btnName[i]))
            {
                setButtonVisible(i);
                break;
            }
        }
    }


    void onMoveFinished()
    {
        Transform panelTrans = transform.Find(panelName[showIndex] + "/Panel");
        if (panelTrans == null) return;
        float x = panelTrans.localPosition.x / (float)Screen.width;

        Transform labelTrans = transform.Find(panelName[showIndex] + "/Index/Label");
        if (labelTrans == null) return;
        UILabel label = labelTrans.GetComponent<UILabel>();
        if (label == null) return;

        int curPage = (int)(Mathf.Abs(x) + 0.5f) + 1;
        int pageNum = 0;
        if (showIndex == 0) pageNum = 4;
        else if (showIndex == 2) pageNum = 13;

        label.text = curPage.ToString() + "/" + pageNum.ToString();
    }


    void playScrollEffect()
    {
        AudioManager.getInstance().Play("Items/MenuSelect.mp3", false);
    }

    void tweenFinished()
    {
        //隐藏其他界面;
        LuoboTool.changeState(transform.parent, "Setup", false);
        LuoboTool.changeState(transform.parent, "Help", false);

        //移除Tween组件结束响应;
        Transform ui = transform.parent;
        TweenPosition tp = ui.GetComponent<TweenPosition>();
        if (tp != null)
        {
            EventDelegate.Remove(tp.onFinished, tweenFinished);
        }
        DestroyImmediate(tp);
        tp = null;
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
