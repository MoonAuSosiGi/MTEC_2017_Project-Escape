using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour {

    public Transform Player; // 플레이어
    public Transform[] CamAnchor; // 카메라 고정 오브젝트 2개 0 = 처음, 1 = 중간, 2 = 마지막
    

    public float[] CamRoateSpeed; // 카메라 회전속도 0 = X, 1 = Y
    public float[] CamDis; // 카메라 거리 조절 0 = 최소, 1 = 최대, 2 = 현재값
    public float CamZoomSpeed; // 카메라 줌인 줌아웃 속도

    public Vector2 CamAngle; // 현재 카메라 각도 X, Y

    public bool RotateNow = true;

	void Start () {

        CamSet();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        AnchorPlanet.MainCam = this.transform;
    }
	
	void Update () {

        if (RotateNow)
        {
            CamRotateCode();
        }
        CursorManager();

    }

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

        Player.Rotate(0, Input.GetAxis("Mouse X") * CamRoateSpeed[0], 0); // 플레이어 캐릭터(카메라 부모 오브젝트) X축 회전

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



        CamAnchor[0].localRotation = Quaternion.Euler(CamAngle.y, 0, 0); // 회전 적용

        CamLastPosSet(); // 카메라가 오브젝트뒤로 나가는 현상 방지

    }

    private void CamSet() // 카메라 초기 위치, 각도 세팅
    {
        this.transform.parent = CamAnchor[2]; // 부모 설정
        this.transform.localPosition = Vector3.zero; // 위치 설정
        this.transform.localRotation = Quaternion.Euler(Vector3.zero); // 각도 설정
    }

    private void OnGUI()
    {
        Event CheckInput = Event.current; // 이벤트 저장

        if (CheckInput.type == EventType.ScrollWheel) // 마우스 휠 이벤트가 맞는지 타입 확인
        {
            CamDis[2] -= Input.GetAxis("Mouse ScrollWheel") * CamZoomSpeed; // 줌인 줌아웃

            if (CamDis[2] < CamDis[0]) // 줌인 줌아웃 최대, 최소 거리 보정
            {
                CamDis[2] = CamDis[0];
            }
            else if (CamDis[2] > CamDis[1])
            {
                CamDis[2] = CamDis[1];
            }

            CamLastPosSet(); // 카메라가 오브젝트뒤로 나가는 현상 방지
        }
    }

    private void CamLastPosSet() // 카메라가 오브젝트뒤로 나가는 현상 방지
    {
        RaycastHit Hitinfo;
        Physics.Raycast(CamAnchor[1].position, CamAnchor[2].rotation * Vector3.back, out Hitinfo, (float)Vector3.Distance(CamAnchor[1].position, Vector3.Lerp(CamAnchor[1].position, CamAnchor[2].position, CamDis[2]))); // 레이캐스트
        //Debug.DrawRay(CamAnchor[1].position, CamAnchor[2].rotation * Vector3.back * Vector3.Distance(CamAnchor[1].position, Vector3.Lerp(CamAnchor[1].position, CamAnchor[2].position, CamDis[2])), Color.red, 0.1f);

        if (Hitinfo.point == Vector3.zero)
        {
            this.transform.position = Vector3.Lerp(CamAnchor[1].position, CamAnchor[2].position, CamDis[2]);
        }
        else
        {
            this.transform.position = Hitinfo.point;
        }
    }


}
