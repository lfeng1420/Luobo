using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

public class DataManager : MonoBehaviour 
{
    private static DataManager _dataManager = null;

    private static GameObject _dataHolder = null;

    /// <summary>
    /// 主题信息;
    /// </summary>
    List<StageData> stageDataList = null;
    /// <summary>
    /// 怪物信息;
    /// </summary>
    List<Monster> monsterDataList = null;
    /// <summary>
    /// 炮塔信息;
    /// </summary>
    List<Tower> towerDataList = null;


    //TMX;

    /// <summary>
    /// 空闲位置，可放置炮塔;
    /// </summary>
    List<SpaceNode> spaceNodeList = null;

    /// <summary>
    /// 怪物行走路径节点集合;
    /// </summary>
    List<MonsterMovePathNode> pathNodeList = null;

    /// <summary>
    /// 地图中所有道具属性;
    /// </summary>
    List<SomeObject> someObjectList = null;

    /// <summary>
    /// 地图宽度;
    /// </summary>
    int mapWidth = 0;
    /// <summary>
    /// 地图高度;
    /// </summary>
    int mapHeight = 0;

    /// <summary>
    /// 瓦片宽度;
    /// </summary>
    int tileWidth = 0;
    /// <summary>
    /// 瓦片高度;
    /// </summary>
    int tileHeight = 0;

    /// <summary>
    /// 地图实际宽度;
    /// </summary>
    int mapRealWidth = 0;
    /// <summary>
    /// 地图实际高度;
    /// </summary>
    int mapRealHeight = 0;


    /// <summary>
    /// xml路径;
    /// </summary>
    private string xmlPath = null;

    /// <summary>
    /// 当前主题;
    /// </summary>
    int curStageChoice = -1;

    /// <summary>
    /// 当前选择关卡
    /// </summary>
    int curLevelChoice = -1;

    /// <summary>
    /// 是否显示提示内容;
    /// </summary>
    bool showTips = true;

    /// <summary>
    /// 单例;
    /// </summary>
    /// <returns></returns>
    public static DataManager getInstance()
    {
        _dataHolder = LuoboTool.getGameObject("DataManager", true);
        _dataManager = _dataHolder.GetComponent<DataManager>();
        if (_dataManager == null)
        {
            _dataManager = _dataHolder.AddComponent<DataManager>();
        }

        return _dataManager;
    }

    /// <summary>
    /// 读取xml;
    /// </summary>
    /// <returns></returns>
    public XmlDocument loadXML()
    {
        string path = xmlPath.Split('.')[0];
        string data = Resources.Load(path).ToString();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(data);
        return xmlDoc;
    }

    public void loadData(string path)
    {
        if(path == "")
        {
            LuoboTool.LogError("Can not Load XML, Path is null!");
            return;
        }

        xmlPath = path;

        if (xmlPath.Contains("Data"))
        {
            loadMTSData();
            loadStatusFile();
        }
        else if (xmlPath.Contains("BGPath"))
        {
            loadTMXData();
        }
    }

    /// <summary>
    /// 解析怪物、炮塔和主题数据;
    /// </summary>
    public void loadMTSData()
    {
        XmlDocument xmlDoc = loadXML();
        if (xmlDoc == null)
        {
            LuoboTool.LogError("Load XML Error! Path:" + xmlPath);
            return;
        }
        XmlNode rootNode = xmlDoc.SelectSingleNode("root");
        
        //怪物数据;
        monsterDataList = new List<Monster>();
        monsterDataList.Clear();
        XmlNodeList monsterNodeList = rootNode.SelectSingleNode("monsterlist").ChildNodes;
        foreach(XmlNode monsterNode in monsterNodeList)
        {
            int monsterID = int.Parse(monsterNode.Attributes[0].InnerText);
            LuoboTool.Log("monsterID:" + monsterID);

            int monsterLife = int.Parse(monsterNode.ChildNodes[0].InnerText);
            LuoboTool.Log("monsterLife:" + monsterLife);

            int monsterSpeed = int.Parse(monsterNode.ChildNodes[1].InnerText);
            LuoboTool.Log("monsterSpeed:" + monsterSpeed);

            Monster monster = new Monster(monsterLife, monsterSpeed);
            monsterDataList.Add(monster);
        }

        //炮塔数据;
        towerDataList = new List<Tower>();
        towerDataList.Clear();
        XmlNodeList towerNodeList = rootNode.SelectSingleNode("towerlist").ChildNodes;
        foreach(XmlNode tower in towerNodeList)
        {
            int towerID = int.Parse(tower.Attributes[0].InnerText);
            LuoboTool.Log("towerID:" + towerID);

            //伤害属性;
            List<float> cdList = new List<float>();
            cdList.Clear();
            List<int> attValList = new List<int>();
            attValList.Clear();
            XmlNode attNode = tower.ChildNodes[0];
            int type = int.Parse(attNode.Attributes[0].InnerText);
            LuoboTool.Log("type:" + type);
            int radius = int.Parse(attNode.Attributes[1].InnerText);
            LuoboTool.Log("radius:" + radius);

            //每次攻击等待时间;
            XmlNodeList cdNodeList = attNode.ChildNodes[0].ChildNodes;
            foreach (XmlNode cd in cdNodeList)
            {
                //每一个等级对应的等待时间;
                float cdValue = float.Parse(cd.InnerText);
                LuoboTool.Log("cdValue:" + cdValue);
                cdList.Add(cdValue);
            }

            //伤害值序列;
            XmlNodeList attNodeList = attNode.ChildNodes[1].ChildNodes;
            foreach (XmlNode att in attNodeList)
            {
                int attVal = int.Parse(att.InnerText);
                LuoboTool.Log("attVal:" + attVal);
                attValList.Add(attVal);
            }

            //升级需要的金钱属性;
            List<int> levelUpList = new List<int>();
            levelUpList.Clear();
            XmlNodeList levelUPNodeList = tower.ChildNodes[1].ChildNodes;
            foreach(XmlNode levelUp in levelUPNodeList)
            {
                //每升一级需要的金钱;
                int gold = int.Parse(levelUp.InnerText);
                LuoboTool.Log("LevelUpNeedMoney:" + gold);
                levelUpList.Add(gold);
            }

            //销毁炮塔获得的金钱属性;
            List<int> destroyInfoList = new List<int>();
            destroyInfoList.Clear();
            XmlNodeList destroyNodeList = tower.ChildNodes[2].ChildNodes;
            foreach(XmlNode destroyInfo in destroyNodeList)
            {
                int getGold = int.Parse(destroyInfo.InnerText);
                LuoboTool.Log("DestroyWillGetGold:" + getGold);
                destroyInfoList.Add(getGold);
            }

            //附加效果类型和属性;
            AddtionalEffect effectType = effectType =
                (AddtionalEffect)(int.Parse(tower.ChildNodes[3].Attributes[0].InnerText));
            LuoboTool.Log("effectType:" + effectType);
            List<int> effValList = new List<int>();
            effValList.Clear();
            if (effectType != AddtionalEffect.None)
            {
                XmlNodeList effValNodeList = tower.ChildNodes[3].ChildNodes[0].ChildNodes;
                foreach (XmlNode effValue in effValNodeList)
                {
                    int value = int.Parse(effValue.InnerText);
                    LuoboTool.Log("EffValue:" + value);
                    effValList.Add(value);
                }
            }

            Tower child = new Tower(towerID, -1, (AttackType)type, radius, cdList, attValList, 
                levelUpList, destroyInfoList, (AddtionalEffect)effectType,
                (effectType != AddtionalEffect.None) ? effValList : null);
            towerDataList.Add(child);
        }


        //主题数据;
        stageDataList = new List<StageData>();
        stageDataList.Clear();

        XmlNode stageListNode = rootNode.SelectSingleNode("stagelist");
        XmlNodeList stageNodeList = stageListNode.ChildNodes;
        foreach (XmlNode stage in stageNodeList)
        {
            int stageID = int.Parse(stage.Attributes[0].InnerText);
            LuoboTool.Log("StageID:" + stageID);
            bool stageLock = ((stageID == 1) ? false : true);
            int levelNum = int.Parse(stage.ChildNodes[0].InnerText);
            LuoboTool.Log("LevelNum:" + levelNum);
            int unlockLevel = ((stageID == 1) ? 1 : 0);
            //关卡信息;
            LuoboTool.Log("[LevelList] BEGIN-------");
            //关卡信息序列;
            List<LevelData> levelList = new List<LevelData>();
            levelList.Clear();
            XmlNodeList levelNodeList = stage.ChildNodes[1].ChildNodes;
            foreach (XmlNode levelInfo in levelNodeList)
            {
                int levelID = int.Parse(levelInfo.Attributes[0].InnerText);
                LuoboTool.Log("levelID:" + levelID);
                int waveNum = int.Parse(levelInfo.Attributes[1].InnerText);
                LuoboTool.Log("waveNum:" + waveNum);
                bool levelLock = ((levelID == 1 && stageID == 1) ? false : true);
                int levelColor = int.Parse(levelInfo.Attributes[2].InnerText);
                LuoboTool.Log("levelColor:" + levelColor);
                bool clearMedal = false;
                int luoboMedal = 0;

                //可用炮塔序列;
                List<int> towerIDList = new List<int>();
                towerIDList.Clear();
                XmlNodeList towerIDNodeList = levelInfo.ChildNodes[0].ChildNodes;
                foreach(XmlNode towerID in towerIDNodeList)
                {
                    int id = int.Parse(towerID.Attributes[0].InnerText);
                    LuoboTool.Log("towerID:" + id);
                    towerIDList.Add(id);
                }

                //怪物波数信息序列;
                List<MonsterWave> monsterWaveList = null;
                List<int> lifeList = null;
                if (levelInfo.ChildNodes.Count >= 2)
                {
                    monsterWaveList = new List<MonsterWave>();
                    monsterWaveList.Clear();
                    XmlNodeList waveNodeList = levelInfo.ChildNodes[1].ChildNodes;
                    foreach (XmlNode waveNode in waveNodeList)
                    {
                        int waveID = int.Parse(waveNode.Attributes[0].InnerText);
                        //怪物ID序列;
                        List<int> monsterIDList = new List<int>();
                        monsterIDList.Clear();
                        XmlNodeList waveMonsterNodeList = waveNode.ChildNodes;
                        foreach (XmlNode node in waveMonsterNodeList)
                        {
                            //怪物ID;
                            int monsterID = int.Parse(node.Attributes[0].InnerText);
                            LuoboTool.Log("monsterID:" + monsterID);
                            //怪物数量;
                            int num = int.Parse(node.Attributes[1].InnerText);
                            LuoboTool.Log("num:" + num);

                            for (int i = 0; i < num; i++)
                            {
                                monsterIDList.Add(monsterID);
                            }
                        }

                        MonsterWave monsterWave = new MonsterWave(monsterIDList);
                        monsterWaveList.Add(monsterWave);
                    }

                    //生命值提升序列;
                    lifeList = new List<int>();
                    lifeList.Clear();
                    XmlNodeList lifeImpList = levelInfo.ChildNodes[2].ChildNodes;
                    foreach (XmlNode lifeNode in lifeImpList)
                    {
                        //怪物ID;
                        int waveID = int.Parse(lifeNode.Attributes[0].InnerText);
                        LuoboTool.Log("waveID:" + waveID);
                        //提升生命值;
                        int life = int.Parse(lifeNode.Attributes[1].InnerText);
                        LuoboTool.Log("life:" + life);

                        lifeList.Add(life);
                    }
                }

                LevelData level = new LevelData(levelLock, levelColor,
                    clearMedal, luoboMedal, waveNum, lifeList, towerIDList, monsterWaveList);
                levelList.Add(level);
            }

            LuoboTool.Log("[LevelList] END---------------");

            //添加到序列中;
            StageData stageData = new StageData(stageLock, levelNum, unlockLevel, levelList);
            stageDataList.Add(stageData);
        }
    }

    public void loadTMXData()
    {
        XmlDocument xmlDoc = loadXML();
        if (xmlDoc == null)
        {
            LuoboTool.LogError("Load TMXFile Error! Path:" + xmlPath);
            return;
        }

        pathNodeList = new List<MonsterMovePathNode>();
        pathNodeList.Clear();
        someObjectList = new List<SomeObject>();
        someObjectList.Clear();
        spaceNodeList = new List<SpaceNode>();
        spaceNodeList.Clear();

        XmlNode rootNode = xmlDoc.SelectSingleNode("map");
        //获取地图宽高以及瓦片宽高;
        mapWidth = int.Parse(rootNode.Attributes[2].InnerText);
        mapHeight = int.Parse(rootNode.Attributes[3].InnerText);
        tileWidth = int.Parse(rootNode.Attributes[4].InnerText);
        tileHeight = int.Parse(rootNode.Attributes[5].InnerText);
        LuoboTool.Log("mapWidth:" + mapWidth + ", mapHeight:" + mapHeight + 
            ", tileWidth:" + tileWidth + ", tileHeight:" + tileHeight);

        //实际宽高;
        mapRealWidth = mapWidth * tileWidth;
        mapRealHeight = mapHeight * tileHeight;

        XmlNode objNode = rootNode.SelectSingleNode("objectgroup");
        string name = objNode.Attributes[0].InnerText;
        int width = int.Parse(objNode.Attributes[1].InnerText);
        int height = int.Parse(objNode.Attributes[2].InnerText);
        LuoboTool.Log("name:" + name + ", width:" + width + ", height:" + height);

        XmlNodeList objList = objNode.ChildNodes;
        foreach(XmlNode obj in objList)
        {
            string objName = obj.Attributes[0].InnerText;
            if(objName.StartsWith("PT"))//路径节点;
            {
                int x = int.Parse(obj.Attributes[1].InnerText);
                int y = int.Parse(obj.Attributes[2].InnerText);
                LuoboTool.Log("MonsterMovePathNode: " + x + ", " + y);
                MonsterMovePathNode node = new MonsterMovePathNode(x, y);
                pathNodeList.Add(node);
            }
            else if(objName.StartsWith("Obj"))//空白位置;
            {
                int startX = int.Parse(obj.Attributes[1].InnerText);
                int startY = int.Parse(obj.Attributes[2].InnerText);
                int objWidth = int.Parse(obj.Attributes[3].InnerText);
                int objHeight = int.Parse(obj.Attributes[4].InnerText);
       
                for(int i = 1; i <= objWidth / tileWidth; i++)
                {
                    for(int j = 1; j <= objHeight / tileHeight; j++)
                    {
                        SpaceNode spaceNode = new SpaceNode(startX + (i - 1) * tileWidth,
                            startY + (j - 1) * tileHeight, tileWidth, tileHeight);
                        LuoboTool.Log("spaceNode: " + spaceNode.m_pointX + ", " +
                            spaceNode.m_pointY + ", " + spaceNode.m_spaceWidth + ", "
                            + spaceNode.m_spaceHeight);
                        spaceNodeList.Add(spaceNode);
                    }
                }
            }
            else if(objName.Contains("Ob"))//道具;
            {
                name = obj.Attributes[0].InnerText;
                string[] str = Regex.Split(name, "Ob", RegexOptions.IgnoreCase);
                int type = int.Parse(str[0]);

                int startX = int.Parse(obj.Attributes[1].InnerText);
                int startY = int.Parse(obj.Attributes[2].InnerText);
                int objWidth = int.Parse(obj.Attributes[3].InnerText);
                int objHeight = int.Parse(obj.Attributes[4].InnerText);

                SomeObject someObj = new SomeObject(startX, startY, objWidth, objHeight,
                    (ObjectType)type);
                someObjectList.Add(someObj);
            }
            else if(objName.Contains("T"))
            {

            }
        }
    }

    /// <summary>
    /// 加载Status.xml状态配置文件;
    /// </summary>
    public void loadStatusFile()
    {
        string file = Application.persistentDataPath + "/Status.xml";
        Debug.Log(file);
        if(File.Exists(file))
        {
            Debug.Log("File Exist");
            XmlDocument xml = new XmlDocument();
            xml.Load(file);
            XmlNode root = xml.SelectSingleNode("root");
            XmlNode stagelist = root.SelectSingleNode("stagelist");
            XmlNodeList stageNodeList = stagelist.ChildNodes;
            foreach (XmlNode stage in stageNodeList)
            {
                int stageID = int.Parse(stage.Attributes[0].InnerText);
                bool stageLock = bool.Parse(stage.ChildNodes[0].InnerText);
                stageDataList[stageID - 1].m_lockState = stageLock;
                int unlockLevel = int.Parse(stage.ChildNodes[1].InnerText);
                stageDataList[stageID - 1].m_unlockLevel = unlockLevel;
                
                XmlNodeList levelNodeList = stage.ChildNodes[2].ChildNodes;
                foreach (XmlNode levelInfo in levelNodeList)
                {
                    int levelID = int.Parse(levelInfo.Attributes[0].InnerText);
                    bool levelLock = bool.Parse(levelInfo.ChildNodes[0].InnerText);
                    stageDataList[stageID - 1].m_levelInfo[levelID - 1].m_lockState = levelLock;
                    bool medal1 = bool.Parse(levelInfo.ChildNodes[1].InnerText);
                    stageDataList[stageID - 1].m_levelInfo[levelID - 1].m_allClearMedal = medal1;
                    int medal2 = int.Parse(levelInfo.ChildNodes[2].InnerText);
                    stageDataList[stageID - 1].m_levelInfo[levelID - 1].m_luoboMedal = medal2;
                }
            }
        }
        else
        {
            Debug.Log("File NOT Exist, Will Create File.");
            XmlDocument xml = new XmlDocument();
            XmlElement root = xml.CreateElement("root");
            XmlElement stagelist = xml.CreateElement("stagelist");
            for(int i = 0; i < 3; i++)
            {
                XmlElement stage = xml.CreateElement("stage");
                stage.SetAttribute("id", (i + 1).ToString("D2"));
                //默认第一个主题解锁;
                XmlElement stageLock = xml.CreateElement("lock");
                stageLock.InnerText = ((i == 0) ? "false" : "true");
                //只解锁第一个关卡;
                XmlElement unlockLevel = xml.CreateElement("unlocklevel");
                unlockLevel.InnerText = ((i == 0) ? "1" : "0");

                stage.AppendChild(stageLock);
                stage.AppendChild(unlockLevel);

                XmlElement levelList = xml.CreateElement("levellist");
                for(int j = 0; j < 9; j++)
                {
                    XmlElement level = xml.CreateElement("level");
                    level.SetAttribute("id", (j + 1).ToString("D2"));
                    //关卡锁;
                    XmlElement levelLock = xml.CreateElement("lock");
                    levelLock.InnerText = ((i == 0 && j == 0)? "false":"true");
                    //道具清除奖牌;
                    XmlElement medal1 = xml.CreateElement("clearmedal");
                    medal1.InnerText = "false";
                    //萝卜奖牌;
                    XmlElement medal2 = xml.CreateElement("luobomedal");
                    medal2.InnerText = "0";

                    level.AppendChild(levelLock);
                    level.AppendChild(medal1);
                    level.AppendChild(medal2);

                    levelList.AppendChild(level);
                }

                stage.AppendChild(levelList);
                stagelist.AppendChild(stage);
            }

            root.AppendChild(stagelist);
            xml.AppendChild(root);
            xml.Save(file);
        }
    }

    public void saveStatusFile()
    {

    }


    public List<StageData> StageData
    {
        get
        {
            return stageDataList;
        }
    }

    public List<Tower> TowerList
    {
        get
        {
            return towerDataList;
        }
    }


    public void setStageAndLevelData(int stage, int level)
    {
        curStageChoice = stage;
        curLevelChoice = level;
    }


    public int LevelChoice
    {
        get
        {
            return curLevelChoice;
        }
        set
        {
            curLevelChoice = value;
        }
    }

    public int StageChoice
    {
        get
        {
            return curStageChoice;
        }
        set
        {
            curStageChoice = value;
        }
    }

    public bool ShowTips
    {
        get
        {
            return showTips;
        }
        set
        {
            showTips = value;
        }
    }

    public List<SpaceNode> SpaceNodeList
    {
        get
        {
            return spaceNodeList;
        }
    }

    public List<MonsterMovePathNode> PathNodeList
    {
        get
        {
            return pathNodeList;
        }
    }

    public List<SomeObject> SomeObjectList
    {
        get
        {
            return someObjectList;
        }
    }

    public List<Monster> MonsterList
    {
        get
        {
            return monsterDataList;
        }
    }
}
