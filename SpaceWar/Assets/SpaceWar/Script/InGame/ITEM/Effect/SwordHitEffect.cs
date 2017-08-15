using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordHitEffect : MonoBehaviour {

    #region SwordHitEffect
    [SerializeField] private WeaponItem m_targetWeapon = null;
    // 애니메이션이 끝났다.
    public void SwordHitAnimationEnd()
    {
        Debug.Log("Animation End");
        m_targetWeapon.SwordHitEffectEnd();
        //gameObject.SetActive(false);
    }
    #endregion
}
