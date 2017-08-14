using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    #region Player_INFO

    private Rigidbody m_rigidBody = null;

    public Transform PlayerTransform;
    public Transform ModelTransform;

    public float MoveSpeed;
    public float RunSpeed;

    public KeyCode Foward;
    public KeyCode Back;
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Run;
    public KeyCode Jump;
    public KeyCode Gun_Shot;
    public KeyCode GetKey;
    public KeyCode ThrowKey;

    public PlayerMove.MoveState PrevState;
    public PlayerMove.MoveState NowState;

    public float JumpH;
    public float JumpT;

    public Transform EffectShowLongAnchor;

    public GameObject WeaponAnchor;
    
    public GameObject Weapon = null;
    public Animator PlayerAnim;

    public float LastAttackTime;

    public GameObject NearWeapon = null;

    public bool IsMoveable = true;
    public bool IsJumpable = true;

    public Transform FirePoint;

    public GameObject m_UseEffect;




    public AnimationCurve JumpCurve;

    public bool NearWeaponPick;

    private Vector3 m_prevPos = Vector3.zero;

    public OxyCharger NearOxyCharger = null;
    public ItemBox NearItemBox = null;
    public Shelter NearShelter = null;
    public SpaceShip m_nearSpaceShip = null;

    private bool m_isShelter = false;

    public bool IS_SHELTER
    {
        get { return m_isShelter; }
        set { m_isShelter = value; }
    }

    float m_coolTime = 0.0f;

    #endregion

    // TEST CODE
    void MoveSend()
    {
        if (!NetworkManager.Instance().LOGIN_STATE)
            return;
        Vector3 velo = m_rigidBody.velocity; //(transform.position - m_prevPos) / Time.deltaTime;

        //  if (PrevState != NowState)
        NetworkManager.Instance().C2SRequestPlayerMove(name ,
            transform.position , velo ,
            transform.localRotation.eulerAngles ,
            transform.GetChild(0).localRotation.eulerAngles);

    }

    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        InvokeRepeating("UseOxy" , 2.0f , 2.0f);
    }

    

    // Update is called once per frame
    void Update()
    {
        PrevState = NowState;
        m_prevPos = transform.position;
        if (IsMoveable)
        {
            Move();
        }


        if (Weapon != null)
        {
            AttackAnim();
            ThrowWeapon();
        }
        else
        {
            GetWeapon();
            ControlObject();
        }

        if (NearWeaponPick)
        {
            NearWeaponPickCheck();
        }

        
        
    }

    void UseOxy()
    {
        if (NetworkManager.Instance().LOGIN_STATE)
        {
            float oxy = Random.Range(0.1f , 10.0f);
            if (GameManager.Instance().PLAYER.m_oxy - oxy >= 0.0f)
                NetworkManager.Instance().C2SRequestPlayerUseOxy(GameManager.Instance().PLAYER.m_name , oxy);
        }
    }

    private void NearWeaponPickCheck()
    {
        if (NearWeapon.transform.parent != null)
        {
            NearWeapon = null;
            m_UseEffect.SetActive(false);
            NearWeaponPick = false;
        }
    }

    public void GetWeapon()
    {
        if (Input.GetKeyDown(GetKey) && Weapon == null && NearWeapon != null)
        {
            
            NearWeapon.transform.parent = WeaponAnchor.transform;
            NearWeapon.transform.localPosition = NearWeapon.GetComponent<Weapon>().LocalSetPos;
            NearWeapon.transform.localRotation = Quaternion.Euler(NearWeapon.GetComponent<Weapon>().LocalSetRot);
            NearWeapon.transform.localScale = NearWeapon.GetComponent<Weapon>().LocalSetScale;
            
            Weapon = NearWeapon;
            Weapon.GetComponent<Weapon>().FirePoint = FirePoint;

            PlayerAnim.gameObject.GetComponent<AttackEvent>().Weapon = NearWeapon;
            Weapon.GetComponent<SphereCollider>().enabled = false;

            m_UseEffect.SetActive(false);
            NearWeaponPick = false;

            // 
            Weapon w = Weapon.GetComponent<Weapon>();
            GameManager.Instance().EquipWeapon(w.CID , 0 , 0);
            NetworkManager.Instance().C2SRequestEquipItem(w.CID , w.WeaponID);
        }
    }


    public void ControlObject()
    {
        if(Input.GetKeyDown(GetKey))
        {
            if (NearOxyCharger != null)
                NearOxyCharger.UseOxy();
            if (NearItemBox != null)
                NearItemBox.UseItemBox();
            if (NearShelter != null)
                NearShelter.DoorControl();
            if (m_nearSpaceShip != null)
                m_nearSpaceShip.StartSpaceShipEngineCharge();
        }

        if(m_nearSpaceShip != null)
        {
            if(Input.GetKey(GetKey))
                m_nearSpaceShip.SpaceShipEngineCharge(0.01f , true);
            
            if (Input.GetKeyUp(GetKey))
                m_nearSpaceShip.StopSpaceShipEngineCharge();
        }
    }

    public void ThrowWeapon()
    {
        if (Input.GetKeyDown(ThrowKey) && Weapon != null)
        {
            RaycastHit ThrowPos;
            Physics.Raycast(ModelTransform.position + (ModelTransform.rotation * (Vector3.up + Vector3.forward) * 1) , 
                (ModelTransform.position - AnchorPlanet.Planet.position).normalized * -3 , out ThrowPos , 10f);


            if (ThrowPos.point == Vector3.zero || ThrowPos.collider.gameObject.CompareTag("NonSpone"))
            {

                Debug.Log(ThrowPos.point);
                return;

            }

            Debug.Log(ThrowPos.normal);

            Weapon.transform.parent = null;
            Weapon.transform.position = ThrowPos.point;


            Vector3 SponeRot = (Weapon.transform.position - AnchorPlanet.PlanetAnchor.position).normalized;

            Quaternion targetRotation = Quaternion.FromToRotation(Weapon.transform.up , SponeRot) * Weapon.transform.rotation;

            Weapon.transform.rotation = targetRotation;

            Weapon.transform.Rotate(Weapon.GetComponent<Weapon>().SponeRot);

            Weapon.transform.Translate(Vector3.right * 0.15f);


            //Weapon.transform.localScale = NearWeapon.GetComponent<Weapon>().ThrowSetScale;



            Weapon.GetComponent<Weapon>().FirePoint = null;

            PlayerAnim.gameObject.GetComponent<AttackEvent>().Weapon = null;
            Weapon.GetComponent<SphereCollider>().enabled = true;


            // TODO WEAPON
            //his.GetComponent<TestStram>().SenddataCall("012/" + Weapon.GetComponent<Weapon>().WeaponID + "/" + Weapon.transform.position.x + "/" + Weapon.transform.position.y + "/" + Weapon.transform.position.z + "/" + Weapon.transform.rotation.x + "/" + Weapon.transform.rotation.y + "/" + Weapon.transform.rotation.z + "/" + Weapon.transform.rotation.w + "/" + Weapon.transform.localScale.x + "/" + Weapon.transform.localScale.y + "/" + Weapon.transform.localScale.z);

            Weapon w = Weapon.GetComponent<Weapon>();

            GameManager.Instance().UnEquipWeapon(w.CID , 0 , 0);
            NetworkManager.Instance().C2SRequestUnEquipItem(w.CID , w.WeaponID , Weapon.transform.position , Weapon.transform.eulerAngles);

            Weapon = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        bool showUseKeyEffect =
            (other.CompareTag("Weapon") && Weapon != null) ||
            (other.CompareTag("OxyCharger") && NearOxyCharger != null) ||
            (other.CompareTag("ItemBox") && NearItemBox != null);

        if (showUseKeyEffect)
        {
            m_UseEffect.transform.GetChild(0).GetChild(0).rotation = this.transform.rotation;
        }

        if (other.CompareTag("Meteor") && IS_SHELTER == false)
        {
            
            if (m_coolTime > 0.0f)
            {
                return;
            }
            //NetworkManager.Instance().C2SRequestPlayerDamage(
            //    (int)NetworkManager.Instance().m_hostID , "" , "Meteor" , 10.0f);
            m_coolTime = 0.5f;
            InvokeRepeating("CoolTimeChecker" , 0.1f , 0.1f);
            
        }

    }

    void CoolTimeChecker()
    {
        m_coolTime -= 0.1f;

        if(m_coolTime <= 0.0f)
        {
            m_coolTime = -1.0f;
            CancelInvoke("CoolTimeChecker");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon") && Weapon == null)
        {
            NearWeapon = other.gameObject;

            // Vector3 pos = NearWeapon.transform.GetChild(0).position;
            m_UseEffect.transform.position = NearWeapon.transform.GetChild(0).position;// new Vector3(pos.x , pos.y , pos.z);
                                                                                       //m_UseEffect.transform.parent = NearWeapon.transform;
                                                                                       //m_UseEffect.transform.rotation = NearWeapon.transform.rotation;

            //m_UseEffect.transform.Rotate(0 , 0 , -90f);


            m_UseEffect.SetActive(true);
            NearWeaponPick = true;

        }

        else if (other.CompareTag("OxyCharger") && NearOxyCharger == null)
        {
            m_UseEffect.SetActive(true);
            NearOxyCharger = other.GetComponent<OxyCharger>();
            Vector3 pos = EffectShowLongAnchor.position;

            m_UseEffect.transform.parent = transform;
            m_UseEffect.transform.position = new Vector3(pos.x , pos.y ,
                transform.position.z);
        }

        else if (other.CompareTag("ItemBox") && NearItemBox == null)
        {
            if (other.GetComponent<ItemBox>().OPENED)
                return;

            m_UseEffect.SetActive(true);
            NearItemBox = other.GetComponent<ItemBox>();
            Vector3 pos = NearItemBox.transform.position;
            m_UseEffect.transform.position = new Vector3(pos.x , pos.y + 0.5f ,
                pos.z);
        }
        else if (other.CompareTag("ShelterDoor") && NearShelter == null)
        {
            m_UseEffect.SetActive(true);
            NearShelter = other.transform.parent.GetComponent<Shelter>();

            if (m_isShelter)
                m_UseEffect.transform.position = other.transform.GetChild(1).position;
            else
                m_UseEffect.transform.position = other.transform.GetChild(0).position;
        }
        else if(other.CompareTag("SpaceShipControlPanel"))
        {
            m_UseEffect.SetActive(true);
            m_UseEffect.transform.position = other.transform.GetChild(0).GetChild(1).position;
            m_nearSpaceShip = other.GetComponent<SpaceShip>();
            m_nearSpaceShip.StartSpaceShipEngineCharge();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            NearWeapon = null;
            m_UseEffect.SetActive(false);
            NearWeaponPick = false;
        }

        else if (other.CompareTag("OxyCharger"))
        {
            m_UseEffect.SetActive(false);
            NearOxyCharger = null;
        }
        else if (other.CompareTag("ItemBox"))
        {
            m_UseEffect.SetActive(false);
            NearItemBox = null;
        }
        else if(other.CompareTag("ShelterDoor"))
        {
            m_UseEffect.SetActive(false);
            NearShelter = null;
        }
        else if (other.CompareTag("SpaceShipControlPanel") && m_nearSpaceShip != null)
        {
            m_UseEffect.SetActive(false);
            m_nearSpaceShip.StopSpaceShipEngineCharge();
            m_nearSpaceShip = null;
        }

    }

    public void AttackAnimPlay(string AnimationName , int AnimLayer)
    {
        
        if (PlayerMove.CheckAnim(PlayerAnim , AnimationName))
        {
            AnimationSettingAndSend("AttackState" , 1);
          //  PlayerAnim.Play(AnimationName , AnimLayer);
        }

    }
    private bool AttackCoolTimeCheck(float CoolTime)
    {
        if (Time.time - LastAttackTime >= CoolTime)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private void AttackAnim()
    {
        if (Input.GetKey(Gun_Shot) && AttackCoolTimeCheck(Weapon.GetComponent<Weapon>().AttackCoolTime) && IsJumpable)
        {
            AttackAnimPlay(Weapon.GetComponent<Weapon>().AttackAnimName , 0);


            // TODO WEAPON
            // this.GetComponent<TestStram>().SenddataCall("02/" + 0 + "/" + Weapon.GetComponent<Weapon>().FirePoint.position.x + "/" + Weapon.GetComponent<Weapon>().FirePoint.position.y + "/" + Weapon.GetComponent<Weapon>().FirePoint.position.z + "/" + this.transform.rotation.x + "/" + this.transform.rotation.y + "/" + this.transform.rotation.z + "/" + this.transform.rotation.w + "/" + PhotonNetwork.time.ToString());
            LastAttackTime = Time.time;
        }
        else
        {
            AnimationSettingAndSend("AttackState" , 0);
        }

    }
    // h 3 T 1
    IEnumerator JumpCall(Transform Character , float H , float T , PlayerMove.MoveState LastState)
    {

        float StartTime = Time.time;
        float Dur = Time.fixedDeltaTime / T;


        AnchorPlanet.Planet.GetComponent<Gravity>().Power = 0;
        //AnchorPlanet.MainCam.GetComponent<CamRotate>().RotateNow = false;

        while (Time.time - StartTime < T)
        {
            float NowT = (Time.time - StartTime) / T;

            Character.Translate(Vector3.up * (H * (JumpCurve.Evaluate(NowT + Time.fixedDeltaTime) - JumpCurve.Evaluate(NowT))));

            Debug.Log(NowT);

            float Speed;

            if ((int)LastState >= 9)
            {
                Speed = RunSpeed;
            }
            else
            {
                Speed = MoveSpeed;
            }

            PlayerMove.MoveCode(PlayerTransform , LastState , Speed,GameManager.Instance().PLAYER.m_name);


            yield return new WaitForFixedUpdate();

        }

        AnchorPlanet.Planet.GetComponent<Gravity>().Power = 100;
        //AnchorPlanet.MainCam.GetComponent<CamRotate>().RotateNow = true;

        AnimationSettingAndSend("JumpState" , 0);
        // PlayerAnim.speed = 1;
        //yield return new WaitForSeconds(0.05f);


        IsJumpable = true;

        IsMoveable = true;



    }

    #region Player Move
    private void Move()
    {
        #region MoveLogic
        if (Input.GetKeyDown(Jump) && IsJumpable)
        {
            StartCoroutine(JumpCall(this.transform , JumpH , JumpT , NowState));

            if (PlayerMove.CheckAnim(PlayerAnim , "Jump"))
            {
                //PlayerAnim.speed = 0.533f / JumpT;
                AnimationSettingAndSend("JumpState" , 1);
                // TargetAnim.CrossFade("Idle", 0.2f, 0);
            }

            IsJumpable = false;
            IsMoveable = true; // test code 
        }

        if (Input.GetKey(Run))
        {
            if (Input.GetKey(Foward) && !Input.GetKey(Back) && !Input.GetKey(Left) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunFoward , RunSpeed,GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunFoward;
            }
            else if (Input.GetKey(Foward) && Input.GetKey(Back))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunFoward , RunSpeed, GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunFoward;
            }
            else if (Input.GetKey(Foward) && Input.GetKey(Left))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunFoward_Left , RunSpeed, GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunFoward_Left;

            }
            else if (Input.GetKey(Foward) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunFoward_Right , RunSpeed, GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunFoward_Right;

            }
            else if (Input.GetKey(Back) && !Input.GetKey(Foward) && !Input.GetKey(Left) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunBack , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunBack;
            }
            else if (Input.GetKey(Back) && Input.GetKey(Left))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunBack_Left , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunBack_Left;
            }
            else if (Input.GetKey(Back) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunBack_Right , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunBack_Right;

            }
            else if (Input.GetKey(Left) && !Input.GetKey(Back) && !Input.GetKey(Foward) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunLeft , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunLeft;
            }
            else if (Input.GetKey(Left) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunLeft , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunLeft;
            }
            else if (Input.GetKey(Right) && !Input.GetKey(Back) && !Input.GetKey(Left) && !Input.GetKey(Foward))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.RunRight , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.RunRight;
            }
            else
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.Idle , RunSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.Idle;
            }
        }
        else
        {
            if (Input.GetKey(Foward) && !Input.GetKey(Back) && !Input.GetKey(Left) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkFoward , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkFoward;
            }
            else if (Input.GetKey(Foward) && Input.GetKey(Back))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkFoward , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkFoward;
            }
            else if (Input.GetKey(Foward) && Input.GetKey(Left))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkFoward_Left , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkFoward_Left;

            }
            else if (Input.GetKey(Foward) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkFoward_Right , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkFoward_Right;

            }
            else if (Input.GetKey(Back) && !Input.GetKey(Foward) && !Input.GetKey(Left) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkBack , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkBack;
            }
            else if (Input.GetKey(Back) && Input.GetKey(Left))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkBack_Left , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkBack_Left;
            }
            else if (Input.GetKey(Back) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkBack_Right , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkBack_Right;

            }
            else if (Input.GetKey(Left) && !Input.GetKey(Back) && !Input.GetKey(Foward) && !Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkLeft , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkLeft;
            }
            else if (Input.GetKey(Left) && Input.GetKey(Right))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkLeft , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkLeft;
            }
            else if (Input.GetKey(Right) && !Input.GetKey(Back) && !Input.GetKey(Left) && !Input.GetKey(Foward))
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.WalkRight , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.WalkRight;
            }
            else
            {
                PlayerMove.MoveCode(PlayerTransform , PlayerMove.MoveState.Idle , MoveSpeed , GameManager.Instance().PLAYER.m_name);

                NowState = PlayerMove.MoveState.Idle;
            }
        }
        #endregion

        MoveSend();

    }
    #endregion

    #region NetworkMessage
    public void AnimationSettingAndSend(string aniName,int aniValue)
    {
        if (aniValue != 1234)
            PlayerAnim.SetInteger(aniName , aniValue);
        else
            PlayerAnim.Play(aniName);
        NetworkManager.Instance().C2SRequestPlayerAnimation(
            GameManager.Instance().PLAYER.m_name , aniName , aniValue);
    }
    #endregion
}
