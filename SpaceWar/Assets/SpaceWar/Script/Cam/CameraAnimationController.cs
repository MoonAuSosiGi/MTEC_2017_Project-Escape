using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimationController : MonoBehaviour {

    #region Camera Animation Controller INFO
    [SerializeField] private PlayerController m_playerController = null;
    #endregion

    #region Animation Event
    public void DamageAnimationEnd()
    {
        m_playerController.CameraDamageAnimationEnd();
    }
    #endregion
}
