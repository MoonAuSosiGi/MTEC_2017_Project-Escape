using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBooster : MonoBehaviour {

    #region Player Booster INFO
    [SerializeField] private Transform m_target = null;
    #endregion

    #region Unity Method
    void FixedUpdate()
    {
        transform.position = m_target.transform.position;
    }
    #endregion
}
