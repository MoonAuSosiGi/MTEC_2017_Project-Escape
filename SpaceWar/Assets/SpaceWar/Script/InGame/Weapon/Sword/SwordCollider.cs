using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordCollider : MonoBehaviour {

    #region SwordCollider_INFO
    private WeaponItem m_targetSword = null;
    private string m_baseHitEffect = null;
    private string m_otherHitEffect = null;

    public WeaponItem TARGET_SWORD { get { return m_targetSword; } set { m_targetSword = value; } }
    public string BASE_HIT_EFFECT { get { return m_baseHitEffect; } set { m_baseHitEffect = value; } }
    public string OTHER_HIT_EFFECT { get { return m_otherHitEffect; } set { m_otherHitEffect = value; } }
    
    #endregion
    
    void OnTriggerEnter(Collider col)
    {
        //// 여기서 이펙트 발생
        Debug.Log("test " + col.name);
        if (!col.CompareTag("Weapon") && !col.CompareTag("Bullet") && !col.CompareTag("DeathZone"))
        {
            if (col.CompareTag("PlayerCharacter"))
            {
                NetworkPlayer p = col.transform.GetComponent<NetworkPlayer>();
                
                if (p != null)
                {
                    if (BASE_HIT_EFFECT != null)
                        CreateEffect(BASE_HIT_EFFECT,col.transform);
                    if(m_targetSword.PLAYER.GetComponent<NetworkPlayer>() == null)
                        NetworkManager.Instance().C2SRequestPlayerDamage((int)p.m_hostID , p.m_userName , TARGET_SWORD.ITEM_ID , TARGET_SWORD.DAMAGE , transform.position);
                }
                else
                {
                    return;
                }
            }
            else if (string.IsNullOrEmpty(col.tag) || col.CompareTag("NonSpone"))
            {
                // 여기에 부딪치면 다른 이펙트를 보여준다.
                if (OTHER_HIT_EFFECT != null)
                    CreateEffect(OTHER_HIT_EFFECT , col.transform);
                else if (BASE_HIT_EFFECT != null)
                    CreateEffect(BASE_HIT_EFFECT , col.transform);
            }
            else
            {
                // 기타 오브젝트
                if (BASE_HIT_EFFECT != null)
                    CreateEffect(BASE_HIT_EFFECT , col.transform);
            }

        }

    }

    void CreateEffect(string path ,Transform target)
    {
        GameObject effect = GameObject.Instantiate(Resources.Load("Art/Resource/Effect/"+path)) as GameObject;
        effect.AddComponent<OneHitEffect>();
        effect.transform.parent = null;
        effect.transform.position = target.position;
    }
}
