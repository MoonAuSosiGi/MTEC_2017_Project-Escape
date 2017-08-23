using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOff : MonoBehaviour {

    public Bullet m_target = null;

    public void EffectOffCall()
    {
        if (m_target == null)
            return;
        m_target.gameObject.SetActive(false);
    }

}
