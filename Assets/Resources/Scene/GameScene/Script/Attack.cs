using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attack : MonoBehaviour 
{
    public List<TowerInfo> towerList = null;

    public List<MonsterInfo> monsterList = null;

    /// <summary>
    /// 攻击序列;
    /// </summary>
    public List<AttackInfo> attackList = null;

    /// <summary>
    /// 攻击标志;
    /// </summary>
    public bool attackFlag = false;
    
	// Use this for initialization
	void Start () 
    {
        towerList = new List<TowerInfo>();
        towerList.Clear();

        monsterList = new List<MonsterInfo>();
        monsterList.Clear();

        attackList = new List<AttackInfo>();
        attackList.Clear();

        //使用相同的怪物序列;
        Move move = transform.GetComponent<Move>();
        if(move != null)
        {
            move.monsterList = monsterList;
        }
	}
	
    /// <summary>
    /// 添加攻击关系到序列中;
    /// </summary>
    /// <param name="tower">炮塔信息</param>
    /// <param name="monster">怪物信息</param>
    void addAttack(int towerIndex, int monIndex)
    {
        AttackInfo attackInfo = new AttackInfo(towerIndex, monIndex);
        attackInfo.readyAttackFlag = false;
        attackInfo.deltaTime = 0;
        float allAngle = calcRotateAngle(towerList[towerIndex].towerTrans.localPosition,
            monsterList[monIndex].monTrans.localPosition);
        attackInfo.rotateAngle = new RotateAngle(0, allAngle);
        attackList.Add(attackInfo);
        LuoboTool.Log("[Attack] ADD:" + towerList[towerIndex].towerTrans.gameObject.name + ", "
            + monsterList[monIndex].monTrans.gameObject.name);
    }

    /// <summary>
    /// 更新攻击序列;
    /// </summary>
    void updateAttack()
    {
        bool[] isExist = new bool[towerList.Count];
        for(int i = 0; i < towerList.Count; i++)
        {
            isExist[i] = false;
        }

        //检查是否有无效的攻击关系;
        List<int> removeList = new List<int>();
        removeList.Clear();

        for(int i = 0; i < attackList.Count; i++)
        {
            Transform monTrans = monsterList[attackList[i].monIndex].monTrans;
            Transform towerTrans = towerList[attackList[i].towerIndex].towerTrans;
            Vector3 mPos = monTrans.localPosition;
            Vector3 tPos = towerTrans.localPosition;
            float radius = getTowerInfo(towerTrans.gameObject.name).m_attackRadius;
            float dis = (mPos.x - tPos.x) * (mPos.x - tPos.x) + (mPos.y - tPos.y) * (mPos.y - tPos.y);

            if (dis > radius * radius)
            {
                removeList.Add(i);
            }
        }

        for(int i = 0; i < removeList.Count; i++)
        {
            int ix = removeList[i] - i;
            removeAttack(ix);
        }

        for (int i = 0; i < attackList.Count; i++ )
        {
            isExist[attackList[i].towerIndex] = true;
        }

        for (int i = 0; i < towerList.Count; i++)
        {
            Tower towerInfo = getTowerInfo(towerList[i].towerTrans.gameObject.name);
            AttackType type = towerInfo.m_attackType;
            if (type == AttackType.One && isExist[i]) continue;
            
            //获取在攻击范围内的最短距离的怪物序号;
            int monIndex = getLowestDisMonsterIndex(towerList[i].towerTrans.localPosition, 
                towerInfo.m_attackRadius);
            if (monIndex == -1) continue;

            //添加到攻击序列中;
            addAttack(i, monIndex);
        }
    }


    int getLowestDisMonsterIndex(Vector3 pos, float radius)
    {
        int index = -1;
        float lowestDis = radius;
        for(int i = 0; i < monsterList.Count; i++)
        {
            Vector3 target = pos - monsterList[i].monTrans.localPosition;
            float dis = target.x * target.x + target.y * target.y;
            if (dis * dis <= lowestDis) continue;
            lowestDis = dis * dis;
            index = i;
        }

        return index;
    }

    void removeMonster(Transform monster = null)
    {
        int index = -1;
        List<int> removeList = new List<int>();
        removeList.Clear();
        for(int i = 0; i < attackList.Count; i++)
        {
            if (monsterList[attackList[i].monIndex].monTrans == monster)
            {
                index = attackList[i].monIndex;
                removeList.Add(i);
            }
        }

        for(int i = 0; i < removeList.Count; i++)
        {
            int ix = removeList[i] - i;
            removeAttack(ix);
        }

        monsterList.RemoveAt(index);
    }


    void removeTower(Transform tower = null)
    {
        List<int> removeList = new List<int>();
        removeList.Clear();
        for (int i = 0; i < attackList.Count; i++)
        {
            if (towerList[attackList[i].towerIndex].towerTrans == tower)
            {
                removeList.Add(i);
            }
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            int ix = removeList[i] - i;
            removeAttack(ix);
        }
    }

    void removeAttack(int index)
    {
        string towerName = towerList[attackList[index].towerIndex].towerTrans.gameObject.name;
        TowerType towerType = (TowerType)(int.Parse(towerName));

        //隐藏怪物可能存在的附加效果;
        LuoboTool.changeState(monsterList[attackList[index].monIndex].monTrans, "Effect/" + towerType.ToString(), false);
        LuoboTool.changeState(monsterList[attackList[index].monIndex].monTrans, "Effect/" + towerType.ToString() + "1", false);
        LuoboTool.changeState(monsterList[attackList[index].monIndex].monTrans, "Blood", false);

        LuoboTool.Log("[Attack] Remove:" + towerName + ", " + monsterList[attackList[index].monIndex].monTrans.gameObject.name);
        attackList.RemoveAt(index);
    }


	// Update is called once per frame
	void Update () 
    {
	    if(attackFlag)
        {
            updateAttack();

        }
	}

    /// <summary>
    /// 旋转炮塔;
    /// </summary>
    void rotateTower(int index)
    {
        if (attackList[index].readyAttackFlag) return;
        Transform t = towerList[attackList[index].towerIndex].towerTrans;
        Transform m = monsterList[attackList[index].monIndex].monTrans;
        float angle = calcRotateAngle(t.localPosition, m.localPosition);
        
    }

    /// <summary>
    /// 计算需要旋转的角度;
    /// </summary>
    /// <param name="src">源位置</param>
    /// <param name="dest">目标位置</param>
    /// <returns>所需旋转的角度</returns>
    float calcRotateAngle(Vector3 src, Vector3 dest)
    {
        Vector3 target = dest - src;
        float angle = Mathf.Rad2Deg * Mathf.Atan(target.y / target.x);
        angle += (target.x >= 0) ? -90 : 90;
        return angle;
    }

    Tower getTowerInfo(string towerName)
    {
        int type = int.Parse(towerName);
        return DataManager.getInstance().TowerList[type];
    }


}
