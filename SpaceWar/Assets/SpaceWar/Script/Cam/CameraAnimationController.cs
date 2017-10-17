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

    // 메테오 부딪힘 이펙트 종료
    public void MeteorEffectAnimationEnd()
    {
        this.GetComponent<Animator>().SetInteger("METEOR" , 0);
    }
    #endregion
}
