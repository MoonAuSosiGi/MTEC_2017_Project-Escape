#ifndef SERVER_H
#define SERVER_H

#include "Client.h"
#include "Item.h"
#include "Shelter.h"
#include "GameRoom.h"
#include "OxyCharger.h"
#include "SpaceShip.h"
#include "SP_Marshaler.h"
#include "../Common/SpaceWar_stub.h"
#include "../Common/SpaceWar_proxy.h"

#include "../Common/Common.h"
#include "../Common/Common.cpp"
#include "../Common/SpaceWar_stub.cpp"
#include "../Common/SpaceWar_proxy.cpp"
#include "../Common/SpaceWar_common.cpp"

// 우주선 잠금 해제까지 남은 시간
int s_spaceShipLockTime = 60;

// 메테오까지 남은시간
int s_meteorCommingSec = 90;
// 데스존 까지 남은 시간
int s_deathZoneCommingSec = 30;

// 바뀌는 메테오 로직
int m_meteorCommingTime[10] = { 30 ,50,70,90,110,130,150,170,190,210 };

// 데스존이 진행하고 있는 인덱스
int s_deathZoneIndex = 0;

// 데스존을 움직이는 주체
int s_deathZoneHostID = -1;

//Meteor ID 
int s_meteorID = 0;

// Death Zone ID
int s_deathZoneID = 0;

class Server : public SpaceWar::Stub
{
public:
	Server() 
	{ 
		m_gameRoom = make_shared <GameRoom>(); 
	}
	~Server() {}

public:
	// P2P 그룹 이름 --------------------------
	HostID m_playerP2PGroup = HostID_None; 
public:

#pragma region Server Class Logic -------------------------------------------------------------------
	// 서버 실행 로직
	void ServerRun();
	// 서버 이벤트 로직
	// 클라 접속시
	void OnClientJoin(CNetClientInfo* clientInfo);
	// 클라 접속 해제시
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

	// 서버 리셋
	void ServerReset();
#pragma endregion

#pragma region C2S  ---------------------------------------------------------------------------------
#pragma region 초기 접속 ========================
#pragma region 공통 구간 ------------------------

	// Connect 초기 접속 요청
	DECRMI_SpaceWar_RequestServerConnect;

	// 팀 선택 바꿈
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect;

	//정상적인 종료 루틴 // 게임 끝났음을 알림 ( 호스트가 보낸다 ))
	DECRMI_SpaceWar_RequestGameExit;

#pragma endregion

#pragma region 호스트 아닌 유저 -----------------
	// 레디
	DECRMI_SpaceWar_RequestNetworkGameReady;
	// 서버 로비 로직 // 로비 화면에 들어왔다.
	DECRMI_SpaceWar_RequestLobbyConnect;

	// 게임이 시작되고 게임 씬으로 넘어왔을 때 다들 요청한다.
	DECRMI_SpaceWar_RequestGameSceneJoin;
#pragma endregion

#pragma region 방장 호스트 전용 -----------------
	// 방장이 맵을 바꿈
	DECRMI_SpaceWar_RequestNetworkChangeMap;
	// 방장이 플레이어 수를 바꿈
	DECRMI_SpaceWar_RequestNetworkPlayerCount;
	// 방장이 게임 모드를 바꿈
	DECRMI_SpaceWar_RequestNetworkGameModeChange;
	// 방장이 게임 시작을 누름
	DECRMI_SpaceWar_RequestNetworkGameStart;
	// 방장이 로비를 나갔다.
	DECRMI_SpaceWar_RequestNetworkHostOut;
#pragma endregion
#pragma endregion

#pragma region 인게임 ===========================

#pragma region 인게임 :: 플레이어 메시지 --------
#pragma region [플레이어 상태 관련 정보]
	// 체력 회복 메시지
	DECRMI_SpaceWar_RequestHpUpdate;
	// 플레이어가 맞았다.
	DECRMI_SpaceWar_RequestPlayerDamage;
	// 플레이어가 숨을 쉬었다.
	DECRMI_SpaceWar_RequestPlayerUseOxy;
	// 플레이어 움직임 처리 
	DECRMI_SpaceWar_NotifyPlayerMove;
	//아이템 장비 // 추후 지울 수 있음
	DECRMI_SpaceWar_NotifyPlayerEquipItem;
#pragma endregion
#pragma endregion

#pragma region 인게임 :: 산소 충전기 ------------
	// 처음에 산소 충전기를 등록한다
	DECRMI_SpaceWar_RequestOxyChargerStartSetup;
	// 산소 충전기 조작 요청 
	DECRMI_SpaceWar_RequestUseOxyChargerStart;
	// 산소 충전기 조작중
	DECRMI_SpaceWar_RequestUseOxyCharger;
	// 산소 충전기 조작 끝
	DECRMI_SpaceWar_RequestUseOxyChargerEnd;
#pragma endregion

#pragma region 인게임 :: 아이템 박스 조작 -------
	// 아이템 박스 조작
	DECRMI_SpaceWar_RequestUseItemBox;
#pragma endregion

#pragma region 인게임 :: 쉘터 조작 --------------
	// 쉘터 등록
	DECRMI_SpaceWar_RequestShelterStartSetup;
	// 쉘터 문 조작
	DECRMI_SpaceWar_RequestShelterDoorControl;

	// 쉘터 입장 퇴장
	DECRMI_SpaceWar_RequestShelterEnter;
#pragma endregion

#pragma region 인게임 :: 아이템 로직 ------------
	// 월드에 아이템 생성 요청
	DECRMI_SpaceWar_RequestWorldCreateItem;
	// 아이템 삭제 요청
	DECRMI_SpaceWar_RequestItemDelete;
	// 월드 아이템 삭제 알림
	DECRMI_SpaceWar_NotifyDeleteItem;
#pragma endregion

#pragma region 인게임 :: 우주선 -----------------
	// 우주선 몇개 있는지 세팅
	DECRMI_SpaceWar_RequestSpaceShipSetup;
	// 우주선 탔음 
	DECRMI_SpaceWar_RequestSpaceShip;
	// 우주선 사용 요청
	DECRMI_SpaceWar_RequestUseSpaceShip;
	// 우주선 사용 취소
	DECRMI_SpaceWar_RequestUseSpaceShipCancel;
#pragma endregion

#pragma region 인게임 :: 데스존 -----------------
	// 데스존이 이동하고 있는 단계를 통보해줘라
	DECRMI_SpaceWar_RequestDeathZoneMoveIndex;
#pragma endregion

#pragma endregion

#pragma region 결과창 ==========================
	// 드로우 게임시 정보 요청
	DECRMI_SpaceWar_RequestDrawGameResult;

	// 결과를 받아오는 것
	DECRMI_SpaceWar_RequestGameEnd;
#pragma endregion
#pragma endregion

	//DECRMI_SpaceWar_RequestClientJoin;

private:
	// 아이템 박스 등 상호작용 오브젝트의 경우 동시접근을 막아야 하므로
	// 현재 접근하고 있는지 체크하는 로직이 필요함, 이에 따른 체크용 변수들
	unordered_map<int, int> m_itemBoxMap;

	// 쉘터의 경우엔 등록해야함
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap;

	// 플레이 타임
	int m_gameStartTime;

	// 아이템박스 인덱스
	int m_itemBoxCreateItemIndex;
public:
	// 전송 프록시
	SpaceWar::Proxy m_proxy;
	// 서버 객체
	shared_ptr<CNetServer> m_netServer;
	// 스레드 lock 을 위함 
	CriticalSection m_critSec;

	// 산소충전기 리스트
	unordered_map<int, shared_ptr<OxyCharger>> m_oxyChargerMap;

	// 우주선 리스트
	unordered_map<int, shared_ptr<SpaceShip>> m_spaceShipMap;

	// 클라이언트 리스트
	unordered_map<HostID, shared_ptr<Client>> m_clientMap;
	
	// 아이템 리스트
	unordered_map<string, shared_ptr<Item>> m_itemMap;

	// 게임 룸
	shared_ptr<GameRoom> m_gameRoom;

	// 게임 유저 리셋
	void ResetUsers();
	
	//우주선 총 카운트 얻기
	int GetSpaceShipCount()
	{
		return m_spaceShipMap.size();
	}
};

// 랜덤함수
float RandomRange(float min, float max)
{
	return ((float(rand()) / float(RAND_MAX)) * (max - min)) + min;
}
#endif