#ifndef SERVER_H
#define SERVER_H

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

// 바뀌는 메테오 로직
int s_meteorCommingTime[10] = { 30 ,50,70,90,110,130,150,170,190,210 };

/**
* @brief		Server :: 게임 서버 클래스
* @details		프라우드넷을 통한 모든 게임 서버 로직이 구현되어 있는 클래스
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Server.h
* @version		0.0.1
*/
class Server : public SpaceWar::Stub
{
public:
	Server();

#pragma region C2S  --------------------------------------------------------------------------------------------------------------------
#pragma region 초기 접속 ========================
#pragma region 공통 구간 ------------------------
	DECRMI_SpaceWar_RequestServerConnect; ///< Connect 초기 접속 요청
	DECRMI_SpaceWar_RequestNetworkGameTeamSelect; ///< 팀 선택 요청
	DECRMI_SpaceWar_RequestGameExit; ///< 정상적인 종료 루틴 // 게임 끝났음을 알림 ( 호스트가 보낸다 ))
#pragma endregion 

#pragma region 호스트 아닌 유저 -----------------
	DECRMI_SpaceWar_RequestNetworkGameReady; ///< 레디 정보가 날아온다.
	DECRMI_SpaceWar_RequestLobbyConnect; ///< 서버 로비 로직 // 로비 화면에 들어왔다.
	DECRMI_SpaceWar_RequestGameSceneJoin; ///< 게임이 시작되고 게임 씬으로 넘어왔을 때 다들 요청한다.
#pragma endregion -------------------------------

#pragma region 방장 호스트 전용 -----------------	
	DECRMI_SpaceWar_RequestNetworkChangeMap; ///< 방장이 맵을 바꿈
	DECRMI_SpaceWar_RequestNetworkPlayerCount;///< 방장이 플레이어 수를 바꿈
	DECRMI_SpaceWar_RequestNetworkGameModeChange; ///< 방장이 게임 모드를 바꿈
	DECRMI_SpaceWar_RequestNetworkGameStart; ///< 방장이 게임 시작을 누름
	DECRMI_SpaceWar_RequestNetworkHostOut; ///< 방장이 로비를 나갔다.
#pragma endregion -------------------------------
#pragma endregion 

#pragma region 인게임 ===========================

#pragma region 인게임 :: 플레이어 메시지 --------
#pragma region [플레이어 상태 관련 정보]
	DECRMI_SpaceWar_RequestHpUpdate; ///< 체력 회복 메시지
	DECRMI_SpaceWar_RequestPlayerDamage; ///< 플레이어가 맞았다.
	DECRMI_SpaceWar_RequestPlayerUseOxy; ///< 플레이어가 숨을 쉬었다.
	DECRMI_SpaceWar_NotifyPlayerMove; ///< 플레이어 움직임 처리 
	DECRMI_SpaceWar_NotifyPlayerEquipItem; ///< 아이템 장비 // 추후 지울 수 있음
#pragma endregion
#pragma endregion

#pragma region 인게임 :: 산소 충전기 ------------
	DECRMI_SpaceWar_RequestOxyChargerStartSetup; ///< 처음에 산소 충전기를 등록한다
	DECRMI_SpaceWar_RequestUseOxyChargerStart; ///< 산소 충전기 조작 요청 
	DECRMI_SpaceWar_RequestUseOxyCharger; ///< 산소 충전기 조작중
	DECRMI_SpaceWar_RequestUseOxyChargerEnd;///< 산소 충전기 조작 끝
#pragma endregion

#pragma region 인게임 :: 아이템 박스 조작 -------
	// 아이템 박스 조작
	DECRMI_SpaceWar_RequestUseItemBox;
#pragma endregion

#pragma region 인게임 :: 쉘터 조작 --------------
	DECRMI_SpaceWar_RequestShelterStartSetup; ///< 쉘터 등록
	DECRMI_SpaceWar_RequestShelterDoorControl; ///< 쉘터 문 조작
	DECRMI_SpaceWar_RequestShelterEnter; ///< 쉘터 입장 퇴장
#pragma endregion

#pragma region 인게임 :: 아이템 로직 ------------
	DECRMI_SpaceWar_RequestWorldCreateItem; ///< 월드에 아이템 생성 요청
	DECRMI_SpaceWar_RequestItemDelete; ///< 아이템 삭제 요청
	DECRMI_SpaceWar_NotifyDeleteItem; ///< 월드 아이템 삭제 알림
#pragma endregion

#pragma region 인게임 :: 우주선 -----------------
	DECRMI_SpaceWar_RequestSpaceShipSetup; ///< 우주선 몇개 있는지 세팅
	DECRMI_SpaceWar_RequestSpaceShip; ///< 우주선 탔음 
	DECRMI_SpaceWar_RequestUseSpaceShip; ///< 우주선 사용 요청
	DECRMI_SpaceWar_RequestUseSpaceShipCancel; ///< 우주선 사용 취소
#pragma endregion

#pragma region 인게임 :: 데스존 -----------------
	DECRMI_SpaceWar_RequestDeathZoneMoveIndex; ///< 데스존이 이동하고 있는 단계를 통보해줘라
#pragma endregion
#pragma endregion

#pragma region 결과창 ==========================
	DECRMI_SpaceWar_RequestDrawGameResult; ///< 드로우 게임시 정보 요청
	DECRMI_SpaceWar_RequestGameEnd; ///< 결과를 받아오는 것
#pragma endregion
#pragma endregion

private:
	HostID m_playerP2PGroup = HostID_None; ///< P2P 그룹의 HostID
	string m_serverPropertiesData; ///< server.properties json 데이터

	int m_gameStartTime; ///< 플레이 타임 계산용 변수
	int m_itemBoxCreateItemIndex; ///< 아이템박스 인덱스

	SpaceWar::Proxy m_proxy; ///< 전송 프록시
	shared_ptr<CNetServer> m_netServer; ///< 서버 객체
	CriticalSection m_critSec; ///< 스레드 lock 을 위함 

	unordered_map<int, int> m_itemBoxMap; ///< 아이템 박스 맵
	unordered_map<int, shared_ptr<Shelter>> m_shelterMap; ///< 쉘터 맵
	unordered_map<int, shared_ptr<OxyCharger>> m_oxyChargerMap; ///< 산소 충전기 맵
	unordered_map<int, shared_ptr<SpaceShip>> m_spaceShipMap; ///< 우주선 맵

	unordered_map<string, shared_ptr<Item>> m_itemMap; ///< 행성에 뿌려져있는 아이템
	shared_ptr<GameRoom> m_gameRoom; ///< 게임룸

#pragma region SpaceShip Lock / Meteor / DeathZone ====================================================================================
	int m_spaceShipLockTime = 0; ///< 우주선 초기 잠금 시간
	int m_deathZoneCommingSec = 0; ///< 데스존 까지 남은 시간
	int m_deathZoneIndex = 0; ///< 데스존이 진행하고 있는 인덱스
	int m_deathZoneHostID = -1; ///< 데스존을 움직이는 주체
	int m_deathZoneID = 0; ///< Death Zone 의 네트워크 아이디
	int m_meteorID = 0; ///< Meteor의 네트워크 아이디
#pragma endregion =====================================================================================================================

public:
#pragma region Server Class Logic -----------------------------------------------------------------------------------------------------
	void ServerFileLoad(); ///< 서버의 정보파일 로드
	bool ServerTableSetup(); ///< 서버 기본 정보 테이블 세팅
	void ServerRun(); ///< 게임 서버 시작
	void OnClientJoin(CNetClientInfo* clientInfo); ///< 클라이언트가 접속 요청
	void OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment); ///< 클라이언트가 서버에서 나갔다.
	void ServerReset(); ///< 서버 리셋 정보를 초기화 한다. 
#pragma region Get / Set Method --------------------------------------------------------------------------------------------------------
	shared_ptr<CNetServer> GetServer(); ///< 서버 객체 얻기
	SpaceWar::Proxy* GetProxy(); ///< 전송 프록시 얻기
	HostID GetP2PID(); ///< P2P 아이디 얻기
	shared_ptr<GameRoom> GetGameRoom(); ///< 게임 룸 객체 얻기
	void SetSpaceShipLockTime(int lockTime); ///< 초기 우주선 잠금값 세팅
	int GetSpaceShipLockTime(); ///< 초기 우주선 잠금값 얻기
	void SetDeathZoneCommingTime(int sec); ///< 데스존까지 남은 시간 세팅
	int GetDeathZoneCommingTime(); ///< 데스존까지 남은 시간 얻기
	void SetDeathZoneIndex(int curIndex); ///< 데스존이 진행하고 있는 인덱스 세팅
	int GetDeathZoneIndex(); ///< 데스존이 진행하고 있는 인덱스 얻기
	void SetDeathZoneHostID(int hostID); ///< 데스존을 현재 움직이고 있는 클라이언트 세팅
	int GetDeathZoneHostID(); ///< 데스존을 현재 움직이고 있는 클라이언트 얻기
	void SetDeathZoneID(int id); ///< 데스존의 네트워크 아이디 세팅
	int GetDeathZoneID(); ///< 데스존 네트워크 아이디 얻기 
	void SetMeteorID(int id); ///< 메테오의 네트워크 아이디 세팅
	int GetMeteorID(); ///< 메테오의 네트워크 아이디 얻기	
#pragma endregion ----------------------------------------------------------------------------------------------------------------------
#pragma endregion ----------------------------------------------------------------------------------------------------------------------


	
	
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

int RandomRange(int min, int max)
{
	return ((int(rand()) / int(RAND_MAX)) * (max - min)) + min;
}
#endif