using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeForEscape.Object.Planet;

// 이 친구는 네트워크에 상관없이 동작해야 한다.
public class GravityManager : Singletone<GravityManager> {

    #region GraivtyManager_INFO

    // -- 행성 리스트 --
    public List<GameObject> m_planetList = new List<GameObject>();

    // 현재 행성의 인덱스
    private int m_curPlanetIndex = 0;

    // 타겟 오브젝트 ( 주인공 캐릭터 )
    private Rigidbody m_targetObject = null;

    // 중력 체크용 벡터
    Vector3 m_gravityUp = Vector3.zero;

    // 파워
    public float m_power = 0.0f;
    private float m_powerValue = 0.0f;

    public float GRAVITY_POWER { get { return m_power; } set { m_power = value; } }
    public GameObject GRAVITY_TARGET { get { return m_targetObject.gameObject; } }

    public GameObject CurrentPlanet { get { return m_planetList[m_curPlanetIndex]; } }

    // Weather Controller
    [SerializeField]
    private WeatherController m_weatherController = null;
    [SerializeField]
    private PlanetTable m_planetTable = null; ///< 행성 테이블 정보
    
    public PlanetTable PLANET_TABLE
    {
        get { return m_planetTable; }
        set { m_planetTable = value; }
    }
    public string PLANET_NAME
    {
        get { return WeatherController.PLANET_NAME; }
    }

    // Obstacle Table
    [SerializeField]
    private PlanetObstacleTable m_obstacleTable = null;

    public PlanetObstacleTable OBSTACLE_TABLE
    {
        get { return m_obstacleTable; }
        set { m_obstacleTable = value; }
    }
    #endregion

    #region UnityMethod

    void Start()
    {
        if (m_planetTable == null)
            return;
        WeatherController.PLANET_NAME = "Kepler";
        StartCoroutine(SetupTable());
    }

    IEnumerator SetupTable()
    {
        string planetName = WeatherController.PLANET_NAME;
        int targetIndex = -1;

        if (m_planetTable == null)
            yield return null;

        for(int i = 0; i < m_planetTable.dataArray.Length; i++)
        {
            if(m_planetTable.dataArray[i].Planetname.Equals(planetName))
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
            yield return null;

        m_power = m_planetTable.dataArray[targetIndex].Gravity;
        m_powerValue = m_power;
        yield return null;
    }
    void FixedUpdate()
    {
        GravityProcess();
    }

    public void SetGravityEnable(bool enable)
    {
        if (enable)
            m_power = m_powerValue;
        else
            m_power = 0.0f;
    }

    void GravityProcess()
    {
        if (m_targetObject == null)
            return;
        m_gravityUp = (m_targetObject.transform.position - m_planetList[m_curPlanetIndex].transform.position).normalized;

        m_targetObject.AddForce(m_gravityUp * -1.0f * m_power);
        Quaternion targetRot = Quaternion.FromToRotation(m_targetObject.transform.up , m_gravityUp) * m_targetObject.transform.rotation;
        m_targetObject.transform.rotation = targetRot;
    }
    #endregion

    #region Main Function
    public void SetGravityTarget(Rigidbody target)
    {
        m_targetObject = target;
    }

    public Vector3 GetPlanetPosition(float offset, float anglex , float anglez)
    {
        float scale = CurrentPlanet.transform.localScale.x + offset;
        float x = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Cos(anglez * Mathf.Deg2Rad);
        float y = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Sin(anglez * Mathf.Deg2Rad);
        float z = scale * Mathf.Cos(anglex * Mathf.Deg2Rad);
        return new Vector3(x , y , z);
    }

    public Vector3 GetPlanetPosition(float anglex , float anglez)
    {
        float scale = CurrentPlanet.transform.localScale.x + ((m_weatherController != null) ?  m_weatherController.GetMeteorOffset() : 0.0f);
        float x = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Cos(anglez * Mathf.Deg2Rad);
        float y = scale * Mathf.Sin(anglex * Mathf.Deg2Rad) * Mathf.Sin(anglez * Mathf.Deg2Rad);
        float z = scale * Mathf.Cos(anglex * Mathf.Deg2Rad);
        return new Vector3(x , y , z);
    }

    public Vector2 GetPositionAngle(Vector3 pos, float offset)
    {
        float r = CurrentPlanet.transform.localScale.x + offset;
        float angle1 = Mathf.Acos(pos.z / r);
        float angle2 = Mathf.Atan(pos.y / pos.x);
        return new Vector2(angle1 , angle2);
    }
    #endregion

}
