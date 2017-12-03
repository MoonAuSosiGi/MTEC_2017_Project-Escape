using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TimeForEscape.Object;

public class PlayerController : MonoBehaviour {

    #region PlayerController_INFO ---------------------------------------------------------------------------
    //싱글 모드일때 처리 -- 171029 기준으로 아직 미동작 따로 처리 해야함
    [SerializeField]  private bool m_signleMode = false;
    [SerializeField]
    private PlayerControlAttackTiming m_animationEvent = null;
    // 현재 보고 있는 옵저버 인덱스
    private int m_observerIndex = -1;

    // 중력 처리를 받기 위한 리지드바디
    private Rigidbody m_rigidBody = null;

    // 애니메이션 제어용 애니메이터
    [SerializeField] private Animator m_animator = null;
    public Animator ANIMATOR { get { return m_animator; } }

    // 데스존 사망했을 때 더이상 살아나지 못함
    private bool m_deathZoneDead = false;
    public bool IS_DEATHZONE_DEAD
    {
        get { return m_deathZoneDead; }
        set { m_deathZoneDead = value; }
    }

    #region HIT EFFECT ------------------------------------------------------------------------------------
    // Hit Effect 전용 렌더러 받기
    [SerializeField]
    private SkinnedMeshRenderer m_renderer = null;

    [SerializeField] private Material m_hitMaterial = null;
    [SerializeField] private Material m_originMaterial = null;
    [SerializeField] private Material m_raderMaterial = null;

    public SkinnedMeshRenderer RENDERER { get { return m_renderer; } }
    public Material HIT_MATERIAL { get { return m_hitMaterial; } }
    public Material ORIGIN_MATERIAL { get { return m_originMaterial; } }
    public Material RADER_MATERIAL {  get { return m_raderMaterial; } }
    #endregion -------------------------------------------------------------------------------------------

    #region Equip Inven System ----------------------------------------------------------------------------
    // 착용 중인 아이템 다섯개
    [SerializeField] Item[] m_equipItems = new Item[5];
    // 그 전에 착용한 아이템 인덱스
    private int m_prevEquipItem = -1;
    // 현재 착용중인 아이템 인덱스
    private int m_curEquipItem = 0;

    // 현재 착용중인 아이템 인덱스는 다른 곳에서도 사용가능해야함
    public int CUR_EQUIP_INDEX { get { return m_curEquipItem; } }
    // 현재 착용중인 무기
    public WeaponItem CURRENT_WEAPON { get { return m_equipItems[m_curEquipItem] as WeaponItem; } }    // 무기

    #endregion --------------------------------------------------------------------------------------------

    #region Camera Anchor ---------------------------------------------------------------------------------
    // 카메라 앵커용 
    public Transform m_camAnchor1 = null;
    public Transform m_camAnchor2 = null;
    public Transform m_camAnchor3 = null;
    public Transform m_camAnchor4 = null;
    public Animator m_camAnimator = null;
    #endregion

    #region Player Info -----------------------------------------------------------------------------------

    #region Player Status Data -----------------------------------------------------------------------------
    // 걷는 속도 -- 테이블로 조작
    [SerializeField] private float m_walkSpeed;
    // 뛰는 속도 -- 테이블로 조작
    [SerializeField] private float m_runSpeed;
    // 대시 속도 -- 테이블로 조작
    [SerializeField] private float m_dashSpeed = 3.0f;
    // 대시 유지 시간 -- 테이블로 조작
    [SerializeField] private float m_dashTick = 1.0f;
    // 대시 사용시 사용 산소 -- 테이블로 조작
    [SerializeField] private float m_dashUseOxy = 20.0f;
    // 점프 파워 -- 테이블로 조작 
    [SerializeField] private float m_jumpHeight = 3.0f;
    // 점프 유지 시간 -- 테이블로 조작
    [SerializeField] private float m_jumpTick = 1.0f;
    // 점프 그래프
    [SerializeField] private AnimationCurve m_jumpCurve = null;
    // 대시 그래프
    [SerializeField] private AnimationCurve m_dashCurve = null;

    // 속도
    public float WALK_SPEED { get { return m_walkSpeed; } set { m_walkSpeed = value; } }
    public float RUN_SPEED { get { return m_runSpeed; } set { m_runSpeed = value; } }
    #endregion -----------------------------------------------------------------------------------------------

    #region Use Effect -------------------------------------------------------------------------------------
    // F 키 이펙트
    
    
    [SerializeField] private Transform m_forward = null;
    #endregion ---------------------------------------------------------------------------------------------

    #region Dash Effect ------------------------------------------------------------------------------------
    // 대시 이펙트
    [SerializeField] private GameObject m_dashEffect = null;

    // 대시 진행 방향
    private Vector3 m_dashDirection = Vector3.forward;

    // 현재 대시 중인지 (애니메이션과는 별개)
    private bool m_isDash = false;

    // NetworkPlayer 에서 가져올 수 있어야 함
    public GameObject DASH_EFFECT { get { return m_dashEffect; } }
    #endregion ---------------------------------------------------------------------------------------------

    #region Network Name -----------------------------------------------------------------------------------
    // 유저네임  
    [SerializeField] private TextMesh m_userNameUI = null;
    public TextMesh USERNAME_UI { get { return m_userNameUI; } }
    public void SetUserName(string name) { m_userNameUI.text = name; }
    #endregion ---------------------------------------------------------------------------------------------

    #region Effect Anchor ----------------------------------------------------------------------------------
    // EFFECT 를 띄우기 위한 위치
    // 머리 
    [SerializeField] private GameObject m_useEffectHeadAnchor = null;
    // 손
    [SerializeField] private GameObject m_useEffectHandAnchor = null;

    //머리는 네트워크상의 캐릭터 HP 바를 띄우기 위한 것으로도 사용
    public GameObject HEAD_ANCHOR { get { return m_useEffectHeadAnchor; } }
    // 무기 앵커
    [SerializeField] private GameObject m_weaponEquipAnchor = null;
    public GameObject WEAPON_ANCHOR { get { return m_weaponEquipAnchor; } }
    #endregion ---------------------------------------------------------------------------------------------

    #region COOL TIME --------------------------------------------------------------------------------------
    // 공격 쿨타임
    private float m_lastCoolTime = 0.0f;

    // 데미지 쿨타임 -- 일반적으로 메테오 쿨타임에 사용
    private float m_damageCoolTime = 0.0f;

    // 메테오 데미지 쿨타임
    private float m_meteorCoolTime = 0.0f;
    #endregion ----------------------------------------------------------------------------------------------

    #region Animator ---------------------------------------------------------------------------------------
    // 모든 애니메이션 컨트롤러  // 단검 - 건 - 라이플 - 로켓런처 - 기타
    [SerializeField]
    private List<AnimationController> m_animationControllerList = new List<AnimationController>();

    [Serializable]
    public class AnimationController
    {
        public AnimationType m_type = AnimationType.ANI_BAREHAND;
        public RuntimeAnimatorController m_controller = null;
    }

    public enum AnimationType
    {
        ANI_BAREHAND = 0,
        ANI_ETC,
        ANI_GUN01,
        ANI_GUN02,
        ANI_MELEE,
        ANI_ROCKETLAUNCHER
    }
    #endregion ---------------------------------------------------------------------------------------------

    #region Action Control ---------------------------------------------------------------------------------
    private bool m_isMoveAble = true;
    private bool m_isJumpAble = true;
    private bool m_isAttackAble = true;
    //shelter 안인지?
    private bool m_isShelter = false;

    public bool IS_MOVE_ABLE { get { return m_isMoveAble; } set { m_isMoveAble = value; } }
    public bool IS_JUMP_ABLE { get { return m_isJumpAble; } set { m_isJumpAble = value; } }
    public bool IS_ATTACK_ABLE { get { return m_isAttackAble; } set { m_isAttackAble = value; } }
    public bool IS_SHELTER { get { return m_isShelter; } set { m_isShelter = value; } }
    #endregion ---------------------------------------------------------------------------------------------

    #region Interaction Object -----------------------------------------------------------------------------
    // 상호작용 오브젝트와 관련된 것

    // 현재 근처에 있는 아이템
    private GameObject m_nearItem = null;

    // 근처에 있는 산소 충전기
    private OxyCharger m_nearOxyCharger = null;
    // 근처에 있는 아이템 박스
    private ItemBox m_nearItemBox = null;
    // 근처에 있는 쉘터
    private Shelter m_nearShelter = null;

    #region SpaceShip ---------------------------------------------------------------------------------------
    // 근처에 있는 우주선
    private SpaceShip m_nearSpaceShip = null;
    // 우주선 사용 요청을 보냈는지
    private bool m_spaceShipRequest = false;
    // 네트워크 매니저에서 받아와야 함
    public bool SPACESHIP_REQUEST {
        get { return m_spaceShipRequest; }
        set {
            m_spaceShipRequest = value;
            if (m_spaceShipRequest == false)
                m_isMoveAble = true;
        } }
    #endregion ----------------------------------------------------------------------------------------------
    #region Player Oxy Charger ------------------------------------------------------------------------------
    // 산소 충전 UI
    private float m_targetOxy = 0.0f;
    private float m_plusOxy = 0.0f;
    private bool m_oxyChargeRequest = false;
    // 네트워크 매니저에서 받아와야함
    public bool OXYCHARGER_REQUEST
    {
        get { return m_oxyChargeRequest; }
        set
        {
            m_oxyChargeRequest = value;
            if (m_oxyChargeRequest == false)
            {
                OxyChargerControlCancle();
            }
        }
    }

    #endregion ----------------------------------------------------------------------------------------------

    #endregion ----------------------------------------------------------------------------------------------

    #endregion ----------------------------------------------------------------------------------------------

    #region OXY ---------------------------------------------------------------------------------------------
    // IDLE 산소 소모량
    private float m_useOxyIDLE = 0.1f;
    // WALK 산소 소모량
    private float m_useOxyWALK = 0.2f;
    // RUN 산소 소모량 
    private float m_useOxyRUN = 1.5f;
    // 산소 충전기에서 충전되는 산소량
    private float m_chargeOxy = 10.0f;

    // 기본으로 지속되는 산소 소모
    void UseOxy()
    {
        if (NetworkManager.Instance() != null)
        {
            float useOxy = 0.1f;//UnityEngine.Random.Range(0.1f , 10.0f);

            bool idle = (m_currentDir == PlayerMoveDir.NONE);
            bool run = (m_currentDir == PlayerMoveDir.RUN_BACK || m_currentDir == PlayerMoveDir.RUN_LEFT
                || m_currentDir == PlayerMoveDir.RUN_RIGHT || m_currentDir == PlayerMoveDir.RUN_FOWARD
                || m_currentDir == PlayerMoveDir.RUN_FOWARD_LEFT || m_currentDir == PlayerMoveDir.RUN_FOWARD_RIGHT
                || m_currentDir == PlayerMoveDir.RUN_BACK_LEFT || m_currentDir == PlayerMoveDir.RUN_BACK_RIGHT);
            if (idle)// && m_currentWeapon == null)
            {
                useOxy = m_useOxyIDLE;
            }
            else if (run)//&& m_currentWeapon == null)
                useOxy = m_useOxyRUN;
            else if (idle == false && run == false)
                useOxy = m_useOxyWALK;

            if (GameManager.Instance().PLAYER.m_hp - useOxy >= 0.0f)
                NetworkManager.Instance().C2SRequestPlayerUseOxy(GameManager.Instance().PLAYER.m_name , useOxy);
        }
    }
    #endregion

    #region Control Key -------------------------------------------------------------------------------------
    // 기본 이동 위 왼쪽 아래 오른쪽
    [SerializeField] private KeyCode m_Up = KeyCode.W;
    [SerializeField] private KeyCode m_Left = KeyCode.A;
    [SerializeField] private KeyCode m_Down = KeyCode.S;
    [SerializeField] private KeyCode m_Right = KeyCode.D;
    [SerializeField] private KeyCode m_Get = KeyCode.F;
    [SerializeField] private KeyCode m_throwKey = KeyCode.T;

    // 레이더
    [SerializeField] private KeyCode m_Rader = KeyCode.R;

    // 대쉬
    [SerializeField] private KeyCode m_dashKey = KeyCode.LeftControl;

    // 인벤토리
    [SerializeField] private KeyCode m_inven1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode m_inven2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode m_inven3 = KeyCode.Alpha3;
    [SerializeField] private KeyCode m_inven4 = KeyCode.Alpha4;
    [SerializeField] private KeyCode m_inven5 = KeyCode.Alpha5;

    // 점프
    [SerializeField] private KeyCode m_Jump = KeyCode.Space;

    // 대시 키
    [SerializeField] private KeyCode m_Dash = KeyCode.LeftShift;

    // 인벤 자동 장착 키
    // 근거리
    [SerializeField] private KeyCode m_ShortRangeWeaponEquip = KeyCode.Q;
    // 원거리
    [SerializeField] private KeyCode m_LongRangeWeaponEquip = KeyCode.E;
    // 기타 무기
    [SerializeField] private KeyCode m_EtcWeaponEquip = KeyCode.C;

    // 인벤토리 키
    [SerializeField] private KeyCode m_InventoryActive = KeyCode.I;

    // 무기 사용
    [SerializeField] private KeyCode m_AttackKey = KeyCode.Mouse0;

    #endregion ---------------------------------------------------------------------------------------------

    #region Player Rotate -----------------------------------------------------------------------------------
    public enum PlayerMoveDir
    {
        NONE,                   // IDLE
        WALK_FOWARD,            // up
        WALK_FOWARD_LEFT,       // up + left
        WALK_FOWARD_RIGHT,      // up + right
        WALK_LEFT,              // left
        WALK_RIGHT,             // right
        WALK_BACK,              // down
        WALK_BACK_LEFT,         // down + left
        WALK_BACK_RIGHT,        // down + right
        RUN_FOWARD,             // run up
        RUN_FOWARD_LEFT,        // run up + left
        RUN_FOWARD_RIGHT,       // run up + right
        RUN_LEFT,               // run left
        RUN_RIGHT,              // run right
        RUN_BACK,               // run back
        RUN_BACK_LEFT,          // run back + left
        RUN_BACK_RIGHT          // run back + right
    }

    private PlayerMoveDir m_currentDir = PlayerMoveDir.NONE;
    #endregion --------------------------------------------------------------------------------------------

    #region Player Animation Var ----------------------------------------------------------------------------
    private int m_attackAniVal = 0;
    private int m_walkAniVal = 0;
    private int m_jumpAniVal = 0;
    private int m_interactionAniVal = 0;

    // 공격 애니메이션
    public int ATTACK_ANI_VALUE
    {
        get { return m_attackAniVal; }
        set
        {
            // 만약 기존에 재생중이었다면?
            if (m_attackAniVal != 0 && value == 0 && m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
            {
                if (m_equipItems[m_curEquipItem] != null)
                    (m_equipItems[m_curEquipItem] as WeaponItem).AnimationEventAttackEnd();
            }

            m_attackAniVal = value;

            // 공격 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if (m_attackAniVal != 0)
            {
                WALK_ANI_VALUE = 0;
                JUMP_ANI_VALUE = 0;
                INTERACTION_ANI_VALUE = 0;
            }
            else if (m_lastCoolTime <= 0)
                m_isAttackAble = true;

            AttackAnimation(m_attackAniVal);
        }
    }

    // 이동 애니메이션 값
    public int WALK_ANI_VALUE
    {
        get { return m_walkAniVal; }
        set
        {
            //if (m_isJumpAble == false)
            //    return;
            m_walkAniVal = value;

            // 이동 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if (m_walkAniVal != 0)
            {
                ATTACK_ANI_VALUE = 0;
                JUMP_ANI_VALUE = 0;
                INTERACTION_ANI_VALUE = 0;
            }
            WalkAnimation(m_walkAniVal);
        }
    }

    // 점프 애니메이션 값 
    public int JUMP_ANI_VALUE
    {
        get { return m_jumpAniVal; }
        set
        {
            m_jumpAniVal = value;
            //if (m_jumpAniVal == 1 && ATTACK_ANI_VALUE != 0)
            //    AttackAnimationEnd();
            // 점프 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if (m_jumpAniVal != 0)
            {
                ATTACK_ANI_VALUE = 0;
                WALK_ANI_VALUE = 0;
                INTERACTION_ANI_VALUE = 0;
            }
            JumpAnimation(m_jumpAniVal);
        }
    }

    // 상호작용 애니메이션 값
    public int INTERACTION_ANI_VALUE
    {
        get { return m_interactionAniVal; }
        set
        {
            m_interactionAniVal = value;
            //if (m_interactionAniVal == 1 && ATTACK_ANI_VALUE != 0)
            //    AttackAnimationEnd();
            if (m_interactionAniVal != 0)
            {
                ATTACK_ANI_VALUE = 0;
                WALK_ANI_VALUE = 0;
                JUMP_ANI_VALUE = 0;
            }
            InteractionAnimation(m_interactionAniVal);
        }
    }

    #endregion --------------------------------------------------------------------------------------------
    
    #region SOUND--------------------------------------------------------------------------------------------
    public AudioSource m_playerSoundSource = null;
    public AudioSource m_playerLoopSource = null;
    public AudioClip m_oxyChargerUseSound = null;
    public AudioClip m_shelterUseSound = null;
    public AudioClip m_itemGetSound = null;

    // 줍는 사운드
    public AudioClip m_pickSound = null;

    // 데미지 사운드
    public AudioClip m_damageHit = null;
    // 죽는 사운드
    public AudioClip m_deadSound = null;
    // 산소 부족 / 체력 부족 사운드
    public AudioClip m_notEnoughHPSound = null;
    public AudioClip m_notEnoughOxySound = null;
    // 부족 -> 회복 사운드
    public AudioClip m_healSound = null;
    // 산소 회복 소리
    public AudioClip m_oxyChargeSound = null;
    // 산소 충전기 ㅈㅈ 
    public AudioClip m_oxyChargeDown = null;

    // 우주선 관련 사운드
    #region Space Ship Sound -------
    public AudioClip m_spaceShipChargeSound = null;
    public AudioClip m_spaceShipChargeFail = null;

    #endregion
    #endregion

    #region Meteor Damage -----------------------------------------------------------------------------------
    private float m_meteorHitDamage = 100.0f; // 메테오 직격타 데미지
    private float m_meteorDamage = 10.0f; // 메테오 공간에 있을 때 데미지 
    #endregion

    #endregion ----------------------------------------------------------------------------------------------

    #region Setup ###########################################################################################

    // 테이블 값 세팅
    public void TableSetup()
    {
        m_forward = transform.GetChild(0);
        m_dashTick = GameManager.Instance().GetGameTableValue(GameManager.DASH_TICK);
        m_dashSpeed = GameManager.Instance().GetGameTableValue(GameManager.DASH_SPEED);
        m_jumpHeight = GameManager.Instance().GetGameTableValue(GameManager.JUMP_POWER);
        m_jumpTick = GameManager.Instance().GetGameTableValue(GameManager.JUMP_TICK);
        m_useOxyIDLE = GameManager.Instance().GetGameTableValue(GameManager.USEOXY_IDLE);
        m_useOxyRUN = GameManager.Instance().GetGameTableValue(GameManager.USEOXY_RUN);
        m_useOxyWALK = GameManager.Instance().GetGameTableValue(GameManager.USEOXY_WALK);
        m_dashUseOxy = GameManager.Instance().GetGameTableValue(GameManager.DASH_USE_OXY);
        m_chargeOxy = GameManager.Instance().GetGameTableValue(GameManager.OXY_CHARGER_USE);
        m_meteorHitDamage = GameManager.Instance().GetGameTableValue(GameManager.METEOR_HIT_DAMAGE);
        m_meteorDamage = GameManager.Instance().GetGameTableValue(GameManager.METEOR_DAMAGE);
        m_damageCoolTime = GameManager.Instance().GetGameTableValue(GameManager.DAMAGE_COOLTIME);

        GameManager.Instance().PLAYER.m_player = this;
        GameManager.Instance().PLAYER.WEIGHT = 0.0f;

    }

    // 3인칭 카메라 세팅
    public void SetCameraThirdPosition()
    {
        CameraManager.Instance().Player = transform;
        CameraManager.Instance().CamAnchor[0] = m_camAnchor1;
        CameraManager.Instance().CamAnchor[1] = m_camAnchor2;
        CameraManager.Instance().CamAnchor[2] = m_camAnchor3;
        CameraManager.Instance().CamAnchor[3] = m_camAnchor4;
        CameraManager.Instance().CamSet();

    }

    #endregion ##############################################################################################

    #region Player UI ---------------------------------------------------------------------------------------
    // 이 함수는 Update 에서 호출되어야 함
    void RotatePlayerName()
    {
        m_userNameUI.transform.rotation = this.transform.rotation;
    }

    void ShowPlayerName(bool isShow)
    {
        m_userNameUI.gameObject.SetActive(isShow);
    }

    #endregion ----------------------------------------------------------------------------------------------

    #region Unity Method ####################################################################################

    void Start()
    {
        m_rigidBody = this.GetComponent<Rigidbody>();

        GravityManager.Instance().SetGravityTarget(m_rigidBody);
        InvokeRepeating("UseOxy" , 1.0f , 1.0f);
        if (m_signleMode)
            SetCameraThirdPosition();
    }

    void FixedUpdate()
    {
        if (IS_DEATH())
        {
            LoopAudioStop();
            ObserverControl();

            if(GameManager.CURRENT_GAMEMODE == GameManager.GameMode.DEATH_MATCH 
                && CameraManager.Instance().DEAD_EFFECT_SHOW
                && IsInvoking("DeathMatchRebirth") == false
                && m_deathZoneDead == false)
            {
                Invoke("DeathMatchRebirth" , 3.0f);
            }
            else if(GameManager.CURRENT_GAMEMODE == GameManager.GameMode.DEATH_MATCH
                && CameraManager.Instance().DEAD_EFFECT_SHOW
                && m_deathZoneDead == true)
            {
                // 여기서 데스매치 끝났는지 체크
                var list = NetworkManager.Instance().NETWORK_PLAYERS;

                int checker = 0;
                for(int i = 0; i < list.Count; i ++)
                {
                    if(list[i].IS_DEATHZONE_DEATH == true)
                    {
                        checker++;
                    }
                }
                if(checker == list.Count)
                {
                    //끝
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Space_1Result");
                }
            }

            return;
        }

        if (IS_MOVE_ABLE)
        {
            MoveProcess();
        }

        if (IS_JUMP_ABLE)
            JumpProcess();

        if (IS_ATTACK_ABLE && m_equipItems[m_curEquipItem] != null &&
            m_equipItems[m_curEquipItem].EQUIP_STATE == true
            && m_equipItems[m_curEquipItem].ITEM_TYPE != Item.ItemType.ETC_RECOVERY)
        {
            AttackProcess();

        }

        RaderProcess();
        RotatePlayerName();
        HealPackProcess();
        GetItemProcess();
        ChangeItemProcess();
        //ControlWeaponObjectProcess();
        ControlWeaponObjectThrowProcess();
        ControlObjectProcess();
        Vector3 dir = m_forward.position - transform.GetChild(0).position;
        dir.Normalize();
        Debug.DrawLine(transform.position , dir , Color.red);


    }

    #endregion ##############################################################################################

    #region Player Rader ------------------------------------------------------------------------------------
    void RaderProcess()
    {
        if(Input.GetKey(m_Rader))
        {
            if (CameraManager.Instance().RADER.IS_SHOW == false)
                CameraManager.Instance().RADER.ShowRader();
            else
                CameraManager.Instance().RADER.HideRader();
        }
    }
    #endregion ----------------------------------------------------------------------------------------------

    #region Player Move --------------------------------------------------------------------------------------

    void MoveProcess()
    {
        bool dash = Input.GetKey(m_Dash);
        bool realDash = Input.GetKeyDown(m_dashKey);
        float horizontalSpeed = 0.0f, verticalSpeed = 0.0f;

        if (realDash && WALK_ANI_VALUE != 3 && m_isJumpAble == true
            && GameManager.Instance().PLAYER.m_oxy >= m_dashUseOxy && m_isDash == false)
        {
            transform.GetChild(0).localRotation =
                Quaternion.Slerp(transform.GetChild(0).localRotation ,
                Quaternion.Euler(GetCurrentAngle()) , 0.23f);
            m_dashDirection = transform.GetChild(0).forward;
            m_isDash = true;
            StartCoroutine(DashCall());
            //transform.Translate(Vector3.forward * m_dashSpeed * Time.deltaTime);
            WALK_ANI_VALUE = 3;
            NetworkManager.Instance().C2SRequestPlayerUseOxy(GameManager.Instance().PLAYER.m_name , m_dashUseOxy);
            return;
        }
        if (WALK_ANI_VALUE == 3 || m_isDash == true)
            return;
        // 가로 이동
        if (Input.GetKey(m_Left)) horizontalSpeed = (dash) ? -m_runSpeed : -m_walkSpeed;
        if (Input.GetKey(m_Right)) horizontalSpeed = (dash) ? m_runSpeed : m_walkSpeed;

        // 세로 이동
        if (Input.GetKey(m_Up)) verticalSpeed = (dash) ? m_runSpeed : m_walkSpeed;
        if (Input.GetKey(m_Down)) verticalSpeed = (dash) ? -m_runSpeed : -m_walkSpeed;

        // 이동
        #region Move Logic
        // 들고 있는 무기의 중량
        float weaponWeight = 0.0f;

        // -- 애니메이션 세팅 --
        if ((Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f))
            WALK_ANI_VALUE = (dash) ? 2 : 1;
        //  WalkAnimation((dash) ? 2 : 1);
        else
            WALK_ANI_VALUE = 0;
        //WalkAnimation(0);

        Vector3 speed = new Vector3(horizontalSpeed , 0 , verticalSpeed);
        speed *= Time.deltaTime;
        transform.Translate(speed);
        #endregion
        // 회전
        #region Rotate Logic
        #region Direction Check
        if (dash)
        {
            // 전진
            if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_FOWARD;
            // 전진 + 왼쪽
            else if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_FOWARD_LEFT;
            // 전진 + 오른쪽
            else if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_FOWARD_RIGHT;
            // 왼쪽
            else if (!Input.GetKey(m_Up) && !Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_LEFT;
            // 오른쪽
            else if (!Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_RIGHT;
            // 후진
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && !Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_BACK;
            // 후진 + 왼쪽
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_BACK_LEFT;
            // 후진 + 오른쪽
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.RUN_BACK_RIGHT;
            else
                m_currentDir = PlayerMoveDir.NONE;

        }
        else
        {
            // 전진
            if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_FOWARD;
            // 전진 + 왼쪽
            else if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_FOWARD_LEFT;
            // 전진 + 오른쪽
            else if (Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_FOWARD_RIGHT;
            // 왼쪽
            else if (!Input.GetKey(m_Up) && !Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_LEFT;
            // 오른쪽
            else if (!Input.GetKey(m_Up) && !Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_RIGHT;
            // 후진
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && !Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_BACK;
            // 후진 + 왼쪽
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && Input.GetKey(m_Left) && !Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_BACK_LEFT;
            // 후진 + 오른쪽
            else if (!Input.GetKey(m_Up) && Input.GetKey(m_Down) && !Input.GetKey(m_Left) && Input.GetKey(m_Right))
                m_currentDir = PlayerMoveDir.WALK_BACK_RIGHT;
            else
                m_currentDir = PlayerMoveDir.NONE;
        }
        #endregion

        float rotateSpeed = 0.23f;

        // if(m_currentDir != PlayerMoveDir.NONE)
        transform.GetChild(0).localRotation =
            Quaternion.Slerp(transform.GetChild(0).localRotation ,
            Quaternion.Euler(GetCurrentAngle()) , rotateSpeed);

        #endregion

        // space ship 조작시
        if (m_nearSpaceShip != null && (Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f))
            m_nearSpaceShip.StopSpaceShipEngineCharge();
        // oxy charger
        if (m_nearOxyCharger != null && (Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f))
        {
            LoopAudioStop();
            INTERACTION_ANI_VALUE = 0;
            m_targetOxy = 0.0f;
            if (m_equipItems[m_curEquipItem] != null)
            {
                m_equipItems[m_curEquipItem].gameObject.SetActive(true);
                WeaponAnimationChange(m_equipItems[m_curEquipItem]);
            }
            else
                WeaponAnimationChange(null);
            GameManager.Instance().SLIDER_UI.HideSlider();
        }
        MoveSend(speed);
    }

    public void DashAnimationEnd()
    {
        // if (WALK_ANI_VALUE == 3)
        WALK_ANI_VALUE = 0;
    }

    Vector3 GetCurrentAngle()
    {
        Vector3 angle = Vector3.zero;
        // 이제 회전
        switch (m_currentDir)
        {
            case PlayerMoveDir.NONE: break;
            case PlayerMoveDir.WALK_FOWARD: angle = Vector3.zero; break;
            case PlayerMoveDir.WALK_BACK: angle = new Vector3(0.0f , 180.0f , 0.0f); break;
            case PlayerMoveDir.WALK_LEFT: angle = new Vector3(0.0f , -90.0f , 0.0f); break;
            case PlayerMoveDir.WALK_RIGHT: angle = new Vector3(0.0f , 90.0f , 0.0f); break;
            case PlayerMoveDir.WALK_FOWARD_LEFT: angle = new Vector3(0.0f , -45.0f , 0.0f); break;
            case PlayerMoveDir.WALK_FOWARD_RIGHT: angle = new Vector3(0.0f , 45.0f , 0.0f); break;
            case PlayerMoveDir.WALK_BACK_LEFT: angle = new Vector3(0.0f , -135.0f , 0.0f); break;
            case PlayerMoveDir.WALK_BACK_RIGHT: angle = new Vector3(0.0f , 135.0f , 0.0f); break;
            case PlayerMoveDir.RUN_FOWARD: angle = new Vector3(0.0f , 0.0f , 0.0f); break;
            case PlayerMoveDir.RUN_BACK: angle = new Vector3(0.0f , 180.0f , 0.0f); break;
            case PlayerMoveDir.RUN_LEFT: angle = new Vector3(0.0f , -90.0f , 0.0f); break;
            case PlayerMoveDir.RUN_RIGHT: angle = new Vector3(0.0f , 90.0f , 0.0f); break;
            case PlayerMoveDir.RUN_FOWARD_LEFT: angle = new Vector3(0.0f , -45.0f , 0.0f); break;
            case PlayerMoveDir.RUN_FOWARD_RIGHT: angle = new Vector3(0.0f , 45.0f , 0.0f); break;
            case PlayerMoveDir.RUN_BACK_LEFT: angle = new Vector3(0.0f , -135.0f , 0.0f); break;
            case PlayerMoveDir.RUN_BACK_RIGHT: angle = new Vector3(0.0f , 135.0f , 0.0f); break;
        }
        return angle;
    }

    void JumpProcess()
    {
        if (Input.GetKey(m_Jump) && m_isJumpAble)
        {
            StartCoroutine(JumpCall());

            m_isJumpAble = false;

        }
    }

    IEnumerator JumpCall()
    {
        float startTime = Time.time;

        //중력 적용 안함
        GravityManager.Instance().SetGravityEnable(false);
        //JumpAnimation(1);

        JUMP_ANI_VALUE = 1;
        while (Time.time - startTime < m_jumpTick)
        {
            float nowTick = (Time.time - startTime) / m_jumpTick;
            transform.Translate(Vector3.up *
                (m_jumpHeight * (m_jumpCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_jumpCurve.Evaluate(nowTick))));
            yield return new WaitForFixedUpdate();
        }
        //JumpAnimation(2);
        JUMP_ANI_VALUE = 2;
        GravityManager.Instance().SetGravityEnable(true);
        m_isJumpAble = true;

    }

    IEnumerator DashCall()
    {
        float startTime = Time.time;


        m_dashEffect.SetActive(true);
        while (Time.time - startTime < m_dashTick)
        {
            Ray ray = new Ray(transform.position , m_dashDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray , out hit))
            {
                float distance = Vector3.Distance(transform.position , hit.transform.position);

                if (!hit.transform.CompareTag("Untagged") && distance <= 5.0f)
                    yield return new WaitForFixedUpdate();
            }
            float nowTick = (Time.time - startTime) / m_dashTick;
            this.transform.RotateAround(
                GravityManager.Instance().CurrentPlanet.transform.position ,
                GravityManager.Instance().GRAVITY_TARGET.transform.GetChild(0).rotation * Vector3.right ,
                (m_dashSpeed * (m_dashCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_dashCurve.Evaluate(nowTick))));

            Vector3 velo = GravityManager.Instance().GRAVITY_TARGET.transform.GetChild(0).rotation * Vector3.right * m_dashSpeed
                * (m_dashCurve.Evaluate(nowTick + Time.fixedDeltaTime) - m_dashCurve.Evaluate(nowTick));

            MoveSend(velo);

            yield return new WaitForFixedUpdate();
        }
        WALK_ANI_VALUE = 0;
        m_isDash = false;
        m_dashEffect.SetActive(false);


    }

    void MoveSend(Vector3 velo)
    {
        if (NetworkManager.Instance() == null)
            return;

        // Vector3 velo = m_rigidBody.velocity;

        NetworkManager.Instance().C2SRequestPlayerMove(name ,
            transform.position , velo ,
            transform.localRotation.eulerAngles ,
            transform.GetChild(0).localRotation.eulerAngles);
    }

    #endregion ------------------------------------------------------------------------------------------------

    #region Player Trigger Collider --------------------------------------------------------------------------

    void OnTriggerEnter(Collider col)
    {
        if (this.enabled == false)
            return;
        if (m_nearItem != null)
            m_nearItem.GetComponent<Item>().OutLineHide();
        // -- F 키를 띄워야 한다. // 근처에 있다면!
        ShowUseEffect(col);


    }

    void OnTriggerStay(Collider col)
    {
        if (this.enabled == false)
            return;
        RotateUseEffect();
        ShowUseEffect(col);
        MeteorDamage(col);

        if (col.CompareTag("WATER"))
            m_animationEvent.WaterSoundChange(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (this.enabled == false)
            return;
        // F 키 닫기
        HideUseEffect(col);

        if (m_nearItem != null)
            m_nearItem.GetComponent<Item>().OutLineHide();
        m_nearItem = null;
        if (col.CompareTag("WATER"))
            m_animationEvent.WaterSoundChange(false);
    }

    #endregion -----------------------------------------------------------------------------------------------

    #region Player Attack ------------------------------------------------------------------------------------

    void AttackProcess()
    {
        if (Input.GetKey(m_AttackKey) && !IsInvoking("AttackCoolTime") && ATTACK_ANI_VALUE != 1)
        {
            m_isAttackAble = false;
            m_isMoveAble = false;
            ATTACK_ANI_VALUE = 1;

            // 공격할땐 정면을 보고 공격
            this.transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);

            WeaponItem item = (m_equipItems[m_curEquipItem] as WeaponItem);
            if (item != null)
            {
                item.Attack(transform);

                if (item.ITEM_TYPE == Item.ItemType.ETC_GRENADE)
                {
                    // 수류탄의 경우 탈착 
                    GameManager.Instance().UnEquipWeapon(m_curEquipItem);

                    // 인벤 하이드
                    if (IsInvoking("HideInvenIcon"))
                        CancelInvoke("HideInvenIcon");
                    Invoke("HideInvenIcon" , 3.0f);
                    GameManager.Instance().m_inGameUI.ShowInvenUI();
                }

            }
            m_lastCoolTime = item.COOL_TIME;
            Invoke("AttackCoolTime" , m_lastCoolTime);
            Invoke("AttackMoveCancel" , 0.3f);
        }

    }
    // 일정시간동안 이동막는거 해제
    void AttackMoveCancel()
    {
        m_isMoveAble = true;
    }

    public void RecoveryItemUseEnd()
    {
        m_isMoveAble = true;
        m_isJumpAble = true;
        // 리커버리 끝
        (m_equipItems[m_curEquipItem]).gameObject.SetActive(false);

        // 이부분에서 삭제요청
        NetworkManager.Instance().C2SRequestItemDelete(m_equipItems[m_curEquipItem].ITEM_NETWORK_ID);

        UnEquipItem(m_equipItems[m_curEquipItem]);
        m_equipItems[m_curEquipItem] = null;
        INTERACTION_ANI_VALUE = 0;

        //이부분에서 전에 들고있는 무기 체크 
        if (m_equipItems[m_prevEquipItem] != null)
        {
            EquipItem(m_equipItems[m_prevEquipItem]);
        }
        else
            SetAnimation(AnimationType.ANI_BAREHAND);
    }

    void AttackCoolTime()
    {
        m_lastCoolTime = 0.0f;
        ATTACK_ANI_VALUE = 0;
    }


    #endregion -----------------------------------------------------------------------------------------------

    #region Show UI Logic ------------------------------------------------------------------------------------
    void ShowUseEffect(Collider col)
    {
        
        if (col.CompareTag("Weapon"))
        {
            m_nearItem = col.gameObject;
            //m_nearWeapon = col.gameObject;
           
           string text = WeaponManager.Instance().GetWeaponData(m_nearItem.GetComponent<Item>().ITEM_ID).Name_kr;
            GameManager.Instance().m_inGameUI.ShowObjectUI(text);
            //test
            m_nearItem.GetComponent<Item>().OutLineShow();

        }
        else if (col.CompareTag("OxyCharger"))
        {
            m_nearOxyCharger = col.GetComponent<OxyCharger>();
            GameManager.Instance().m_inGameUI.ShowObjectUI("");
        }
        else if (col.CompareTag("ItemBox"))
        {
            m_nearItemBox = col.GetComponent<ItemBox>();
            if (m_nearItemBox.OPENED)
            {
                m_nearItemBox = null;
                return;
            }

            GameManager.Instance().m_inGameUI.ShowObjectUI("");
        }
        else if (col.CompareTag("ShelterDoor"))
        {
            m_nearShelter = col.transform.parent.GetComponent<Shelter>();
            GameManager.Instance().m_inGameUI.ShowObjectUI("");
        }
        else if (col.CompareTag("SpaceShipControlPanel"))
        {
            if(GameManager.CURRENT_GAMEMODE == GameManager.GameMode.DEATH_MATCH)
            {
                return;
            }

            if (NetworkManager.Instance().SPACE_SHIP_ENABLE == false)
            {
                GameManager.Instance().m_inGameUI.ShowDebugLabel("우주선 최초 잠김 상태");
                return;
            }
            else
            {
                GameManager.Instance().m_inGameUI.ShowDebugLabel("우주선 최초 잠김이 풀린 상태");
            }
            m_nearSpaceShip = col.GetComponent<SpaceShip>();
            GameManager.Instance().m_inGameUI.ShowObjectUI("");

        }
        else if (col.CompareTag("Recoverykit"))
        {
            m_nearItem = col.gameObject;//col.GetComponent<HealPackItem>();
            
            string text = WeaponManager.Instance().GetWeaponData(m_nearItem.GetComponent<Item>().ITEM_ID).Name_kr;
            GameManager.Instance().m_inGameUI.ShowObjectUI(text);
            m_nearItem.GetComponent<Item>().OutLineShow();
        }
    }

    void HideUseEffect(Collider col)
    {
        LoopAudioStop();

        GameManager.Instance().m_inGameUI.HideObjectUI();

        if (col == null)
            return;

        if (col.CompareTag("Weapon"))
        {
            //  MeshRenderer renderer = m_nearItem.GetComponentInChildren<MeshRenderer>();
            if(m_nearItem != null)
                m_nearItem.GetComponent<Item>().OutLineHide();
        }
        else if (col.CompareTag("OxyCharger"))
        {
            if (m_oxyChargeRequest == true)
            {
                OxyChargerControlCancle();
            }
            m_nearOxyCharger = null;
        }
        else if (col.CompareTag("ItemBox"))
        {
            m_nearItemBox = null;
        }
        else if (col.CompareTag("ShelterDoor"))
        {
            m_nearShelter = null;
        }
        else if (col.CompareTag("SpaceShipControlPanel"))
        {
            if (GameManager.CURRENT_GAMEMODE == GameManager.GameMode.DEATH_MATCH)
            {
                return;
            }
            //  우주선 연료창 닫기
            if (m_nearSpaceShip != null)
                m_nearSpaceShip.StopSpaceShipEngineCharge();
            m_nearSpaceShip = null;
        }
        else if (col.CompareTag("Recoverykit"))
        {
            if (m_nearItem != null)
                m_nearItem.GetComponent<Item>().OutLineHide();
            m_nearItem = null;
            
        }
    }

    void RotateUseEffect()
    {

    }
    #endregion -----------------------------------------------------------------------------------------------

    #region Player Item Logic --------------------------------------------------------------------------------

    #region Inven Logic ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // 아이템 변경 로직 FixedUpdate 에서 부름
    void ChangeItemProcess()
    {
        int index = -1;

        if (Input.GetKey(m_inven1)) index = 0;
        if (Input.GetKey(m_inven2)) index = 1;
        if (Input.GetKey(m_inven3)) index = 2;
        if (Input.GetKey(m_inven4)) index = 3;
        if (Input.GetKey(m_inven5)) index = 4;

        if (index != -1)// && m_equipItems[index] != null)
        {
            GameManager.Instance().m_inGameUI.ShowDebugLabel("Inven index " + index + " cur " + m_curEquipItem);

            // 기존 아이템이 장착되어있다면 해제해야한다.
            if (m_equipItems[m_curEquipItem] != null)
            {
                m_equipItems[m_curEquipItem].EQUIP_STATE = false;
                m_equipItems[m_curEquipItem].gameObject.SetActive(false);
            }

            EquipItem(m_equipItems[index] , index , false);

        }
    }

    // 아이템 줍는 로직 FixedUpdate 에서 부름
    void GetItemProcess()
    {
        if (Input.GetKeyDown(m_Get) && m_nearItem != null)
        {
            // 주울 때 알아서 index 를 결정한다.
            Item nearItem = m_nearItem.GetComponent<Item>();
            int index = GetEquipItemIndex(nearItem.ITEM_TYPE);
            AudioPlay(m_itemGetSound);

            // 다만 item 의 type 이 원거리 무기 일 경우엔 
            // 1번 ,2번 인덱스에 나눠서 넣어야 한다.
            if (index == 1)
            {
                if (m_equipItems[index] != null)
                {
                    index = 2;
                }
            }

            EquipItem(nearItem , index , true);
            m_nearItem = null;
        }
    }

    // 실 아이템 장비로직
    void EquipItem(Item item , int curSelect = -1 , bool getItem = false)
    {
        if (item == null)
        {
            if (GameManager.Instance() != null)
                GameManager.Instance().EquipWeapon(null , 0 , 0);
            if (curSelect != -1)
                m_curEquipItem = curSelect;
            return;
        }

        if (getItem)
        {
            GameManager.Instance().PLAYER.WEIGHT += item.ITEM_WEIGHT;
        }
        // 현재 착용중인 장비는 끈다
        if (m_equipItems[m_curEquipItem] != null)
        {
            m_equipItems[m_curEquipItem].gameObject.SetActive(false);
        }

        // 해당 인덱스 장비 있으면 해제
        if (getItem == true && m_equipItems[curSelect] != null)
        {
            Debug.Log("UNEQUIP");
            UnEquipItem(m_equipItems[curSelect] , curSelect);
        }
        m_prevEquipItem = m_curEquipItem;
        m_equipItems[curSelect] = item;
        m_curEquipItem = curSelect;

        item.EQUIP_STATE = true;
        item.gameObject.SetActive(true);
        item.transform.parent = m_weaponEquipAnchor.transform;
        item.transform.localPosition = item.LOCAL_SET_POS;
        item.transform.localRotation = Quaternion.Euler(item.LOCAL_SET_ROT);
        item.transform.localScale = item.LOCAL_SET_SCALE;
        item.OutLineHide();
        GameManager.Instance().m_inGameUI.HideObjectUI();

        // 기존 코드에 이부분에 무기 발사 앵커를 세팅해주는게 있었지만 무기마다 다르니 무기껄로 쓰기로

        item.EQUIP_STATE = true;

        WeaponAnimationChange(item);

        GameManager.Instance().m_inGameUI.ShowInvenUI();
        if (IsInvoking("HideInvenIcon"))
            CancelInvoke("HideInvenIcon");
        Invoke("HideInvenIcon" , 3.0f);

        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestEquipItem(item.ITEM_ID ,
                item.ITEM_NETWORK_ID);

    }
    // 장착해제
    public void UnEquipItem(Item item , int index = -1)
    {
        if (index == -1)
            index = GetEquipItemIndex(item.ITEM_TYPE);

        //장비 해제해야함
        item.gameObject.SetActive(true);
        item.EQUIP_STATE = false;

        GameManager.Instance().PLAYER.WEIGHT -= item.ITEM_WEIGHT;
        GameManager.Instance().m_inGameUI.ThrowWeapon(index);

        RaycastHit throwRayHit;
        Physics.Raycast(transform.position + (transform.rotation * (Vector3.up + Vector3.forward)) ,
            (transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized * -3.0f , out throwRayHit , 10.0f);

        if (item.ITEM_TYPE == Item.ItemType.ETC_RECOVERY)
            GameManager.Instance().SLIDER_UI.HideSlider();

        SetAnimation(AnimationType.ANI_BAREHAND);
        ATTACK_ANI_VALUE = 0;

        item.transform.parent = null;
        item.transform.position = throwRayHit.point;

        Vector3 sponRot = (item.transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized;
        Quaternion targetRot = Quaternion.FromToRotation(item.transform.up , sponRot) * item.transform.rotation;

        //  m_currentWeapon.transform.rotation = targetRot;
        item.transform.Rotate(item.SPONE_ROTATITON);
        item.transform.Translate(Vector3.right * 0.15f);
        item.EQUIP_STATE = false;


        if (GameManager.Instance() != null)
            GameManager.Instance().UnEquipWeapon(m_curEquipItem);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestUnEquipItem(item.ITEM_ID , item.ITEM_NETWORK_ID ,
                item.transform.position , item.transform.eulerAngles);

        m_equipItems[index] = null;


        // 인벤 하이드
        if (IsInvoking("HideInvenIcon"))
            CancelInvoke("HideInvenIcon");
        Invoke("HideInvenIcon" , 3.0f);
        GameManager.Instance().m_inGameUI.ShowInvenUI();
    }

    void HideInvenIcon()
    {
        GameManager.Instance().m_inGameUI.HideInvenUI();
    }
    // 장착해제 -- 수류탄 ( 수류탄만 예외 처리 )
    public void UnEquipGrenade(Item item)
    {
        item.transform.parent = null;
        if (GameManager.Instance() != null)
            GameManager.Instance().UnEquipWeapon(m_curEquipItem,true);
        //WeaponAnimationChange(null);
    }
    // 버리는 로직 FixedUpdate 에서 부름
    void ControlWeaponObjectThrowProcess()
    {
        if (Input.GetKey(m_throwKey) && m_equipItems[m_curEquipItem] != null)
        {
            UnEquipItem(m_equipItems[m_curEquipItem] , m_curEquipItem);
        }
    }
    #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    #region Item Use Logic :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // 힐팩 아이템 사용
    void HealPackProcess()
    {
        if (m_equipItems[m_curEquipItem] != null 
            && m_equipItems[m_curEquipItem].ITEM_TYPE == Item.ItemType.ETC_RECOVERY)
        {
            if (Input.GetKey(m_AttackKey))
            {
                IS_MOVE_ABLE = false;
                IS_JUMP_ABLE = false;
                //   WeaponAnimationChange(m_currentRecoveryKit);

                if (m_interactionAniVal != 1)
                {
                    INTERACTION_ANI_VALUE = 1;
                }
                (m_equipItems[m_curEquipItem] as HealPackItem).Recovery(this);

            }

            if (Input.GetKeyUp(m_AttackKey))
            {
                IS_MOVE_ABLE = true;
                IS_JUMP_ABLE = true;
                //애니메이션 끝 
                INTERACTION_ANI_VALUE = 0;

                (m_equipItems[m_curEquipItem] as HealPackItem).RecoveryUp();
            }
        }
    }
    #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    #endregion -----------------------------------------------------------------------------------------------

    #region Player Damage And Dead Observer ------------------------------------------------------------------

    #region Damage Effect ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // 데미지 이펙트 //
    public void DamageEffect(bool showHitEffect = true , bool characterDamageAnimationShow = true)
    {
        if (showHitEffect == true && GameManager.Instance().WINNER == false)
            CameraManager.Instance().ShowHitEffect();
        else
            CameraManager.Instance().HideHitEffect();
        m_renderer.material = HIT_MATERIAL;
        if (characterDamageAnimationShow == true
            && GameManager.Instance().PLAYER.m_hp > 0.0f)
            AnimationPlay("Damage");
        Invoke("DamgeEffectEnd" , 0.1f);
    }
    // 카메라 데미지 애니메이션 재생 후 애니메이션을 꺼야 함
    public void CameraDamageAnimationEnd()
    {
        m_camAnimator.SetInteger("DAMAGE" , 0);
    }

    void DamgeEffectEnd()
    {
        m_renderer.material = ORIGIN_MATERIAL;
        if (GetComponent<NetworkPlayer>() != null)
        {
            Vector4[] vecs = { new Vector4(3.68276f , 0.0f , 6.0f) ,
            new Vector4(0.0f , 6.0f , 0.7862077f) , new Vector4(0.0f , 3.517242f , 6.0f) };
            int index = NetworkManager.Instance().NETWORK_PLAYERS.IndexOf(GetComponent<NetworkPlayer>());

            if (index < vecs.Length)
                transform.GetChild(0).GetChild(0).GetChild(3)
                        .GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_EmissionColor" ,
                        vecs[index]);
        }

    }
    #endregion :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // 데미지를 입을때
    public void Damage(Vector3 dir , string reason = null)
    {
        DamageEffect(true , 
            !reason.Equals("DeathZone") 
            && !reason.Equals("Meteor") 
            && !reason.Equals("oxy")
            && !reason.Equals("Gas"));

        AudioPlay(m_damageHit);
        // 우주선 생성 도중 데미지를 입었을 경우 캔슬됨
        if (m_nearSpaceShip != null && !reason.Equals("oxy") && !reason.Equals("DeathZone"))
            SpaceShipControlCancel();

        if (m_nearOxyCharger != null && m_targetOxy > 0.0f && !reason.Equals("oxy") && !reason.Equals("DeathZone"))
        {
            INTERACTION_ANI_VALUE = 0;
            m_targetOxy = 0.0f;
            if (CURRENT_WEAPON != null)
            {
                CURRENT_WEAPON.gameObject.SetActive(true);
                WeaponAnimationChange(CURRENT_WEAPON);
            }
            GameManager.Instance().SLIDER_UI.HideSlider();

            if (CURRENT_WEAPON != null)
            {
                CURRENT_WEAPON.gameObject.SetActive(true);
                WeaponAnimationChange(CURRENT_WEAPON);
            }
            GameManager.Instance().SLIDER_UI.HideSlider();
        }
    }

    // 사망
    public void Dead()
    {
        AudioPlay(m_deadSound);
    }

    // 사망 애니메이션 끝
    public void DeadAnimationEnd()
    {
        Debug.Log("Dead Animation End");
        if(GameManager.CURRENT_GAMEMODE == GameManager.GameMode.SURVIVAL)
            NetworkManager.Instance().IS_LOSE = true;
    }

    // 옵저버 조작
    void ObserverControl()
    {
        if (Input.GetKeyDown(KeyCode.Space) && CameraManager.Instance().DEAD_EFFECT_SHOW)
        {
            //옵저버 조작
            var list = NetworkManager.Instance().NETWORK_PLAYERS;
            m_observerIndex++;

            if (m_observerIndex == -1)
                m_observerIndex = 0;
            if (m_observerIndex >= list.Count)
            {
                Camera.main.transform.parent = m_camAnchor3;
                m_observerIndex = -1;
                CameraManager.Instance().ShowDeadCameraEffect_NoEffect();
                Camera.main.transform.localPosition = Vector3.zero; // 위치 설정
                Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero); // 각도 설정
            }
            else
            {

                CameraManager.Instance().HideDeadCameraEffect();
                // 우주선에 탄 놈일 경우 다르게 처리
                if (list[m_observerIndex].TARGET_SPACESHIP != null)
                {
                    var np = list[m_observerIndex].GetComponent<PlayerController>();
                    if (np.m_camAnchor3.childCount > 0)
                    {
                        var cam = np.m_camAnchor3.GetChild(0);
                        cam.parent = null;
                        cam.gameObject.SetActive(true);
                    }
                    list[m_observerIndex].TARGET_SPACESHIP.SpaceShipCameraEndSetup();
                }
                else
                {
                    var np = list[m_observerIndex].GetComponent<PlayerController>();
                    var target = np.m_camAnchor3;
                    
                    Camera.main.transform.parent = target;
                    Camera.main.transform.localPosition = Vector3.zero; // 위치 설정
                    Camera.main.transform.localRotation = Quaternion.Euler(Vector3.zero); // 각도 설정
                }
            }
            Debug.Log("Ob " + m_observerIndex);

        }
        else if(m_observerIndex != -1)
        {
            var list = NetworkManager.Instance().NETWORK_PLAYERS;

            if (m_observerIndex >= list.Count)
                return;


            CameraManager.Instance().HideDeadCameraEffect();
            // 우주선에 탄 놈일 경우 다르게 처리
            if (list[m_observerIndex].TARGET_SPACESHIP != null)
            {
                var np = list[m_observerIndex].GetComponent<PlayerController>();

                if(np.m_camAnchor3.childCount > 0)
                {
                    var cam = np.m_camAnchor3.GetChild(0);
                    cam.parent = null;
                    cam.gameObject.SetActive(true);
                }
                list[m_observerIndex].TARGET_SPACESHIP.SpaceShipCameraEndSetup();
            }
            else
            {
                //var np = list[m_observerIndex].GetComponent<PlayerController>();
                //var target = np.m_camAnchor3;

                //Camera.main.transform.parent = target;
            }
        }
    }

    // 죽었는지 체크
    public bool IS_DEATH()
    {
        return GameManager.Instance() != null && GameManager.Instance().PLAYER != null && GameManager.Instance().PLAYER.m_hp <= 0.0f;
    }

    // 데스매치 부활
    void DeathMatchRebirth()
    {
        NetworkManager.Instance().RequestRebirth((int)NetworkManager.Instance().HOST_ID , true);
    }
    #endregion -----------------------------------------------------------------------------------------------

    #region Player Object Interaction ------------------------------------------------------------------------

    // 모든 상호작용 오브젝트 처리 FixedUpdate에서 부름
    void ControlObjectProcess()
    {
        if (Input.GetKeyDown(m_Get))
        {
            if (m_nearItemBox != null)
            {
                AudioPlay(m_pickSound);
                m_nearItemBox.UseItemBox();
            }
            if (m_nearShelter != null)
            {
                AudioPlay(m_shelterUseSound);
                m_nearShelter.DOOR_STATE = !m_nearShelter.DOOR_STATE;
                m_nearShelter.DoorControl();
            }

            if (m_nearSpaceShip != null)
            {
                if (CURRENT_WEAPON != null)
                    CURRENT_WEAPON.gameObject.SetActive(false);

                IS_MOVE_ABLE = false;
            }

        }
        // 우주선 상호작용
        SpaceShipInteraction();
        // 산소 충전기 상호작용
        OxyChargerInteraction();

    }

    #region OxyCharger Interaction Logic ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // 실 사용 로직
    void OxyChargerInteraction()
    {
        // 산소 충전 요청을 보내야 한다.

        //최초 사용 요청
        if (m_oxyChargeRequest == false && Input.GetKeyDown(m_Get) && m_nearOxyCharger != null)
        {
            Debug.Log("OxyCharger 최초 사용 요청");
            m_oxyChargeRequest = true;
            m_isMoveAble = false;
            NetworkManager.Instance().C2SRequestUseOxyChargerStart(m_nearOxyCharger);
        }

        // 사용 가능할때
        if (m_nearOxyCharger != null && m_oxyChargeRequest == true && Input.GetKey(m_Get))
        {
            Debug.Log("OxyCharger 사용 가능 " + m_nearOxyCharger.OXY_CHARGER_ID + " " + m_nearOxyCharger.OXY_CHARGER_ENABLE);
            if (m_nearOxyCharger.OXY_CHARGER_ENABLE == true)
            {
                ControlOxyChargerProcess();
            }

        }

        // 중도 취소
        if (Input.GetKeyUp(m_Get))
        {
            OxyChargerControlCancle();
        }
    }

    // 산소 충전 가능 상태가 되자마자 첨 세팅
    public void OxyChargerEnableSetup()
    {
        AudioPlay(m_oxyChargerUseSound);
        LoopAudioPlay(m_oxyChargeSound);
        m_targetOxy = GameManager.Instance().PLAYER.m_fullOxy - GameManager.Instance().PLAYER.m_oxy;
        m_plusOxy = 0.0f;
        SetAnimation(AnimationType.ANI_ETC);
        if (m_equipItems[m_curEquipItem] != null)
        {
            m_equipItems[m_curEquipItem].gameObject.SetActive(false);
        }
        INTERACTION_ANI_VALUE = 1;
        GameManager.Instance().SLIDER_UI.ShowSlider();
        GameManager.Instance().SLIDER_UI.Reset();
    }

    // 산소 충전가능한 상황일때
    void ControlOxyChargerProcess()
    {
        if (m_nearOxyCharger == null)
            return;

     //   if (Input.GetKey(m_Get))
        {
            float oxy = m_chargeOxy * Time.deltaTime;
            m_plusOxy += oxy;
            GameManager.Instance().SLIDER_UI.SliderProcess(m_plusOxy / m_targetOxy);

            if (m_nearOxyCharger.CURRENT_OXY > 0.0f)
                m_nearOxyCharger.UseOxy(oxy);
            else
            {
                AudioPlay(m_oxyChargeDown);
                LoopAudioStop();
                INTERACTION_ANI_VALUE = 0;
                m_targetOxy = 0.0f;
                if (m_equipItems[m_curEquipItem] != null)
                {
                    m_equipItems[m_curEquipItem].gameObject.SetActive(true);
                    WeaponAnimationChange(m_equipItems[m_curEquipItem]);
                }
                else
                    WeaponAnimationChange(null);
                GameManager.Instance().SLIDER_UI.HideSlider();
                return;
            }

            if (GameManager.Instance().PLAYER.m_oxy >= GameManager.Instance().PLAYER.m_fullOxy)
            {
                m_isMoveAble = true;
                INTERACTION_ANI_VALUE = 0;
                if (m_equipItems[m_curEquipItem] != null)
                {
                    m_equipItems[m_curEquipItem].gameObject.SetActive(true);
                    WeaponAnimationChange(m_equipItems[m_curEquipItem]);

                }
                else
                    WeaponAnimationChange(null);
                GameManager.Instance().SLIDER_UI.HideSlider();
            }
        }
        if (Input.GetKeyUp(m_Get))
        {
            m_oxyChargeRequest = false;
            m_isMoveAble = true;
            //산소 충전 끝
            NetworkManager.Instance().C2SRequestPlayerUseEndOxyCharger(m_nearOxyCharger);

            LoopAudioStop();
            INTERACTION_ANI_VALUE = 0;
            m_targetOxy = 0.0f;
            if (m_equipItems[m_curEquipItem] != null)
            {
                m_equipItems[m_curEquipItem].gameObject.SetActive(true);
                WeaponAnimationChange(m_equipItems[m_curEquipItem]);
            }
            else
                WeaponAnimationChange(null);
            GameManager.Instance().SLIDER_UI.HideSlider();
        }
    }

    void OxyChargerControlCancle()
    {
        
        m_isMoveAble = true;
        m_oxyChargeRequest = false;
        if (m_nearOxyCharger != null)
            NetworkManager.Instance().C2SRequestPlayerUseEndOxyCharger(m_nearOxyCharger);
    }
    #endregion

    #region Meteor Interaction Logic ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    // 메테오 데미지
    void MeteorDamage(Collider col)
    {
        if (col.CompareTag("Meteor") && IS_SHELTER == false && IS_DEATH() == false)
        {
            AudioPlay(m_damageHit);
            if (m_meteorCoolTime > 0.0f)
                return;

            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestPlayerDamage(
                    (int)NetworkManager.Instance().m_hostID , "" , "Meteor" , m_meteorDamage , Vector3.zero);
            m_meteorCoolTime = m_damageCoolTime;
            Invoke("MeteorCoolTimeChecker" , m_meteorCoolTime);
        }
    }

    void MeteorCoolTimeChecker()
    {
        m_meteorCoolTime = -1.0f;
        CancelInvoke("MeteorCoolTimeChecker");     
    }
    #endregion

    #region SpaceShip Interaction Logic :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

    // 실 사용 로직
    void SpaceShipInteraction()
    {
        // 최초 사용 요청 
        if (m_spaceShipRequest == false && Input.GetKeyDown(m_Get) && m_nearSpaceShip != null)
        {
            m_spaceShipRequest = true;
            m_isMoveAble = false;
            NetworkManager.Instance().C2SRequestUseSpaceShip(m_nearSpaceShip.SPACESHIP_ID);
        }

        // 사용 가능 할때 
        if (m_spaceShipRequest == true && Input.GetKey(m_Get))
        {
            if (m_nearSpaceShip != null && m_nearSpaceShip.IS_SPACESHIP_ENABLED == true)
            {
                m_nearSpaceShip.SpaceShipEngineChargeProcess();
            }
            else
                SpaceShipControlCancel();
        }

        // 중도 취소 
        if (Input.GetKeyUp(m_Get))
        {
            if (m_nearSpaceShip != null)
            {
                SpaceShipControlCancel();
            }

        }
    }

    // 우주선 사용가능 상태일때 최초 수행시 
    public void SpaceShipEnable()
    {
        SetAnimation(AnimationType.ANI_ETC);
        INTERACTION_ANI_VALUE = 1;
        AudioPlay(m_spaceShipChargeSound);
        Debug.Log("우주선 사용가능 ");
        m_nearSpaceShip.StartSpaceShipEngineCharge();
    }

    void SpaceShipControlCancel()
    {
        IS_MOVE_ABLE = true;
        INTERACTION_ANI_VALUE = 0;
        m_spaceShipRequest = false;
        NetworkManager.Instance().C2SRequestUseSpaceShipCancel(m_nearSpaceShip.SPACESHIP_ID);
        if (CURRENT_WEAPON != null)
        {
            CURRENT_WEAPON.gameObject.SetActive(true);
            WeaponAnimationChange(CURRENT_WEAPON);
        }
        else
            WeaponAnimationChange(null);

        m_nearSpaceShip.StopSpaceShipEngineCharge();
        AudioPlay(m_spaceShipChargeFail);
    }

    // 우주선 작동 시작시 우주선 측에서 호출함 
    public void SpaceShipChargeEnd()
    {
        HideUseEffect(null);
    }
    #endregion

    #endregion -----------------------------------------------------------------------------------------------

    #region Animation Function -------------------------------------------------------------------------------

    // PlayerControlAttackTiming 에서 Attack 시점을 알려주었다,
    public void AttackAnimationEvent()
    {
        // 애니메이션 상에서 어택 시점을 조절해야 할 경우 여기로 들어옴
        if(m_equipItems[m_curEquipItem] != null)
        {
            WeaponItem item = (m_equipItems[m_curEquipItem] as WeaponItem);
            item.AnimationEventAttack();

            if(item.ITEM_TYPE == Item.ItemType.ETC_GRENADE)
            {
                m_equipItems[m_curEquipItem] = null;
            }
        }
    }
    
    public void AttackAnimationEnd()
    {
        // 네트워크 플레이어는 수행되면 안됨
        if (enabled == false)
            return;
        // 원래 수류탄 전용 
        if (m_equipItems[m_curEquipItem] == null)
        {
            ATTACK_ANI_VALUE = 0;
            SetAnimation(AnimationType.ANI_BAREHAND);
        }


    }
    void WalkAnimation(int value)
    {
        m_animator.SetInteger("WALK" , value);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "WALK" , value);
    }

    void JumpAnimation(int value)
    {
        m_animator.SetInteger("JUMP" , value);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "JUMP" , value);
    }

    void AttackAnimation(int value)
    {    
        if (value == 0)
            m_isMoveAble = true;
        m_animator.SetInteger("ATTACK" , value);
        if (m_equipItems[m_curEquipItem] != null && m_equipItems[m_curEquipItem].ITEM_TYPE != Item.ItemType.ETC_RECOVERY)
            (m_equipItems[m_curEquipItem] as WeaponItem).AnimationEventAttackEnd();
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "ATTACK" , value);
    }

    void InteractionAnimation(int value)
    {
        m_animator.SetInteger("INTERACTION" , value);

        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "INTERACTION" , value);
    }

    // 애니메이션 강제 재생 / 현재 데미지 / 사망
    public void AnimationPlay(string aniName)
    {
        m_animator.Play(aniName);
        // 강제 재생
        NetworkManager.Instance().C2SRequestPlayerAnimation(
           GameManager.Instance().PLAYER.m_name , aniName , 1234);
    }

    // 애니메이션 체인지
    void SetAnimation(AnimationType type)
    {
        for(int i =0; i < m_animationControllerList.Count; i++)
        {
            if(m_animationControllerList[i].m_type == type)
            {
                int walk = m_animator.GetInteger("WALK");
                int jump = m_animator.GetInteger("JUMP");
                int attack = m_animator.GetInteger("ATTACK");
                m_animator.runtimeAnimatorController = m_animationControllerList[i].m_controller;
                WalkAnimation(walk);
                JumpAnimation(jump);
                AttackAnimation(attack);
                break;
            }
        }
    }

    #endregion -----------------------------------------------------------------------------------------------

    #region Audio Control ------------------------------------------------------------------------------------

    // 주인공 체력 / 산소 부족시 재생시켜라
    public void NotEnoughHp()
    {
        LoopAudioPlay(m_notEnoughHPSound);
    }

    public void NotEnoughOxy()
    {
        Debug.Log("NotEnoughOxy");
        LoopAudioPlay(m_notEnoughOxySound);
    }
    // 막 회복되었을 때
    public void HealHPAndOxy()
    {
        AudioPlay(m_healSound);
    }

    void AudioPlay(AudioClip clip)
    {
        if (m_playerSoundSource.clip != clip && m_playerSoundSource.isPlaying == true)
            m_playerSoundSource.Stop();
        m_playerSoundSource.clip = clip;
        if(m_playerSoundSource.isPlaying == false)
            m_playerSoundSource.Play();
    }

    void LoopAudioPlay(AudioClip clip)
    {
        if (m_playerLoopSource.clip != null && m_playerLoopSource.clip == clip && m_playerLoopSource.isPlaying == true)
            return;

        m_playerLoopSource.clip = clip;
        m_playerLoopSource.Play();
    }

    void LoopAudioStop()
    {
        if (m_playerLoopSource.clip != null && m_playerLoopSource.isPlaying == true)
            m_playerLoopSource.Stop();
    }

    #endregion ------------------------------------------------------------------------------------------------

    #region Util Method ---------------------------------------------------------------------------------------

    // 아이템 인벤 어디에 해당되는지 알려주는 메소드
    public int GetEquipItemIndex(Item.ItemType type)
    {
        switch (type)
        {
            case Item.ItemType.NONE: return -1;
            case Item.ItemType.MELEE: return 0;
            case Item.ItemType.GUN:
            case Item.ItemType.RIFLE:
            case Item.ItemType.ROCKETLAUNCHER: return 1;
            default: return 3;
        }
    }

    // 아이템 애니메이션 변경 
    public void WeaponAnimationChange(Item item)
    {
        NetworkManager.Instance().C2SRequestPlayerAnimation(NetworkManager.Instance().USER_NAME , "CW" , -1);
        if (item == null)
        {
            SetAnimation(AnimationType.ANI_BAREHAND);
            return;
        }
        else
        {
            switch (item.ITEM_TYPE)
            {
                case Item.ItemType.GUN: SetAnimation(AnimationType.ANI_GUN01); break;
                case Item.ItemType.RIFLE: SetAnimation(AnimationType.ANI_GUN02); break;
                case Item.ItemType.MELEE: SetAnimation(AnimationType.ANI_MELEE); break;
                case Item.ItemType.ROCKETLAUNCHER: SetAnimation(AnimationType.ANI_ROCKETLAUNCHER); break;
                case Item.ItemType.ETC_GRENADE: SetAnimation(AnimationType.ANI_ETC); break;
                case Item.ItemType.ETC_RECOVERY: SetAnimation(AnimationType.ANI_ETC); break;
            }
        }

    }

    // 현재 애니메이터 가져오기
    public RuntimeAnimatorController GetCurrentAnimator(AnimationType type)
    {
        for (int i = 0; i < m_animationControllerList.Count; i++)
        {
            if (m_animationControllerList[i].m_type == type)
            {
                return m_animationControllerList[i].m_controller;
            }
        }
        return null;
    }

    #endregion ------------------------------------------------------------------------------------------------
}
