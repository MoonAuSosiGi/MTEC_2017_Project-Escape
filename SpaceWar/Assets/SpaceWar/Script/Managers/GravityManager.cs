using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float GRAVITY_POWER { get { return m_power; } set { m_power = value; } }
    public GameObject GRAVITY_TARGET { get { return m_targetObject.gameObject; } }

    public GameObject CurrentPlanet { get { return m_planetList[m_curPlanetIndex]; } }
    #endregion

    #region UnityMethod
    void FixedUpdate()
    {
        GravityProcess();
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
    #endregion

}
