using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{

    #region InGameUI_INFO

    public UISlider m_hpSlider = null;
    public UISlider m_oxySlider = null;

    public GameObject m_South = null;
    public GameObject m_North = null;

    public GameObject m_equipInfo = null;

    public interface PlayerHPOxyUpdate
    {
        void ChangeHP(float curHp , float prevHp,float maxHp);
        void ChangeOxy(float curOxy , float prevOxy,float maxOxy);   
    }

    private List<PlayerHPOxyUpdate> m_uiUpdate = new List<PlayerHPOxyUpdate>();

    float m_mapBGWidth = 0.0f;
    #endregion

    #region UnityMethod
    void Start()
    {
        startPosition = m_North.transform.position;
        rationAngleToPixel = numberOfPixelsNorthToNorth / 360f;
    }

    void FixedUpdate()
    {
        PlanetDirCheck();
    }

    #endregion

    #region PlayerHP_OXY
    public void AddUIUpdate(PlayerHPOxyUpdate inter)
    {
        if (!m_uiUpdate.Contains(inter))
            m_uiUpdate.Add(inter);
    }

    public void PlayerHPUpdate(float curHp,float prevHP, float maxHp)
    {
        //m_playerHPBar.GetComponent<RectTransform>().sizeDelta = 
        //    new Vector2(m_mainHPBarWidth * (curHp / maxHp) , m_mainHPBarHeight);

        m_hpSlider.value = (curHp / maxHp);

        foreach (var inter in m_uiUpdate)
            inter.ChangeHP(curHp , prevHP,maxHp);
    }

    public void PlayerOxyUpdate(float curOxy,float prevOxy, float maxOxy)
    {
        //m_playerOXYBar.GetComponent<RectTransform>().sizeDelta = new Vector2(m_mainHPBarWidth * (curOxy / maxOxy) , m_mainHPBarHeight);

        m_oxySlider.value = (curOxy / maxOxy);

        foreach (var inter in m_uiUpdate)
            inter.ChangeOxy(curOxy , prevOxy,maxOxy);
    }
    #endregion



    #region PlanetDirChecker


    public float numberOfPixelsNorthToNorth = 0.0f;
    public GameObject target;
    Vector3 startPosition;
    float rationAngleToPixel = 1.0f;


    void PlanetDirCheck()
    {
        Vector3 perp = Vector3.Cross(Vector3.forward , target.transform.forward);
        float dir = Vector3.Dot(perp , Vector3.up);
        m_North.transform.position = startPosition + (new Vector3(Vector3.Angle(target.transform.forward , Vector3.forward) * Mathf.Sign(dir) * rationAngleToPixel , 0 , 0));

        ////Vector3 angles = Camera.main.transform.eulerAngles;

        ////Debug.Log("angle " + angles);

        ////Vector3 north = Camera.main.transform.position;
        ////north.z += 1.0f;


        ////Vector3 screenPos = Camera.main.WorldToViewportPoint(north);
        ////Vector3 nscx = Camera.main.WorldToViewportPoint(m_North.transform.position);
        ////screenPos.x = 0.5f + (screenPos.x - 0.5f) * (m_North.GetComponent<UISprite>().width / Camera.main.pixelWidth) * 0.9f;
        ////float posX = (screenPos.x - 0.5f + nscx.x) * 0.5f;

        ////Vector3 t = new Vector3(posX , 0 , 0);


        ////float width = m_North.transform.parent.GetComponent<UISprite>().width;
        ////t = new Vector3(0.5f + posX / width , 0.0f , 0.0f);
        ////Vector3 a = Camera.main.ViewportToWorldPoint(t);
        ////m_North.transform.position = new Vector3(a.x , m_North.transform.position.y , m_North.transform.position.z);

        // Player p = GameManager.Instance().PLAYER.m_player;
        // if (p == null)
        //     return;
        // Vector3 newP = Camera.main.transform.forward;

        // var southDir = m_SouthPosition.transform.position - newP;
        // var northDir = m_NorthPosition.transform.position - newP;

        // //southDir.Normalize();
        // //northDir.Normalize();



        // var southAngle = Mathf.Abs(Mathf.Atan2(southDir.x , southDir.y) * Mathf.Rad2Deg);
        // var northAngle = Mathf.Abs(Mathf.Atan2(northDir.x , northDir.y) * Mathf.Rad2Deg);

        //// var angle = Mathf.Abs(Mathf.Atan2(newP.x , newP.y) * Mathf.Rad2Deg);


        // Vector3 southp = m_southPos.transform.localPosition;
        // Vector3 northp = m_northPos.transform.localPosition;
        // Vector3 center = Vector2.zero; // m_MapBG.transform.localPosition;

        // // 180도면 정 중앙에 있어야함

        // float w = (southAngle / 180.0f) + (m_mapBGWidth / 180.0f);


        // // North 는 0에 근접할수록  South 는 -180에 근접할수록
        // //   Debug.Log("South " + southAngle + " d " + ((180.0f - southAngle) / 180.0f) + " north " + northAngle + " w " + w);
        // //w = (northAngle )
        // //m_northPos.transform.position = new Vector3()
        // Ray ray = new Ray(Camera.main.transform.position , northDir);
        // RaycastHit hit;
        // Physics.Raycast(ray , out hit);
        // Debug.DrawRay(ray.origin , ray.direction , Color.red);
        // if(hit.transform != null)
        // {
        //     Debug.Log(hit.transform.name);
        // }
        // else
        // {




        // }

        // float angle = Mathf.Atan2(Camera.main.transform.forward.z , Camera.main.transform.forward.x) * Mathf.Rad2Deg;

        // Vector3 perp = Vector3.Cross(Vector3.forward , Camera.main.transform.forward);
        // float dir = Vector3.Dot(perp , Vector3.up);

        // //w = m_MapBG.transform.localPosition.x + Vector3.Angle(Camera.main.transform.forward , Vector3.forward) * Mathf.Sign(dir) * (180.0f/360.0f);
        // //m_northPos.transform.localPosition = new Vector3(w , northp.y , northp.z);

        //// angle = Mathf.Atan2(Camera.main.transform.)

        // //    m_southPos.transform.localPosition = new Vector3(w  ,
        // //     southp.y , southp.z);

    }
    public float GetAngleBetween3DVector(Vector3 vec1 , Vector3 vec2)
    {
        float theta = Vector3.Dot(vec1 , vec2) / (vec1.magnitude * vec2.magnitude);
        Vector3 dirAngle = Vector3.Cross(vec1 , vec2);
        float angle = Mathf.Acos(theta) * Mathf.Rad2Deg;
        if (dirAngle.z < 0.0f) angle = 360 - angle;
        Debug.Log("사잇각 : " + angle);
        return angle;
    }
    #endregion

    #region EquipWeapon
    public void EquipWeapon(int itemCID,int curCount,int maxCount)
    {
        m_equipInfo.SetActive(true);
    }

    public void UnEquipWeapon(int itemCID,int curCount,int maxCount)
    {
        m_equipInfo.SetActive(false);
    }
    #endregion
}
