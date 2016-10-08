using UnityEngine;
using System.Collections;

public class SelectStage : MonoBehaviour {

    bool stageState = true;//是否选择主题状态;

    /// <summary>
    /// 主题数量
    /// </summary>
    int stageNum = 0;

    /// <summary>
    /// 当前闯过的关卡数
    /// </summary>
    int[] curPassLevelNum;

    /// <summary>
    /// 各主题对应关卡数量
    /// </summary>
    int[] levelNum;

    /// <summary>
    /// 保存当前主题cellWidth
    /// </summary>
    float stageCellWidth = 960;

    /// <summary>
    /// 保存当前关卡cellWidth
    /// </summary>
    float levelCellWidth = 520;

    /// <summary>
    /// 当前主题选择;
    /// </summary>
    int curStageChoice = -1;

    /// <summary>
    /// 当前关卡选择;
    /// </summary>
    int curLevelChoice = -1;

    /// <summary>
    /// 主题Panel
    /// </summary>
    Transform stagePanel;
    
    /// <summary>
    /// 关卡Panel
    /// </summary>
    Transform levelPanel;

    /// <summary>
    /// Panel滑动标记
    /// </summary>
    int moveFlag = 0;


	// Use this for initialization
	void Start () 
    {
        //主题数量;
        stageNum = DataManager.getInstance().StageData.Count;
        levelNum = new int[stageNum];
        curPassLevelNum = new int[stageNum];

        loadData();//读取数据;

        stagePanel = LuoboTool.getTransform(transform, "Panel/StagePanel/Panel");
        levelPanel = LuoboTool.getTransform(transform, "Panel/LevelPanel/Panel");

        //绑定;
        Transform find = transform.Find("Panel/StagePanel/Panel/Grid");
        if (find != null)
        {
            for (int i = 1; i <= stageNum; i++)
            {
                GameObject go = find.Find("Stage" + i).gameObject;
                UIEventListener eventListener = go.GetComponent<UIEventListener>();
                if (eventListener == null)
                {
                    eventListener = go.AddComponent<UIEventListener>();
                }
                eventListener.onClick = onButtonClick;
            }
        }

        string[] btnName = {"Home", "Help", "Left", "Right", "Back", "Start", "Lock"};
        for( int i = 0; i < btnName.Length; i++ )
        {
            GameObject go = transform.Find("Buttons/" + btnName[i]).gameObject;
            UIEventListener listener = go.GetComponent<UIEventListener>();
            if (listener == null)
            {
                listener = go.AddComponent<UIEventListener>();
            }
            listener.onClick = onButtonClick;
        }

        //Panel滑动;
        UIScrollView uiscrollView = stagePanel.GetComponent<UIScrollView>();
        if(uiscrollView !=  null)
        {
            uiscrollView.onStoppedMoving = onStoppedMoving;
            uiscrollView.onDragStarted = onDragStarted;
        }
        uiscrollView = levelPanel.GetComponent<UIScrollView>();
        if(uiscrollView !=  null)
        {
            uiscrollView.onDragStarted = onDragStarted;
            uiscrollView.onStoppedMoving = onStoppedMoving;
        }

        //设置当前界面;
        setUIState();
        //设置左右切换按钮;
        setLeftOrRightButtonVisible();
        //设置页码;
        setCurStageTag();
	}

    /// <summary>
    /// Panel停止滑动;
    /// </summary>
    void onStoppedMoving()
    {
        moveFlag = 0;
    }

    /// <summary>
    /// Panel滑动开始;
    /// </summary>
    void onDragStarted()
    {
        moveFlag = 1;
        AudioManager.getInstance().Play("Items/MenuSelect.mp3", false);
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (moveFlag == 1)
        {
            if(stageState)
            {
                int oldStageChoice = curStageChoice;
                curStageChoice = (int)(-stagePanel.localPosition.x / stageCellWidth + 0.5f);

                //curStageChoice范围控制;
                if(curStageChoice < 0)curStageChoice = 0;
                else if (curStageChoice >= stageNum) curStageChoice = stageNum - 1;

                //仅当选择发生改变时更新左右按钮状态;
                if (oldStageChoice != curStageChoice)
                {
                    setLeftOrRightButtonVisible();
                    setCurStageTag();
                }
            }
            else
            {
                int oldLevelChoice = curLevelChoice;
                curLevelChoice = (int)(-levelPanel.localPosition.x / levelCellWidth + 0.5f);

                //curLevelChoice范围控制;
                if (curLevelChoice < 0) curLevelChoice = 0;
                else if (curLevelChoice >= levelNum[curStageChoice]) curLevelChoice = levelNum[curStageChoice] - 1;

                //仅当关卡选择发生改变时更新关卡信息;
                if(oldLevelChoice != curLevelChoice)
                {
                    setCurLevelInfo();
                    changeLevelColor(oldLevelChoice);
                }
            }
        }
	}

    /// <summary>
    /// 根据当前界面设置各个组件显示状态;
    /// </summary>
    void setUIState()
    {
        //Home和Back按钮;
        LuoboTool.changeState(transform, "Buttons/Home", stageState);
        LuoboTool.changeState(transform, "Buttons/Back", !stageState);

        //开始和已锁定按钮;
        if(stageState)
        {
            LuoboTool.changeState(transform, "Buttons/Start", false);
            LuoboTool.changeState(transform, "Buttons/Lock", false);
        }

        //左右切换按钮;
        LuoboTool.changeState(transform, "Buttons/Left", stageState);
        LuoboTool.changeState(transform, "Buttons/Right", stageState);

        //页码;
        LuoboTool.changeState(transform, "Pagetag", stageState);

        //主题Panel和关卡Panel
        LuoboTool.changeState(transform, "Panel/StagePanel", stageState);
        LuoboTool.changeState(transform, "Panel/LevelPanel", !stageState);

        //底部背景，仅在选择关卡时显示;
        setBottomImg();

        //重置位置;
        setPanelPosition();
    }

    /// <summary>
    /// 设置左右切换按钮状态;
    /// </summary>
    void setLeftOrRightButtonVisible()
    {
        if (!stageState) return;
        LuoboTool.changeState(transform, "Buttons/Left", stageState && curStageChoice > 0);
        LuoboTool.changeState(transform, "Buttons/Right", stageState && curStageChoice < stageNum - 1);
    }


    void setBottomImg()
    {
        LuoboTool.changeState(transform, "Background/BottomImg", !stageState);
        if (stageState)return;

        //加载Atlas;
        UIAtlas atlas = Resources.Load("Scene/SelectStage/Atlas/Stage" + (curStageChoice + 1).ToString()
            + " Atlas", typeof(UIAtlas)) as UIAtlas;
        UISprite sprite = LuoboTool.getSprite(transform, "Background/BottomImg");
        if(sprite == null)
        {
            LuoboTool.LogError("[SelectStage]:Get Background/BottomImg Sprite Error!");
            return;
        }
        sprite.atlas = atlas;
    }

    void setCurStageTag()
    {
        if (!stageState) return;
        for (int i = 0; i < stageNum; i++ )
        {
            LuoboTool.setSprite(transform, "Pagetag/" + i.ToString(),
                (i == curStageChoice) ? "theme_pos_active" : "theme_pos_normal");
        }
    }

    /// <summary>
    /// 设置当前关卡信息，例如：怪物波数，可用炮塔等;
    /// </summary>
    void setCurLevelInfo()
    {
        if (stageState) return;
        int waveNum = DataManager.getInstance().StageData[curStageChoice].m_levelInfo[curLevelChoice].m_waveNum;
        Transform t = LuoboTool.getTransform(transform, "Panel/LevelPanel/levelNum");
        //修改坐标;
        t.localPosition = new Vector3((20 == waveNum) ? 0 : 129, (20 == waveNum) ? 220 : 222,
                t.localPosition.z);
        LuoboTool.setSprite(t, "ss_waves_" + waveNum.ToString("D2"));

        LuoboTool.setSprite(transform, "Panel/LevelPanel/useTower",
            "ss_towers_" + (curLevelChoice + 1).ToString("D2"));

        //设置按钮状态;
        bool lockState = DataManager.getInstance().StageData[curStageChoice].m_levelInfo[curLevelChoice].m_lockState;
        LuoboTool.changeState(transform, "Buttons/Start", !lockState);
        LuoboTool.changeState(transform, "Buttons/Lock", lockState);
    }

    void onButtonClick(GameObject go)
    {
        //播放音效;
        AudioManager.getInstance().Play("Main/Select.mp3", false);

        string name = go.name;
        if(name.Equals("Home"))
        {
            //StartCoroutine(SceneManager.getInstance().openWindow("Scene/MainScene/ui", 1, true, true));
            DataManager.getInstance().StageChoice = -1;
            DataManager.getInstance().LevelChoice = -1;
            SceneManager.getInstance().openWindow("Scene/MainScene/UI", 0.1f);
            Resources.UnloadUnusedAssets();
            return;
        }

        if(name.Contains("Stage")) //Stage Click;
        {
            name = name.Replace("Stage", "");
            int index = int.Parse(name);
            if (curStageChoice != (index - 1))
            {
                LuoboTool.LogError("curStageChoice:" + curStageChoice + ", index - 1:" 
                    + (index - 1) + ", NOT EQUAL!");
                return;
            }

            bool isLock = DataManager.getInstance().StageData[curStageChoice].m_lockState;
            if (isLock)
            {
                LuoboTool.Log("[SelectStage]:Stage " + 
                    (curStageChoice + 1) + " has been locked!");
                return;
            }

            //准备关卡;
            cloneLevel();

            //切换关卡选择界面;
            stageState = false;

            //重置位置;
            curLevelChoice = 0;
            changeLevelColor(-1);

            //显示关卡选择界面;
            setUIState();

            //设置当前关卡信息;
            setCurLevelInfo();
            return;
        }

        if (name.Equals("Back"))//Button Click;
        {
            //显示关卡选择界面;
            stageState = true;
            setUIState();

            //设置左右切换按钮;
            setLeftOrRightButtonVisible();
            //设置页码;
            setCurStageTag();
            return;
        }

        if (name.Equals("Left") || name.Equals("Right"))
        {
            //设置点击状态;
            moveFlag = 1;

            if(name.Equals("Left"))
            {
                curStageChoice = (curStageChoice - 1 >= 0) ? curStageChoice - 1 : curStageChoice;
            }
            else if(name.Equals("Right"))
            {
                curStageChoice = (curStageChoice + 1 < stageNum) ? curStageChoice + 1 : curStageChoice;
            }

            //调整Panel位置;
            setPanelPosition(true);

            //设置左右切换按钮;
            setLeftOrRightButtonVisible();
            //设置页码;
            setCurStageTag();
            return;
        }

        if(name.Contains("Map") || name.Equals("Start"))
        {
            bool isLock = DataManager.getInstance().StageData[curStageChoice].m_levelInfo[curLevelChoice].m_lockState;
            if (isLock)
            {
                LuoboTool.Log("[SelectStage]:Stage " + (curStageChoice + 1)
                    + " Level " + (curLevelChoice + 1) + " has been locked!");
                return;
            }

            //仅当在第一个主题第一个关卡时弹出引导;
            if (!(curStageChoice == 0 && curLevelChoice == 0))
            {
                DataManager.getInstance().ShowTips = false;
            }

            DataManager.getInstance().setStageAndLevelData(curStageChoice, curLevelChoice);

            SceneManager.getInstance().openWindow("Scene/GameScene/UI", 0.1f);
            AudioManager.getInstance().Play("Main/BGMusic.mp3", true, false);
            return;
        }
        
    }

    /// <summary>
    /// 设置Panel位置;
    /// </summary>
    /// <param name="needReset"></param>
    void setPanelPosition(bool takeAction = false)
    {
        float targetX = (stageState) ? (-curStageChoice * stageCellWidth) : 
            (-curLevelChoice * levelCellWidth);
        Transform find = transform.Find("Panel/" + ((stageState) ? "StagePanel" : "LevelPanel") + "/Panel");
        if (find != null)
        {
            if(takeAction)
            {
                SpringPanel springPanel = find.GetComponent<SpringPanel>();
                if (springPanel == null)
                {
                    springPanel = find.gameObject.AddComponent<SpringPanel>();
                }
                springPanel.target = new Vector3(targetX, 0, 0);
                springPanel.onFinished = onFinished;
                springPanel.enabled = true;
            }
            else
            {
                SpringPanel springPanel = find.GetComponent<SpringPanel>();
                if(springPanel != null)
                {
                    Destroy(springPanel);
                }
                find.localPosition = new Vector3(targetX, 0, 0);
                UIPanel panel = find.GetComponent<UIPanel>();
                if (panel != null)
                {
                    panel.clipOffset = new Vector2(-targetX, 0);
                }
            }
        }
    }

    /// <summary>
    /// Panel自动调整结束;
    /// </summary>
    void onFinished()
    {
        
    }

    /// <summary>
    /// 拷贝关卡GameObject;
    /// </summary>
    /// <param name="stage"></param>
    void cloneLevel()
    {
        UIAtlas atlas = Resources.Load("Scene/SelectStage/Atlas/Stage" 
            + (curStageChoice + 1).ToString() + " Atlas", typeof(UIAtlas)) as UIAtlas;
        for (int i = 1; i <= levelNum[curStageChoice]; i++)
        {
            Transform trans = LuoboTool.getTransform(transform, "Panel/LevelPanel/Panel/Grid/"
                + "Map_" + i.ToString());
            //地图设置;
            UISprite sprite = LuoboTool.getSprite(trans, "Image");
			if(sprite != null)
			{
				sprite.atlas = atlas;
				sprite.spriteName = "ss_map" + i.ToString("D2");
				sprite.color = new Color(0.5f, 0.5f, 0.5f, 1);
			}

            //锁设置;
            LuoboTool.changeState(trans, "Lock", 
                DataManager.getInstance().StageData[curStageChoice].m_levelInfo[i - 1].m_lockState);
            
            //所有道具清除奖牌设置;
            LuoboTool.changeState(trans, "AllClearMedal",
                DataManager.getInstance().StageData[curStageChoice].m_levelInfo[i - 1].m_allClearMedal);
            //萝卜等级;
            int medal = DataManager.getInstance().StageData[curStageChoice].m_levelInfo[i - 1].m_luoboMedal;
            LuoboTool.changeState(trans, "LuoboMedal", medal > 0);
            if (medal > 0)
            {
                LuoboTool.setSprite(trans, "LuoboMedal", 
                    "gainhonor_" + medal.ToString());
            }

            //修改关卡可用炮塔信息对应图集;
            sprite = LuoboTool.getSprite(transform, "Panel/LevelPanel/useTower");
            if(sprite != null)
            {
                sprite.atlas = atlas;
            }

            //绑定点击处理方法;
            UIEventListener eventListener = trans.GetComponent<UIEventListener>();
            if(eventListener == null)
            {
                eventListener = trans.gameObject.AddComponent<UIEventListener>();
            }

            eventListener.onClick = onButtonClick;
        }

        //重排列;
        Transform parent = LuoboTool.getTransform(transform, "Panel/LevelPanel/Panel/Grid");
        if(parent != null)
        {
            UIGrid grid = parent.GetComponent<UIGrid>();
            if (grid != null)
            {
                grid.Reposition();
            }
        }
    }


	void changeLevelColor(int oldLevelChoice)
	{
        if(oldLevelChoice != -1)
        {
            UISprite sprite = LuoboTool.getSprite(transform, "Panel/LevelPanel/Panel/Grid/" +
                  "Map_" + (oldLevelChoice + 1).ToString() + "/Image");
            if (sprite != null)
            {
                sprite.color = new Color(0.5f, 0.5f, 0.5f, 1);
            }

            //更改当前选中的地图颜色;
            sprite = LuoboTool.getSprite(transform, "Panel/LevelPanel/Panel/Grid/" +
                      "Map_" + (curLevelChoice + 1).ToString() + "/Image");
            if (sprite != null)
            {
                sprite.color = new Color(1, 1, 1, 1);
            }
        }
        else
        {
            Transform trans = LuoboTool.getTransform(transform, "Panel/LevelPanel/Panel/Grid");
            for (int i = 0; i < levelNum[curStageChoice]; i++)
            {
                UISprite sprite = LuoboTool.getSprite(trans, "Map_" + (i + 1).ToString() + "/Image");
                if(sprite != null)
                {
                    if(i == curLevelChoice)
                    {
                        sprite.color = new Color(1, 1, 1, 1);
                    }
                    else
                    {
                        sprite.color = new Color(0.5f, 0.5f, 0.5f, 1);
                    }
                }
            }
        }
	}

    /// <summary>
    /// 读取数据;
    /// </summary>
    void loadData()
    {
        //initialize;
        for (int i = 0; i < stageNum; i++)
        {
            curPassLevelNum[i] = DataManager.getInstance().StageData[i].m_unlockLevel;
            levelNum[i] = DataManager.getInstance().StageData[i].m_levelNum;
        }

        curStageChoice = DataManager.getInstance().StageChoice;
        curLevelChoice = DataManager.getInstance().LevelChoice;

        //返回到选关界面;
        if(curStageChoice >= 0)
        {
            LuoboTool.Log("[RETURN]curStageChoice:" + curStageChoice + ", curLevelChoice:" + curLevelChoice);
            stageState = false;

            //显示所有关卡;
            cloneLevel();
            //设置当前关卡信息;
            setCurLevelInfo();
            //设置关卡颜色;
            changeLevelColor(-1);
        }
        else
        {
            //重置为0;
            curStageChoice = 0;
            curLevelChoice = 0;
        }

        setStageInfo();
    }

    /// <summary>
    /// 设置主题信息;
    /// </summary>
    void setStageInfo()
    {
        Transform find = null;
        for (int i = 1; i <= stageNum; i++)
        {
            find = transform.Find("Panel/StagePanel/Panel/Grid/Stage" + i.ToString());
            if(find != null)
            {
                UISprite sprite = find.Find("levelNum").GetComponent<UISprite>();
                sprite.spriteName = "bookmark_" + curPassLevelNum[i - 1].ToString() + "-9";//关卡信息;
                //Debug.Log("[SelectStage::changeStageState]levelNum SpriteName:" + sprite.spriteName);
                //如果当前闯过的关卡数大于0，说明已解锁;第一个主题默认为解锁状态;
                if (curPassLevelNum[i - 1] > 0 || (1 == i && 0 == curPassLevelNum[i - 1]))
                {
                    LuoboTool.changeState(find, "lock", false);
                }
                else if(0 == curPassLevelNum[i - 1] && i > 1)
                {//如果闯过关卡数为0，说明还未解锁，显示锁;
                    LuoboTool.changeState(find, "lock", true);
                }
            }
        }
    }
}
