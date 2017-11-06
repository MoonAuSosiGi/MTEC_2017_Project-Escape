#include "stdafx.h"
#include "GameRoom.h"

/**
 * @brief	기본 생성자 (초기화)
*/
GameRoom::GameRoom()
{
	m_limitPlayerCount = 10;
	m_gameMode = DEATH_MATCH;
	m_teamMode = false;
}

/**
 * @brief	클라이언트 맵 얻기
 * @return	클라이언트 맵을 리턴
*/
unordered_map<HostID, shared_ptr<RoomClient>> GameRoom::GetClientMap()
{
	return m_clientMap;
}

#pragma region Room Setting Method =======================================================================================
/** 
 * @brief	플레이어의 수 제한을 두는 메서드
 * @param	count 제한을 둘 수
*/
void GameRoom::SetPlayerLimitCount(int count)
{
	m_limitPlayerCount = count;
}

/**
 * @brief	현재 플레이어 제한 수 가져오기
 * @return	게임 가능한 플레이어의 수를 리턴한다.
*/
int GameRoom::GetPlayerLimitCount()
{
	return m_limitPlayerCount;
}

/**
 * @brief	게임 모드 세팅
 * @param	gameMode 변경할 게임 모드
*/
void GameRoom::SetGameMode(int gameMode)
{
	m_gameMode = gameMode;
}

/**
 * @brief	팀 모드 세팅
 * @param	teamMode  true 팀 모드, false 개인전 모드
*/
void GameRoom::SetTeamMode(bool teamMode)
{
	m_teamMode = teamMode;
}

/**
 * @brief	팀 모드 얻기
 * @return	팀 모드인지를 알아올 수 있다.
*/
bool GameRoom::GetTeamMode() 
{ 
	return m_teamMode; 
}

/**
 * @brief	게임 모드 얻기
 * @return	게임 모드인지를 얻어올 수 있다.
*/
int GameRoom::GetGameMode() 
{ 
	return m_gameMode; 
}

/**
 * @brief	게임 룸 클리어
 * @detail	게임이 종료되거나 / 유저가 다 나갔거나 할 경우 세팅을 초기화한다.
*/
void GameRoom::ClearRoom()
{
	m_clientMap.clear();
	m_limitPlayerCount = 10;
	m_gameMode = SURVIVAL;
	m_teamMode = false;
}

#pragma endregion =========================================================================================================

#pragma region Client Method ==============================================================================================
/**
 * @brief	새로운 클라이언트가 접속했다.
 * @detail	새로운 클라이언트가 들어올 때 맵에 추가하는 로직
 * @param	hostID 접속한 클라이언트의 HostID
 * @param	userName 접속한 클라이언트의 유저 이름
 * @param	critSection 크리티컬 섹션 / 추가 로직시 잠금하는 로직
 * @return	정상적으로 접속이 수행되었는지에 대한 유무를 리턴. true 면 정상 추가, false 면 접속 불가
*/
bool GameRoom::NewClientConnect(HostID hostID, string userName, CriticalSection& critSection)
{
	// 현재 접속인원이 접속 제한 수를 초과했는지
	if (m_clientMap.size() >= m_limitPlayerCount)
		return false;

	{
		// 클라이언트를 map 에 추가하기 위해 크리티컬 섹션을 잠금
		CriticalSectionLock lock(critSection, true);
		// 클라이언트 생성 후 맵에 추가
		auto newClient = make_shared<RoomClient>(hostID, userName);
		m_clientMap[hostID] = newClient;
		// 현재 플레이어 수를 증가시킴
		m_currentPlayerCount++;
	}
	return true;
}

/**
 * @brief	클라이언트가 접속을 끊었다.
 * @detail	클라이언트가 접속을 끊어 해당 클라이언트 객체를 제거하는 로직
 * @param	hostID 접속을 끊은 클라이언트의 HostID
*/
void GameRoom::LogOutClient(HostID hostID)
{
	// 나간 클라이언트가 클라이언트 맵에 있는지 체크
	auto iter = m_clientMap.find(hostID);

	if (iter != m_clientMap.end())
	{
		// 해당 클라이언트가 있다면 정상적으로 삭제
		m_clientMap.erase(iter);
	}
	
	// 다 나갔을 경우 클리어
	if (m_clientMap.size() <= 0)
	{
		cout << "Server :: 접속한 클라이언트의 수가 0이 되었습니다. " << endl;
		this->ClearRoom();
	}
}

/**
 * @brief	클라이언트가 방에 접속해있는지
 * @param	hostID 검사할 클라이언트의 HostID
 * @return	방 접속 유무 리턴
*/
bool GameRoom::IsRoomClient(HostID hostID)
{
	auto iter = m_clientMap.find(hostID);

	return  iter != m_clientMap.end();
}

/**
 * @brief	클라이언트의 레디 정보 세팅
 * @param	hostID 레디 정보를 세팅할 HostID
 * @param	ready 세팅될 레디 정보
*/
void GameRoom::SetReady(HostID hostID, bool ready)
{
	m_clientMap[hostID]->SetReady(ready);
}

/**
 * @brief	팀 변경 정보 
 * @param	hostID 팀 변경을 할 클라이언트의 HostID
 * @param	red 팀이 적/청 밖에 없으므로 적색인지?
*/
void GameRoom::TeamChange(HostID hostID, bool red)
{
	m_clientMap[hostID]->SetTeamColor(red);
}
#pragma endregion =========================================================================================================

#pragma region Game Method ================================================================================================
/**
 * @brief	게임 스타트 가능 유무
 * @detail	게임 스타트 가능 유무 : 레디가 다 눌려있는지 / 팀전일 경우 밸런스가 맞는지
 * @return	true 리턴시 게임 시작 가능 / false 리턴시 게임 시작 불가
*/
bool GameRoom::GameStartCheck()
{
	// 게임 시작 유무 리턴용
	bool gameStart = true;

	// 적팀 청팀 카운트용
	int redTeam,  blueTeam;
	redTeam = blueTeam = 0;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		// 호스트는 방장이므로, 호스트가 아닐 때만 레디 체크
		if (!iter->second->IsHost())
		{
			gameStart = iter->second->IsReady();

			// 한명이라도 레디를 하지 않았으면 더 체크할 필요가 없음
			if (gameStart == false)
				return false;
		}

		// 팀별로 카운트하기
		if (iter->second->IsRedTeam())
			redTeam++;
		else
			blueTeam++;


		iter++;
	}


	// 팀 모드일 경우 밸런스 맞는지 체크
	if (m_teamMode)
		gameStart = (redTeam == blueTeam);

	return gameStart;
}

/**
 * @brief	모두 게임씬에 왔는지 체크
 * @detail	모두 게임씬에 왔는지 체크 후 , 먼저 접속한 사람 / 아이템에 대한 정보 등을 전송한다.
*/
bool GameRoom::IsGameSceneAllReady()
{
	auto iter = m_clientMap.begin();

	while (iter != m_clientMap.end())
	{
		bool ready = iter->second->IsGameScene();

		// 하나라도 불만족시 준비가 안되어있음
		if (!ready)
			return false;

		iter++;
	}

	return true;
}

/**
 * @brief	현재 게임중인지를 얻어온다.
 * @return	현재 게임중일 경우 true 를 리턴
*/
bool GameRoom::IsGameRunning()
{
	return m_isGameRunning;
}

/**
 * @brief	현재 게임 중인지를 세팅한다.
 * @param	running 게임 중인지에 대한 정보
*/
void GameRoom::SetGameRunning(bool running)
{
	m_isGameRunning = running;
}

/**
 * @brief	특정 클라 제외한 HostID 리스트 얻기
 * @param	hostID 제외할 클라이언트의 HostID
 * @return	해당 클라이언트를 제외한 HostID 리스트를 리턴
*/
forward_list<HostID> GameRoom::GetOtherClients(HostID hostID)
{
	forward_list<HostID> list;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if (iter->second->GetHostID() != hostID)
			list.push_front(iter->second->GetHostID());
		iter++;
	}

	return list;
}

/**
 * @brief	특정 클라 제외한 리스트 얻기
 * @detail	GetOtherClients(HostID) 와는 다르게 RoomClient 전체 정보를 얻어올 수 있다.
 * @param	hostID 제외할 클라이언트의 HostID
 * @return	해당 클라이언트를 제외한 리스트를 리턴
*/
forward_list<RoomClient> GameRoom::GetOtherClientInfos(HostID hostID)
{
	forward_list<RoomClient> list;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if (iter->second->GetHostID() != hostID)
			list.push_front(*iter->second);
		iter++;
	}

	return list;
}

/**
 * @brief	접속해있는 모든 클라이언트의 HostID 얻기
 * @detail	모든 클라이언트의 HostID 만 필요할 때 사용
 * @return	모든 클라이언트의 HostID 리스트
*/
forward_list<HostID> GameRoom::GetAllClient()
{
	forward_list<HostID> list;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		list.push_front(iter->second->GetHostID());
		iter++;
	}
	return list;
}

/**
 * @brief	특정 클라이언트의 정보를 얻어온다.
 * @detail	특정 클라이언트 하나만 RoomClient 정보를 얻는다.
 * @param	hostID 정보를 얻어올 클라이언트의 HostID
 * @return	특정 클라이언트의 정보를 RoomClient 형태로 받아온다. nullptr 일 수 있다.
*/
shared_ptr<RoomClient> GameRoom::GetClient(HostID hostID)
{ 
	// 맵에 정상 등록된 hostID 인지 체크 후 리턴
	if (IsRoomClient(hostID))
		return m_clientMap[hostID];
	else
		return nullptr;
}

#pragma endregion =========================================================================================================







