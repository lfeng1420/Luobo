using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class StageData
{
    public bool m_lockState;//是否锁定;
    public int m_levelNum;//关卡数量;
    public int m_unlockLevel;
    public List<LevelData> m_levelInfo;//关卡信息;

    public StageData(bool lockState = true, int levelNum = 0,
        int unlockLevel = 0, List<LevelData> levelInfo = null)
    {
        m_lockState = lockState;
        m_levelNum = levelNum;
        m_unlockLevel = unlockLevel;
        m_levelInfo = levelInfo;
    }
}

public class LevelData
{
    public bool m_lockState;//锁定状态;
    public int m_colorType;//颜色类型;
    public bool m_allClearMedal;//所有道具全部清除奖牌;
    public int m_luoboMedal;//萝卜奖牌等级;
    public int m_waveNum;//怪物波数;
    public List<int> m_lifeImprove;//每一波怪物提升的生命值;
    public List<int> m_towerList;//可用塔序号列表;
    public List<MonsterWave> m_monsterWave;//每一波对应的怪物序列;

    public LevelData(bool lockState, int colorType, bool allClearMedal, int luoboMedal,
        int waveNum, List<int> lifeImprove, List<int> towerList, List<MonsterWave> monsterWave)
    {
        m_lockState = lockState;
        m_colorType = colorType;
        m_allClearMedal = allClearMedal;
        m_luoboMedal = luoboMedal;
        m_waveNum = waveNum;

        m_lifeImprove = lifeImprove;
        m_towerList = towerList;
        m_monsterWave = monsterWave;
    }
}

public class MonsterWave
{
    public List<int> m_monsterList;//每一波对应的怪物ID序列;

    public MonsterWave(List<int> monsterList = null)
    {
        m_monsterList = monsterList;
        /*m_monsterList = new List<Monster>();
        Monster monster = new Monster();
        for(int i = 0; i < monsterList.Count; i++)
        {
            Monster.setData(ref monster, monsterList[i]);
            m_monsterList.Add(monster);
        }*/
    }
}

public class Monster
{
    public int m_monsterLife;//怪物生命值;
    public int m_walkSpeed;//怪物行走速度;

    public Monster(int monsterLife = 0, int walkSpeed = 0)
    {
        m_monsterLife = monsterLife;
        m_walkSpeed = walkSpeed;
    }
}

public class Tower
{
    public int m_towerID;//炮塔ID;
    public int m_level;//塔等级;
    public AttackType m_attackType;//攻击类型;
    public float m_attackRadius;//攻击半径;
    public List<float> m_attackCD;//每一级对应的攻击等待时间;
    public List<int> m_attackValue;//每一级对应的伤害值;
    public List<int> m_updateNeedMoney;//每升一级需要的金钱;
    public List<int> m_destroyGetMoney;//销毁可获得的金钱;
    public AddtionalEffect m_effectType;//附加效果类型;
    public List<int> m_effectValue;//每一级对应的附加效果值;

    public Tower(int towerID = 0, int level = 0, AttackType attackType = AttackType.One,
        float attackRadius = 0f, List<float> attackCD = null,
        List<int> value = null, List<int> gold = null, List<int> getGold = null,
        AddtionalEffect effectType = AddtionalEffect.Slow,
        List<int> effectValue = null)
    {
        m_towerID = towerID;
        m_level = level;
        m_attackType = attackType;
        m_attackRadius = attackRadius;
        m_attackCD = attackCD;
        m_attackValue = value;
        m_updateNeedMoney = gold;
        m_destroyGetMoney = getGold;
        m_effectType = effectType;
        m_effectValue = effectValue;
    }
}

public enum AttackType
{
    One,//一个;
    Multiple,//多个;
    //Line,//直线;
}

public enum AddtionalEffect
{
    None,
    Slow,//减慢速度;
    Frozen,//冰冻，暂停移动;
    Poison,//毒，附加伤害;
}


public class SpaceNode
{
    public int m_pointX;
    public int m_pointY;
    public int m_spaceWidth;
    public int m_spaceHeight;

    public SpaceNode(int x = 0, int y = 0, int w = 0, int h = 0)
    {
        m_pointX = x;
        m_pointY = y;
        m_spaceWidth = w;
        m_spaceHeight = h;
    }

    public void Log()
    {
        LuoboTool.Log("pointX:" + m_pointX + ", pointY:" + m_pointY + ", spaceWidth:" +
            m_spaceWidth + ", spaceHeight:" + m_spaceHeight);
    }
}


/// <summary>
/// 怪物移动路径节点;
/// </summary>
public class MonsterMovePathNode
{
    public int m_nodeX;
    public int m_nodeY;

    public MonsterMovePathNode(int _x = 0, int _y = 0)
    {
        m_nodeX = _x;
        m_nodeY = _y;
    }

    public void Log()
    {
        LuoboTool.Log("nodeX:" + m_nodeX + ", nodeY:" + m_nodeY);
    }
}

/// <summary>
/// 场景道具;
/// </summary>
public class SomeObject
{
    public int m_pointX;
    public int m_pointY;
    public int m_objWidth;
    public int m_objHeight;
    public ObjectType m_objType;//道具类型;
    public int m_lifeValue;

    public SomeObject(int _x = 0, int _y = 0, int w = 0, int h = 0,
        ObjectType type = ObjectType.None, int life = 0)
    {
        m_pointX = _x;
        m_pointY = _y;
        m_objWidth = w;
        m_objHeight = h;
        m_objType = type;
        m_lifeValue = life;
    }

    public void Log()
    {
        LuoboTool.Log("pointX:" + m_pointX + ", pointY:" + m_pointY + ", objWidth:" +
            m_objWidth + ", objHeight:" + m_objHeight + ", objType:" + m_objType +
            ", lifeValue" + m_lifeValue);
    }
}

public enum ObjectType
{
    None,
    SmallClone,//小云;
    Other,
    Planet,//行星;
    Rainbow,//彩虹;
    BigClone,//大云;
    Other2,
    Balloon,//气球;
}

public enum ObjectColor
{
    None,
    White, //白色;
    Purple,//紫色;
}

public enum TowerType
{
    Bottle,
    Shit,
    Fan,
    Star,
    Ball,
}

public enum MonsterType
{
    none,
    fat_boss_green,
    boss_big,
    fat_green,
    fly_boss_blue,
    fly_blue,
    fly_boss_yellow,
    fly_yellow,
    land_boss_nima,
    land_nima,
    land_boss_pink,
    land_pink,
    land_boss_star,
    land_star,
}



/// <summary>
/// ----------------------------------------------
/// </summary>



public class MonsterInfo
{
    public Transform monTrans = null;
    public Monster monInfo = null;
    public int curNodeIndex = -1;
    public MonsterInfo(Transform _trans, Monster _info, int _index)
    {
        monTrans = _trans;
        monInfo = _info;
        curNodeIndex = _index;
    }
}


public class TowerInfo
{
    public Transform towerTrans = null;
    public int towerLevel = -1;
    public TowerInfo(Transform _tower, int _level)
    {
        towerTrans = _tower;
        towerLevel = _level;
    }
}

public class AttackInfo
{
    public int towerIndex = -1;
    public int monIndex = -1;
    public RotateAngle rotateAngle = null;
    public bool readyAttackFlag = false;
    public float deltaTime = 0f;
    public AttackInfo(int _tower, int _monster)
    {
        towerIndex = _tower;
        monIndex = _monster;
        readyAttackFlag = false;
        deltaTime = 0;
    }
}

public class RotateAngle
{
    public float curAngle = 0f;
    public float allAngle = 0f;

    public RotateAngle(float _cur, float _all)
    {
        curAngle = _cur;
        allAngle = _all;
    }
}