// Server.cpp : �ܼ� ���� ���α׷��� ���� �������� �����մϴ�.
//

#include "stdafx.h"
#include "Server.h"

Server server;

int main()
{
	srand((unsigned int)time(NULL));
	

	server.ServerRun();
    return 0;
}
#pragma region Meteor

void MeteorLoop(void*)
{
	s_meteorCommingSec--;

	if(s_meteorCommingSec % 5 == 0)
		cout << "���׿� " << s_meteorCommingSec << " �� ���� " << endl;

	
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
			if (s_meteorCommingSec == -60)
			{
				s_meteorCommingSec = 10;
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
	//CTimerThread meteorThread(func, 1000, nullptr);
	//
	//meteorThread.Start();

	// -- ���� �Ķ���� ���� ( �������� / ��Ʈ ���� ) ---------------------------//
	CStartServerParameter pl;
	pl.m_allowServerAsP2PGroupMember = true;
	pl.m_protocolVersion = g_protocolVersion;
	pl.m_tcpPorts.Add(g_serverPort);

	// -- Stub , Proxy Attach ���� ������ ���� �� �ִ� --------------------------//
	m_netServer->AttachStub(this);
	m_netServer->AttachProxy(&m_proxy);

	// -- ���� ���ٽ� -----------------------------------------------------------//

	//-- ���� ���� --------------------------------------------------------------//
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

	m_gameStartTime = m_netServer->GetTimeMs();
	// Ŭ���̾�Ʈ�� ���Դ� 
	m_netServer->OnClientJoin = [this](CNetClientInfo *clientInfo) { OnClientJoin(clientInfo); };

	// Ŭ���̾�Ʈ�� ������
	m_netServer->OnClientLeave = [this](CNetClientInfo *clientInfo, ErrorInfo* errinfo, const ByteArray& comment) {
		OnClientLeave(clientInfo, errinfo, comment);
	};

	// ���� ����
	m_netServer->OnError = [](ErrorInfo *errorInfo) {
		cout << "type : " << errorInfo->m_errorType << endl;
		printf("OnError : %s\n", StringT2A(errorInfo->ToString()).GetString());
	};

	// ���� ���
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
			// ���������� �߰��Ǿ���
			cout << "Server : P2P Member JoinAckComplete " << memberHostID << endl;
		}
		else
		{
			cout << "Server : P2P Member JoinAckComplete failed " << endl;
		}
	};

	string line;
	getline(std::cin, line);
	//meteorThread.Stop();
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
		cout << "�α������� ���� Ŭ���̾�Ʈ�� �������ϴ�. " << clientInfo->m_HostID << endl;

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

	// �ߺ� �������� üũ
	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		if (iter->second->m_userName == id)
		{
			// �ߺ�
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend,
				"�ߺ� �����Դϴ�.");

		}
		iter++;
	}

	if (m_gameRoom != nullptr)
	{
		if (m_gameRoom->NewClientConnect(remote, id, m_critSec) == false)
		{
			cout << "Login Failed .. �� �̻� ������ �� �����ϴ�. " << endl;
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend, "�� �̻� ������ �� �����ϴ�.");
			return true;
		}
	}

	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		auto newClient = make_shared<Client>();
		newClient->m_hostID = remote;
		newClient->m_userName = id;
		m_clientMap[remote] = newClient;
	}
	int count = m_clientMap.size();
	cout << "Server : LoginSuccess "<< count << endl;
	

	m_proxy.NotifyLoginSuccess(remote, RmiContext::ReliableSend,(int)remote,(count == 1));
	
	if (m_gameRoom != nullptr)
	{
		forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);
		auto iter = list.begin();

		while (iter != list.end())
		{
			m_proxy.NotifyNetworkConnectUser(*iter, RmiContext::ReliableSend, (int)remote, id);
			iter++;
		}
	}
	return true;
}

DEFRMI_SpaceWar_RequestClientJoin(Server)
{
	cout << "Server : RequestClientJoin "<<name << endl;
	
	auto iter = m_clientMap.find((HostID)hostID);
	if (iter == m_clientMap.end())
	{
		cout << "�߸��� Ŭ���̾�Ʈ " << endl;
		return true;
	}
	// ��ο��� ��Ÿ���� �ؾ��� 
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

	// ������ ���� ����
	for each(auto item in m_itemMap)
	{
		m_proxy.NotifyCreateItem((HostID)hostID, RmiContext::ReliableSend, (int)HostID_Server,
			item.second->m_itemCID, item.second->m_itemID, item.second->pos, item.second->rot);
	}
	
	return true;
}

DEFRMI_SpaceWar_RequestWorldCreateItem(Server)
{
	cout << "RequestWorldCreateItem  item CID : " << itemCID << " x : "<< pos.x << " y : " << pos.y << " z : "<<pos.z  << endl;
	auto iter = m_clientMap.find((HostID)hostID);

	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		auto newItem = make_shared<Item>();
		newItem->m_itemCID = itemCID;
		newItem->m_itemID = itemID;
		newItem->pos.x = pos.x;
		newItem->pos.y = pos.y;
		newItem->pos.z = pos.z;
		newItem->rot.x = rot.x;
		newItem->rot.y = rot.y;
		newItem->rot.z = rot.z;
		m_itemMap[itemID] = newItem;
	}

	if (iter == m_clientMap.end())
	{
		cout << "�߸��� ��û" << endl;
	}

	// ���� ��� �ܿ� ���� ��û ������
	for each(auto c in m_clientMap)
	{
		if (c.first != iter->first)
		{
			m_proxy.NotifyCreateItem(c.first, RmiContext::ReliableSend, 
				(int)(iter->first), itemCID,itemID, pos, rot);
		}
	}
	return true;
}

DEFRMI_SpaceWar_NotifyDeleteItem(Server)
{
	cout << "NotifyDeleteItem " << itemID << endl;

	auto iter = m_itemMap.find(itemID);
	
	if (iter == m_itemMap.end())
	{
		cout << "�߸��� ��û" << endl;
	}
	{
		// �߰�����
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
	return true;
}

DEFRMI_SpaceWar_NotifyPlayerEquipItem(Server)
{
	cout << "NotifyPlayerEquipItem " << hostID << " item " << itemID << endl;
	return true;
}

DEFRMI_SpaceWar_RequestPlayerDamage(Server)
{
	cout << "Request Player Damage ������ " << sendHostID << " ������ " << targetHostID << " ������ " << damage << endl;
	
	float prevHp = m_clientMap[(HostID)targetHostID]->hp;
	m_clientMap[(HostID)targetHostID]->hp -= damage;

	if (m_clientMap[(HostID)targetHostID]->hp <= 0.0f)
	{
		m_clientMap[(HostID)targetHostID]->PlayerDead(m_netServer->GetTimeMs());
		cout << " �׾��� " + targetHostID << endl;
		// ���� �Ŀ� ��ý�Ʈ ��� ����
		forward_list<int> list = m_clientMap[(HostID)targetHostID]->GetAssistClientList();
		for (auto value : list)
		{
			//ų�ѳ�
			if ((HostID)sendHostID == (HostID)value)
				continue;
			m_clientMap[(HostID)value]->m_assistCount++;
			m_proxy.NotifyKillInfo((HostID)value, RmiContext::ReliableSend, m_clientMap[(HostID)targetHostID]->m_userName, false, m_clientMap[(HostID)value]->m_killCount, m_clientMap[(HostID)value]->m_assistCount);
		}
		m_clientMap[(HostID)sendHostID]->m_killCount++;
		m_proxy.NotifyKillInfo((HostID)sendHostID, RmiContext::ReliableSend, m_clientMap[(HostID)targetHostID]->m_userName, true, m_clientMap[(HostID)sendHostID]->m_killCount, m_clientMap[(HostID)sendHostID]->m_assistCount);
	}
	else
	{
		// �̳༮�� ��ý�Ʈ üũ�� �ؾ���
		m_clientMap[(HostID)targetHostID]->DamageClient((int)sendHostID,m_netServer->GetTimeMs());
	}


	m_proxy.NotifyPlayerChangeHP(m_playerP2PGroup, RmiContext::ReliableSend, targetHostID, m_clientMap[(HostID)targetHostID]->m_userName, m_clientMap[(HostID)targetHostID]->hp, prevHp, MAX_HP);
	return true;
}

DEFRMI_SpaceWar_RequestPlayerUseOxy(Server)
{
	cout << "RequestPlayerUseOxy " << useOxy << endl;
	
	float prevOxy =	m_clientMap[(HostID)sendHostID]->oxy;
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
		
		// ù ���
		m_itemBoxMap[itemBoxIndex] = sendHostID;

		int itemCode = 0; // ���⼭ �����
		m_proxy.NotifyUseItemBox(m_playerP2PGroup, RmiContext::ReliableSend,
			sendHostID, itemBoxIndex, itemCode);
		return true;
	}

	// �̹� �����
	
	

	return true;
}

// ���� ���
DEFRMI_SpaceWar_RequestShelterStartSetup(Server)
{
	cout << "RequestShelterStartSetup " << shelterID << endl;

	auto newshelter = make_shared<Shelter>();
	newshelter->m_shelterID = shelterID;
	m_shelterMap[shelterID] = newshelter;
	return true;
}

// ���� �� ���� 
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

// ���� ����
DEFRMI_SpaceWar_RequestShelterEnter(Server)
{
	cout << "RequestShelterEnter " << sendHostID << endl;

	// ���� ���� 
	bool prevState = m_shelterMap[shelterID]->m_lightState;
	bool prevDoorState = m_shelterMap[shelterID]->m_doorState;
	if (enter)
		m_shelterMap[shelterID]->ShelterEnter();
	else
		m_shelterMap[shelterID]->ShelterExit();

	// ���� ���¿� �޶����� ���
	if (m_shelterMap[shelterID]->m_lightState != prevState || 
		m_shelterMap[shelterID]->m_doorState != prevDoorState)
	{
		// ���� ����
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
	cout << "��ģ�� ���ּ� ���� " << winPlayerID << endl;
	m_clientMap[(HostID)winPlayerID]->PlayerWin();
	return true;
}

DEFRMI_SpaceWar_RequestGameEnd(Server)
{
	cout << "RequestGameEnd -- ���� ����  - " << endl;
	
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
				cout << " ���� ������ " << endl;
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

// �κ� ȭ�鿡 ���Դ�.
DEFRMI_SpaceWar_RequestLobbyConnect(Server)
{
	// ���� ��ģ������ ��� ������ ������.
	cout << "Lobby Connect " << endl;
	forward_list<RoomClient> list = m_gameRoom->GetOtherClientInfos(remote);

	//���� ���Ӹ�带 ������.
	m_proxy.NotifyNetworkGameModeChange(remote, RmiContext::ReliableSend, m_gameRoom->GetGameMode(), m_gameRoom->GetTeamMode());

	// �÷��� ī��Ʈ�� ����
	m_proxy.NotifyNetworkGamePlayerCountChange(remote, RmiContext::ReliableSend, m_gameRoom->GetPlayerLimitCount());

	auto iter = list.begin();

	while (iter != list.end())
	{
		m_proxy.NotifyNetworkUserSetup(remote, RmiContext::ReliableSend, (int)iter->GetHostID(),iter->GetName(), iter->IsReady(), iter->IsRedTeam());
		iter++;
	}
	return true;
}

// �� ������ �ٲ� 
DEFRMI_SpaceWar_RequestNetworkGameTeamSelect(Server)
{
	cout << "Team ���� " << endl;
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

// ����
DEFRMI_SpaceWar_RequestNetworkGameReady(Server)
{
	cout << " Ready ���� " << endl;

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

// ������ ���� �ٲ�
DEFRMI_SpaceWar_RequestNetworkChangeMap(Server)
{
	cout << " Map ���� " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameChangeMap(*iter, RmiContext::ReliableSend, mapName);
		iter++;
	}
	
	return true;
}

// ������ ���� ��带 �ٲ� 
DEFRMI_SpaceWar_RequestNetworkGameModeChange(Server)
{
	cout << " Game Mode ���� " << endl;

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

// ������ �÷��̾� ���� �ٲ�
DEFRMI_SpaceWar_RequestNetworkPlayerCount(Server)
{
	cout << " Player Count ���� " << playerCount << endl;

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

// ������ ���� ������ ����
DEFRMI_SpaceWar_RequestNetworkGameStart(Server)
{
	cout << " Game Start ���� " << endl;

	if (!m_gameRoom->GameStartCheck())
	{
		//����
		m_proxy.NotifyNetworkGameStartFailed(m_playerP2PGroup, RmiContext::ReliableSend);
		return true;
		
	}

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

//������ ����
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

	cout << "ȣ��Ʈ�� �������Ƿ� ���� �� Ŭ���� " << endl;

	return true;
}

DEFRMI_SpaceWar_ReqeustGameSceneJoin(Server)
{
	return true;
}

#pragma endregion