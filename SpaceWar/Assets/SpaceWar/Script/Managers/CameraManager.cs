using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraManager : Singletone<CameraManager>
{
    #region CameraManager INFO

    #region CamRotate
    public Transform Player; // 플레이어
    // 카메라 고정 오브젝트 2개 0 = 처음, 1 = 중간, 2 = 마지막
    // 3 = FPS Anchor  ( 카메라가 여기로 이동 )
    public Transform[] CamAnchor;

    public float[] CamRoateSpeed; // 카메라 회전속도 0 = X, 1 = Y
    public float[] CamDis; // 카메라 거리 조절 0 = 최소, 1 = 최대, 2 = 현재값
    public float CamZoomSpeed; // 카메라 줌인 줌아웃 속도

    public Vector2 CamAngle; // 현재 카메라 각도 X, Y

    public bool RotateNow = true;

    // 캐릭터 로테이트
    private bool m_playerRotate = true;
    public bool PLAYER_ROTATE { get { return m_playerRotate; } set { m_playerRotate = value; } }

    private bool m_fpsMode = false;

    public bool FPS_MODE
    {
        get { return m_fpsMode; }
        set
        {
            m_fpsMode = value;

            //Debug.Log("FPS MODE " + m_fpsMode);

            if (m_fpsMode)
            {
                transform.SetParent(CamAnchor[3] , false);
                transform.localEulerAngles = new Vector3(0.0f , 0.0f , 0.0f);
                transform.localPosition = new Vector3(0.0f , 0.0f , 0.0f);
                Camera.main.fieldOfView = 70.0f;

            }
            else
            {
                Camera.main.fieldOfView = 20.0f;
                transform.SetParent(CamAnchor[2] , false);
            }
        }
    }
    #endregion

    #region Effect 
    [SerializeField]
    private GameObject m_damageEffect = null;
    [SerializeField]
    private GameObject m_oxyEffect = null;
    [SerializeField]
    private VignetteAndChromaticAberration m_hitBlur = null;

    // 기존 재생되고 있는 이펙트
    private GameObject m_curDamageEffect = null;
    private GameObject m_curOxyEffect = null;

    // 아예 없을 때 이펙트
    private GameObject m_notEnoughHpEffect = null;
    private GameObject m_notEnoughOxyEffect = null;

    // 이펙트 체크
    private bool m_deadEffectShow = false;
    public bool DEAD_EFFECT_SHOW { get { return m_deadEffectShow; } set { m_deadEffectShow = value; } }

    #endregion

    #region Rader
    [SerializeField] private RaderObject m_rader = null;
    public RaderObject RADER { get { return m_rader; } }
    #endregion

    #endregion

    #region UnityMethod
    void Start()
    {

        CamSet();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // AnchorPlanet.MainCam = this.transform;


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ShowHitEffect();
        }
        if ((GameManager.Instance() != null && GameManager.Instance().PLAYER != null && GameManager.Instance().PLAYER.m_hp <= 0.0f))
            return;

        if (RotateNow)
        {
            CamRotateCode();
        }
        // ZoomProcess();
        CursorManager();


    }
    #endregion
    #region Camera Move Method

    void CursorManager()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.visible)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    private void CamRotateCode()  // 카메라 회전
    {

        if (PLAYER_ROTATE && !Input.GetKey(KeyCode.LeftAlt))
            Player.Rotate(0 , Input.GetAxis("Mouse X") * CamRoateSpeed[0] , 0); // 플레이어 캐릭터(카메라 부모 오브젝트) X축 회전
        else
        {
            // transform.RotateAround(Player.transform.position , Vector3.down, Input.GetAxis("Mouse X") * CamRoateSpeed[0]);
        }

        // Y축 회전 시작
        if (CamAngle.y <= 27f && CamAngle.y >= -62f) // 최대,최소 각 체크
        {
            CamAngle.y -= Input.GetAxis("Mouse Y") * CamRoateSpeed[1]; // Y축 회전
        }

        if (CamAngle.y < -62f) // 최대, 최소 각 보정
        {
            CamAngle.y = -62f;
        }
        else if (CamAngle.y > 27f)
        {
            CamAngle.y = 27f;
        }
        // Y축 회전 끝


        CamAnchor[0].localRotation = Quaternion.Euler(CamAngle.y , 0 , 0); // 회전 적용
        if (!m_fpsMode)
            CamLastPosSet(); // 카메라가 오브젝트뒤로 나가는 현상 방지

    }

    public void CamSet() // 카메라 초기 위치, 각도 세팅
    {
        this.transform.parent = CamAnchor[2]; // 부모 설정
        this.transform.localPosition = Vector3.zero; // 위치 설정
        this.transform.localRotation = Quaternion.Euler(Vector3.zero); // 각도 설정
        this.enabled = true;
        //    FPS_MODE = true;
    }


    void OnGUI()
    {

        Event CheckInput = Event.current; // 이벤트 저장

        if (CheckInput.type == EventType.ScrollWheel) // 마우스 휠 이벤트가 맞는지 타입 확인
        {
            CamDis[2] -= Input.GetAxis("Mouse ScrollWheel") * CamZoomSpeed; // 줌인 줌아웃


            // 줌인 최대
            if (CamDis[2] < CamDis[0]) // 줌인 줌아웃 최대, 최소 거리 보정
            {

                CamDis[2] = CamDis[3];//CamDis[2] = CamDis[0];

                FPS_MODE = true;

            }
            // 줌아웃 최대
            else if (CamDis[2] > CamDis[1])
            {

                CamDis[2] = CamDis[1];
                FPS_MODE = false;
            }
            if (!m_fpsMode)
                CamLastPosSet(); // 카메라가 오브젝트뒤로 나가는 현상 방지
        }
    }


    private void CamLastPosSet() // 카메라가 오브젝트뒤로 나가는 현상 방지
    {
        RaycastHit Hitinfo;
        Physics.Raycast(CamAnchor[1].position , CamAnchor[2].rotation * Vector3.back , out Hitinfo , (float)Vector3.Distance(CamAnchor[1].position , Vector3.Lerp(CamAnchor[1].position , CamAnchor[2].position , CamDis[2]))); // 레이캐스트
        //Debug.DrawRay(CamAnchor[1].position, CamAnchor[2].rotation * Vector3.back * Vector3.Distance(CamAnchor[1].position, Vector3.Lerp(CamAnchor[1].position, CamAnchor[2].position, CamDis[2])), Color.red, 0.1f);
        if (Hitinfo.transform != null &&
            (Hitinfo.transform.CompareTag("NoCameraCollider")
            || Hitinfo.transform.CompareTag("ShelterDoor")
            || Hitinfo.transform.CompareTag("Weapon")
            || Hitinfo.transform.CompareTag("ItemBox")
            || Hitinfo.transform.CompareTag("PlayerCharacter")
            || Hitinfo.transform.CompareTag("Satellite")))
        {
            this.transform.position = Vector3.Lerp(CamAnchor[1].position , CamAnchor[2].position , CamDis[2]);
            return;
        }
        if (Hitinfo.point == Vector3.zero)
        {
            this.transform.position = Vector3.Lerp(CamAnchor[1].position , CamAnchor[2].position , CamDis[2]);
        }
        else
        {
            this.transform.position = Hitinfo.point;
        }
    }
    #endregion

    #region Camera Effect Method

    public void ShowDeadCameraEffect()
    {
        Invoke("DeadCameraEffect" , 3.0f);
    }
    public void HideDeadCameraEffect()
    {
        var curver = this.GetComponent<ColorCorrectionCurves>();
        curver.enabled = false;
    }

    void DeadCameraEffect()
    {
        var curver = this.GetComponent<ColorCorrectionCurves>();
        curver.enabled = true;

        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , 1.0f , "to" , 0.0f ,
            "time" , 3.0f , "onupdatetarget" , gameObject ,
            "onupdate" , "DeadCameraEffectUpdate",
            "oncompletetarget",gameObject,
            "oncomplete","DeadCameraEffectEnd"));
    }

    void DeadCameraEffectEnd()
    {
        DEAD_EFFECT_SHOW = true;
    }

    void DeadCameraEffectUpdate(float v)
    {
        var curver = this.GetComponent<ColorCorrectionCurves>();
        curver.saturation = v;
    }

    public void ShowMeteorCameraEffect(float distance)
    {
        float[] check = GameManager.Instance().METEOR_EFFECT_DISTANCE;
        Animator animator = transform.parent.GetComponent<Animator>();

        for (int i = 0; i < check.Length; i++)
        {
            if(check[i] >= distance)
            {
                // 엑셀에는 하나 부족한 상태로 들어감
                // 그러므로 check.length + 1의 갯수다 실제론
                GameManager.Instance().m_inGameUI.ShowDebugLabel("메테오 레벨 " + (check.Length + 1 - i) + " distance " + distance);
                animator.SetInteger("METEOR" , (check.Length + 1 - i));
                return;
            }
        }
        // 이곳을 오면 나머지 놈들
        animator.SetInteger("METEOR" , (check.Length + 1));
    }

    public void ShowHitEffect(bool damageEffect = true)
    {
        if (GameManager.Instance().WINNER)
            return;

        GameObject effect = null;

        if (damageEffect)
        {
            EffectRemove(m_curDamageEffect);

            m_curDamageEffect = GameObject.Instantiate(m_damageEffect);
            effect = m_curDamageEffect;
        }
        else
        {
            EffectRemove(m_curOxyEffect);

            m_curOxyEffect = GameObject.Instantiate(m_oxyEffect);
            effect = m_curOxyEffect;
        }
        effect.SetActive(true);
        effect.transform.position = new Vector3(0.0f , 0.0f , 0.533f);
        effect.transform.SetParent(transform.GetChild(2) , false);
        

        CameraSpriteEffect e = effect.GetComponent<CameraSpriteEffect>();
        e.AlphaEffectStart();

        if (iTween.Count(gameObject) > 0)
        {
            iTween.Stop(gameObject);
        }

        m_hitBlur.enabled = true;
        m_hitBlur.blurDistance = 1.0f;
        m_hitBlur.chromaticAberration = 20.0f;

        HideHitEffect(effect);
        //Invoke("HideHitEffect" , 0.1f);

    }

    public void HideHitEffect()
    {
        EffectRemove(m_curDamageEffect);
        EffectRemove(m_curOxyEffect);
    }

    public void HideHitEffect(GameObject effect)
    {
        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , 1.0f ,
            "to" , 0.0f ,
            "onupdatetarget" , gameObject ,
            "time" , 0.6f ,
            "onupdate" , "BlurHide" ,
            "oncompletetarget" , gameObject ,
            "oncompleteparams" , effect ,
            "oncomplete" , "HitHideEnd"));

        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , 20.0f ,
            "to" , 0.0f ,
            "onupdatetarget" , gameObject ,
            "time" , 0.6f ,
            "onupdate" , "BlurAbHide"));

    }

    // 항상 체력 부족 띄워놓기
    public void ShowNotEnoughHPEffect()
    {
        GameManager.Instance().m_inGameUI.ShowDebugLabel("Show Not Enough HP Effect");
        if (m_notEnoughHpEffect != null)
        {
            m_notEnoughHpEffect.SetActive(true);
            return;
        }
        m_notEnoughHpEffect = GameObject.Instantiate(m_damageEffect);
        m_notEnoughHpEffect.transform.position = new Vector3(0.0f , 0.0f , 0.533f);
        m_notEnoughHpEffect.transform.SetParent(transform.GetChild(2) , false);

        GameObject.Destroy(m_notEnoughHpEffect.GetComponent<CameraSpriteEffect>());
        m_notEnoughHpEffect.SetActive(true);

    }

    // 항상 산소 부족 띄워놓기
    public void ShowNotEnoughOxyEffect()
    {
        GameManager.Instance().m_inGameUI.ShowDebugLabel("Show Not Enough Oxy Effect");
        if (m_notEnoughOxyEffect != null)
        {
            m_notEnoughOxyEffect.SetActive(true);
            return;
        }
        m_notEnoughOxyEffect = GameObject.Instantiate(m_oxyEffect);
        m_notEnoughOxyEffect.transform.position = new Vector3(0.0f , 0.0f , 0.533f);
        m_notEnoughOxyEffect.transform.SetParent(transform.GetChild(2) , false);
        GameObject.Destroy(m_notEnoughOxyEffect.GetComponent<CameraSpriteEffect>());
        m_notEnoughOxyEffect.SetActive(true);
    }

    // 항상 띄워놓는 ui 끌때
    public void HideNotEnoughHpEffect()
    {
        if(m_notEnoughHpEffect != null)
        {
            m_notEnoughHpEffect.SetActive(false);
        }
    }

    // 항상 띄워놓는 ui 끌때
    public void HideNotEnoughOxyEffect()
    {
        if (m_notEnoughOxyEffect != null)
        {
            m_notEnoughOxyEffect.SetActive(false);
        }
    }

    void BlurHide(float v)
    {
        m_hitBlur.blurDistance = v;
    }

    void BlurAbHide(float v)
    {
        m_hitBlur.chromaticAberration = v;
    }

    public void EffectHitExit()
    {
        transform.GetChild(2).gameObject.SetActive(false);
    }

    public void EffectRemove(GameObject effect)
    {
        if (effect != null)
        {
            GameObject.Destroy(effect);
        }
    }

    void HitHideEnd(GameObject effect)
    {
        EffectRemove(effect);
        m_hitBlur.enabled = false;
    }
    #endregion
}
