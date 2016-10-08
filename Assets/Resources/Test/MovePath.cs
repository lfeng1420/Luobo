using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovePath : MonoBehaviour 
{

    public enum MonsterDIR
    {
        None,
        UP,
        DOWN,
        LEFT,
        RIGHT,
    }

    /// <summary>
    /// 当前路径节点索引;
    /// </summary>
    int[] index = null;

    /// <summary>
    /// 速度减慢标志;
    /// </summary>
    public bool[] isDown = null;

    public bool moveFlag = false;

    List<Transform> monsterList = null;

    Transform attack = null;

    float delta = 0f;

    /// <summary>
    /// 路径节点序列;
    /// </summary>
    List<Vector3> pathNode;

    Transform towerTrans = null;
    Transform tBulletTrans = null;

    Vector3 towerPos = Vector3.zero;

	// Use this for initialization
	void Start () 
    {
        index = new int[transform.childCount];
        isDown = new bool[transform.childCount];

        pathNode = new List<Vector3>();
        pathNode.Clear();

        Transform trans = transform;
        Transform parent = transform.parent;

        //获取路径节点;
        if (parent != null)
        {
            trans = LuoboTool.getTransform(parent, "PathNodeList");
            if(trans != null)
            {
                for(int i = 0; i < 8; i++)
                {
                    Transform child = LuoboTool.getTransform(trans, i.ToString("D2"));
                    if(child != null)
                    {
                        pathNode.Add(child.localPosition);
                    }
                }
            }

            towerTrans = LuoboTool.getTransform(parent, "TowerList/01");
            if(towerTrans != null)
            {
                tBulletTrans = LuoboTool.getTransform(towerTrans, "Bullet");
                towerPos = towerTrans.localPosition;
            }
        }

        monsterList = new List<Transform>();
        monsterList.Clear();
        for(int i = 0; i < transform.childCount; i++)
        {
            isDown[i] = false;
            index[i] = 0;
            trans = transform.Find("Monster" + (i + 1).ToString("D2"));
            if(trans != null)
            {
                trans.localPosition = pathNode[0];
                monsterList.Add(trans);
            }
        }
	}


    MonsterDIR getDirection(int num)
    {
        if (index[num] == 7) return MonsterDIR.None;
        Vector3 dis = pathNode[index[num] + 1] - pathNode[index[num]];
        if (dis.x < 0) return MonsterDIR.LEFT;
        else if (dis.x > 0) return MonsterDIR.RIGHT;
        else if (dis.y < 0) return MonsterDIR.DOWN;
        else if (dis.y > 0) return MonsterDIR.UP;
        return MonsterDIR.None;
    }

    
	// Update is called once per frame
	void Update () 
    {
        if (moveFlag)
        {
            moveFlag = false;
            for(int i = 0; i < monsterList.Count; i++)
            {
                if (index[i] >= 7) continue;
                moveFlag = true;
                int monsterSpeed = 50;
                if(isDown[i])
                {
                    monsterSpeed = 20;
                }

                Vector3 pos = monsterList[i].localPosition;
                MonsterDIR dir = getDirection(i);
                switch (dir)
                {
                    case MonsterDIR.None: return;
                    case MonsterDIR.LEFT:
                        pos.x -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                        if (pos.x <= pathNode[index[i] + 1].x)
                        {
                            index[i]++;
                        }
                        break;
                    case MonsterDIR.RIGHT:
                        pos.x += Mathf.Abs(monsterSpeed * Time.deltaTime);
                        if (pos.x >= pathNode[index[i] + 1].x)
                        {
                            index[i]++;
                        }
                        break;
                    case MonsterDIR.UP:
                        pos.y += Mathf.Abs(monsterSpeed * Time.deltaTime);
                        if (pos.y >= pathNode[index[i] + 1].y)
                        {
                            index[i]++;
                        }
                        break;
                    case MonsterDIR.DOWN:
                        pos.y -= Mathf.Abs(monsterSpeed * Time.deltaTime);
                        if (pos.y <= pathNode[index[i] + 1].y)
                        {
                            index[i]++;
                        }
                        break;
                }
                LuoboTool.Log("i:" + i + ", monster:" + monsterList[i].gameObject.name);
                monsterList[i].localPosition = pos;
                checkTowerAttackState(monsterList[i]);
            }

            delta += Time.deltaTime;
            if (delta >= 0.5f)
            {
                delta = 0f;
                playAttackEffect();
            }
        }
	}

    void checkTowerAttackState(Transform monTrans)
    {
        if (monTrans == null) return;
        int towerType = 0;
        Vector3 monPos = monTrans.localPosition;
        float distance = (monPos.x - towerPos.x) * (monPos.x - towerPos.x) + (monPos.y - towerPos.y) * (monPos.y - towerPos.y);
        bool inArea = (distance <= 200 * 200);

        if(inArea)
        {
            if(attack == null)
            {
                attack = monTrans;
                delta = 0f;
                LuoboTool.Log("Attack ADD:" + monTrans.gameObject.name);
            }
        }
        else
        {
            if(attack == monTrans)
            {
                attack = null;
                LuoboTool.Log("Attack REMOVE:" + monTrans.gameObject.name);
            }
        }
    }

    void playAttackEffect()
    {
        if(attack == null)return;
        Transform monster = attack;
        Vector3 to = monster.localPosition - towerPos;

        TweenPosition tweenPos = tBulletTrans.GetComponent<TweenPosition>();
        if(tweenPos == null)
        {
            tweenPos = tBulletTrans.gameObject.AddComponent<TweenPosition>();
            EventDelegate.Add(tweenPos.onFinished, bulletFinished);
        }
        tweenPos.ResetToBeginning();
        tweenPos.from = Vector3.zero;
        tweenPos.to = to;
        tweenPos.duration = 0.2f;
        tweenPos.enabled = true;
        tBulletTrans.gameObject.SetActive(true);
    }

    void bulletFinished()
    {
        tBulletTrans.gameObject.SetActive(false);
    }
}
