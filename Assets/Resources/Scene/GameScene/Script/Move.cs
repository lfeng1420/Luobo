using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Move : MonoBehaviour 
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


    public List<Vector3> pathNodeList = null;

    public List<MonsterInfo> monsterList = null;

    public bool moveFlag = false;

    public int eatCount = 0;

	// Use this for initialization
	void Start () 
    {
        monsterList = new List<MonsterInfo>();
        monsterList.Clear();
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(moveFlag)
        {
            for(int i = 0; i < monsterList.Count; i++)
            {
                bool flag = updatePos(i);
                if(flag)
                {
                    removeMonster(i);
                    eatCount++;
                }
            }
        }
	}

    bool updatePos(int index)
    {
        Vector3 pos = monsterList[index].monTrans.localPosition;
        //获取怪物速度;
        int monsterSpeed = monsterList[index].monInfo.m_walkSpeed;
        MonsterDIR dir = getDirection(index);
        switch (dir)
        {
            case MonsterDIR.None: break;
            case MonsterDIR.LEFT:
                pos.x -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.x <= pathNodeList[monsterList[index].curNodeIndex + 1].x)
                {
                    pos.x = pathNodeList[monsterList[index].curNodeIndex + 1].x;
                    monsterList[index].curNodeIndex++;
                }
                break;
            case MonsterDIR.RIGHT:
                pos.x += Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.x >= pathNodeList[monsterList[index].curNodeIndex + 1].x)
                {
                    pos.x = pathNodeList[monsterList[index].curNodeIndex + 1].x;
                    monsterList[index].curNodeIndex++;
                }
                break;
            case MonsterDIR.UP:
                pos.y += Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.y >= pathNodeList[monsterList[index].curNodeIndex + 1].y)
                {
                    pos.y = pathNodeList[monsterList[index].curNodeIndex + 1].y;
                    monsterList[index].curNodeIndex++;
                }
                break;
            case MonsterDIR.DOWN:
                pos.y -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                if (pos.y <= pathNodeList[monsterList[index].curNodeIndex + 1].y)
                {
                    pos.y = pathNodeList[monsterList[index].curNodeIndex + 1].y;
                    monsterList[index].curNodeIndex++;
                }
                break;
        }

        //更新位置;
        monsterList[index].monTrans.localPosition = pos;
        return monsterList[index].curNodeIndex == pathNodeList.Count - 2;
    }


    /// 获取当前怪物移动方向;
    /// </summary>
    /// <param name="num">当前怪物序号</param>
    /// <returns></returns>
    MonsterDIR getDirection(int index)
    {
        Vector3 dis = pathNodeList[monsterList[index].curNodeIndex + 1] -
            pathNodeList[monsterList[index].curNodeIndex];
        if (dis.x < 0) return MonsterDIR.LEFT;
        else if (dis.x > 0) return MonsterDIR.RIGHT;
        else if (dis.y < 0) return MonsterDIR.DOWN;
        else if (dis.y > 0) return MonsterDIR.UP;
        return MonsterDIR.None;
    }

    void removeMonster(int index)
    {
        Transform monster = monsterList[index].monTrans;
        Transform parent = LuoboTool.getTransform(transform, "List/MonsterList/Delete");
        monster.parent = parent;

        monsterList.RemoveAt(index);

        //通知GameScene脚本移除monster;
    }

    /// <summary>
    /// 根据怪物Transform从队列中移除;
    /// </summary>
    /// <param name="monTrans">怪物Transform</param>
    public void removeMonster(Transform monTrans)
    {
        for(int i = 0; i < monsterList.Count; i++)
        {
            if(monsterList[i].monTrans == monTrans)
            {
                removeMonster(i);
                break;
            }
        }
    }
}
