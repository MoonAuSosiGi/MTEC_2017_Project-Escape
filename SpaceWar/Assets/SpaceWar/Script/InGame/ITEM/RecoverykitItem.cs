using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverykitItem : Item {

    #region RecoveryKitItem INFO

    [SerializeField] private Vector3 m_localSetPos = Vector3.zero;
    [SerializeField] private Vector3 m_localSetRot = Vector3.zero;
    [SerializeField] private Vector3 m_localSetScale = Vector3.zero;
    [SerializeField] private Vector3 m_sponeRotation = Vector3.zero;

    public Vector3 LOCAL_SET_POS { get { return m_localSetPos; } set { m_localSetPos = value; } }
    public Vector3 LOCAL_SET_ROT { get { return m_localSetRot; } set { m_localSetRot = value; } }
    public Vector3 LOCAL_SET_SCALE { get { return m_localSetScale; } set { m_localSetScale = value; } }
    public Vector3 SPONE_ROTATITON { get { return m_sponeRotation; } set { m_sponeRotation = value; } }

    #endregion

    #region RecoveryKit Method

    public void RecoveryKitHealEnd()
    {
        // 힐팩 사라짐
    }
    

    #endregion

}
