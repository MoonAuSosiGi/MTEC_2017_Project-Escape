#ifndef SERVER_H
#define SERVER_H

#include "Client.h"
#include "Item.h"
#include "Shelter.h"
#include "GameRoom.h"
#include "SP_Marshaler.h"
#include "../Common/SpaceWar_stub.h"
#include "../Common/SpaceWar_proxy.h"

#include "../Common/Common.h"
#include "../Common/Common.cpp"
#include "../Common/SpaceWar_stub.cpp"
#include "../Common/SpaceWar_proxy.cpp"
#include "../Common/SpaceWar_common.cpp"

// 메테오까지 남은시간
int s_meteorCommingSec = 11;


class Server : public SpaceWar::Stub
{
public:
	Server() { m_gameRoom = make_shared <GameRoom>(); }
	~Server() {}

public:
	// P2P 그룹 이름 --------------------------
	HostID m_playerP2PGroup = HostID_None; 
public:
	// 기본 서버 로직 -------------------------
	void ServerRun();

	DECRMI_SpaceWar_RequestServerConnect;
	DECRMI_SpaceWar_RequestClientJoin;
	DECRMI_SpaceWar_RequestWorldCreateItem;
	DECRMI_SpaceWar_NotifyDeleteItem;
	DECRMI_SpaceWar_NotifyPlayerMove;

	//아이템 장비
	DECRMI_SpaceWar_NotifyPlayerEquipItem;

	// 데미지 요청
	DECRMI_SpaceWar_RequestPlayerDamage;

	// 숨을 쉬었다.
	DECRMI_SpaceWar_RequestPlayerUseOxy;

	// 산소 충전기 사용
	DECRMI_SpaceWar_RequestUseOxyCharger;

	// 아이템 박스 사용
	DECRMI_SpaceWar_RequestUseItemBox;

	// 쉘터 등록
	DECRMI_SpaceWar_RequestShelterStartSetup;
	// 쉘터 문 조작
	DECRMI_SpaceWar_RequestShelterDoorControl;

	// 쉘터 입장 퇴장
	DECRMI_SpaceWar_RequestShelterEnter;


	// 우주선 탔음 
	DECRMI_SpaceWar_RequestSpaceShip;
	//게임 끝
	DECRMI_SpaceWar_RequestGameEnd;

	// 서버 로비 로직
	DECRMI_SpaceWar_RequestLobbyConnect;
	// 팀 선택 요청
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect;
	// 레디
	DECRMI_SpaceWar_RequestNetworkGameReady;
	// 맵 바꿈 요청
	DECRMI_SpaceWar_RequestNetworkChangeMap;
	// 방장이 인원 수를 조작
	DECRMI_SpaceWar_RequestNetworkPlayerCount;
	// 방장이 게임 모드를 바꿈
	DECRMI_SpaceWar_RequestNetworkGameModeChange;
	// 방장이 게임 시작을 누름
	DECRMI_SpaceWar_RequestNetworkGameStart;
	// 방장이 로비를 나갔다.
	DECRMI_SpaceWar_RequestNetworkHostOut;

	// 게임 씬에 들어왔다는 것을 알려야지
	DECRMI_SpaceWar_RequestGameSceneJoin;

	// 서버 이벤트 로직
	void OnClientJoin(CNetClientInfo* clientInfo);
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment);

	// 메테오 루프
//	void MeteorLoop(void*);

private:
	// 아이템 박스 등 상호작용 오브젝트의 경우 동시접근을 막아야 하므로
	// 현재 접근하고 있는지 체크하는 로직이 필요함, 이에 따른 체크용 변수들
	unordered_map<int, int> m_itemBoxMap;
	unordered_map<int, int> m_oxyChargerMap;

	// 쉘터의 경우엔 등록해야함
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap;

	// 플레이 타임
	int m_gameStartTime;
public:
	// 전송 프록시
	SpaceWar::Proxy m_proxy;
	// 서버 객체
	shared_ptr<CNetServer> m_netServer;
	// 스레드 lock 을 위함 
	CriticalSection m_critSec;

	// 클라이언트 리스트
	unordered_map<HostID, shared_ptr<Client>> m_clientMap;
	
	// 아이템 리스트
	unordered_map<int, shared_ptr<Item>> m_itemMap;

	// 게임 룸
	shared_ptr<GameRoom> m_gameRoom;

	// 게임 유저 리셋
	void ResetUsers();
};

// 랜덤함수
float RandomRange(float min, float max)
{
	return ((float(rand()) / float(RAND_MAX)) * (max - min)) + min;
}
#endif