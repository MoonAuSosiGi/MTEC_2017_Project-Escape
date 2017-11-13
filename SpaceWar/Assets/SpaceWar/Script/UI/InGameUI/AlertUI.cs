using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AlertUI : MonoBehaviour {

    #region AlertUI INFO
    [SerializeField] private GameObject m_StartPosition = null;
    [SerializeField] private GameObject m_ShowPosition = null;
    [SerializeField] private GameObject m_HidePosition = null;
    [SerializeField] private GameObject m_DefaultAlertObj = null;
    [SerializeField] private List<AlertData> m_alertList = new List<AlertData>();
    
    public enum AlertType
    {
        NONE = 0,
        METEOR_ATTACK,
        ENGINE_STARTING,
        ENGINE_READY,
        DEATH_ZONE
    }

    [System.Serializable]
    private struct AlertData
    {
        public string id;
        public AlertType type;
        public int tick;
        public GameObject target;
        public AlertData(AlertType t,string id,int tick,GameObject target)
        {
            type = t;
            this.id = id;
            this.tick = tick;
            this.target = target;
        }
    }

    #endregion 

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            AlertShow(AlertType.METEOR_ATTACK,"" , UnityEngine.Random.Range(30,120) , "Meteor Attack");
    }

    #region Alert UI Method
    public void AlertShow(AlertType type,string id , int tick , string desc)
    {

        int ckID = GetDataIndex(id);
        if(ckID != -1)
        {
            AlertData d = m_alertList[ckID];
            m_alertList[ckID] = new AlertData(d.type , d.id , tick , d.target);
            var t = TimeSpan.FromSeconds(tick);
            GetTick(m_alertList[ckID].target).text = t.Minutes.ToString("00") + " : " + t.Seconds.ToString("00");
            return;
        }

        GameObject alert = GameObject.Instantiate(m_DefaultAlertObj);
        alert.SetActive(true);
        AlertData data = new AlertData(type, id , tick , alert);

        alert.transform.parent = transform;
        alert.transform.localScale = new Vector3(1.0f , 1.0f , 1.0f);
        alert.transform.position = m_StartPosition.transform.position;

        string iconName = null;
        switch (type)
        {
            case AlertType.METEOR_ATTACK: iconName = "Icon_Meteor"; break;
            case AlertType.DEATH_ZONE: iconName = "Icon_DeathZone"; break;
            case AlertType.ENGINE_STARTING: iconName = "Icon_SpaceShip"; break;
            case AlertType.ENGINE_READY: iconName = "Icon_SpaceShip"; break;
        }

        UISprite spr = GetIcon(alert);

        if (string.IsNullOrEmpty(iconName))
            spr.enabled = false;
        else
            spr.spriteName = iconName;
        var time = TimeSpan.FromSeconds(tick);

        GetTick(alert).text = time.Minutes.ToString("00") + " : " + time.Seconds.ToString("00");//string.Format("%0d:%0d" , tick / 60 , tick % 60);
        GetDesc(alert).text = desc;
        
        int index = -1;
        for (int i = 0; i < m_alertList.Count; i++)
        {
            AlertData d = m_alertList[i];

            // 내가 이친구보다 위로 올라가야함
            Debug.Log("tick " + d.tick + " data  " + data.tick + " d.id " + d.id + " data.id " + data.id);
            if (d.tick > data.tick)
            {
                alert.transform.position = new Vector3(
                    alert.transform.position.x ,
                    d.target.transform.position.y ,
                    alert.transform.position.z);
                LeftShowAlert(alert);
                // 즉 이 친구부터는 쭉 내려가야함
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            Debug.Log("여길와야하지 " + index + " " + m_alertList[0].id);
        //    LeftShowAlert(alert);
            for (int i = index; i < m_alertList.Count; i++)
            {
                AlertData d = m_alertList[i];
                iTween.MoveBy(d.target , iTween.Hash(
                    "y" , -0.15f));
            }
            m_alertList.Insert(index , data);
        }
        else if(m_alertList.Count >= 1)
        {
            Debug.Log("내가 맨 마지막이야 ");
            // 내가 맨 마지막이야
            data.target.transform.position = new Vector3(
                data.target.transform.position.x ,
                m_alertList[m_alertList.Count - 1].target.transform.position.y ,
                data.target.transform.position.z);

            iTween.MoveBy(data.target , iTween.Hash(
               "y" , -0.15f ,
               "time",0.1f,
               "easeType" , "easeOutQuint" ,
               "oncompletetarget" , gameObject ,
               "oncompleteparams" , data.target ,
               "oncomplete" , "LeftShowAlert"));
            m_alertList.Add(data);
        }
        else
        {
            // 맨 처음이야
            LeftShowAlert(alert);
            m_alertList.Add(data);
        }

        
    }
    

    void LeftShowAlert(GameObject alert)
    {
        iTween.MoveTo(alert , iTween.Hash(
            "x" , m_ShowPosition.transform.position.x ,
            "time",0.1f,
            "easeType" , "easeOutQuint"));
    }

    public void AlertHide(string id)
    {
        int index = GetDataIndex(id);
        Debug.Log("Alert HIDE " + index + " " + id);
        if(index != -1)
        {
            iTween.MoveTo(m_alertList[index].target , iTween.Hash(
                "y" , m_HidePosition.transform.position.y ,
                "oncompletetarget" , gameObject ,
                "oncomplete" , "AlertHideEnd" ,
                "oncompleteparams" , index));

                for (int i = 0; i < m_alertList.Count; i++)
                {
                    AlertData d = m_alertList[i];
                    iTween.MoveBy(d.target , iTween.Hash(
                        "y" , +0.15f));
                }
        }
    }

    void AlertHideEnd(int index)
    {
        GameObject.Destroy(m_alertList[index].target);
        m_alertList.RemoveAt(index);
    }

    #region UIObject Get
    UISprite GetBG(GameObject target)
    {
        UISprite bg = target.transform.GetChild(0).GetComponent<UISprite>();
        return bg;
    }

    UISprite GetIcon(GameObject target)
    {
        UISprite icon = target.transform.GetChild(1).GetComponent<UISprite>();
        return icon;
    }

    UILabel GetTick(GameObject target)
    {
        UILabel label = target.transform.GetChild(2).GetComponent<UILabel>();
        return label;
    }

    UILabel GetDesc(GameObject target)
    {
        UILabel label = target.transform.GetChild(3).GetComponent<UILabel>();
        return label;
    }
    #endregion
    #endregion

    #region Util Method

    int GetDataIndex(string id)
    {
        for(int i = 0; i < m_alertList.Count; i++)
        {
            if (m_alertList[i].id.Equals(id))
                return i;
        }
        return -1;
    }
    #endregion
}
