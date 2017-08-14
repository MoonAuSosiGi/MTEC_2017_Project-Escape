using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlAttackTiming : MonoBehaviour {

    #region PlayerControlAttackTiming_INFO
    [SerializeField] private PlayerController m_player = null;
    #endregion

    public void ItsAttackTime()
    {
        m_player.AttackAnimationEvent();
    }

    public void AttackEnd()
    {
        m_player.AttackAnimation(0);
    }
}
