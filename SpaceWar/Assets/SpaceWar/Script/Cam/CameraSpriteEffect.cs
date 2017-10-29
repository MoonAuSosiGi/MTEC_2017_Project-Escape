using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpriteEffect : MonoBehaviour {

    #region Camera Sprite Effect INFO

    // Quad Renderer
    [SerializeField] private MeshRenderer m_renderer = null;

    // Target Material
    [SerializeField] private Material m_targetMaterial = null;

    private float m_endTime = 0.6f;
    [SerializeField] private Color m_StartColor = Color.white;
    [SerializeField] private Color m_TargetColor = Color.white;

    public Color START_COLOR_EFFECT { get { return m_StartColor; } set { m_StartColor = value; } }
    public Color TARGET_COLOR_EFFECT { get { return m_TargetColor; } set { m_TargetColor = value; } }
    #endregion

    #region Unity Method 
    void Awake()
    {
        if(m_targetMaterial != null)
        {
            m_targetMaterial.SetColor("_TintColor" , m_StartColor);
        }
    }
    #endregion

    #region Effect
    
    public void AlphaEffectStart()
    {
        if (m_targetMaterial == null)
            return;


        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , m_StartColor.a ,
            "to" , m_TargetColor.a ,
            "time", m_endTime,
            "onupdatetarget" , gameObject ,
            "onupdate" , "AlphaEffectUpdate" ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "AlphaEffectEnd"));
    }
    
    void AlphaEffectUpdate(float v)
    {
        if (m_targetMaterial == null)
            return;
        if (GameManager.Instance().WINNER == true)
        {
            iTween.Stop(gameObject);
            AlphaEffectEnd();
            return;
        }
        Color c = m_targetMaterial.GetColor("_TintColor");
        m_targetMaterial.SetColor("_TintColor" , new Color(c.r , c.g , c.b , v));
    }

    void AlphaEffectEnd()
    {
        m_targetMaterial.SetColor("_TintColor" , m_StartColor);
        gameObject.SetActive(false);
        //GameObject.Destroy(gameObject);
    }
    #endregion
}
