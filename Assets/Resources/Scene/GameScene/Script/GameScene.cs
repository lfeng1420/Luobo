using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScene : MonoBehaviour 
{
    /// <summary>
    /// 怪物移动方向;
    /// </summary>
    public enum MonsterDIR
    {
        None,
        UP,
        DOWN,
        LEFT,
        RIGHT,
    }

    public class Attack
    {
        public Transform tower = null;
        public Transform monster = null;
        public Attack(Transform _tower, Transform _monster)
        {
            tower = _tower;
            monster = _monster;
        }
    }


    /// <summary>
    /// 炮塔信息类;
    /// tower：保存炮塔Transform，炮塔类型可能有多个，但Transform只有一个;
    /// towerLevel:炮塔等级;
    /// </summary>
    public class TowerInfo
    {
        public Transform tower = null;
        public int towerLevel = -1;
        public TowerInfo(Transform _tower, int _level)
        {
            tower = _tower;
            towerLevel = _level;
        }
    }

    public class MonsterInfo
    {
        public Transform monTrans = null;
        public Monster info = null;
        public MonsterInfo(Transform _trans, Monster _info)
        {
            monTrans = _trans;
            info = _info;
        }
    }
    /// <summary>
    /// 当前关卡信息;
    /// </summary>
    LevelData levelData = null;

    /// <summary>
    /// 当前关卡;
    /// </summary>
    int curLevelChoice = 0;

    /// <summary>
    /// 当前主题;
    /// </summary>
    int curStageChoice = 0;

    /// <summary>
    /// 当前金币数量;
    /// </summary>
    int curGold = 0;

    /// <summary>
    /// 当前怪物波数;
    /// </summary>
    int curWaveIndex = 1;

    /// <summary>
    /// 倒计时;
    /// </summary>
    int readyCount;

    /// <summary>
    /// 当前选择的空白位置Transform;
    /// </summary>
    Transform curSpaceTrans = null;

    /// <summary>
    /// 当前选中的道具索引;
    /// </summary>
    int curSomeObjIndex = -1;

    /// <summary>
    /// 当前选中的炮塔索引;
    /// </summary>
    Transform curTowerTrans = null;

    /// <summary>
    /// 萝卜生命;
    /// </summary>
    int luoboLife = 10;


    List<Vector3> movePathNode = null;

    /// <summary>
    /// 怪物移动标志;
    /// </summary>
    bool monsterMoveFlag = false;

    /// <summary>
    /// 保存各怪物当前已到达的节点序号;
    /// </summary>
    List<int> monsterMoveNodeIndex = null;

    /// <summary>
    /// 保存所有怪物的Transform;
    /// </summary>
    List<MonsterInfo> monsterList = null;

    /// <summary>
    /// 当前指向的怪物Transform;
    /// </summary>
    Transform curPointAtMonTrans = null;

    /// <summary>
    /// 已创建的炮塔信息序列;
    /// </summary>
    List<TowerInfo> towerList = null;

    /// <summary>
    /// 攻击关系队列;
    /// </summary>
    List<Attack> attackList = null;

    /// <summary>
    /// 炮塔时间间隔序列;
    /// </summary>
    List<float> deltaList = null;

    /// <summary>
    /// 萝卜动画播放标记;
    /// </summary>
    bool playLuoboAnim = true;

    /// <summary>
    /// 加速标志;
    /// </summary>
    bool douSpeed = false;

    /// <summary>
    /// 继续/暂停标志;
    /// </summary>
    bool isPlaying = true;

	// Use this for initialization
	void Start () 
    {
        LuoboTool.changeState(transform, "Top/Gold/Num", false);
        LuoboTool.changeState(transform, "Aim/Forbidden", false);

        //按钮响应绑定处理方法;
        Transform trans = LuoboTool.getTransform(transform, "Tip/Buttons/Skip");
        UIEventListener eventListener = LuoboTool.getEventListener(trans);
        if (eventListener != null)
        {
            eventListener.onClick = onButtonClick;
        }

        string[] btnStr = { "Top/Buttons/Speed", "Top/Buttons/Play", "Top/Buttons/Option", 
                              "Menu/Options/Buttons/Continue", "Menu/Options/Buttons/Select", 
                              "Menu/Options/Buttons/Restart"};
        for ( int i = 0; i < btnStr.Length; i++ )
        {
            trans = LuoboTool.getTransform(transform, btnStr[i]);
            eventListener = LuoboTool.getEventListener(trans);
            if (eventListener != null)
            {
                eventListener.onClick = onButtonClick;
            }
        }
        
        //胜利失败界面按钮响应;
        string[] gameOverBtnName = {"GameOver/Buttons/Continue", 
                               "GameOver/Buttons/Select", "GameOver/Buttons/Restart"};
        for ( int i = 0; i < gameOverBtnName.Length; i++ )
        {
            trans = LuoboTool.getTransform(transform, gameOverBtnName[i]);
            eventListener = LuoboTool.getEventListener(trans);
            if (eventListener != null)
            {
                eventListener.onClick = onGameOverButtonClick;
            }
        }

        trans = LuoboTool.getTransform(transform, "Background/TouchArea");
        LuoboTool.changeState(trans, true);
        eventListener = LuoboTool.getEventListener(trans);
        if (eventListener != null)
        {
            eventListener.onClick = onButtonClick;
        }

        //炮塔升级或出售菜单点击响应;
        trans = LuoboTool.getTransform(transform, "Menu/TowerMenu/Update");
        eventListener = LuoboTool.getEventListener(trans);
        if (eventListener != null)
        {
            eventListener.onClick = onTowerMenuClick;
        }
        trans = LuoboTool.getTransform(transform, "Menu/TowerMenu/Delete");
        eventListener = LuoboTool.getEventListener(trans);
        if (eventListener != null)
        {
            eventListener.onClick = onTowerMenuClick;
        }
        
        //绑定萝卜点击处理;
        trans = LuoboTool.getTransform(transform, "StartAndEnd/Luobo");
        if(trans != null)
        {
            eventListener = LuoboTool.getEventListener(trans);
            if (eventListener != null)
            {
                eventListener.onClick = onButtonClick;
            }
        }

        //初始化List;
        monsterMoveNodeIndex = new List<int>();
        movePathNode = new List<Vector3>();
        monsterList = new List<MonsterInfo>();
        towerList = new List<TowerInfo>();
        attackList = new List<Attack>();
        deltaList = new List<float>();
        
        douSpeed = false;
        isPlaying = true;

        Init();
	}
	
    /// <summary>
    /// 获取与炮塔距离最近的怪物;
    /// </summary>
    /// <param name="towerPos">炮塔位置</param>
    /// <param name="radius">炮塔攻击半径</param>
    /// <returns></returns>
    Transform getLowestDistance(Vector3 towerPos, float radius)
    {
        Transform trans = null;
        float minDis = radius * radius;
        for(int i = 0; i < monsterList.Count; i++)
        {
            if (!monsterList[i].monTrans.gameObject.activeSelf) continue;
            Vector2 size = LuoboTool.getSpriteSize(monsterList[i].monTrans);
            Vector3 monPos = monsterList[i].monTrans.localPosition + new Vector3(0, size.y / 2, 0);
            float distance = (monPos.x - towerPos.x) * (monPos.x - towerPos.x) +
                (monPos.y - towerPos.y) * (monPos.y - towerPos.y);
            if(distance <= minDis)
            {
                trans = monsterList[i].monTrans;
                minDis = distance;
            }
        }
        return trans;
    }

    /// <summary>
    /// 更新攻击关系序列;
    /// </summary>
    void updateAttackInfo()
    {
        //标记是否存在攻击关系;
        bool[] isExist = new bool[towerList.Count];
        for(int i = 0; i < isExist.Length; i++)
        {
            isExist[i] = false;
        }

        //先更新AttackList序列中的攻击关系，可能存在已经失效的关系;
        List<int> removeList = new List<int>();
        removeList.Clear();
        for(int i = 0; i < attackList.Count; i++)
        {
            Vector2 size = LuoboTool.getSpriteSize(attackList[i].monster);
            Vector3 monsterPos = attackList[i].monster.localPosition +
                new Vector3(0, size.y / 2, 0);
            Vector3 towerPos = attackList[i].tower.localPosition;
            Tower info = getTowerInfo(attackList[i].tower);
            float radius = info.m_attackRadius;

            float distance = (monsterPos.x - towerPos.x) * (monsterPos.x - towerPos.x)
                + (monsterPos.y - towerPos.y) * (monsterPos.y - towerPos.y);
            if(distance > radius * radius)
            {
                removeList.Add(i);
            }
        }
        //移除失效的攻击关系;
        removeListElem(removeList);

        for (int i = 0; i < attackList.Count; i++ )
        {
            int index = getTowerIndex(attackList[i].tower);
            isExist[index] = true;
        }

        for (int i = 0; i < towerList.Count; i++)
        {
            Tower info = getTowerInfo(towerList[i].tower);
            if (info.m_attackType != AttackType.Multiple && isExist[i]) continue;

            //获取距离当前炮塔最近的怪物;
            Transform monTrans = getLowestDistance(towerList[i].tower.localPosition,
            info.m_attackRadius);
            if (monTrans != null)
            {
                Attack attack = new Attack(towerList[i].tower, monTrans);
                attackList.Add(attack);
                deltaList.Add(0.3f);
                LuoboTool.Log("[updateAttackInfo]Attack ADD:" + i + ", " 
                + getMonsterIndex(monTrans));
            }
        }
    }


    void removeListElem(List<int> delList)
    {
        for (int i = 0; i < delList.Count; i++)
        {
            int ix = delList[i] - i;
            string towerName = attackList[ix].tower.gameObject.name;
            TowerType towerType = (TowerType)(int.Parse(towerName));
            int towerIx = getTowerIndex(attackList[ix].tower);
            int monIx = getMonsterIndex(attackList[ix].monster);

            LuoboTool.Log("[updateAttackInfo]Attack REMOVE:" + towerIx + ", " + monIx + ", " + (towerType.ToString() + (towerList[towerIx].towerLevel + 1) + "1"));

            //重设炮塔Sprite;
            Transform imgTrans = LuoboTool.getTransform(attackList[ix].tower, "Tower/towerImg");
            UISpriteAnimation spriteAnim = imgTrans.gameObject.GetComponent<UISpriteAnimation>();
            if (spriteAnim != null)
            {
                spriteAnim.enabled = false;
                spriteAnim.Reset();
            }

            //恢复速度;
            int moIx = int.Parse(attackList[ix].monster.gameObject.name) - 1;
            if (towerType == TowerType.Shit)
            {
                monsterList[monIx].info.m_walkSpeed = (int)(DataManager.getInstance().MonsterList[moIx].m_walkSpeed * (douSpeed ? 1.5f : 1));
                //LuoboTool.Log("[BACK]monIx:" + monIx + ", m_walkSpeed:" + monsterList[monIx].info.m_walkSpeed + ", moIx:" + moIx + ", m_walkSpeed:" + DataManager.getInstance().MonsterList[moIx].m_walkSpeed * (douSpeed ? 1.5f : 1));
            }

            //隐藏怪物可能存在的附加效果;
            LuoboTool.changeState(attackList[ix].monster, "Effect/" + towerType.ToString(), false);
            LuoboTool.changeState(attackList[ix].monster, "Effect/" + towerType.ToString() + "1", false);
            LuoboTool.changeState(attackList[ix].monster, "Blood", false);

            attackList.RemoveAt(ix);
            deltaList.RemoveAt(ix);
        }
    }


    /// <summary>
    /// 更新萝卜生命值及显示图片;
    /// </summary>
    void updateLuoboLife()
    {
        //playLuoboAnim = (luoboLife < 4)?false:playLuoboAnim;
        //需要调整萝卜位置;
        Transform trans = LuoboTool.getTransform(transform, "StartAndEnd/Luobo/Rotate");
        trans.localPosition = (luoboLife < 4) ? new Vector3(-12, -25, 0) : Vector3.zero;

        if(luoboLife < 4)
        {
            //停用TweenRotation;
            TweenRotation tweenRotate = trans.GetComponent<TweenRotation>();
            if(tweenRotate != null)
            {
                tweenRotate.enabled = false;
                tweenRotate.ResetToBeginning();
            }
        }

        if(luoboLife != 10)
        {
            trans = LuoboTool.getTransform(trans, "Luobo");
            //停用SpriteAnimation;
            UISpriteAnimation spriteAnim = trans.GetComponent<UISpriteAnimation>();
            if(spriteAnim != null)
            {
                spriteAnim.enabled = false;
            }
        }

        //设置萝卜Sprite;
        string spriteName = "hlb" + ((luoboLife == 5 || luoboLife == 7) ? luoboLife.ToString("D2") : (luoboLife - 1).ToString("D2"));
        LuoboTool.setSprite(trans, spriteName);
        
        //设置生命;
        LuoboTool.setSprite(transform, "StartAndEnd/Life", "BossHP" + luoboLife.ToString("D2"));
    }


	// Update is called once per frame
	void Update () 
    {
        if(monsterMoveFlag)
        {
            monsterMoveFlag = false;
            for (int i = 0; i < monsterMoveNodeIndex.Count; i++)
            {
                monsterMoveFlag = true;
                //更新位置;
                Vector3 pos = updateMonsterPosition(i);
                //更新当前指向位置;
                if (monsterList[i].monTrans == curPointAtMonTrans)
                {
                    Transform pointTrans = LuoboTool.getTransform(transform, "Aim/Point");
                    if(pointTrans != null)
                    {
                        pointTrans.localPosition = pos;
                    }
                }

                if (monsterMoveNodeIndex[i] == (movePathNode.Count - 2))
                {
                    if(luoboLife > 0)
                    {
                        AudioManager.getInstance().Play("Items/Crash.mp3", false);
                        luoboLife--;
                    }
                    updateLuoboLife();
                    deleteMonster(i);

                    //如果萝卜生命为0,显示游戏结束界面;
                    if(luoboLife == 0)
                    {
                        //播放失败音效;
                        AudioManager.getInstance().Play("Items/Lose.mp3", false);
                        monsterMoveFlag = false;
                        setGameOverUI(true, false);
                        return;
                    }
                }
            }
            //更新攻击关系队列;
            updateAttackInfo();
            //攻击动画;
            playAttackEffect();
            //检查当前波数，是否满足进入下一波;
            Transform t = LuoboTool.getTransform(transform, "List/MonsterList/Normal");
            if (t != null && t.childCount == 0)
            {
                if (curWaveIndex <= levelData.m_waveNum)
                {
                    curWaveIndex++;
                    setWaveShowNum(false);
                    if (curWaveIndex == levelData.m_waveNum)
                    {
                        LuoboTool.changeState(transform, "Background/FinalWave", true);
                        Invoke("hideFinalWave", 1.5f);

                        //播放最后一波音效;
                        AudioManager.getInstance().Play("Items/Finalwave.mp3", false);
                    }
                    else if (curWaveIndex > levelData.m_waveNum)
                    {
                        curWaveIndex = levelData.m_waveNum;
                        //播放胜利音效;
                        AudioManager.getInstance().Play("Items/Perfect.mp3", false);
                        monsterMoveFlag = false;
                        setGameOverUI(true, true);
                        return;
                    }
                }
                LuoboTool.Log("curWaveIndex:" + curWaveIndex);
                monsterMoveFlag = false;
                Invoke("initAllMonster", 4);
                return;
            }
            else if (t != null)
            {
                monsterMoveFlag = true;
            }
        }
	}

    /// <summary>
    /// 隐藏最后一波;
    /// </summary>
    void hideFinalWave()
    {
        LuoboTool.changeState(transform, "Background/FinalWave", false);
    }

    /// <summary>
    /// 显示攻击动画;
    /// </summary>
    void playAttackEffect()
    {
        for (int i = 0; i < deltaList.Count; i++)
        {
            deltaList[i] += Time.deltaTime;

            //获取炮塔攻击时间间隔;
            Tower towerInfo = getTowerInfo(attackList[i].tower);
            int towerLevel = towerInfo.m_level;
            float towerCD = towerInfo.m_attackCD[towerLevel];
            TowerType type = (TowerType)(towerInfo.m_towerID - 1);
            int towerIndex = getTowerIndex(attackList[i].tower);

            //如果是便便炮塔，减速;
            int moIx = int.Parse(attackList[i].monster.gameObject.name) - 1;
            int monIx = getMonsterIndex(attackList[i].monster);
            if (type == TowerType.Shit)
            {
                monsterList[monIx].info.m_walkSpeed = (int)(DataManager.getInstance().MonsterList[moIx].m_walkSpeed * 
                (douSpeed ? 1.5f : 1) - towerInfo.m_effectValue[towerLevel]);
                //LuoboTool.Log("[SLOW DOWN] monIx:" + monIx + ", m_walkSpeed:" + monsterList[monIx].info.m_walkSpeed + ", moIx:" + moIx + ", m_walkSpeed:" + DataManager.getInstance().MonsterList[moIx].m_walkSpeed * (douSpeed ? 1.5f : 1));
            }


            if (deltaList[i] > towerCD * (douSpeed ? 0.5f : 1f))
            {
                deltaList[i] = 0f;

                //获取怪物和炮塔位置;
                Vector2 size = LuoboTool.getSpriteSize(attackList[i].monster);
                Vector3 monsterPos = attackList[i].monster.localPosition + 
                    new Vector3(0, size.y / 2, 0);
                Vector3 towerPos = attackList[i].tower.localPosition;
                Vector3 target = monsterPos - towerPos;
                float angle = Mathf.Rad2Deg * Mathf.Atan(target.y / target.x);
                angle += (target.x >= 0) ?-90 : 90;

                Transform towerImgTrans = LuoboTool.getTransform(attackList[i].tower, "Tower/towerImg");
                if(towerImgTrans != null && getTowerRotateFlag(type))
                {
                    towerImgTrans.localEulerAngles = new Vector3(0, 0, angle);
                }
                //打开UISpriteAnimation组件;
                UISpriteAnimation spriteAnim = towerImgTrans.GetComponent<UISpriteAnimation>();
                if(spriteAnim != null)
                {
                    spriteAnim.Reset();
                    spriteAnim.enabled = true;
                }

                Transform bulletTrans = LuoboTool.getTransform(attackList[i].tower, "Bullet");
                if (bulletTrans != null)
                {
                    TweenPosition tweenPosition = bulletTrans.GetComponent<TweenPosition>();
                    if (tweenPosition == null)
                    {
                        tweenPosition = bulletTrans.gameObject.AddComponent<TweenPosition>();
                        EventDelegate.Add(tweenPosition.onFinished, bulletTweenFinished);
                    }
                    
                    tweenPosition.ResetToBeginning();
                    tweenPosition.from = Vector3.zero;
                    tweenPosition.to = monsterPos - towerPos;
                    tweenPosition.duration = 0.2f * (douSpeed ? 0.5f : 1f);
                    tweenPosition.delay = 0;
                    tweenPosition.enabled = true;
                    bulletTrans.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 显示萝卜动画;
    /// </summary>
    void playLuoboAnimation()
    {
        Transform luoboTrans = LuoboTool.getTransform(transform, "StartAndEnd/Luobo/Rotate");
        Transform spriteTrans = LuoboTool.getTransform(transform, "StartAndEnd/Luobo/Rotate/Luobo");
        TweenRotation tweenRotate = luoboTrans.GetComponent<TweenRotation>();
        //只在萝卜满血时播放随机动画;
        if (luoboLife == 10)
        {
            //随机选择SpriteAnimation或TweenRotation;
            int randVal = (Random.Range(0, 100)) % 3;

            UISpriteAnimation spriteAnim = spriteTrans.GetComponent<UISpriteAnimation>();
            if (spriteAnim == null)
            {
                spriteAnim = spriteTrans.gameObject.AddComponent<UISpriteAnimation>();
            }

            //获取Sprite;
            UISprite sprite = LuoboTool.getSprite(spriteTrans);
            if(randVal == 0)
            {
                sprite.spriteName = "hlb11";
                spriteAnim.namePrefix = "hlb1";
                spriteAnim.framesPerSecond = 10;
                spriteAnim.loop = false;
                spriteAnim.Reset();
                spriteAnim.enabled = true;
                tweenRotate.enabled = false;
            }
            else if(randVal == 1)
            {
                sprite.spriteName = "hlb21";
                spriteAnim.namePrefix = "hlb2";
                spriteAnim.framesPerSecond = 10;
                spriteAnim.loop = false;
                spriteAnim.Reset();
                spriteAnim.enabled = true;
                tweenRotate.enabled = false;
            }
            else if(randVal == 2)
            {
                sprite.spriteName = "hlb09";
                spriteAnim.enabled = false;

                tweenRotate.enabled = false;
                tweenRotate.ResetToBeginning();
                tweenRotate.enabled = true;
            }
        }
        else if (luoboLife >= 4)
        {
            //只进行旋转;
            if (tweenRotate != null)
            {
                tweenRotate.enabled = false;
                tweenRotate.ResetToBeginning();
                tweenRotate.enabled = true;
            }
        }
        else
        {
            CancelInvoke("playLuoboAnimation");
        }
    }

    /// <summary>
    /// 炮弹TweenPosition结束;
    /// </summary>
    void bulletTweenFinished()
    {
        //隐藏炮弹;
        GameObject go = TweenPosition.current.gameObject;
        go.SetActive(false);

        //获取对应的怪物;
        Transform towerTrans = go.transform.parent;
        Transform monsterTrans = null;
        for(int i = 0; i < attackList.Count; i++)
        {
            if(attackList[i].tower == towerTrans)
            {
                monsterTrans = attackList[i].monster;
                break;
            }
        }

        
        if(monsterTrans == null)
        {
            LuoboTool.Log("[bulletTweenFinished]:Get Monster Attacked By Tower:" + towerTrans.gameObject.name + " Failed!");
            return;
        }

        //获取炮塔类型和等级;
        int towerIndex = getTowerIndex(towerTrans);
        int towerLevel = towerList[towerIndex].towerLevel;
        Tower towerInfo = getTowerInfo(towerIndex);
        if (towerInfo == null)
        {
            return;
        }
        TowerType towerType = (TowerType)(towerInfo.m_towerID - 1);
        //根据炮塔类型播放音效;
        AudioManager.getInstance().Play("Towers/" + towerType.ToString() + ".mp3", false);

        //获取第一个效果;
        Transform effectTrans = LuoboTool.getTransform(monsterTrans, "Effect/" + towerType.ToString());

        //重置第二个效果的TweenAlpha组件;
        Transform otherEffectTrans = LuoboTool.getTransform(monsterTrans, "Effect/" + towerType.ToString() + "1");
        if (otherEffectTrans != null)
        {
            TweenAlpha tweenAlpha = otherEffectTrans.GetComponent<TweenAlpha>();
            if (tweenAlpha == null)
            {
                tweenAlpha = otherEffectTrans.gameObject.AddComponent<TweenAlpha>();
                tweenAlpha.from = 1;
                tweenAlpha.to = 0;
                tweenAlpha.duration = 0.3f;
                tweenAlpha.delay = 0;
                tweenAlpha.ignoreTimeScale = true;
                EventDelegate.Add(tweenAlpha.onFinished, tweenAlphaEffectFinished);
            }

            tweenAlpha.enabled = false;
            tweenAlpha.ResetToBeginning();
            tweenAlpha.enabled = true;

            //显示第二个效果;
            LuoboTool.changeState(otherEffectTrans, true);
        }
        else
        {
            //只有第一个效果，为第一个效果添加TweenAlpha组件;
            TweenAlpha tweenAlpha = effectTrans.GetComponent<TweenAlpha>();
            if(tweenAlpha == null)
            {
                tweenAlpha = effectTrans.gameObject.AddComponent<TweenAlpha>();
                tweenAlpha.from = 1;
                tweenAlpha.to = 0;
                tweenAlpha.duration = 0.4f;
                tweenAlpha.delay = 0;
                tweenAlpha.ignoreTimeScale = true;
            }

            tweenAlpha.ResetToBeginning();
            tweenAlpha.enabled = true;
        }

        //显示第一个效果;
        LuoboTool.changeState(effectTrans, true);


        //更新怪物血量;
        int index = getMonsterIndex(monsterTrans);
        if(index != -1)
        {
            LuoboTool.Log("[bulletTweenFinished] MonsterName:" + monsterTrans.gameObject.name + ", Last Life:" + monsterList[index].info.m_monsterLife);
            monsterList[index].info.m_monsterLife -= towerInfo.m_attackValue[towerLevel];
            LuoboTool.Log("[bulletTweenFinished] Current Life:" + monsterList[index].info.m_monsterLife);

            //如果剩余血量低于0，移除该怪物;
            if(monsterList[index].info.m_monsterLife <= 0)
            {
                //播放音效
                int monType = int.Parse(monsterList[index].monTrans.gameObject.name);
                string audioName = getMonsterSound((MonsterType)monType);
                LuoboTool.Log("audioName:" + audioName);
                AudioManager.getInstance().Play(audioName, false);
                
                Vector3 pos = monsterList[index].monTrans.localPosition;
                Vector2 size = LuoboTool.getSpriteSize(monsterList[index].monTrans);
                deleteMonster(index);
                //显示怪物消除动画;
                createKillAnim(pos + new Vector3(0, size.y / 2, 0), 14, 0);
                return;
            }

            //计算减少的血量与总血量的比例;
            string monsterName = monsterTrans.gameObject.name;
            int impLife = levelData.m_lifeImprove[curWaveIndex - 1];
            int allLife = DataManager.getInstance().MonsterList[int.Parse(monsterName) - 1].m_monsterLife + impLife;
            float rate = (allLife - monsterList[index].info.m_monsterLife) * 1.0f / allLife * 1.0f;
            //受到攻击后，血量大于0，显示血条;
            Transform bloodTrans = LuoboTool.getTransform(monsterTrans, "Blood");
            //设置Slider组件;
            if(bloodTrans != null)
            {
                UISlider slider = bloodTrans.GetComponent<UISlider>();
                if(slider != null)
                {
                    slider.value = rate;
                }
            }
            LuoboTool.changeState(bloodTrans, true);
        }
    }


    void tweenAlphaEffectFinished()
    {
        TweenAlpha.current.ResetToBeginning();
        TweenAlpha.current.enabled = true;
    }

    /// <summary>
    /// 获取炮塔信息;
    /// </summary>
    /// <param name="tower">炮塔Transform</param>
    /// <returns></returns>
    Tower getTowerInfo(Transform tower)
    {
        int index = getTowerIndex(tower);
        int type = int.Parse(towerList[index].tower.gameObject.name);
        //LuoboTool.Log("index:" + index + "type:" + type);
        Tower info = DataManager.getInstance().TowerList[type];
        //更新等级;
        info.m_level = towerList[index].towerLevel;
        return info;
    }

    /// <summary>
    /// 获取炮塔信息;
    /// </summary>
    /// <param name="tower">指定炮塔Transform</param>
    /// <returns></returns>
    Tower getTowerInfo(int index)
    {
        int type = int.Parse(towerList[index].tower.gameObject.name);
        Tower info = DataManager.getInstance().TowerList[type];
        //更新等级;
        //info.m_level = towerList[index].towerLevel;
        return info;
    }

    /// <summary>
    /// 获取指定炮塔在炮塔序列中的序号;
    /// </summary>
    /// <param name="tower">指定炮塔Transform</param>
    /// <returns></returns>
    int getTowerIndex(Transform tower)
    {
        for (int i = 0; i < towerList.Count; i++)
        {
            if (towerList[i].tower == tower)
            {
                return i;
            }
        }

        LuoboTool.Log("[getTowerIndex]: NOT FOUND");
        return -1;
    }


    Monster getMonsterInfo(Transform monster)
    {
        int index = getMonsterIndex(monster);
        if(index == -1)return null;
        return monsterList[index].info;
    }

    int getMonsterIndex(Transform monster)
    {
        for(int i = 0; i < monsterList.Count; i++)
        {
            if(monster.gameObject == monsterList[i].monTrans.gameObject)
            {
                return i;
            }
        }

        LuoboTool.Log("[getMonsterIndex]: NOT FOUND");
        return -1;
    }

    /// <summary>
    /// 设置指定空白位置各组件状态;
    /// </summary>
    /// <param name="trans">空白位置Transform</param>
    /// <param name="animState">Animator状态</param>
    /// <param name="sprAnimState">spriteAnim状态</param>
    /// <param name="coliderState">BoxColider状态</param>
    /// <param name="spriteName">Sprite名称</param>
    /// <param name="alpha">Sprite alpha</param>
    void setSpaceCompEnable(Transform trans, bool animState, bool sprAnimState, 
        bool coliderState, string spriteName = "", float alpha = 0f)
    {
        if (trans == null) return;

        //设置Animation组件状态;
        TweenAlpha tweenAlpha = trans.GetComponent<TweenAlpha>();
        if (tweenAlpha != null)
        {
            tweenAlpha.enabled = animState;
        }

        //设置SpriteAnimation组件状态;
        UISpriteAnimation spriteAnim = trans.GetComponent<UISpriteAnimation>();
        if (spriteAnim == null)
        {
            spriteAnim = trans.gameObject.AddComponent<UISpriteAnimation>();
            spriteAnim.namePrefix = "select_";
            spriteAnim.framesPerSecond = 6;
        }
        spriteAnim.enabled = sprAnimState;
        
        //BoxCollider状态;
        BoxCollider boxCollider = trans.GetComponent<BoxCollider>();
        if(boxCollider == null)
        {
            boxCollider = trans.gameObject.AddComponent<BoxCollider>();
            boxCollider.center = new Vector3(40, -40, 0);
            boxCollider.size = new Vector3(80, 80, 1);
        }
        boxCollider.enabled = coliderState;

        UISprite sprite = LuoboTool.getSprite(trans);
        if(trans != null)
        {
            if (!spriteName.Equals("")) 
            {
                sprite.spriteName = spriteName;
                sprite.MakePixelPerfect();
            }
            sprite.alpha = alpha;
        }
    }

    /// <summary>
    /// 设置某一集合中所有空白位置各组件状态;
    /// </summary>
    /// <param name="trans">空白位置Transform</param>
    /// <param name="b1">Animator状态</param>
    /// <param name="b2">spriteAnim状态</param>
    /// <param name="b3">BoxColider状态</param>
    /// <param name="spriteName">Sprite名称</param>
    /// <param name="alpha">Sprite alpha</param>
    void setAllSpaceCompEnable(Transform trans, bool b1, bool b2, bool b3,
        string spriteName = "", float alpha = 0f)
    {
        if (trans == null) return;
        for(int i = 0; i < trans.childCount; i++)
        {
            setSpaceCompEnable(trans.GetChild(i), b1, b2, b3, spriteName, alpha);
        }
    }

    /// <summary>
    /// 道具设置;
    /// </summary>
    void initSomeObject()
    {
        string[] colorStr = { "_white", "_purple" };
        Transform trans = transform.Find("List/ObjectList/Sprite");
        Transform parent = transform.Find("List/ObjectList/Normal");
        Transform delObj = transform.Find("List/ObjectList/Delete");
        for (int i = 0; i < DataManager.getInstance().SomeObjectList.Count; i++)
        {
            GameObject go = null;
            ObjectType type = DataManager.getInstance().SomeObjectList[i].m_objType;

            string spriteName = "cloud" + ((int)type).ToString("D2") +
                colorStr[levelData.m_colorType];
            LuoboTool.Log("ObjSpriteName:" + spriteName);

            //设置位置;
            int x = DataManager.getInstance().SomeObjectList[i].m_pointX;
            int y = -DataManager.getInstance().SomeObjectList[i].m_pointY;
            Vector3 pos = new Vector3(x, y, 0);

            //从已删除道具集合中获取一个;
            if (delObj.childCount > 0)
            {
                Transform t = delObj.GetChild(0);
                LuoboTool.initObj(t, parent, i.ToString("D2"), pos);
                LuoboTool.setSprite(t, spriteName);
                go = t.gameObject;
            }
            else
            {
                //创建;
                go = LuoboTool.cloneObj(parent, trans, pos, i.ToString("D2"));
                LuoboTool.setSprite(go.transform, spriteName);
            }

            //设置BoxColider;
            Vector2 size = LuoboTool.getSpriteSize(go.transform);
            BoxCollider boxColider = go.GetComponent<BoxCollider>();
            if(boxColider == null)
            {
                boxColider = go.AddComponent<BoxCollider>();
            }
            boxColider.center = new Vector3(size.x / 2, -size.y / 2, 0);
            boxColider.size = new Vector3(size.x, size.y, 0);

            //关联点击事件响应;
            UIEventListener eventListener = go.GetComponent<UIEventListener>();
            if(eventListener == null)
            {
                eventListener = go.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onMonsterOrObjectClick;
        }
    }


    /// <summary>
    /// 设置空白位置;
    /// </summary>
    void initSpaceObject()
    {
        LuoboTool.changeState(transform, "List/SpaceList", true);
        Transform trans = transform.Find("List/SpaceList/Anim");
        Transform parent = transform.Find("List/SpaceList/Normal");
        Transform delObj = transform.Find("List/SpaceList/Delete");
        for (int i = 0; i < DataManager.getInstance().SpaceNodeList.Count; i++)
        {
            //设置位置;
            int x = DataManager.getInstance().SpaceNodeList[i].m_pointX;
            int y = -DataManager.getInstance().SpaceNodeList[i].m_pointY;
            Vector3 pos = new Vector3(x, y, 0);
            string spaceName = (int)pos.x + "_" + (int)pos.y;

            //是否已经存在;
            Transform t = parent.Find(spaceName);
            if (t != null)
            {
                setSpaceCompEnable(t, true, false, false);
                continue;
            }

            //从已删除的空白位置集合中重置一个;
            if(delObj.childCount > 0)
            {
                t = delObj.GetChild(0);
                LuoboTool.initObj(t, parent, spaceName, pos);
                setSpaceCompEnable(t, true, false, false);
                continue;
            }

            //创建一个空白位置;
            GameObject go = LuoboTool.cloneObj(parent, trans, pos, spaceName);
            setSpaceCompEnable(go.transform, true, false, false);

            //绑定点击事件处理;
            UIEventListener eventListener = go.GetComponent<UIEventListener>();
            if(eventListener == null)
            {
                eventListener = go.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onSpaceClick;
        }
    }

    /// <summary>
    /// 初始化所有怪物;
    /// </summary>
    void initAllMonster()
    {
        //初始化当前出现的怪物序号;
        //monsterNum = -1;

        //清空怪物已到达节点索引序列;
        monsterMoveNodeIndex.Clear();
        //清除所有怪物信息;
        monsterList.Clear();
        //每一波怪物开始时清空攻击序列;
        attackList.Clear();
        //清空时间间隔序列;
        deltaList.Clear();

        //获取当前波数怪物信息;
        MonsterWave monsterWave = levelData.m_monsterWave[curWaveIndex - 1];
        Transform parent = LuoboTool.getTransform(transform, "List/MonsterList/Normal");
        Transform delList = LuoboTool.getTransform(transform, "List/MonsterList/Delete");
        Transform cloneObj = LuoboTool.getTransform(transform, "List/MonsterList/template");

        for(int i = 0; i < monsterWave.m_monsterList.Count; i++)
        {
            Transform t = null;
            int index = monsterWave.m_monsterList[i];
            string monName = index.ToString();

            if(delList.childCount > 0)
            {
                t = delList.GetChild(0);
                LuoboTool.initObj(t, parent, monName, movePathNode[0], false);
            }
            else
            {
                GameObject go = LuoboTool.cloneObj(parent, cloneObj, movePathNode[0], monName);
                t = go.transform;
                go.SetActive(false);
            }

            //获取怪物宽高，设置怪物图片;
            MonsterType type = (MonsterType)monsterWave.m_monsterList[i];
            UISprite sprite = LuoboTool.getSprite(t);
            if (sprite != null)
            {
                sprite.spriteName = type.ToString() + "01";
                sprite.MakePixelPerfect();
            }
            Vector2 size = LuoboTool.getSpriteSize(t);

            //设置BoxColider;
            BoxCollider boxColider = t.gameObject.GetComponent<BoxCollider>();
            if(boxColider == null)
            {
                boxColider = t.gameObject.AddComponent<BoxCollider>();
            }
            boxColider.center = new Vector3(0, size.y / 2, 0);
            boxColider.size = new Vector3(size.x, size.y, 0);
            boxColider.enabled = true;

            //关联点击事件处理;
            UIEventListener eventListener = t.gameObject.GetComponent<UIEventListener>();
            if(eventListener == null)
            {
                eventListener = t.gameObject.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onMonsterOrObjectClick;

            //设置SpriteAnimation组件;
            UISpriteAnimation spriteAnim = t.GetComponent<UISpriteAnimation>();
            if(spriteAnim == null)
            {
                spriteAnim = t.gameObject.AddComponent<UISpriteAnimation>();
            }
            spriteAnim.namePrefix = type.ToString();
            spriteAnim.framesPerSecond = 6;
            spriteAnim.loop = true;
            spriteAnim.enabled = true;

            //调整血条位置;
            Transform bloodTrans = LuoboTool.getTransform(t, "Blood");
            if (bloodTrans != null)
            {
                bloodTrans.localPosition = new Vector3(0, size.y, 0);
            }
            LuoboTool.changeState(bloodTrans, false);

            //初始化附加效果状态;
            Transform effectTrans = LuoboTool.getTransform(t, "Effect");
            for(int j = 0; j < effectTrans.childCount; j++)
            {
                Transform child = effectTrans.GetChild(j);
                LuoboTool.changeState(child, false);
            }

            //设置怪物信息;
            Monster data = DataManager.getInstance().MonsterList[index - 1];
            int impLife = levelData.m_lifeImprove[curWaveIndex - 1];
            Monster monster = new Monster(data.m_monsterLife + impLife, data.m_walkSpeed);
            MonsterInfo info = new MonsterInfo(t, monster);
            //添加到怪物序列中;
            monsterList.Add(info);
        }

        //怪物移动开始;
        monsterMoveFlag = true;
        //每2秒显示一只怪物;
        StartCoroutine(showCurrentMonster());
    }


    void removeAllMonster()
    {
        Transform t = LuoboTool.getTransform(transform, "List/MonsterList/Normal");
        Transform parent = LuoboTool.getTransform(transform, "List/MonsterList/Delete");
        while(t.childCount > 0)
        {
            Transform child = t.GetChild(0);
            child.parent = parent;
        }

        //清空攻击序列;
        attackList.Clear();
        //清空怪物已到达节点索引序列;
        monsterMoveNodeIndex.Clear();
        //清空时间间隔序列;
        deltaList.Clear();
        //清除所有怪物信息;
        monsterList.Clear();
    }


    /// <summary>
    /// 关卡相应界面初始化;
    /// </summary>
    void Init()
    {
        //设置对应界面显示或隐藏;
        LuoboTool.changeState(transform, "Animation/ReadyAnim", false);
        LuoboTool.changeState(transform, "Menu/Options", false);

        //初始化索引;
        curSpaceTrans = null;
        curSomeObjIndex = -1;
        curTowerTrans = null;
        curPointAtMonTrans = null;
        curWaveIndex = 1;

        //初始化倒计时;
        readyCount = 0;

        //初始化萝卜动画播放标记;
        playLuoboAnim = true;

        //初始化怪物移动标志为false;
        monsterMoveFlag = false;

        //清空炮塔序列;
        towerList.Clear();

        //获取当前选择;
        curStageChoice = DataManager.getInstance().StageChoice;
        curLevelChoice = DataManager.getInstance().LevelChoice;
        levelData = DataManager.getInstance().StageData[curStageChoice].m_levelInfo[curLevelChoice];
        LuoboTool.Log("curStageChoice:" + curStageChoice + ", curLevelChoice:" + curLevelChoice);

        //播放主题音效;
        string BGMName = "Items/BGMusic" + (curStageChoice + 1).ToString("D2") + ".mp3";
        AudioManager.getInstance().Play(BGMName, true, true, true);

        //设置怪物波数;
        setWaveShowNum(true);
        setWaveShowNum(false);

        //设置地图;
        setCurMap();

        //加载地图信息;
        string bgPath = "BGPath" + (curStageChoice + 1) + "_" + (curLevelChoice + 1) + ".tmx";
        DataManager.getInstance().loadData("Scene/GameScene/TMX/" + bgPath);

        //初始化场景道具;
        initSomeObject();

        //初始化炮塔菜单;
        initCreateTowerMenu();

        //初始化金币;
        setGoldShowNum(450);

        //隐藏创建炮塔菜单，仅在点击空白位置时显示;
        LuoboTool.changeState(transform, "Menu/CreateTowerMenu", false);

        //设置出生牌位置;
        string[] colorStr = { "_white", "_purple" };
        Transform trans = transform.Find("StartAndEnd/Start");
        int startX = DataManager.getInstance().PathNodeList[0].m_nodeX - 5;
        int startY = -DataManager.getInstance().PathNodeList[0].m_nodeY;
        if (trans != null)
        {
            trans.localPosition = new Vector3(startX, startY, 0);
        }
        LuoboTool.setSprite(trans, "start01" + colorStr[levelData.m_colorType]);

        //设置萝卜位置;
        int count = DataManager.getInstance().PathNodeList.Count;
        startX = DataManager.getInstance().PathNodeList[count - 2].m_nodeX + 43;
        startY = -DataManager.getInstance().PathNodeList[count - 2].m_nodeY - 58;
        trans = transform.Find("StartAndEnd/Luobo");
        if (trans != null)
        {
            trans.localPosition = new Vector3(startX, startY, 0);
        }
        //设置萝卜生命位置;
        trans = transform.Find("StartAndEnd/Life");
        startX = DataManager.getInstance().PathNodeList[count - 1].m_nodeX;
        startY = -DataManager.getInstance().PathNodeList[count - 1].m_nodeY;
        if (trans != null)
        {
            trans.localPosition = new Vector3(startX, startY, 0);
        }
        updateLuoboLife();

        //路径节点;
        createPathNode();
        
        //设置怪物出生位置动画;
        setMonsterStartAnim();
        LuoboTool.changeState(transform, "Animation/ArrowAnim", false);

        //显示提示;
        bool tipsShow = DataManager.getInstance().ShowTips;
        showTips(tipsShow);
        if(!tipsShow)
        {
            tipsTweenFinished();
        }

        //StopCoroutine("playLuoboAnimation");
        //开始播放萝卜动画;
        InvokeRepeating("playLuoboAnimation", 0, 4);
    }

    /// <summary>
    /// 重新开始;
    /// </summary>
    void Restart()
    {
        //设置对应界面显示或隐藏;
        LuoboTool.changeState(transform, "Animation/ReadyAnim", false);

        //初始化索引;
        curSpaceTrans = null;
        curSomeObjIndex = -1;
        curTowerTrans = null;
        curPointAtMonTrans = null;

        //一些数据初始化;
        curWaveIndex = 1;
        luoboLife = 10;

        //初始化倒计时;
        readyCount = 0;

        //初始化萝卜动画播放标记;
        playLuoboAnim = true;

        //初始化怪物移动标志为false;
        monsterMoveFlag = false;

        //移除怪物和炮塔;
        removeAllTower();
        removeAllMonster();

        //播放主题音效;
        string BGMName = "Items/BGMusic" + (curStageChoice + 1).ToString("D2") + ".mp3";
        AudioManager.getInstance().Play(BGMName, true, true, true);

        //初始化金币;
        setGoldShowNum(450);

        //设置当前怪物波数;
        setWaveShowNum(false);

        //初始化场景道具;
        //initSomeObject();

        //重置萝卜生命;
        updateLuoboLife();

        //隐藏创建炮塔菜单，仅在点击空白位置时显示;
        LuoboTool.changeState(transform, "Menu/CreateTowerMenu", false);

        tipsTweenFinished();

        //StopCoroutine("playLuoboAnimation");
        //开始播放萝卜动画;
        InvokeRepeating("playLuoboAnimation", 0, 4);
    }


    /// <summary>
    /// 设置显示金币数量;
    /// </summary>
    void setGoldShowNum(int gold)
    {
        curGold = gold;
        Transform numTrans = LuoboTool.getTransform(transform, "Top/Gold/Num0");
        if (numTrans == null)
        {
            LuoboTool.LogError("[setGoldShowNum]:Get Top/Gold/Num0 Error!");
            return;
        }
        Vector3 pos = numTrans.localPosition;

        string numArray = curGold.ToString();
        for(int i = 0; i < numArray.Length; i++)
        {
            int num = int.Parse(numArray[i].ToString());
            //调整坐标;
            Transform trans = LuoboTool.getTransform(transform, "Top/Gold/Num" + i.ToString());
            if(trans != null)
            {
                trans.localPosition = pos;
                trans.gameObject.SetActive(true);
            }
            //更改数字;
            LuoboTool.setSprite(trans, "numwhite-hd_" + (num + 3).ToString("D2"));
            pos.x += 20;
        }
        for(int i = numArray.Length; i <= 9; i++)
        {
            LuoboTool.changeState(transform, "Top/Gold/Num" + i.ToString(), false);
        }

        //检查所有炮塔，更新可升级状态;
        for(int i = 0; i < towerList.Count; i++)
        {
            Tower info = getTowerInfo(towerList[i].tower);
            if (towerList[i].towerLevel == 2)
            {
                LuoboTool.changeState(towerList[i].tower, "Tower/Update", false);
            }
            else
            {
                int needGold = info.m_updateNeedMoney[towerList[i].towerLevel + 1];
                LuoboTool.changeState(towerList[i].tower, "Tower/Update", needGold <= curGold);
            }
        }

        //更新新建炮塔菜单;
        updateCreateTowerMenu();

        //更新当前炮塔升级需要的金钱以及销毁将获得的金钱;
        updateTowerMenu();
    }

    /// <summary>
    /// 设置怪物波数显示;
    /// isAll为true时表示显示关卡对应怪物波数;
    /// isAll为false时表示当前怪物波数;
    /// </summary>
    /// <param name="isAll"></param>
    void setWaveShowNum(bool isAll)
    {
        string path = "Top/WaveInfo/" + ((isAll)? "AllWave_":"CurWave_");
        int waveNum = (isAll) ? levelData.m_waveNum : curWaveIndex;
        //设置左边数字;
        LuoboTool.setSprite(transform, path + "Left",
            "numyellow-hd_" + (waveNum / 10 + 3).ToString("D2"));
        //设置右边数字;
        LuoboTool.setSprite(transform, path + "Right",
            "numyellow-hd_" + (waveNum % 10 + 3).ToString("D2"));
    }


    /// <summary>
    /// 显示提示信息;
    /// </summary>
    void showTips(bool isVisible)
    {
        LuoboTool.changeState(transform, "Tip", isVisible);
        if (!isVisible)
        {
            return;
        }

        //设置提示内容;
    }

    /// <summary>
    /// 结束提示;
    /// </summary>
    void hideTips()
    {
        Transform trans = LuoboTool.getTransform(transform, "Tip");
        TweenPosition tweenPos = trans.GetComponent<TweenPosition>();
        if (tweenPos != null)
        {
            DestroyImmediate(tweenPos);
        }
        tweenPos = trans.gameObject.AddComponent<TweenPosition>();
        tweenPos.from = new Vector3(0, 0, 0);
        tweenPos.to = new Vector3(0, 600, 0);
        tweenPos.delay = 0;
        tweenPos.duration = 0.5f;
        tweenPos.ignoreTimeScale = true;
        EventDelegate.Add(tweenPos.onFinished, tipsTweenFinished);
        tweenPos.enabled = true;
    }

    void tipsTweenFinished()
    {
        //隐藏提示;
        if (DataManager.getInstance().ShowTips)
        {
            DataManager.getInstance().ShowTips = false;
            showTips(false);
        }

        //初始化空白位置;
        initSpaceObject();
        //准备动画;
        InvokeRepeating("showReadyAnim", 0, 1);
        //设置出生位置动画状态，仅在准备动画出现后显示;
        LuoboTool.changeState(transform, "Animation/ArrowAnim", true);

        //8秒后隐藏开始位置箭头动画;
        Invoke("hideArrowAnim", 6);
    }

    /// <summary>
    /// 设置当前地图;
    /// </summary>
    void setCurMap()
    {
        UISprite sprite = LuoboTool.getSprite(transform, "Background/Map");
        if (sprite == null)
        {
            LuoboTool.LogError("[setCurMap]:Get Background/Map Error!");
            return;
        }

        string atlasName = "Theme" + (curStageChoice + 1).ToString() + "Map";
        if(curLevelChoice >= 8)
        {
            atlasName += "Ex";
        }
        atlasName += " Atlas";

        if(!atlasName.Equals(sprite.atlas))
        {
            LuoboTool.Log("[setCurMap]:" + sprite.atlas + " -> " + atlasName);
            UIAtlas atlas = Resources.Load("Scene/GameScene/Atlas/" + atlasName,
                typeof(UIAtlas)) as UIAtlas;
            if (atlas == null)
            {
                LuoboTool.LogError("[setCurMap]:Load Scene/GameScene/Atlas/Theme1Map Atlas Error!");
                return;
            }
            sprite.atlas = atlas;
            LuoboTool.Log("[setCurMap]:Set Atlas Success.");
        }
        else
        {
            LuoboTool.Log("[setCurMap]:No Need to Change Atlas.");
        }

        sprite.spriteName = "BG" + (curLevelChoice + 1).ToString() + "-hd";
        sprite.MakePixelPerfect();
        
        //设置位置偏移;
        int[] dx = { 0, 0, 0, 0, 0, 0, 0, -75, -62};
        Vector3 pos = sprite.transform.localPosition;
        pos.x = 480 + dx[curLevelChoice];
        sprite.transform.localPosition = pos;

        //设置点击事件响应;
        BoxCollider boxColider = sprite.gameObject.GetComponent<BoxCollider>();
        if(boxColider == null)
        {
            boxColider = sprite.gameObject.AddComponent<BoxCollider>();
        }
        boxColider.size = new Vector3(sprite.localSize.x, sprite.localSize.y, 0);
        boxColider.center = new Vector3(-sprite.localSize.x / 2, 
            sprite.localSize.y / 2, 0);

        UIEventListener eventListener = 
            sprite.gameObject.GetComponent<UIEventListener>();
        if(eventListener == null)
        {
            eventListener = sprite.gameObject.AddComponent<UIEventListener>();
        }
        eventListener.onClick = onButtonClick;
    }


    void initCreateTowerMenu()
    {
        Transform cloneObj = LuoboTool.getTransform(transform, "Menu/CreateTowerMenu/Grid/template");
        if(cloneObj == null)
        {
            LuoboTool.LogError("[setCreateTowerMenu]:Cannot find 'Menu/CreateTowerMenu/Grid/template'.");
            return;
        }
        Transform parent = LuoboTool.getTransform(transform, "Menu/CreateTowerMenu/Grid");
        
        int count = levelData.m_towerList.Count;
        for(int i = 0; i < count; i++)
        {
            int towerType = levelData.m_towerList[i] - 1;
            int needGold = DataManager.getInstance().TowerList[towerType].m_updateNeedMoney[0];

            GameObject go = LuoboTool.cloneObj(parent, cloneObj, Vector3.zero, 
                ((TowerType)towerType).ToString() + "_" + levelData.m_towerList[i].ToString("D2"));

            //设置炮塔Sprite;
            LuoboTool.Log("Scene/GameScene/Atlas/Tower/T" + ((TowerType)towerType).ToString() + " Atlas");
            UIAtlas atlas = Resources.Load("Scene/GameScene/Atlas/Tower/T"
                + ((TowerType)towerType).ToString() + " Atlas", typeof(UIAtlas)) as UIAtlas;
            LuoboTool.setSprite(go.transform, "tower", ((TowerType)towerType).ToString() +
                ((curGold >= needGold) ? "01" : "00"), atlas);

            UIEventListener eventListener = go.GetComponent<UIEventListener>();
            if(eventListener == null)
            {
                eventListener = go.AddComponent<UIEventListener>();
            }

            eventListener.onClick = onCreateTowerMenuClick;
        }

        //重新排列;
        UIGrid grid = parent.GetComponent<UIGrid>();
        if(grid != null)
        {
            grid.Reposition();
        }
    }


    void updateCreateTowerMenu()
    {
        //更新创建炮塔菜单;
        Transform gridTrans = LuoboTool.getTransform(transform, "Menu/CreateTowerMenu/Grid");
        if (gridTrans != null)
        {
            for (int i = 0; i < gridTrans.childCount; i++)
            {
                Transform t = gridTrans.GetChild(i);
                string childName = t.gameObject.name;
                if (childName.Equals("template")) continue;
                int index = t.gameObject.name.IndexOf("_");
                string temp = t.gameObject.name.Substring(index + 1, t.gameObject.name.Length - index - 1);
                int type = int.Parse(t.gameObject.name.Substring(index + 1, t.gameObject.name.Length - index - 1));
                int needGold = DataManager.getInstance().TowerList[type - 1].m_updateNeedMoney[0];
                LuoboTool.Log("curGold:" + curGold + ", needGold:" + needGold);
                LuoboTool.setSprite(t, "tower", ((TowerType)(type - 1)).ToString() +
                        ((curGold >= needGold) ? "01" : "00"));
            }
        }
    }

    /// <summary>
    /// 创建路径节点;
    /// </summary>
    void createPathNode()
    {
        //清空路径节点序列;
        movePathNode.Clear();

        Transform parent = transform.Find("List/PathNodeList");
        if (parent == null) return;
        Transform cloneObj = transform.Find("List/PathNodeList/template");
        if (cloneObj == null) return;
        int count = DataManager.getInstance().PathNodeList.Count;
        for(int i = 0; i < count; i++)
        {
            int x = DataManager.getInstance().PathNodeList[i].m_nodeX - 480;
            int y = 320 - DataManager.getInstance().PathNodeList[i].m_nodeY;
            Vector3 pos = new Vector3(x, y, 0);
            Transform trans = LuoboTool.getTransform(parent, i.ToString("D2"));
            if(trans != null)
            {
                trans.localPosition = pos;
            }
            else
            {
                LuoboTool.cloneObj(parent.gameObject, cloneObj.gameObject, pos, i.ToString("D2"));
            }

            movePathNode.Add(new Vector3(x + 40, y - 60, 0));
        }
    }


    void setMonsterStartAnim()
    {
        //根据第0个节点和第1个节点设置怪物出生位置箭头动画;
        int disX = 0;//箭头之间的X间距;
        int disY = 0;//箭头之间的Y间距;
        int rotate = 0;//旋转角度;
        int x0 = DataManager.getInstance().PathNodeList[0].m_nodeX - 480;
        int y0 = 320 - DataManager.getInstance().PathNodeList[0].m_nodeY;
        int x1 = DataManager.getInstance().PathNodeList[1].m_nodeX - 480;
        int y1 = 320 - DataManager.getInstance().PathNodeList[1].m_nodeY;
        int posX = DataManager.getInstance().PathNodeList[0].m_nodeX - 480;//动画X位置;
        int posY = -DataManager.getInstance().PathNodeList[0].m_nodeY + 320;//动画Y位置;

        if ((x1 - x0) != 0)
        {
            disX = (x1 > x0) ? 38 : (-38);
            rotate = (x1 > x0) ? 0 : 180;
            posX += (x1 > x0) ? 57 : (-57);
            posY += -40;
        }
        else if ((y1 - y0) != 0)
        {
            disY = (y1 > y0) ? 38 : (-38);
            rotate = (y1 > y0) ? 90 : (-90);
            posX += 40;
            posY += (y1 > y0) ? 57 : (-57);
        }

        Transform trans = transform.Find("Animation/ArrowAnim");
        if (trans != null)
        {
            //位置调整;
            trans.localPosition = new Vector3(posX, posY, 0);

            for (int i = 1; i <= 3; i++)
            {
                Transform child = trans.Find("arrow_" + i.ToString());
                if (child == null) continue;
                child.localPosition = new Vector3(disX * (i - 1), disY * (i - 1), 0);
                child.Rotate(0, 0, (float)rotate);
            }
        }
    }


    void onMonsterOrObjectClick(GameObject go)
    {
        //播放音效;
        AudioManager.getInstance().Play("Items/ShootSelect.mp3", false);

        CancelInvoke("hidePoint");

        Vector3 pos = go.transform.localPosition;
        Transform parent = go.transform.parent.parent;
        Transform pointTrans = LuoboTool.getTransform(transform, "Aim/Point");
        LuoboTool.changeState(pointTrans, true);
        //包括自身的一半高度;
        Vector2 size = LuoboTool.getSpriteSize(pointTrans);
        pos.y += size.y / 2;

        if(parent.gameObject.name.Equals("ObjectList"))
        {
            //点击的是道具;
            pos += new Vector3(-480, 320, 0);
            pos.x += LuoboTool.getSpriteSize(go.transform).x / 2;
        }
        else if(parent.gameObject.name.Equals("MonsterList"))
        {
            //更新当前指向的怪物序号;
            curPointAtMonTrans = go.transform;
            //点击的是怪物;
            pos.y += LuoboTool.getSpriteSize(go.transform).y / 2;
        }
        //更新位置;
        pointTrans.localPosition = pos;

        //3秒后隐藏;
        Invoke("hidePoint", 3);
    }


    void hidePoint()
    {
        curPointAtMonTrans = null;
        LuoboTool.changeState(transform, "Aim/Point", false);
    }


    void onButtonClick(GameObject go)
    {
        if (go.name.Equals("Map"))
        {
            //播放音效;
            AudioManager.getInstance().Play("Items/SelectFault.mp3", false);

            Vector3 pos = Input.mousePosition;
            pos.x -= Screen.width / 2;
            pos.y -= Screen.height / 2;

            int x = Mathf.Abs((int)pos.x) / 80;
            int y = Mathf.Abs((int)pos.y) / 80;

            float maxX = 80 * ((pos.x > 0) ? (x + 1) : (-x));
            float minX = 80 * ((pos.x > 0) ? x : (-x - 1));
            float maxY = 80 * ((pos.y > 0) ? (y + 1) : (-y));
            float minY = 80 * ((pos.y > 0) ? y : (-y - 1));

            pos.x = (maxX + minX) / 2;
            pos.y = (maxY + minY) / 2;

            LuoboTool.changeState(transform, "Aim/Forbidden", true);
            Transform forbidden = LuoboTool.getTransform(transform, "Aim/Forbidden");
            forbidden.localPosition = pos;
            //重设TweenAlpha;
            TweenAlpha tweenAlpha = forbidden.GetComponent<TweenAlpha>();
            tweenAlpha.ResetToBeginning();
            tweenAlpha.enabled = true;

            return;
        }

        if(go.name.Equals("Luobo"))
        {
            //播放音效;
            AudioManager.getInstance().Play("Items/carrot2.mp3", false);

            //停止播放动画;
            CancelInvoke("playLuoboAnimation");
            
            Transform trans = LuoboTool.getTransform(go.transform, "Rotate/Luobo");
            //停用TweenRotation;
            TweenRotation tweenRotate = trans.GetComponent<TweenRotation>();
            if(tweenRotate != null)
            {
                tweenRotate.enabled = false;
                tweenRotate.ResetToBeginning();
            }
            //更改SpriteAnimation;
            UISpriteAnimation spriteAnim = trans.GetComponent<UISpriteAnimation>();
            if(spriteAnim == null)
            {
                spriteAnim = trans.gameObject.AddComponent<UISpriteAnimation>();
            }
            spriteAnim.enabled = false;
            spriteAnim.namePrefix = "hlb1";
            spriteAnim.loop = false;
            spriteAnim.framesPerSecond = 10;
            spriteAnim.Reset();
            spriteAnim.enabled = true;
            //延时4s;
            InvokeRepeating("playLuoboAnimation", 4, 4);

            return;
        }

        //播放音效;
        AudioManager.getInstance().Play("Items/MenuSelect.mp3", false);
        if(go.name.Equals("Skip"))
        {
            hideTips();
        }
        else if (go.name.Equals("Option"))
        {
            monsterMoveFlag = false;     
            LuoboTool.changeState(transform, "Menu/Options", true);
        }
        else if (go.name.Equals("Continue"))
        {
            monsterMoveFlag = true;
            LuoboTool.changeState(transform, "Menu/Options", false);
        }
        else if (go.name.Equals("Select"))
        {
            DataManager.getInstance().setStageAndLevelData(curStageChoice, curLevelChoice);
            SceneManager.getInstance().openWindow("Scene/SelectStage/UI", 0.1f);
        }
        else if (go.name.Equals("Restart"))
        {
            LuoboTool.changeState(transform, "Menu/Options", false);
            Restart();
        }
        else if(go.name.Equals("Speed"))
        {
            douSpeed = !douSpeed;
        }
        else if(go.name.Equals("Play"))
        {
            isPlaying = !isPlaying;
            monsterMoveFlag = isPlaying;
            LuoboTool.changeState(transform, "Top/Status", !monsterMoveFlag);
            LuoboTool.changeState(transform, "Top/WaveInfo", monsterMoveFlag);

            UIButton button = go.GetComponent<UIButton>();
            button.normalSprite = "pause" + (isPlaying ? "0":"1") + "1";
            button.hoverSprite = "pause" + (isPlaying ? "0" : "1") + "1";
            button.pressedSprite = "pause" + (isPlaying ? "0" : "1") + "2";
        }
    }
    
    
    void onGameOverButtonClick(GameObject go)
    {
        LuoboTool.Log("Name:" + go.name);
        //播放音效;
        AudioManager.getInstance().Play("Items/MenuSelect.mp3", false);
        if(go.name.Equals("Continue"))
        {
            setGameOverUI(false);
            return;
        }
        else if(go.name.Equals("Restart"))
        {
            setGameOverUI(false);
            Restart();
            return;
        }
        else if(go.name.Equals("Select"))
        {
            DataManager.getInstance().setStageAndLevelData(curStageChoice, curLevelChoice);
            SceneManager.getInstance().openWindow("Scene/SelectStage/UI", 0.1f);
            return;
        }
    }
    

    /// <summary>
    /// 显示Ready动画;
    /// </summary>
    void showReadyAnim()
    {
        if(readyCount <= 4)
        {
            LuoboTool.setSprite(transform, "Animation/ReadyAnim/Count", "countdown_" + readyCount.ToString("D2"));
            LuoboTool.changeState(transform, "Animation/ReadyAnim", true);
            readyCount++;
            if(readyCount > 4)
            {
                //播放音效;
                AudioManager.getInstance().Play("Items/GO.mp3", false);
            }
            else if (readyCount > 1)
            {
                AudioManager.getInstance().Play("Items/CountDown.mp3", false);
            }
        }
        else if(readyCount > 4)
        {
            CancelInvoke("showReadyAnim");
            LuoboTool.changeState(transform, "Animation/ReadyAnim", false);
            //停用所有空白位置的Animator组件;

            Transform t = transform.Find("List/SpaceList/Normal");
            setAllSpaceCompEnable(t, false, false, true, "alphaMask", 1);
        }
    }


    void hideArrowAnim()
    {
        LuoboTool.changeState(transform, "Animation/ArrowAnim", false);
        initAllMonster();
    }


    int getHideMonsterIndex()
    {
        for(int index = 0; index < monsterList.Count; index++)
        {
            Transform trans = monsterList[index].monTrans;
            if(!trans.gameObject.activeSelf)
            {
                return index;
            }
        }

        return monsterList.Count;
    }

    IEnumerator showCurrentMonster()
    {
        //当前波怪物总数;
        int count = levelData.m_monsterWave[curWaveIndex - 1].m_monsterList.Count;
        int index = getHideMonsterIndex();
        while (index != monsterList.Count)
        {
            LuoboTool.Log("[showCurrentMonster]index:" + index);
            if (monsterMoveFlag)
            {
                //播放怪物出现音效;
                AudioManager.getInstance().Play("Items/MC.mp3", false);

                LuoboTool.changeState(monsterList[index].monTrans, true);
                monsterMoveNodeIndex.Add(0);

                float time = 1 * (douSpeed ? 0.5f : 1f);
                if (curWaveIndex == levelData.m_waveNum)
                {
                    time *= 2;
                }
                yield return new WaitForSeconds(time);//延时1s;
                index = getHideMonsterIndex();
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    /// <summary>
    /// 获取当前怪物移动方向;
    /// </summary>
    /// <param name="num">当前怪物序号</param>
    /// <returns></returns>
    MonsterDIR getDirection(int num)
    {
        Vector3 dis = movePathNode[monsterMoveNodeIndex[num] + 1] -
            movePathNode[monsterMoveNodeIndex[num]];
        if (dis.x < 0) return MonsterDIR.LEFT;
        else if (dis.x > 0) return MonsterDIR.RIGHT;
        else if (dis.y < 0) return MonsterDIR.DOWN;
        else if (dis.y > 0) return MonsterDIR.UP;
        return MonsterDIR.None;
    }

    /// <summary>
    /// 更新怪物位置，并返回怪物头顶位置;
    /// </summary>
    /// <param name="index">怪物索引</param>
    /// <returns>怪物头顶位置</returns>
    Vector3 updateMonsterPosition(int index)
    {
        Vector3 pos = monsterList[index].monTrans.localPosition;
        //获取怪物ID;
        int monsterID = levelData.m_monsterWave[curWaveIndex - 1].m_monsterList[index];
        //获取怪物行走速度;
        int monsterSpeed = (int)(monsterList[index].info.m_walkSpeed * (douSpeed ? 1.5f : 1));
        MonsterDIR dir = getDirection(index);
        switch (dir)
        {
            case MonsterDIR.None: break;
            case MonsterDIR.LEFT:
                pos.x -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.x <= movePathNode[monsterMoveNodeIndex[index] + 1].x)
                {
                    pos.x = movePathNode[monsterMoveNodeIndex[index] + 1].x;
                    monsterMoveNodeIndex[index]++;
                }
                break;
            case MonsterDIR.RIGHT:
                pos.x += Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.x >= movePathNode[monsterMoveNodeIndex[index] + 1].x)
                {
                    pos.x = movePathNode[monsterMoveNodeIndex[index] + 1].x;
                    monsterMoveNodeIndex[index]++;
                }
                break;
            case MonsterDIR.UP:
                pos.y += Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.y >= movePathNode[monsterMoveNodeIndex[index] + 1].y)
                {
                    pos.y = movePathNode[monsterMoveNodeIndex[index] + 1].y;
                    monsterMoveNodeIndex[index]++;
                }
                break;
            case MonsterDIR.DOWN:
                pos.y -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.y <= movePathNode[monsterMoveNodeIndex[index] + 1].y)
                {
                    pos.y = movePathNode[monsterMoveNodeIndex[index] + 1].y;
                    monsterMoveNodeIndex[index]++;
                }
                break;
        }

        //更新位置;
        monsterList[index].monTrans.localPosition = pos;
        pos.y += LuoboTool.getSpriteSize(monsterList[index].monTrans).y;

        return pos;
    }


    string getMonsterSound(MonsterType type)
    {
        string audioName = "Monsters/";
        int randVal = 0;
        int num = 0;
        switch(type)
        {
            case MonsterType.boss_big:
                audioName += "BigBoss.mp3";
                break;
            case MonsterType.fat_boss_green:
            case MonsterType.fat_green:
                randVal = (Random.Range(0, 100)) % 3;
                audioName += "Fat" + (curStageChoice + 1) + "4" + (randVal + 1) + ".mp3";
                break;
            //case MonsterType.fat_green:
            //    break;
            case MonsterType.fly_blue:
                if (curStageChoice == 1) num = 3;
                else num = 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Fly" + (curStageChoice + 1) + "5" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.fly_boss_blue:
                if (curStageChoice == 1) num = 3;
                else num = 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Fly" + (curStageChoice + 1) + "5" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.fly_boss_yellow:
                randVal = (Random.Range(0, 100)) % 3;
                audioName += "Fly" + (curStageChoice + 1) + "6" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.fly_yellow:
                randVal = (Random.Range(0, 100)) % 3;
                audioName += "Fly" + (curStageChoice + 1) + "6" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_boss_nima:
                num = (curStageChoice == 1) ? 2 : 3;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "3" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_boss_pink:
                if (curStageChoice == 0) num = 3;
                else if (curStageChoice == 1) num = 1;
                else if (curStageChoice == 2) num = 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "1" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_boss_star:
                num = (curStageChoice == 2) ? 3 : 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "2" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_nima:
                num = (curStageChoice == 1) ? 2 : 3;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "3" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_pink:
                if (curStageChoice == 0) num = 3;
                else if (curStageChoice == 1) num = 1;
                else if (curStageChoice == 2) num = 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "1" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.land_star:
                num = (curStageChoice == 2) ? 3 : 2;
                randVal = (Random.Range(0, 100)) % num;
                audioName += "Land" + (curStageChoice + 1) + "2" + (randVal + 1) + ".mp3";
                break;
            case MonsterType.none:
                break;
        }

        return audioName;
    }


    void deleteMonster(int index)
    {
        Vector3 pos = monsterList[index].monTrans.localPosition;

        //重置当前指向的怪物序号;
        if (monsterList[index].monTrans == curPointAtMonTrans)
        {
            CancelInvoke("hidePoint");
            LuoboTool.changeState(transform, "Aim/Point", false);
            curPointAtMonTrans = null;
        }

        //更新AttackList;
        List<int> removeList = new List<int>();
        removeList.Clear();
        for(int i = 0; i < attackList.Count; i++)
        {
            if(attackList[i].monster == monsterList[index].monTrans)
            {
                removeList.Add(i);
            }
        }

        removeListElem(removeList);

        //更改父级Normal为Delete;
        Transform parent = LuoboTool.getTransform(transform, "List/MonsterList/Delete");
        if(parent != null)
        {
            monsterList[index].monTrans.parent = parent;
        }

        LuoboTool.Log("[deleteMonster] REMOVE Monster:" + index);
        monsterMoveNodeIndex.RemoveAt(index);
        monsterList.RemoveAt(index);
    }


    void onCreateTowerMenuClick(GameObject go)
    {
        //播放音效;
        AudioManager.getInstance().Play("Items/MenuSelect.mp3", false);

        if (curSpaceTrans == null)
        {
            LuoboTool.LogError("[onCreateTowerMenuClick]: curSpaceTrans is null!");
            return;
        }

        int type = int.Parse(LuoboTool.getSubstirng(go.name, "_", false));
        int needGold = DataManager.getInstance().TowerList[type - 1].m_updateNeedMoney[0];
        if(curGold > needGold)
        {
            createTower((TowerType)(type - 1), curSpaceTrans.localPosition);
            setGoldShowNum(curGold - needGold);
            Transform trans = curSpaceTrans;
            onSpaceClick(curSpaceTrans.gameObject);
            deleteSpace(trans);
        }
    }

    void createTower(TowerType towerType, Vector3 position)
    {
        Transform parent = LuoboTool.getTransform(transform, "List/TowerList/Normal");
        Transform cloneObj = LuoboTool.getTransform(transform, "List/TowerList/template");
        Transform delList = LuoboTool.getTransform(transform, "List/TowerList/Delete");

        //左上对齐调整为居中对齐;
        Vector3 pos = new Vector3(position.x - 440, position.y + 280, 0);
        //游戏物体名：炮塔ID;
        string objName = ((int)towerType).ToString("D2");

        Transform t = null;
        //从已删除的炮塔集合中拉取一个;
        if(delList.childCount > 0)
        {
            t = delList.GetChild(0);
            LuoboTool.initObj(t, parent, objName, pos);
        }
        else
        {
            GameObject go = LuoboTool.cloneObj(parent.gameObject, cloneObj.gameObject, 
                pos, objName);
            go.SetActive(true);
            t = go.transform;
        }

        //绑定点击响应事件处理;
        UIEventListener eventListener = t.gameObject.GetComponent<UIEventListener>();
        if(eventListener == null)
        {
            eventListener = t.gameObject.AddComponent<UIEventListener>();
        }
        eventListener.onClick = onTowerClick;

        UIAtlas atlas = Resources.Load("Scene/GameScene/Atlas/Tower/T" + towerType.ToString()
             + " Atlas", typeof(UIAtlas)) as UIAtlas;
        //设置炮塔;
        Transform towerImgTrans = LuoboTool.getTransform(t, "Tower/towerImg");
        LuoboTool.setSprite(towerImgTrans, towerType.ToString() + "11", atlas);
        towerImgTrans.localEulerAngles = Vector3.zero;
        LuoboTool.setSprite(t, "Tower/towerBG", towerType.ToString() + "-11", atlas);
        //更改SpriteAnimation;
        UISpriteAnimation spriteAnim = towerImgTrans.GetComponent<UISpriteAnimation>();
        if (spriteAnim == null)
        {
            spriteAnim = towerImgTrans.gameObject.AddComponent<UISpriteAnimation>();
        }
        spriteAnim.namePrefix = towerType.ToString() + "1";
        spriteAnim.framesPerSecond = 6;
        spriteAnim.loop = false;
        spriteAnim.enabled = false;

        //设置炮弹;
        Transform bulTrans = LuoboTool.getTransform(t, "Bullet/Sprite");
        LuoboTool.setSprite(bulTrans, "P" + towerType.ToString() + "11", atlas);
        LuoboTool.changeState(t, "Bullet", false);
        //更改SpriteAnimation;
        spriteAnim = bulTrans.GetComponent<UISpriteAnimation>();
        if(spriteAnim == null)
        {
            spriteAnim = bulTrans.gameObject.AddComponent<UISpriteAnimation>();
        }
        spriteAnim.namePrefix = "P" + towerType.ToString() + "1";
        spriteAnim.framesPerSecond = 6;
        spriteAnim.loop = true;
        spriteAnim.enabled = true;

        //调整位置;
        setTowerPositionOffset(t, towerType, 0);

        //添加创建的炮塔到序列中;
        TowerInfo info = new TowerInfo(t, 0);
        towerList.Add(info);
    }


    void removeAllTower()
    {
        Transform t = LuoboTool.getTransform(transform, "List/TowerList/Normal");
        Transform parent = LuoboTool.getTransform(transform, "List/TowerList/Delete");
        while (t.childCount > 0)
        {
            Transform child = t.GetChild(0);
            child.parent = parent;
        }

        towerList.Clear();
    }


    void createKillAnim(Vector3 pos, int gold, int cloudType = 0)
    {
        Transform parent = LuoboTool.getTransform(transform, "List/KillAnimList/Normal");
        Transform clone = LuoboTool.getTransform(transform, "List/KillAnimList/kill");
        Transform delList = LuoboTool.getTransform(transform, "List/KillAnimList/Delete");

        GameObject go = null;
        if(delList.childCount > 0)
        {
            go = delList.GetChild(0).gameObject;
            LuoboTool.initObj(go.transform, parent, "killAnim", pos, false);
        }
        else
        {
            go = LuoboTool.cloneObj(parent, clone, pos, "killAnim");
            go.SetActive(false);
        }

        Transform cloudTrans = LuoboTool.getTransform(go.transform, "Cloud");
        UISpriteAnimation spriteAnim = cloudTrans.GetComponent<UISpriteAnimation>();
        if(spriteAnim != null)
        {
            spriteAnim.namePrefix = "air" + cloudType.ToString();
            spriteAnim.Reset();
            spriteAnim.enabled = true;
        }
        Transform goldTrans = LuoboTool.getTransform(go.transform, "Gold");
        LuoboTool.setSprite(goldTrans, "money" + gold.ToString("D2"));
        TweenPosition tweenPos = goldTrans.GetComponent<TweenPosition>();
        if(tweenPos != null)
        {
            EventDelegate.Add(tweenPos.onFinished, killAnimTweenFinished);
            tweenPos.ResetToBeginning();
            tweenPos.enabled = true;
        }

        go.SetActive(true);

        //更新金币;
        setGoldShowNum(curGold + gold);
    }


    void killAnimTweenFinished()
    {
        GameObject go = TweenPosition.current.gameObject;
        Transform t = go.transform.parent;
        Transform parent = LuoboTool.getTransform(t.parent.parent, "Delete");
        LuoboTool.initObj(t, parent, "killAnim", Vector3.zero);

        //移除关联;
        EventDelegate.Remove(TweenPosition.current.onFinished, killAnimTweenFinished);
    }


    /// <summary>
    /// 根据类型调整炮塔背景和图片位置;
    /// </summary>
    /// <param name="towerTrans">炮塔Transform</param>
    /// <param name="type">炮塔类型</param>
    /// <param name="level">炮塔等级</param>
    void setTowerPositionOffset(Transform towerTrans, TowerType type, int level)
    {
        Transform towerImgTrans = LuoboTool.getTransform(towerTrans, "Tower/towerImg");
        Transform towerBgTrans = LuoboTool.getTransform(towerTrans, "Tower/towerBG");
        Transform updateTrans = LuoboTool.getTransform(towerTrans, "Tower/Update");
        //初始化位置;
        towerImgTrans.localPosition = Vector3.zero;
        towerBgTrans.localPosition = Vector3.zero;
        updateTrans.localPosition = Vector3.zero;
        switch(type)
        {
            case TowerType.Ball: 
                break;
            case TowerType.Bottle:
                towerImgTrans.localPosition = new Vector3(-4, 4, 0);
                updateTrans.localPosition = new Vector3(-4, 50, 0);
                break;
            case TowerType.Fan: 
                break;
            case TowerType.Shit:
                towerBgTrans.localPosition = new Vector3(1, -12 + level * (-4), 0);
                updateTrans.localPosition = new Vector3(1, 50, 0);
                break;
            case TowerType.Star: 
                break;
        }
    }

    void deleteSpace(Transform trans)
    {
        if(trans == null)return;
        Transform parent = LuoboTool.getTransform(transform, "List/SpaceList/Delete");
        trans.parent = parent;
    }


    void createSpace(Vector3 towerPos)
    {
        Transform delList = LuoboTool.getTransform(transform, "List/SpaceList/Delete");
        Transform parent = LuoboTool.getTransform(transform, "List/SpaceList/Normal");
        Transform cloneTrans = LuoboTool.getTransform(transform, "List/SpaceList/Anim");
        Transform t = null;
        Vector3 pos = towerPos + new Vector3(440, -280, 0);
        string spaceName = pos.x + "_" + pos.y;

        //从已删除的空白位置集合中拉取一个;
        if(delList.childCount > 0)
        {
            t = delList.GetChild(0);
            LuoboTool.initObj(t, parent, spaceName, pos);
        }
        else
        {
            GameObject go = LuoboTool.cloneObj(parent, cloneTrans, pos, spaceName);
            t = go.transform;

            //绑定点击事件处理;
            UIEventListener eventListener = go.GetComponent<UIEventListener>();
            if (eventListener == null)
            {
                eventListener = go.AddComponent<UIEventListener>();
            }
            eventListener.onClick = onSpaceClick;
        }

        //设置组件状态;
        setSpaceCompEnable(t, false, false, true, "alphaMask", 1);
    }

    /// <summary>
    /// 更新炮塔;
    /// </summary>
    /// <param name="isUpdate">更新/销毁</param>
    void updateTower(bool isUpdate)
    {
        //播放音效;
        string audioName = ((isUpdate) ? "TowerUpdata" : "TowerSell") + ".mp3";
        AudioManager.getInstance().Play("Items/" + audioName, false);

        int towerIndex = getTowerIndex(curTowerTrans);
        Tower info = getTowerInfo(towerIndex);
        int towerID = info.m_towerID;
        TowerType towerType = (TowerType)info.m_towerID - 1;
        string name = (towerID - 1).ToString("D2");
        LuoboTool.Log("[updateTower]:name->" + name + ", towerType->" + towerType);

        //更新保存的炮塔信息;
        Transform tower = towerList[towerIndex].tower;
        if (isUpdate && info.m_level < 2)
        {
            towerList[towerIndex].towerLevel += 1;
            //更新炮塔状态;

            LuoboTool.Log("[updateTower]: List/TowerList/Normal/" + name);

            //更新炮塔;
            Transform towerImgTrans = LuoboTool.getTransform(tower, "Tower/towerImg");
            LuoboTool.setSprite(towerImgTrans, towerType.ToString() + (towerList[towerIndex].towerLevel + 1) + "1");
            
            //更新炮塔SpriteAnimation组件;
            UISpriteAnimation spriteAnim = towerImgTrans.GetComponent<UISpriteAnimation>();
            if (spriteAnim == null)
            {
                spriteAnim = towerImgTrans.gameObject.AddComponent<UISpriteAnimation>();
            }
            spriteAnim.namePrefix = towerType.ToString() + (towerList[towerIndex].towerLevel + 1);
            spriteAnim.framesPerSecond = 6;
            spriteAnim.loop = false;
            spriteAnim.enabled = false;

            //更新炮弹;
            Transform bulTrans = LuoboTool.getTransform(tower, "Bullet/Sprite");
            LuoboTool.setSprite(bulTrans, "P" + towerType.ToString() +
                (towerList[towerIndex].towerLevel + 1) + "1");
            //更新炮弹SpriteAnimation组件;
            spriteAnim = bulTrans.GetComponent<UISpriteAnimation>();
            if (spriteAnim == null)
            {
                spriteAnim = bulTrans.gameObject.AddComponent<UISpriteAnimation>();
            }
            spriteAnim.namePrefix = "P" + towerType.ToString() + (towerList[towerIndex].towerLevel + 1);
            spriteAnim.framesPerSecond = 6;
            spriteAnim.loop = true;
            spriteAnim.enabled = true;

            setTowerPositionOffset(tower, towerType, towerList[towerIndex].towerLevel);

            onTowerClick(tower.gameObject);
        }
        else
        {
            //更改炮塔父级;
            Transform parent = tower.parent.parent.Find("Delete");
            tower.parent = parent;
            int index = towerIndex;

            //隐藏菜单;
            onTowerClick(tower.gameObject);
            //创建对应的空白位置;
            createSpace(tower.localPosition);

            //移除对应的攻击关系;
            List<int> removeList = new List<int>();
            removeList.Clear();
            for (int i = 0; i < attackList.Count; i++)
            {
                if (attackList[i].tower == tower)
                {
                    removeList.Add(i);
                }
            }

            removeListElem(removeList);

            //移除炮塔信息;
            towerList.RemoveAt(index);
        }
    }


    void updateTowerMenu()
    {
        if (curTowerTrans == null) return;
        Tower tower = getTowerInfo(curTowerTrans);
        int updateNeedMoney = (tower.m_level < 2) ? tower.m_updateNeedMoney[tower.m_level + 1] : -1;
        int destroyGetMoney = tower.m_destroyGetMoney[tower.m_level];

        if (updateNeedMoney == -1)
        {
            LuoboTool.setSprite(transform, "Menu/TowerMenu/Update", "upgrade_0_CN");
        }
        else
        {
            LuoboTool.setSprite(transform, "Menu/TowerMenu/Update",
            "upgrade_" + ((curGold >= updateNeedMoney) ? "" : "-") + updateNeedMoney);
        }
        LuoboTool.setSprite(transform, "Menu/TowerMenu/Delete", "sell_" + destroyGetMoney);
    }


    void onTowerMenuClick(GameObject go)
    {
        UISprite sprite = LuoboTool.getSprite(go.transform);
        string spriteName = sprite.spriteName;
        LuoboTool.Log("spriteName:" + spriteName);
        //顶级;
        if(spriteName.Equals("upgrade_0_CN"))
        {
            return;
        }

        int index = spriteName.IndexOf("_");
        int index1 = spriteName.IndexOf("-");
        if(go.name.Equals("Update"))
        {
            //未找到-，说明可以升级;
            if(index1 == -1)
            {
                int needGold = int.Parse(spriteName.Substring(index + 1));
                LuoboTool.Log("needGold:" + needGold);
                setGoldShowNum(curGold - needGold);

                updateTower(true);
            }
            else
            {//金币不够，不能升级;

            }
        }
        else if (go.name.Equals("Delete"))
        {
            int getGold = int.Parse(spriteName.Substring(index + 1));
            setGoldShowNum(curGold + getGold);

            updateTower(false);
        }
    }


    void onTowerClick(GameObject go)
    {
        setMenuVisible(1, go);
    }

    void onSpaceClick(GameObject go)
    {
        setMenuVisible(0, go);
    }

    void setMenuVisible(int type, GameObject go)
    {
        int index = -1;
        Vector3 pos = go.transform.localPosition;

        Transform createTrans = LuoboTool.getTransform(transform, "Menu/CreateTowerMenu");
        if (createTrans == null) return;
        Transform towerMenuTrans = LuoboTool.getTransform(transform, "Menu/TowerMenu");
        if (towerMenuTrans == null) return;

        if (curSpaceTrans == null && curTowerTrans == null)//当前未点击任何空白位置或炮塔;
        {
            //播放音效;
            AudioManager.getInstance().Play("Items/TowerSelect.mp3", false);

            if (0 == type)//空白位置点击;
            {
                //边界判定;
                float x = pos.x - 480;
                float y = pos.y + 320;
                int count = levelData.m_towerList.Count;
                int towerListWidth = ((count >= 4) ? 4 : (count % 4)) * 90;
                int towerListHeight = ((count / 4) + 1) * 90;
                if (x + 40 + towerListWidth / 2 >= 480)
                {
                    x += 80 - towerListWidth / 2;
                }
                else if (x + 40 - towerListWidth / 2 <= -480)
                {
                    x += towerListWidth / 2;
                }
                else
                {
                    x += 40;
                }
                if (y >= 40)
                {
                    y -= 80 + towerListHeight / 2;
                }
                else if (y < 40)
                {
                    y += towerListHeight / 2;
                }
                //调整菜单位置;
                createTrans.localPosition = new Vector3(x, y, 1);

                //显示菜单;
                LuoboTool.changeState(createTrans, true);
                
                Transform t = LuoboTool.getTransform(createTrans, "Grid");
                setTweenScaleState(t);

                //激活相应组件;
                setSpaceCompEnable(go.transform, false, true, true, "", 1);
                
                //更新当前选择;
                curSpaceTrans = go.transform;
            }
            else if (1 == type)//炮塔位置点击;
            {
                //显示菜单;
                LuoboTool.changeState(towerMenuTrans, true);

                index = getTowerIndex(go.transform);

                //更新当前选择;
                curTowerTrans = go.transform;

                //更新当前炮塔升级需要的金钱以及销毁将获得的金钱;
                updateTowerMenu();
                
                towerMenuTrans.localPosition = pos;
                setTweenScaleState(towerMenuTrans);
            }
        }
        else if (curSpaceTrans != null)
        {
            //播放音效;
            AudioManager.getInstance().Play("Items/TowerDeselect.mp3", false);

            Transform t = LuoboTool.getTransform(createTrans, "Grid");
            setTweenScaleState(t);
            setSpaceCompEnable(curSpaceTrans, false, false, true, "alphaMask", 1);
            curSpaceTrans = null;
        }
        else if(curTowerTrans != null)
        {
            //播放音效;
            AudioManager.getInstance().Play("Items/TowerDeselect.mp3", false);
            curTowerTrans = null;
            setTweenScaleState(towerMenuTrans);
        }
    }
    
    ///<summary>
    ///设置游戏结束界面;
    ///</summary>
    void setGameOverUI(bool isShow, bool isPass = true)
    {
        LuoboTool.changeState(transform, "GameOver", isShow);
        if(!isShow)return;
        //设置游戏结束界面数字;
        Transform numTrans = LuoboTool.getTransform(transform, "GameOver/Num");
        numTrans.localPosition = new Vector3(0, (isPass) ? 0 : 4, 0);
        string[] path = {"allWave_", "curWave_", "curLevel_"};
        int[] num = {levelData.m_waveNum, curWaveIndex, curLevelChoice + 1};
        for(int i = 0; i < num.Length; i++)
        {
            //设置左边数字;
            LuoboTool.setSprite(numTrans, path[i] + "Left",
                "numyellow-hd_" + (num[i] / 10 + 3).ToString("D2"));
            //设置右边数字;
            LuoboTool.setSprite(numTrans, path[i] + "Right",
                "numyellow-hd_" + (num[i] % 10 + 3).ToString("D2"));
        }
        
        //设置其他图片状态;
        LuoboTool.changeState(transform, "GameOver/Pass", isPass);
        LuoboTool.changeState(transform, "GameOver/Fail", !isPass);
        LuoboTool.changeState(transform, "GameOver/Buttons/Continue", isPass);
        LuoboTool.changeState(transform, "GameOver/Buttons/Select", !isPass);
    }


    void setTweenScaleState(Transform t)
    {
        bool state = true;
        if (t == null) return;
        Vector3 scale = t.localScale;
        
        if(scale == Vector3.one)
        {//1,1,1->0.01,0.01,0.01;
            state = false;
        }
        else
        {//0.01,0.01,0.01->1,1,1;
            t.localScale = Vector3.one * 0.01f;
        }

        TweenScale tweenScale = t.gameObject.GetComponent<TweenScale>();
        if(tweenScale != null)
        {
            Destroy(tweenScale);
        }
        tweenScale = t.gameObject.AddComponent<TweenScale>();
        tweenScale.from = Vector3.one * ((state) ? 0.01f : 1);
        tweenScale.to = Vector3.one * ((state) ? 1 : 0.01f);
        tweenScale.duration = 0.15f;
        tweenScale.ignoreTimeScale = true;
        tweenScale.delay = 0;
        EventDelegate.Add(tweenScale.onFinished, tweenScaleFinished);
        tweenScale.enabled = true;
    }

    void tweenScaleFinished()
    {
        if(curSpaceTrans == null)
        {
            LuoboTool.changeState(transform, "Menu/CreateTowerMenu", false);
        }
        else if(curTowerTrans == null)
        {
            LuoboTool.changeState(transform, "Menu/TowerMenu", false);
        }
    }


    bool getTowerRotateFlag(TowerType type)
    {
        bool flag = false;
        switch(type)
        {
            case TowerType.Bottle:
                flag = true;
                break;
            case TowerType.Ball: 
            case TowerType.Fan: 
            case TowerType.Shit:
            case TowerType.Star: 
                flag = false;
                break;
        }

        return flag;
    }
}
