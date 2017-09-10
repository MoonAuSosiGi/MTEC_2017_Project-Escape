// Server.cpp : 콘솔 응용 프로그램에 대한 진입점을 정의합니다.
//

#include "stdafx.h"
#include "Server.h"

Server server;
bool s_GameRunning = false;

int main()
{
	srand((unsigned int)time(NULL));
	

	server.ServerRun();
    return 0;
}
#pragma region Meteor

void MeteorLoop(void*)
{
	if (s_GameRunning == false)
		return;
	s_meteorCommingSec--;

	if(s_deathZoneCommingSec > 0)
		s_deathZoneCommingSec--;


	//if(s_meteorCommingSec % 5 == 0 && s_meteorCommingSec >= 0)
	//	cout << "메테오 " << s_meteorCommingSec << " 초 남음 " << endl;
	//if (s_deathZoneCommingSec % 5 == 0)
	//	cout << "<Server> Death Zone 생성까지 " << s_deathZoneCommingSec << " 초 남았습니다." << endl;
	
	server.m_proxy.NotifyMeteorCreateTime(server.m_playerP2PGroup, RmiContext::UnreliableSend, s_meteorCommingSec);

	{
		if (s_meteorCommingSec == 0)
		{
			float anglex = RandomRange(-360.0f, 360.0f);
			float anglez = RandomRange(-360.0f, 360.0f);

			cout << "anglex " << anglex << " anglez " << anglez;
			server.m_proxy.NotifyMeteorCreate(server.m_playerP2PGroup, RmiContext::ReliableSend, anglex,anglez);

		}

		else if (s_meteorCommingSec < 0)
		{
			//if (s_meteorCommingSec == -120)
			{
				s_meteorCommingSec = 90;
			}
		}
	}

	// 여기는 데스존 체크
	if (s_deathZoneCommingSec == 0)
	{
		int index = (int)RandomRange(0, server.GetSpaceShipCount());
		
		// 생성
		server.m_proxy.NotifyDeathZoneCreate(server.m_playerP2PGroup, RmiContext::ReliableSend, index);

		// 움직이는 주체 설정
		auto iter = server.m_clientMap.begin();
		while (iter != server.m_clientMap.end())
		{
			if (iter->second->m_state == ALIVE)
			{
				// 움직이는 주체 통보
				server.m_proxy.NotifyDeathZoneMoveHostAndIndexSetup(
					server.m_playerP2PGroup,
					RmiContext::ReliableSend,
					(int)iter->second->m_hostID, 
					s_deathZoneIndex);
				s_deathZoneHostID = (int)iter->second->m_hostID;
				break;
			}
			iter++;
		}
		s_deathZoneCommingSec--;
	}
	// 데스존이 움직이는 루프 
	else if (s_deathZoneCommingSec == -1)
	{
		if(server.m_clientMap.size() <= 0)
			return;
		if (server.m_clientMap.find((HostID)s_deathZoneHostID) == server.m_clientMap.end())
			return;
		if (server.m_clientMap[(HostID)s_deathZoneHostID]->m_state != ALIVE)
		{
			// 움직이는 주체 설정
			auto iter = server.m_clientMap.begin();
			while (iter != server.m_clientMap.end())
			{
				if (iter->second->m_state == ALIVE)
				{
					// 움직이는 주체 통보
					server.m_proxy.NotifyDeathZoneMoveHostAndIndexSetup(
						server.m_playerP2PGroup,
						RmiContext::ReliableSend,
						(int)iter->second->m_hostID,
						s_deathZoneIndex);
					s_deathZoneHostID = (int)iter->second->m_hostID;
					break;
				}
				iter++;
			}
		}
	}

}
#pragma endregion



void Server::ServerRun()
{
	m_netServer = shared_ptr<CNetServer>(CNetServer::Create());

//	typedef void(*ThreadProc)(void* ctx);
	void(*func)(void*);
	func = &MeteorLoop;
	CTimerThread meteorThread(func, 1000, nullptr);
	//
	meteorThread.Start();

	// -- 서버 파라매터 지정 ( 프로토콜 / 포트 지정 ) ---------------------------//
	CStartServerParameter pl;
	pl.m_allowServerAsP2PGroupMember = true;
	pl.m_protocolVersion = g_protocolVersion;
	pl.m_tcpPorts.Add(g_serverPort);

	// -- Stub , Proxy Attach 이제 보내고 받을 수 있다 --------------------------//
	m_netServer->AttachStub(this);
	m_netServer->AttachProxy(&m_proxy);

	// -- 서버 람다식 -----------------------------------------------------------//

	//-- 서버 시작 --------------------------------------------------------------//
	try
	{
		m_netServer->Start(pl);
		m_playerP2PGroup = m_netServer->CreateP2PGroup();
		m_netServer->JoinP2PGroup(HostID_Server, m_playerP2PGroup);

	}
	catch (Exception &e)
	{
		cout << "Server Start Failed : " << e.what() << endl;
		return;
	}

	
	// 클라이언트가 들어왔다 
	m_netServer->OnClientJoin = [this](CNetClientInfo *clientInfo) { OnClientJoin(clientInfo); };

	// 클라이언트가 나갔다
	m_netServer->OnClientLeave = [this](CNetClientInfo *clientInfo, ErrorInfo* errinfo, const ByteArray& comment) {
		OnClientLeave(clientInfo, errinfo, comment);
	};

	// 서버 에러
	m_netServer->OnError = [](ErrorInfo *errorInfo) {
		cout << "type : " << errorInfo->m_errorType << endl;
		printf("OnError : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};

	// 서버 경고
	m_netServer->OnWarning = [](ErrorInfo *errorInfo) {
		printf("OnWarning : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};

	//
	m_netServer->OnInformation = [](ErrorInfo *errorInfo) {
		cout << "type : " << errorInfo->m_errorType << endl;
		printf("OnInformation : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};
	m_netServer->OnException = [](const Exception &e) {
		printf("OnInformation : %s\n", e.what());
	};

	m_netServer->OnP2PGroupJoinMemberAckComplete = [](HostID groupHostID, HostID memberHostID, ErrorType result)
	{
		if (result == ErrorType_Ok)
		{
			// 성공적으로 추가되었음
			cout << "Server : P2P Member JoinAckComplete " << memberHostID << endl;
		}
		else
		{
			cout << "Server : P2P Member JoinAckComplete failed " << endl;
		}
	};

	string line;
	getline(std::cin, line);
	meteorThread.Stop();
}

void Server::ResetUsers()
{
	auto iter = m_clientMap.begin();

	while (iter != m_clientMap.end())
	{
		iter->second->Reset();
		iter++;
	}

	m_itemMap.clear();
}

void Server::OnClientJoin(CNetClientInfo* clientInfo)
{
	cout << "OnClientJoin " << clientInfo->m_HostID << endl;
	
	m_netServer->JoinP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);
}

void Server::OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment)
{
	cout << "OnClientLeave " << clientInfo->m_HostID << endl;

	auto iter = m_clientMap.find((clientInfo->m_HostID));
	m_netServer->LeaveP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);

	if (iter == m_clientMap.end())
	{
		cout << "로그인하지 않은 클라이언트가 나갔습니다. " << clientInfo->m_HostID << endl;

		return;
	}

	m_proxy.NotifyPlayerLost(m_playerP2PGroup,RmiContext::ReliableSend,clientInfo->m_HostID);
	m_gameRoom->LogOutClient(iter->second->m_hostID);
	
	m_clientMap.erase(iter);

}

#pragma region C2S Method
DEFRMI_SpaceWar_RequestServerConnect(Server)
{
	cout << "C2S-- RequestServerConnect " << id << " remote " << remote << endl;

	// 중복 접속인지 체크
	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if (iter->second->m_userName == id)
		{
			// 중복
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend,
				"중복 접속입니다.");

		}
		iter++;
	}

	if (m_gameRoom != nullptr)
	{
		if (m_gameRoom->NewClientConnect(remote, id, m_critSec) == false)
		{
			cout << "Login Failed .. 더 이상 참여할 수 없습니다. " << endl;
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend, "더 이상 참여할 수 없습니다.");
			return true;
		}
	}

	{
		// 추가로직
		CriticalSectionLock lock(m_critSec, true);
		auto newClient = make_shared<Client>();
		newClient->m_hostID = remote;
		newClient->m_userName = id;
		m_clientMap[remote] = newClient;
	}
	int count = m_clientMap.size();
	cout << "Server : LoginSuccess "<< count << endl;
	

	m_proxy.NotifyLoginSuccess(remote, RmiContext::ReliableSend,(int)remote,(count == 1));
	
	// 로비에서 처리하는걸로
	/*if (m_gameRoom != nullptr)
	{
		forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);
		auto iter = list.begin();

		while (iter != list.end())
		{
			m_proxy.NotifyNetworkConnectUser(*iter, RmiContext::ReliableSend, (int)remote, id);
			iter++;
		}
	}*/
	return true;
}

// TODO 서버 구조 변경으로 삭제 대상
DEFRMI_SpaceWar_RequestClientJoin(Server)
{
	cout << "Server : RequestClientJoin "<<name << endl;
	
	auto iter = m_clientMap.find((HostID)hostID);
	if (iter == m_clientMap.end())
	{
		cout << "잘못된 클라이언트 " << endl;
		return true;
	}
	// 모두에게 나타나게 해야함 
	for each (auto c in m_clientMap)
	{
		if (c.first != iter->first)
		{
			auto otherClient = c.second;
			m_proxy.NotifyOtherClientJoin(c.first, 
				RmiContext::ReliableSend,
				iter->second->m_hostID,
				iter->second->m_userName,
				iter->second->x,
				iter->second->y,
				iter->second->z);

			m_proxy.NotifyOtherClientJoin(iter->first,
				RmiContext::ReliableSend,
				c.second->m_hostID,
				c.second->m_userName,
				c.second->x,
				c.second->y,
				c.second->z);
		}
	}


	return true;
}

DEFRMI_SpaceWar_RequestWorldCreateItem(Server)
{
	cout << "RequestWorldCreateItem  itemID : " << itemID  << " x : "<< pos.x << " y : " << pos.y << " z : "<<pos.z  << endl;
	auto iter = m_clientMap.find((HostID)hostID);

	{
		// 추가로직
		CriticalSectionLock lock(m_critSec, true);
		auto newItem = make_shared<Item>();
		newItem->m_itemID = itemID;
		newItem->m_networkID = networkID;
		newItem->pos.x = pos.x;
		newItem->pos.y = pos.y;
		newItem->pos.z = pos.z;
		newItem->rot.x = rot.x;
		newItem->rot.y = rot.y;
		newItem->rot.z = rot.z;
		m_itemMap[networkID] = newItem;
	}

	if (iter == m_clientMap.end())
	{
		cout << "잘못된 요청" << endl;
	}

	// 보낸 사람 외에 생성 요청 보내기
	for each(auto c in m_clientMap)
	{
		if (c.first != iter->first)
		{
			m_proxy.NotifyCreateItem(c.first, RmiContext::ReliableSend, 
				(int)(iter->first),itemID,networkID, pos, rot);
		}
	}
	return true;
}

DEFRMI_SpaceWar_NotifyDeleteItem(Server)
{
	cout << "NotifyDeleteItem " << networkID << endl;

	auto iter = m_itemMap.find(networkID);
	
	if (iter == m_itemMap.end())
	{
		cout << "잘못된 요청" << endl;
	}
	{
		// 추가로직
		CriticalSectionLock lock(m_critSec, true);
		m_itemMap.erase(iter);
	}
	return true;
}

DEFRMI_SpaceWar_NotifyPlayerMove(Server)
{
	m_clientMap[(HostID)hostID]->x = curX;
	m_clientMap[(HostID)hostID]->y = curY;
	m_clientMap[(HostID)hostID]->z = curZ;

	m_gameRoom->GetClient((HostID)hostID)->SetPosition(curX, curY, curZ);
	return true;
}

DEFRMI_SpaceWar_NotifyPlayerEquipItem(Server)
{
	cout << "NotifyPlayerEquipItem " << hostID << " item " << itemID << endl;
	return true;
}

DEFRMI_SpaceWar_RequestPlayerDamage(Server)
{
	cout << "Request Player Damage 때린놈 " << sendHostID << " 맞은놈 " << targetHostID << " 데미지 " << damage << endl;
	cout << "현재 총인원 " << m_clientMap.size() << endl;

	if (m_clientMap[(HostID)targetHostID]->m_state == DEATH)
		return true;
	
	float prevHp = m_clientMap[(HostID)targetHostID]->hp;
	m_clientMap[(HostID)targetHostID]->hp -= damage;

	if (m_clientMap[(HostID)targetHostID]->hp <= 0.0f)
	{
		cout << " 죽었다 " + targetHostID << endl;
		m_clientMap[(HostID)targetHostID]->hp = 0.0f;
		m_clientMap[(HostID)targetHostID]->PlayerDead(m_netServer->GetTimeMs());
		
		// 죽은 후에 어시스트 목록 갱신
		forward_list<int> list = m_clientMap[(HostID)targetHostID]->GetAssistClientList();
		for (auto value : list)
		{
			//킬한놈
			if ((HostID)sendHostID == (HostID)value)
				continue;
			m_clientMap[(HostID)value]->m_assistCount++;
			m_proxy.NotifyKillInfo((HostID)value, RmiContext::ReliableSend, m_clientMap[(HostID)targetHostID]->m_userName, false, m_clientMap[(HostID)value]->m_killCount, m_clientMap[(HostID)value]->m_assistCount);
		}

		if(sendHostID != targetHostID)
			m_clientMap[(HostID)sendHostID]->m_killCount++;

		m_proxy.NotifyKillInfo((HostID)sendHostID, RmiContext::ReliableSend, m_clientMap[(HostID)targetHostID]->m_userName, true, m_clientMap[(HostID)sendHostID]->m_killCount, m_clientMap[(HostID)sendHostID]->m_assistCount);

		// 이 부분에서 다 죽었는지 체크
		int deadCheck = 0;
		auto iter = m_clientMap.begin();
		while (iter != m_clientMap.end())
		{
			deadCheck += (iter->second->m_state == DEATH) ? 0 : 1;
			iter++;
		}

		// 다죽음 - 즉 드로우 게임 
		if (deadCheck == 0)
		{
			iter = m_clientMap.begin();
			while (iter != m_clientMap.end())
			{
				// 드로우 게임임을 날린다
				m_proxy.NotifyDrawGame(iter->second->m_hostID, RmiContext::ReliableSend);
				iter++;
			}
		}
	}
	else
	{
		// 이녀석은 어시스트 체크를 해야함
		if(targetHostID != sendHostID)
			m_clientMap[(HostID)targetHostID]->DamageClient((int)sendHostID,m_netServer->GetTimeMs());
	}


	m_proxy.NotifyPlayerChangeHP(m_playerP2PGroup, RmiContext::ReliableSend, targetHostID, weaponName, m_clientMap[(HostID)targetHostID]->hp, prevHp, MAX_HP,dir);
	return true;
}

DEFRMI_SpaceWar_RequestHpUpdate(Server)
{
	cout << "HP Update 요청 " << endl;

	auto iter = m_clientMap.begin();

	float prevHp = 0.0f;
	while (iter != m_clientMap.end())
	{
		if (iter->second->m_hostID == remote)
		{
			prevHp = iter->second->hp;
			iter->second->hp = hp;
			break;
		}
		iter++;
	}

	iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		m_proxy.NotifyPlayerChangeHP(iter->second->m_hostID, RmiContext::ReliableSend, (int)remote, "HpUpdate", hp, prevHp, MAX_HP, Vector3(0.0, 0.0, 0.0));
		iter++;
	}
	return true;
}

DEFRMI_SpaceWar_RequestPlayerUseOxy(Server)
{
	//cout << "RequestPlayerUseOxy " << useOxy << endl;
	float prevOxy = m_clientMap[(HostID)sendHostID]->oxy;

	if (m_clientMap[(HostID)sendHostID]->oxy - useOxy < 0)
	{
		m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID, m_clientMap[(HostID)sendHostID]->m_userName,0.0f, prevOxy, MAX_OXY);
		return true;
	}
	
	
	m_clientMap[(HostID)sendHostID]->oxy -= useOxy;

	m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID, m_clientMap[(HostID)sendHostID]->m_userName, m_clientMap[(HostID)sendHostID]->oxy, prevOxy, MAX_OXY);

	return true;
}

DEFRMI_SpaceWar_RequestUseOxyCharger(Server)
{
	cout << "RequestUseOxyCharger " << endl;

	CriticalSectionLock lock(m_critSec, true);
	
	float prevOxy = m_clientMap[(HostID)sendHostID]->oxy;
	m_clientMap[(HostID)sendHostID]->oxy += userOxy;

	m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID, m_clientMap[(HostID)sendHostID]->m_userName, m_clientMap[(HostID)sendHostID]->oxy, prevOxy, MAX_OXY);

	m_proxy.NotifyUseOxyCharger(m_playerP2PGroup, RmiContext::ReliableSend, sendHostID,
		oxyChargerIndex, userOxy);
	return true;
}

DEFRMI_SpaceWar_RequestUseItemBox(Server)
{
	cout << "RequestUseItemBox " << itemBoxIndex << endl;

	// TESTCODE
	if (m_itemBoxMap.find(itemBoxIndex) == m_itemBoxMap.end())
	{
		
		// 첫 사용
		m_itemBoxMap[itemBoxIndex] = sendHostID;

		string itemID = "temp"; // 여기서 줘야하는데 일단은 클라에서 주는 것으로..
		cout << "item Code " << itemID << endl;
		string networkID = "server_" + m_itemBoxCreateItemIndex;
		
		m_proxy.NotifyUseItemBox(m_playerP2PGroup, RmiContext::ReliableSend,
			sendHostID, itemBoxIndex, itemID, networkID);
		return true;
	}

	// 이미 사용중
	
	

	return true;
}

// 쉘터 등록
DEFRMI_SpaceWar_RequestShelterStartSetup(Server)
{
	cout << "RequestShelterStartSetup " << shelterID << endl;

	auto newshelter = make_shared<Shelter>();
	newshelter->m_shelterID = shelterID;
	m_shelterMap[shelterID] = newshelter;
	return true;
}

// 쉘터 문 조작 
DEFRMI_SpaceWar_RequestShelterDoorControl(Server)
{
	cout << "RequestShelterDoorControl " << shelterID << " state " << doorState << endl;

	m_shelterMap[shelterID]->ShelterDoorStateChange(doorState);


	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if((int)iter->second->m_hostID != sendHostID)
			m_proxy.NotifyShelterInfo(iter->second->m_hostID,
				RmiContext::ReliableSend, sendHostID, shelterID,
				doorState, m_shelterMap[shelterID]->m_lightState);

		iter++;
	}
	

	return true;
}

// 쉘터 입장
DEFRMI_SpaceWar_RequestShelterEnter(Server)
{
	cout << "RequestShelterEnter " << sendHostID << endl;

	// 기존 상태 
	bool prevState = m_shelterMap[shelterID]->m_lightState;
	bool prevDoorState = m_shelterMap[shelterID]->m_doorState;
	if (enter)
		m_shelterMap[shelterID]->ShelterEnter();
	else
		m_shelterMap[shelterID]->ShelterExit();

	// 기존 상태와 달라졌을 경우
	if (m_shelterMap[shelterID]->m_lightState != prevState || 
		m_shelterMap[shelterID]->m_doorState != prevDoorState)
	{
		// 상태 전송
		m_proxy.NotifyShelterInfo(m_playerP2PGroup,
			RmiContext::ReliableSend, sendHostID,
			shelterID, m_shelterMap[shelterID]->m_doorState, m_shelterMap[shelterID]->m_lightState);
	}


	return true;
}

#pragma endregion

#pragma region Game_RESULT

DEFRMI_SpaceWar_RequestSpaceShip(Server)
{
	cout << "이친구 우주선 탔다 " << winPlayerID << endl;
	m_clientMap[(HostID)winPlayerID]->PlayerWin();
	return true;
}

DEFRMI_SpaceWar_RequestDrawGameResult(Server)
{
	cout << "드로우 결과 요청 " << endl;
	auto iter = m_clientMap.begin();

	float playTime = m_netServer->GetTimeMs() - m_gameStartTime;

	while (iter != m_clientMap.end())
	{

		HostID targetID = iter->first;

		int winState = 2;

		m_proxy.NotifyGameResultInfoMe(targetID, RmiContext::ReliableSend, "test", winState,
			playTime, iter->second->m_killCount, iter->second->m_assistCount, iter->second->m_deathCount, 100);

		auto resultIter = m_clientMap.begin();
		while (resultIter != m_clientMap.end())
		{
			if (resultIter->first != iter->first)
			{
				cout << " 정보 보내기 " << endl;
				m_proxy.NotifyGameResultInfoOther(targetID, RmiContext::ReliableSend,
					resultIter->second->m_userName, resultIter->second->m_state);
			}

			resultIter++;
		}
		m_proxy.NotifyGameResultShow(targetID, RmiContext::ReliableSend);

		iter++;
	}

	return true;
}

DEFRMI_SpaceWar_RequestGameEnd(Server)
{
	cout << "RequestGameEnd -- 게임 종료  - " << endl;
	s_GameRunning = false;

	

	// 여기서 문제  생겼었음

	if (m_clientMap[remote]->m_state != SPACESHIP)
		return true;

	
	auto iter = m_clientMap.begin();

	float playTime = m_netServer->GetTimeMs() - m_gameStartTime;

	while (iter != m_clientMap.end())
	{

		HostID targetID = iter->first;

		int winState = (m_clientMap[targetID]->m_state == SPACESHIP) ? 1 : 0;
		
		m_proxy.NotifyGameResultInfoMe(targetID, RmiContext::ReliableSend, "test", winState,
			playTime, iter->second->m_killCount, iter->second->m_assistCount, iter->second->m_deathCount, 100);

		auto resultIter = m_clientMap.begin();
		while (resultIter != m_clientMap.end())
		{
		//	if (resultIter->first != iter->first)
			{
				cout << " 정보 보내기 " << endl;
				m_proxy.NotifyGameResultInfoOther(targetID, RmiContext::ReliableSend,
					resultIter->second->m_userName, resultIter->second->m_state);
			}
			
			resultIter++;
		}
		m_proxy.NotifyGameResultShow(targetID, RmiContext::ReliableSend);

		iter++;
	}
	

	
	return true;
}
#pragma endregion

#pragma region Network Lobby

// 로비 화면에 들어왔다.
DEFRMI_SpaceWar_RequestLobbyConnect(Server)
{
	// 이제 이친구한테 모든 정보를 날린다.
	cout << "Lobby Connect " << endl;
	forward_list<RoomClient> list = m_gameRoom->GetOtherClientInfos(remote);

	//먼저 게임모드를 날린다.
	m_proxy.NotifyNetworkGameModeChange(remote, RmiContext::ReliableSend, m_gameRoom->GetGameMode(), m_gameRoom->GetTeamMode());

	// 플레이 카운트도 날림
	m_proxy.NotifyNetworkGamePlayerCountChange(remote, RmiContext::ReliableSend, m_gameRoom->GetPlayerLimitCount());

	auto iter = list.begin();

	while (iter != list.end())
	{
		m_proxy.NotifyNetworkUserSetup(remote, RmiContext::ReliableSend, (int)iter->GetHostID(),iter->GetName(), iter->IsReady(), iter->IsRedTeam());

		if (remote != iter->GetHostID())
			m_proxy.NotifyNetworkConnectUser(iter->GetHostID(), RmiContext::ReliableSend, remote, m_gameRoom->GetClient(remote)->GetName());
		iter++;
	}
	return true;
}

// 팀 선택을 바꿈 
DEFRMI_SpaceWar_RequestNetworkGameTeamSelect(Server)
{
	cout << "Team 변경 " << endl;
	m_gameRoom->TeamChange(remote, teamRed);

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);
	
	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameTeamChange(*iter, RmiContext::ReliableSend,(int)remote, teamRed);
		iter++;
	}

	
	return true;
}

// 레디
DEFRMI_SpaceWar_RequestNetworkGameReady(Server)
{
	cout << " Ready 변경 " << endl;

	m_gameRoom->SetReady(remote, ready);

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkReady(*iter, RmiContext::ReliableSend,(int)remote,m_gameRoom->GetClient(remote)->GetName(), ready);
		iter++;
	}
	return true;
}

// 방장이 맵을 바꿈
DEFRMI_SpaceWar_RequestNetworkChangeMap(Server)
{
	cout << " Map 변경 " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameChangeMap(*iter, RmiContext::ReliableSend, mapName);
		iter++;
	}
	
	return true;
}

// 방장이 게임 모드를 바꿈 
DEFRMI_SpaceWar_RequestNetworkGameModeChange(Server)
{
	cout << " Game Mode 변경 " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	m_gameRoom->SetGameMode(gameMode);
	m_gameRoom->SetTeamMode(teamMode);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameModeChange(*iter, RmiContext::ReliableSend, gameMode, teamMode);
		iter++;
	}
	return true;
}

// 방장이 플레이어 수를 바꿈
DEFRMI_SpaceWar_RequestNetworkPlayerCount(Server)
{
	cout << " Player Count 변경 " << playerCount << endl;

	m_gameRoom->SetPlayerLimitCount(playerCount);
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGamePlayerCountChange(*iter, RmiContext::ReliableSend, playerCount);
		iter++;
	}
	return true;
}

// 방장이 게임 시작을 누름
DEFRMI_SpaceWar_RequestNetworkGameStart(Server)
{
	cout << " Game Start 변경 " << endl;
	m_itemMap.clear();
	//if (!m_gameRoom->GameStartCheck())
	//{
	//	//실패
	//	m_proxy.NotifyNetworkGameStartFailed(m_playerP2PGroup, RmiContext::ReliableSend);
	//	return true;
	//	
	//}
	m_gameStartTime = m_netServer->GetTimeMs();
	s_GameRunning = true;
	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		m_proxy.NotifyNetworkGameStart(iter->second->m_hostID, RmiContext::ReliableSend);
		iter++;
	}

	/*forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameStart(*iter, RmiContext::ReliableSend);
		iter++;
	}*/
	return true;
}

//방장이 나감
DEFRMI_SpaceWar_RequestNetworkHostOut(Server)
{
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameHostOut(*iter, RmiContext::ReliableSend);
		iter++;
	}

	m_gameRoom->ClearRoom();
	s_GameRunning = false;
	m_gameRoom->ClearRoom();
	m_clientMap.clear();
	m_itemMap.clear();
	m_itemBoxMap.clear();
	//m_oxyChargerMap.clear();
	s_deathZoneCommingSec = 180;

	cout << "호스트가 나갔으므로 게임 룸 클리어 " << endl;

	return true;
}

DEFRMI_SpaceWar_RequestGameSceneJoin(Server)
{
	shared_ptr<RoomClient> client = m_gameRoom->GetClient(remote);

	client->SetGameScene(true);
	client->SetPosition(pos);

	// 모든 클라이언트가 준비되었는지 확인 

	cout << "우와 " << m_gameRoom->IsGameSceneAllReady() << endl;
	if (m_gameRoom->IsGameSceneAllReady())
	{
		// 이 경우 이제 모든 클라에게 접속 정보를 보낸다.
		forward_list<HostID> list = m_gameRoom->GetAllClient();

		auto iter = list.begin();
		while (iter != list.end())
		{
			forward_list<RoomClient> otherUsers = m_gameRoom->GetOtherClientInfos(*iter);

			auto iter2 = otherUsers.begin();
			while (iter2 != otherUsers.end())
			{
				Vector3 pos = iter2->GetPos();
				m_proxy.NotifyOtherClientJoin(*iter, RmiContext::ReliableSend,
					iter2->GetHostID(), iter2->GetName(), pos.x, pos.y, pos.z);
				iter2++;
			}

			// 호스트가 아니면 아이템 생성 명령도 보내야 함
			if (m_gameRoom->GetClient(*iter)->IsHost() == false)
			{
				// 아이템 생성 샌드
				for each(auto item in m_itemMap)
				{
					m_proxy.NotifyCreateItem(*iter, RmiContext::ReliableSend, (int)HostID_Server,
						item.second->m_itemID,item.second->m_networkID, item.second->pos, item.second->rot);
				}
			}
			

			iter++;
		}
	}

	
	return true;
}

#pragma endregion

DEFRMI_SpaceWar_RequestGameExit(Server)
{
	s_GameRunning = false;
	m_gameRoom->ClearRoom();
	m_clientMap.clear();
	m_itemMap.clear();
	m_itemBoxMap.clear();
	//m_oxyChargerMap.clear();
	s_deathZoneCommingSec = 180;
	return true;
}

#pragma region Death Zone
DEFRMI_SpaceWar_RequestSpaceShipSetup(Server)
{
	cout << "우주선의 갯수가 세팅됩니다.." << spaceShipCount << endl;
	m_spaceshipCount = spaceShipCount;
	return true;
};

DEFRMI_SpaceWar_RequestDeathZoneMoveIndex(Server)
{
	cout << "Death Zone Move Target Index " << moveIndex << endl;
	s_deathZoneIndex = moveIndex;

	// 바뀔때 통보도 해주어야함
	m_proxy.NotifyDeathZoneMoveHostAndIndexSetup(m_playerP2PGroup, RmiContext::ReliableSend, s_deathZoneHostID, s_deathZoneIndex);

	return true;
};
#pragma endregion
