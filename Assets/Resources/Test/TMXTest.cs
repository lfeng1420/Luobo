using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TMXTest : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {

        DataManager.getInstance().loadData("XML/Data.xml");
	}


    /*
    void setSomeObjectPos()
    {
        //设置空闲位置;
        Transform trans = transform.Find("Camera/Anchor/Empty/Sprite");
        for (int i = 0; i < DataManager.getInstance().SpaceNodeList.Count; i++)
        {
            int x = DataManager.getInstance().SpaceNodeList[i].m_pointX;
            int y = -DataManager.getInstance().SpaceNodeList[i].m_pointY;
            Vector3 pos = new Vector3(x, y, 0);
            LuoboTool.cloneObj(trans.parent, trans.gameObject, pos, i.ToString("D2"));
        }

        //设置场景中的道具;
        string[] colorStr = {"_white", "_purple"};
        trans = transform.Find("Camera/Anchor/Object/Sprite");
        for(int i = 0; i < DataManager.getInstance().SomeObjectList.Count; i++)
        {
            ObjectType type = DataManager.getInstance().SomeObjectList[i].m_objType;

            string spriteName = "cloud" + ((int)type).ToString("D2") + colorStr[0];
            LuoboTool.Log("ObjSpriteName:" + spriteName);
            int x = DataManager.getInstance().SomeObjectList[i].m_pointX;
            int y = -DataManager.getInstance().SomeObjectList[i].m_pointY;
            Vector3 pos = new Vector3(x, y, 0);
            GameObject go = LuoboTool.cloneObj(trans.parent, trans.gameObject, pos, (int)type + "Ob");
            LuoboTool.setSprite(go.transform, spriteName);
        }

        //设置怪物出生点;
        trans = transform.Find("Camera/Anchor/StartAndEnd/Start");
        if(trans != null)
        {
            int startX = DataManager.getInstance().PathNodeList[0].m_nodeX - 10;
            int startY = -DataManager.getInstance().PathNodeList[0].m_nodeY;
            trans.localPosition = new Vector3(startX, startY, 0);
        }

        //设置萝卜和血条位置;
        int count = DataManager.getInstance().PathNodeList.Count;
        trans = transform.Find("Camera/Anchor/StartAndEnd/Luobo");
        if (trans != null)
        {
            int startX = DataManager.getInstance().PathNodeList[count - 2].m_nodeX - 20;
            int startY = -DataManager.getInstance().PathNodeList[count - 2].m_nodeY;
            trans.localPosition = new Vector3(startX, startY, 0);
        }

        trans = transform.Find("Camera/Anchor/StartAndEnd/Life");
        if (trans != null)
        {
            int startX = DataManager.getInstance().PathNodeList[count - 1].m_nodeX;
            int startY = -DataManager.getInstance().PathNodeList[count - 1].m_nodeY;
            trans.localPosition = new Vector3(startX, startY, 0);
        }

    }*/

    
	
	// Update is called once per frame
	void Update () 
    {
        
	}
}
