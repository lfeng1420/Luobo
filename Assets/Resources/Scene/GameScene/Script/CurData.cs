using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CurData : MonoBehaviour 
{
    List<MonsterInfo> monsterList = null;
    List<TowerInfo> towerList = null;
    CurData _instance = null;

    public List<MonsterInfo> MonsterList
    {
        get
        {
            return monsterList;
        }
    }

    public List<TowerInfo> TowerList
    {
        get
        {
            return towerList;
        }
    }

    public CurData getInstance
    {
        get
        {
            return _instance;
        }
    }


	// Use this for initialization
	void Start () 
    {
        _instance = transform.GetComponent<CurData>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}

    public void removeMonster(int index)
    {
        monsterList.RemoveAt(index);
    }

    public void removeTower(int index)
    {
        towerList.RemoveAt(index);
    }

    public MonsterInfo getMonsterInfo(int index)
    {
        return monsterList[index];
    }

    public TowerInfo getTowerInfo(int index)
    {
        return towerList[index];
    }

    public int getTowerIndex(Transform towerTrans)
    {
        for(int i = 0; i < towerList.Count; i++)
        {
            if(towerList[i].towerTrans == towerTrans)
            {
                return i;
            }
        }

        return -1;
    }


    public int getMonsterIndex(Transform monTrans)
    {
        for (int i = 0; i < towerList.Count; i++)
        {
            if (monsterList[i].monTrans == monTrans)
            {
                return i;
            }
        }

        return -1;
    }
}
