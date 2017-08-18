using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    #region PlayerController_INFO
    [SerializeField]  private bool m_signleMode = false;

    private CharacterController m_characterController = null;
    private Rigidbody m_rigidBody = null;
    private Camera m_camera = null;
    [SerializeField] private Animator m_animator = null;
    public Animator ANIMATOR { get { return m_animator; } }

    #region Camera Anchor
    //public Transform m_ThirdAnchor = null;
    //public Transform m_FpsAnchor = null;
    public Transform m_camAnchor1 = null;
    public Transform m_camAnchor2 = null;
    public Transform m_camAnchor3 = null;
    public Transform m_camAnchor4 = null;
    #endregion

    #region Player Info --------------------------------------------------------------------------------------------------
    [SerializeField] private GameObject m_modeObject = null;
    [SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_runSpeed;
    [SerializeField] private float m_jumpSpeed;
    [SerializeField] private float m_jumpHeight = 3.0f;
    [SerializeField] private float m_jumpTick = 1.0f;
    [SerializeField] private AnimationCurve m_jumpCurve = null;
    [SerializeField] private GameObject m_useEffect = null;
    

    // USE EFFECT ANCHOR
    [SerializeField] private GameObject m_useEffectHeadAnchor = null;
    [SerializeField] private GameObject m_useEffectHandAnchor = null;
    // 무기 앵커
    [SerializeField] private GameObject m_weaponEquipAnchor = null;
    public GameObject WEAPON_ANCHOR { get { return m_weaponEquipAnchor; } }

    // 공격 쿨타임
    private float m_lastCoolTime = 0.0f;

    // 메테오 데미지 쿨타임
    private float m_meteorCoolTime = 0.0f;

    [SerializeField] private List<AnimationController> m_animationControllerList = new List<AnimationController>();

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


    private bool m_isMoveAble = true;
    private bool m_isJumpAble = true;
    private bool m_isAttackAble = true;   
    //shelter 안인지?
    private bool m_isShelter = false;

    public bool IS_MOVE_ABLE { get { return m_isMoveAble; } set { m_isMoveAble = value; } }
    public bool IS_JUMP_ABLE { get { return m_isJumpAble; } set { m_isJumpAble = value; } }
    public bool IS_ATTACK_ABLE { get { return m_isAttackAble; } set { m_isAttackAble = value; } }
    public bool IS_SHELTER { get { return m_isShelter; } set { m_isShelter = value; } }

    #region Interaction Object -----------------------------------------------------------------------------------------------
    // 상호작용 오브젝트와 관련된 것
    private GameObject m_nearWeapon = null;
    private GameObject m_nearItem = null;
    private RecoverykitItem m_nearRecoveryKit = null;
    private OxyCharger m_nearOxyCharger = null;
    private ItemBox m_nearItemBox = null;
    private Shelter m_nearShelter = null;
    private SpaceShip m_nearSpaceShip = null;
    
    #endregion

    // 착용중인 것
    private WeaponItem m_currentWeapon = null;
    private RecoverykitItem m_currentRecoveryKit = null;

    #endregion

    #region OXY -----------------------------------------------------------------------------------------------------------
    void UseOxy()
    {
        if(NetworkManager.Instance() != null)
        {
            float useOxy = 0.1f;//UnityEngine.Random.Range(0.1f , 10.0f);

            bool idle = (m_currentDir == PlayerMoveDir.NONE);
            bool run = (m_currentDir == PlayerMoveDir.RUN_BACK || m_currentDir == PlayerMoveDir.RUN_LEFT
                || m_currentDir == PlayerMoveDir.RUN_RIGHT || m_currentDir == PlayerMoveDir.RUN_FOWARD || m_currentDir == PlayerMoveDir.RUN_FOWARD_LEFT || m_currentDir == PlayerMoveDir.RUN_FOWARD_RIGHT
                || m_currentDir == PlayerMoveDir.RUN_BACK_LEFT || m_currentDir == PlayerMoveDir.RUN_BACK_RIGHT);
            if (idle)// && m_currentWeapon == null)
            {
                useOxy = 0.1f;
            }
            else if (run )//&& m_currentWeapon == null)
                useOxy = 1.5f;
            else if (idle == false && run == false)
                useOxy = 0.2f;

            if (GameManager.Instance().PLAYER.m_hp - useOxy > 0.0f)
                NetworkManager.Instance().C2SRequestPlayerUseOxy(GameManager.Instance().PLAYER.m_name , useOxy);
        }
    }
    #endregion


    #region Control Key -----------------------------------------------------------
    // 기본 이동 위 왼쪽 아래 오른쪽
    [SerializeField] private KeyCode m_Up       = KeyCode.W;
    [SerializeField] private KeyCode m_Left     = KeyCode.A;
    [SerializeField] private KeyCode m_Down     = KeyCode.S;
    [SerializeField] private KeyCode m_Right    = KeyCode.D;
    [SerializeField] private KeyCode m_Get      = KeyCode.F;
    [SerializeField] private KeyCode m_throwKey = KeyCode.T;

    // 점프
    [SerializeField] private KeyCode m_Jump     = KeyCode.Space;

    // 대시 키
    [SerializeField] private KeyCode m_Dash     = KeyCode.LeftShift;

    // 인벤 자동 장착 키
    // 근거리
    [SerializeField] private KeyCode m_ShortRangeWeaponEquip    = KeyCode.Q;
    // 원거리
    [SerializeField] private KeyCode m_LongRangeWeaponEquip     = KeyCode.E;
    // 기타 무기
    [SerializeField] private KeyCode m_EtcWeaponEquip           = KeyCode.C;

    // 인벤토리 키
    [SerializeField] private KeyCode m_InventoryActive          = KeyCode.I;

    // 무기 사용
    [SerializeField] private KeyCode m_AttackKey                = KeyCode.Mouse0;

    #endregion

    #region Player Rotate
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
    #endregion

    #region Player Animation Var ------------------------------------------------------------------------------
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
            m_attackAniVal = value;

            // 공격 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if(m_attackAniVal != 0)
            {
                WALK_ANI_VALUE = 0;
                JUMP_ANI_VALUE = 0;
                INTERACTION_ANI_VALUE = 0;
            }

            AttackAnimation(m_attackAniVal);
        }
    }

    // 이동 애니메이션 값
    public int WALK_ANI_VALUE
    {
        get { return m_walkAniVal; }
        set
        {
            m_walkAniVal = value;

            // 이동 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if(m_walkAniVal != 0)
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
            
            // 점프 애니메이션을 재생할 때 다른 값들은 어떻게 되어야 하는가?
            if(m_jumpAniVal != 0)
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

            if(m_interactionAniVal != 0)
            {
                ATTACK_ANI_VALUE = 0;
                WALK_ANI_VALUE = 0;
                JUMP_ANI_VALUE = 0;
            }
            InteractionAnimation(m_interactionAniVal);
        }
    }

    #endregion
    #region Player Oxy Charger 
    private float m_targetOxy = 0.0f;
    private float m_plusOxy = 0.0f;
    #endregion

    #endregion

    #region Unity Method
    void Start()
    {
        m_characterController = this.GetComponent<CharacterController>();
        m_camera = Camera.main;

        m_rigidBody = this.GetComponent<Rigidbody>();

        GravityManager.Instance().SetGravityTarget(m_rigidBody);
        InvokeRepeating("UseOxy" , 1.0f , 1.0f);
        if (m_signleMode)
            SetCameraThirdPosition();
    }

    public void SetCameraThirdPosition()
    {
        CameraManager.Instance().Player = transform;
        CameraManager.Instance().CamAnchor[0] = m_camAnchor1;
        CameraManager.Instance().CamAnchor[1] = m_camAnchor2;
        CameraManager.Instance().CamAnchor[2] = m_camAnchor3;
        CameraManager.Instance().CamAnchor[3] = m_camAnchor4;
        CameraManager.Instance().CamSet();

        //m_camera.transform.SetParent(m_ThirdAnchor , false);
        //m_camera.transform.localEulerAngles = new Vector3(0.0f , 0.0f , 0.0f);
        //m_camera.transform.localPosition = new Vector3(0.0f , 0.0f , 0.0f);
    }


    void FixedUpdate()
    {

        if (IS_MOVE_ABLE)
            MoveProcess();

        if (IS_JUMP_ABLE)
            JumpProcess();

        if (IS_ATTACK_ABLE && m_currentWeapon != null && m_currentWeapon.gameObject.activeSelf == true)
        {
            AttackProcess();
            
        }

        ControlWeaponObjectProcess();
        ControlWeaponObjectThrowProcess();
        ControlObjectProcess();


    }
    #endregion

    #region Player Move ---------------------------------------------------------------------------------------

    void MoveProcess()
    {
        bool dash = Input.GetKey(m_Dash);
        float horizontalSpeed = 0.0f, verticalSpeed = 0.0f;

        if (Input.GetKey(m_AttackKey))
            return;

        // 가로 이동
        if(Input.GetKey(m_Left))    horizontalSpeed = (dash) ? -m_runSpeed : -m_walkSpeed;
        if(Input.GetKey(m_Right))   horizontalSpeed = (dash) ? m_runSpeed : m_walkSpeed;

        // 세로 이동
        if(Input.GetKey(m_Up))      verticalSpeed = (dash) ? m_runSpeed : m_walkSpeed;
        if (Input.GetKey(m_Down))   verticalSpeed = (dash) ? -m_runSpeed : -m_walkSpeed;

        // 이동
        #region Move Logic
        // 들고 있는 무기의 중량
        float weaponWeight = 0.0f;

        // -- 애니메이션 세팅 --
        if (Mathf.Abs(verticalSpeed) > 0.0f || Mathf.Abs(horizontalSpeed) > 0.0f)
            WALK_ANI_VALUE = (dash) ? 2 : 1;
        //  WalkAnimation((dash) ? 2 : 1);
        else
            WALK_ANI_VALUE = 0;
            //WalkAnimation(0);

        Vector3 speed = new Vector3(horizontalSpeed , 0 , verticalSpeed);
        transform.Translate(speed * Time.deltaTime);
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
        Vector3 angle = Vector3.zero;
        // 이제 회전
        switch(m_currentDir)
        {
            case PlayerMoveDir.NONE:                                                           break;
            case PlayerMoveDir.WALK_FOWARD:         angle = Vector3.zero;                       break;
            case PlayerMoveDir.WALK_BACK:           angle = new Vector3(0.0f , 180.0f , 0.0f);  break;
            case PlayerMoveDir.WALK_LEFT:           angle = new Vector3(0.0f , -90.0f , 0.0f);  break;
            case PlayerMoveDir.WALK_RIGHT:          angle = new Vector3(0.0f , 90.0f , 0.0f);   break;
            case PlayerMoveDir.WALK_FOWARD_LEFT:    angle = new Vector3(0.0f , -45.0f , 0.0f);  break;
            case PlayerMoveDir.WALK_FOWARD_RIGHT:   angle = new Vector3(0.0f , 45.0f , 0.0f);   break;
            case PlayerMoveDir.WALK_BACK_LEFT:      angle = new Vector3(0.0f , -135.0f , 0.0f); break;
            case PlayerMoveDir.WALK_BACK_RIGHT:     angle = new Vector3(0.0f , 135.0f , 0.0f);  break;
            case PlayerMoveDir.RUN_FOWARD:          angle = new Vector3(0.0f , 0.0f , 0.0f);    break;
            case PlayerMoveDir.RUN_BACK:            angle = new Vector3(0.0f , 180.0f , 0.0f);  break;
            case PlayerMoveDir.RUN_LEFT:            angle = new Vector3(0.0f , -90.0f , 0.0f);  break;
            case PlayerMoveDir.RUN_RIGHT:           angle = new Vector3(0.0f , 90.0f , 0.0f);   break;
            case PlayerMoveDir.RUN_FOWARD_LEFT:     angle = new Vector3(0.0f , -45.0f , 0.0f);  break;
            case PlayerMoveDir.RUN_FOWARD_RIGHT:    angle = new Vector3(0.0f , 45.0f , 0.0f);   break;
            case PlayerMoveDir.RUN_BACK_LEFT:       angle = new Vector3(0.0f , -135.0f , 0.0f); break;
            case PlayerMoveDir.RUN_BACK_RIGHT:      angle = new Vector3(0.0f , 135.0f , 0.0f);  break;
        }
       // if(m_currentDir != PlayerMoveDir.NONE)
        transform.GetChild(0).localRotation = 
            Quaternion.Slerp(transform.GetChild(0).localRotation , 
            Quaternion.Euler(angle) , rotateSpeed);

        #endregion
        MoveSend();
    }

    void JumpProcess()
    {
        if(Input.GetKey(m_Jump) && m_isJumpAble)
        {
            StartCoroutine(JumpCall());
            
            m_isJumpAble = false;
            
        }
    }

    IEnumerator JumpCall()
    {
        float startTime = Time.time;

        //중력 적용 안함
        GravityManager.Instance().GRAVITY_POWER = 0.0f;
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
        GravityManager.Instance().GRAVITY_POWER = 100.0f;
        m_isJumpAble = true;
        
    }

    void MoveSend()
    {
        if (NetworkManager.Instance() == null)
            return;

        Vector3 velo = m_rigidBody.velocity;

        NetworkManager.Instance().C2SRequestPlayerMove(name ,
            transform.position , velo ,
            transform.localRotation.eulerAngles ,
            transform.GetChild(0).localRotation.eulerAngles);
    }

    #endregion

    #region Player Trigger Collider ------------------------------------------------------------------------
    
    void OnTriggerEnter(Collider col)
    {
        if (this.enabled == false)
            return;
        // -- F 키를 띄워야 한다. // 근처에 있다면!
        ShowUseEffect(col);
        

    }

    void OnTriggerStay(Collider col)
    {
        if (this.enabled == false)
            return;
        RotateUseEffect();
        MeteorDamage(col);
    }

    void OnTriggerExit(Collider col)
    {
        if (this.enabled == false)
            return;
        // F 키 닫기
        HideUseEffect(col);
        
    }
    #endregion

    #region Player Attack --------------------------------------------------------------------------------
    void AttackProcess()
    {
        if(m_currentWeapon.WEAPON_TYPE == WeaponItem.WeaponType.ETC_RECOVERY)
        {
            if(Input.GetKey(m_AttackKey) && m_currentWeapon != null)
            {
                if(m_interactionAniVal != 1)
                {
                    INTERACTION_ANI_VALUE = 1;
                }
                m_currentWeapon.Recovery(this);
                
            }

            if(Input.GetKeyUp(m_AttackKey) && m_currentWeapon != null)
            {
                //애니메이션 끝 
                INTERACTION_ANI_VALUE = 0;

                m_currentWeapon.RecoveryUp();
            }
        }
        else
        {
            if (Input.GetKey(m_AttackKey) && m_lastCoolTime <= 0.0f && IS_JUMP_ABLE)
            {
                m_isMoveAble = false;
                //   AttackAnimation(1);
                ATTACK_ANI_VALUE = 1;

                // 공격할땐 정면을 보고 공격
                this.transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
                m_currentWeapon.Attack(transform);

                m_lastCoolTime = m_currentWeapon.COOL_TIME;
                Invoke("AttackCoolTime" , m_lastCoolTime);
            }
        }

       
    }

    public void RecoveryItemUseEnd()
    {
        // 리커버리 끝
        m_currentWeapon.gameObject.SetActive(false);
        // 이부분에서 삭제요청
        m_currentWeapon = null;
        InteractionAnimation(0);

        //이부분에서 전에 들고있는 무기 체크 
        SetAnimation(AnimationType.ANI_BAREHAND);
    }

    void AttackCoolTime()
    {
        m_lastCoolTime = -0.0f;
    }

    public void AttackAnimationEnd()
    {
        if (enabled == false)
            return;

        if(m_currentWeapon.WEAPON_TYPE == WeaponItem.WeaponType.ETC_GRENADE)
        {
            m_currentWeapon = null;
            SetAnimation(AnimationType.ANI_BAREHAND);
        }

        AttackAnimation(0);
    }
    #endregion


    #region Player Object Interaction ---------------------------------------------------------------------

    void ShowUseEffect(Collider col)
    {
        if (col.CompareTag("Weapon"))
        {
            m_nearWeapon = col.gameObject;
            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_nearWeapon.transform.position;

        }
        else if (col.CompareTag("OxyCharger"))
        {
            m_nearOxyCharger = col.GetComponent<OxyCharger>();
            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_useEffectHeadAnchor.transform.position;
        }
        else if (col.CompareTag("ItemBox"))
        {
            m_nearItemBox = col.GetComponent<ItemBox>();
            if(m_nearItemBox.OPENED)
            {
                m_nearItemBox = null;
                return;
            }

            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_nearItemBox.transform.position;
        }
        else if (col.CompareTag("ShelterDoor"))
        {
            m_nearShelter = col.transform.parent.GetComponent<Shelter>();
            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_useEffectHandAnchor.transform.position;
        }
        else if (col.CompareTag("SpaceShipControlPanel"))
        {
            m_nearSpaceShip = col.GetComponent<SpaceShip>();
            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_useEffectHeadAnchor.transform.position;
            //  우주선 연료창 띄우기
            m_nearSpaceShip.StartSpaceShipEngineCharge();
        }
        else if(col.CompareTag("Recoverykit"))
        {
            m_nearRecoveryKit = col.GetComponent<RecoverykitItem>();
            m_useEffect.SetActive(true);
            m_useEffect.transform.position = m_nearRecoveryKit.transform.position;
        }
    }

    void HideUseEffect(Collider col)
    {
        if (col.CompareTag("Weapon"))
        {
            m_nearWeapon = null;
            m_useEffect.SetActive(false);
        }
        else if (col.CompareTag("OxyCharger"))
        {
            m_nearOxyCharger = null;
            m_useEffect.SetActive(false);
        }
        else if (col.CompareTag("ItemBox"))
        {
            m_nearItemBox = null;
            m_useEffect.SetActive(false);
        }
        else if (col.CompareTag("ShelterDoor"))
        {
            m_nearShelter = null;
            m_useEffect.SetActive(false);
        }
        else if (col.CompareTag("SpaceShipControlPanel"))
        {
            //  우주선 연료창 닫기
            if (m_nearSpaceShip != null)
                m_nearSpaceShip.StopSpaceShipEngineCharge();
            m_nearSpaceShip = null;
            m_useEffect.SetActive(false);
        }
        else if (col.CompareTag("Recoverykit"))
        {
            m_nearRecoveryKit = null;
            m_useEffect.SetActive(false);
        }

    }

    void RotateUseEffect()
    {
        if (m_useEffect.activeSelf)
            m_useEffect.transform.GetChild(0).GetChild(0).rotation = this.transform.rotation;
    }

    void ControlWeaponObjectProcess()
    {
        if(Input.GetKey(m_Get) && m_nearWeapon != null)
        {
            if (m_currentWeapon != null)
                ThrowWeapon();
            m_currentWeapon = m_nearWeapon.GetComponent<WeaponItem>();
            m_nearWeapon.transform.parent = m_weaponEquipAnchor.transform;
            m_nearWeapon.transform.localPosition = m_currentWeapon.LOCAL_SET_POS;
            m_nearWeapon.transform.localRotation = Quaternion.Euler(m_currentWeapon.LOCAL_SET_ROT);
            m_nearWeapon.transform.localScale = m_currentWeapon.LOCAL_SET_SCALE;

            m_nearWeapon = null;
            m_useEffect.SetActive(false);

            // 기존 코드에 이부분에 무기 발사 앵커를 세팅해주는게 있었지만 무기마다 다르니 무기껄로 쓰기로

            m_currentWeapon.EquipWeapon();

            m_useEffect.SetActive(false);

            WeaponAnimationChange();

            if (GameManager.Instance() != null)
                GameManager.Instance().EquipWeapon(m_currentWeapon.ITEM_ID , 0 , 0);
            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestEquipItem(m_currentWeapon.ITEM_ID , m_currentWeapon.ITEM_NETWORK_ID);
            
        }
    }
    void ControlWeaponObjectThrowProcess()
    {
        if (Input.GetKey(m_throwKey) && m_currentWeapon != null)
        {
            ThrowWeapon();
        }
    }
    void ThrowWeapon()
    {
        RaycastHit throwRayHit;
        Physics.Raycast(transform.position + (transform.rotation * (Vector3.up + Vector3.forward)) ,
            (transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized * -3.0f , out throwRayHit , 10.0f);

        if(m_currentWeapon.WEAPON_TYPE == WeaponItem.WeaponType.ETC_RECOVERY)
            GameManager.Instance().SLIDER_UI.HideSlider();

        SetAnimation(AnimationType.ANI_BAREHAND);
        ATTACK_ANI_VALUE = 0;
        //test
        //if (throwRayHit.collider != null && throwRayHit.collider.CompareTag("NonSpone"))
        //    return;

        m_currentWeapon.transform.parent = null;
        m_currentWeapon.transform.position = throwRayHit.point;

        Vector3 sponRot = (m_currentWeapon.transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized;
        Quaternion targetRot = Quaternion.FromToRotation(m_currentWeapon.transform.up , sponRot) * m_currentWeapon.transform.rotation;

      //  m_currentWeapon.transform.rotation = targetRot;
        m_currentWeapon.transform.Rotate(m_currentWeapon.SPONE_ROTATITON);
        m_currentWeapon.transform.Translate(Vector3.right * 0.15f);
        m_currentWeapon.UnEquipWeapon();

        SetAnimation(AnimationType.ANI_BAREHAND);
        
        if(GameManager.Instance() != null)
            GameManager.Instance().UnEquipWeapon(m_currentWeapon.ITEM_ID , 0 , 0);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestUnEquipItem(m_currentWeapon.ITEM_ID , m_currentWeapon.ITEM_ID ,
                m_currentWeapon.transform.position , m_currentWeapon.transform.eulerAngles);
        m_currentWeapon = null;
    }

    // 일단 리커버리를 임시로 무기 아이템으로 처리한다.

    //// 리커버리 장착키 누름
    //void ControlRecoveryObjectProcess()
    //{
    //    if(Input.GetKeyDown(m_Get) && m_nearRecoveryKit != null)
    //    {
    //        m_currentRecoveryKit = m_nearRecoveryKit;
    //        m_currentRecoveryKit.transform.parent = m_weaponEquipAnchor.transform;
    //        m_currentRecoveryKit.transform.localPosition = m_currentRecoveryKit.LOCAL_SET_POS;
    //        m_currentRecoveryKit.transform.localRotation = Quaternion.Euler(m_currentRecoveryKit.LOCAL_SET_ROT);
    //        m_currentRecoveryKit.transform.localScale = m_currentRecoveryKit.LOCAL_SET_SCALE;

    //        m_currentRecoveryKit.EquipRecoveryKit();
    //        m_nearRecoveryKit = null;

    //        SetAnimation(AnimationType.ANI_ETC);
    //    }
    //}

    //// 리커버리 장착한 상태로 해제키 누름
    //void DropRecoveryObjectProcess()
    //{
    //    if(Input.GetKeyDown(m_Get) && m_currentRecoveryKit != null)
    //    {
    //        RaycastHit throwRayHit;
    //        Physics.Raycast(transform.position + (transform.rotation * (Vector3.up + Vector3.forward)) ,
    //            (transform.position - GravityManager.Instance().CurrentPlanet.transform.position).normalized * -3.0f , out throwRayHit , 10.0f);

    //        // 여기서 들고 있던 아이템이 있을 경우 해당 무기로 변경  처리를 넣자
    //        SetAnimation(AnimationType.ANI_BAREHAND);
    //    }
    //}

    void ControlObjectProcess()
    {
         if(Input.GetKeyDown(m_Get))
        {
            
            if (m_nearItemBox != null) m_nearItemBox.UseItemBox();
            if (m_nearShelter != null) m_nearShelter.DoorControl();
            if (m_nearSpaceShip != null)
            {
                if(m_currentWeapon != null)
                   m_currentWeapon.gameObject.SetActive(false);
                SetAnimation(AnimationType.ANI_ETC);
                INTERACTION_ANI_VALUE = 1;
                IS_MOVE_ABLE = false;
                m_nearSpaceShip.StartSpaceShipEngineCharge();
            }
            
        }
         if(Input.GetKey(m_Get))
        {
            if (m_nearSpaceShip != null)
            {
                m_nearSpaceShip.SpaceShipEngineChargeProcess();
            }
        }
         if(Input.GetKeyUp(m_Get))
        {
            if (m_nearSpaceShip != null)
            {
                IS_MOVE_ABLE = true;
                INTERACTION_ANI_VALUE = 0;
                if (m_currentWeapon != null)
                {
                    m_currentWeapon.gameObject.SetActive(true);
                    WeaponAnimationChange();
                }
                    
                m_nearSpaceShip.StopSpaceShipEngineCharge();
            }
        }
         

        ControlOxyChargerProcess();
    }

    void ControlOxyChargerProcess()
    {
        if (m_nearOxyCharger == null)
            return;

        if (Input.GetKeyDown(m_Get))
        {

            m_targetOxy = 100.0f - GameManager.Instance().PLAYER.m_oxy;
            m_plusOxy = 0.0f;
            SetAnimation(AnimationType.ANI_ETC);
            if (m_currentWeapon != null)
            {
                m_currentWeapon.gameObject.SetActive(false);

            }
            INTERACTION_ANI_VALUE = 1;
            GameManager.Instance().SLIDER_UI.ShowSlider();
            GameManager.Instance().SLIDER_UI.Reset();
            
        }

        if(Input.GetKey(m_Get))
        {
            float oxy = 10.0f * Time.deltaTime;
            m_plusOxy += oxy;
            GameManager.Instance().SLIDER_UI.SliderProcess(m_plusOxy / m_targetOxy);
            m_nearOxyCharger.UseOxy(oxy);

            if(GameManager.Instance().PLAYER.m_oxy >= 100.0f)
            {
                InteractionAnimation(0);
                if(m_currentWeapon != null)
                {
                    m_currentWeapon.gameObject.SetActive(true);
                    WeaponAnimationChange();
                    
                }
                GameManager.Instance().SLIDER_UI.HideSlider();
            }
        }
        if(Input.GetKeyUp(m_Get))
        {
            INTERACTION_ANI_VALUE = 0;
            m_targetOxy = 0.0f;
            if (m_currentWeapon != null)
            {
                m_currentWeapon.gameObject.SetActive(true);
                WeaponAnimationChange();
            }
            GameManager.Instance().SLIDER_UI.HideSlider();
        }
    }


    // 메테오 데미지
    void MeteorDamage(Collider col)
    {
        if(col.CompareTag("Meteor") && IS_SHELTER == false)
        {
            if (m_meteorCoolTime > 0.0f)
                return;

            if (NetworkManager.Instance() != null)
                NetworkManager.Instance().C2SRequestPlayerDamage(
                    (int)NetworkManager.Instance().m_hostID , "" , "Meteor" , 10.0f,Vector3.zero);
            m_meteorCoolTime = 0.5f;
            InvokeRepeating("MeteorCoolTimeChecker" , 0.1f , 0.1f);
        }
    }

    void MeteorCoolTimeChecker()
    {
        m_meteorCoolTime -= 0.1f;
        if(m_meteorCoolTime <= 0.0f)
        {
            m_meteorCoolTime = -1.0f;
            CancelInvoke("MeteorCoolTimeChecker");
        }
    }

    public void Damage(Vector3 dir,string reason = null)
    {
        if (m_nearSpaceShip != null && !reason.Equals("oxy") && !reason.Equals("DeathZone"))
            m_nearSpaceShip.StopSpaceShipEngineCharge();

        if(m_nearOxyCharger != null && m_targetOxy > 0.0f && !reason.Equals("oxy") && !reason.Equals("DeathZone"))
        {
            INTERACTION_ANI_VALUE = 0;
            m_targetOxy = 0.0f;
            if (m_currentWeapon != null)
            {
                m_currentWeapon.gameObject.SetActive(true);
                WeaponAnimationChange();
            }
            GameManager.Instance().SLIDER_UI.HideSlider(); InteractionAnimation(0);
            if (m_currentWeapon != null)
            {
                m_currentWeapon.gameObject.SetActive(true);
                WeaponAnimationChange();
            }
            GameManager.Instance().SLIDER_UI.HideSlider();
        }
        // 보류

        //CameraManager.Instance().PLAYER_ROTATE = false;
        //Vector3 rot = Quaternion.LookRotation( (dir - transform.GetChild(0).position).normalized).eulerAngles;
        //transform.GetChild(0).localEulerAngles = new Vector3(0.0f , rot.y , 0.0f) ;

        //if (IsInvoking("DamageAniCheck") == false)
        //    InvokeRepeating("DamageAniCheck" , 0.5f , Time.deltaTime);
    }

    void DamageAniCheck()
    {
        if(m_animator.GetCurrentAnimatorStateInfo(0).IsName("Damage") == false)
        {
            CancelInvoke("DamageAniCheck");
            CameraManager.Instance().PLAYER_ROTATE = true;
        }
    }

    
    #endregion

    #region Animation Function

    void WeaponAnimationChange()
    {
        switch (m_currentWeapon.WEAPON_TYPE)
        {
            case WeaponItem.WeaponType.GUN: SetAnimation(AnimationType.ANI_GUN01); break;
            case WeaponItem.WeaponType.RIFLE: SetAnimation(AnimationType.ANI_GUN02); break;
            case WeaponItem.WeaponType.MELEE: SetAnimation(AnimationType.ANI_MELEE); break;
            case WeaponItem.WeaponType.ROCKETLAUNCHER: SetAnimation(AnimationType.ANI_ROCKETLAUNCHER); break;
            case WeaponItem.WeaponType.ETC_GRENADE: SetAnimation(AnimationType.ANI_ETC); break;
            case WeaponItem.WeaponType.ETC_RECOVERY: SetAnimation(AnimationType.ANI_ETC); break;
        }
    }

    public void AttackAnimationEvent()
    {
        // 애니메이션 상에서 어택 시점을 조절해야 할 경우 여기로 들어옴
        if(m_currentWeapon != null)
        {
            m_currentWeapon.AnimationEventAttack();
        }
    }

    void WalkAnimation(int value)
    {
        if (m_animator.GetInteger("WALK") == value)
            return;
        m_animator.SetInteger("WALK" , value);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "WALK" , value);
    }

    void JumpAnimation(int value)
    {
        if (m_animator.GetInteger("JUMP") == value)
            return;
        m_animator.SetInteger("JUMP" , value);
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "JUMP" , value);
    }

    public void AttackAnimation(int value)
    {    
        if (m_animator.GetInteger("ATTACK") == value)
            return;
        if (value == 0)
            m_isMoveAble = true;
        m_animator.SetInteger("ATTACK" , value);
        if (m_currentWeapon != null)
            m_currentWeapon.AnimationEventAttackEnd();
        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "ATTACK" , value);
    }

    public void InteractionAnimation(int value)
    {
        if (m_animator.GetInteger("INTERACTION") == value)
            return;
        m_animator.SetInteger("INTERACTION" , value);

        if (NetworkManager.Instance() != null)
            NetworkManager.Instance().C2SRequestPlayerAnimation(
                NetworkManager.Instance().USER_NAME , "INTERACTION" , value);
    }

    public void AnimationPlay(string aniName)
    {
        m_animator.Play(aniName);
        // 강제 재생
        NetworkManager.Instance().C2SRequestPlayerAnimation(
           GameManager.Instance().PLAYER.m_name , aniName , 1234);
    }

    bool CheckAnimaton(string stateName)
    {
        if (m_animator.GetNextAnimatorStateInfo(0).IsName(stateName))
        {
            if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                return true;
            else
                return false;
        }
        else
            return false;
    }

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

    #endregion
}
