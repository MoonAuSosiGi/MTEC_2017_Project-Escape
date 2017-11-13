#include "stdafx.h"
#include "Server.h"
Server server;
bool s_GameRunning = false;


/**
 * @brief ���� ���α׷��� ���� ����
 * 
*/
int main()
{
	srand((unsigned int)time(NULL));
	
	server.ServerFileLoad();
	server.ServerRun();
    return 0;
}
/**
 * @brief	Server �⺻ ������
 * @detail	GameRoom ���� ���� �⺻ �ʱ�ȭ ���� ����
*/
Server::Server()
{
	m_gameRoom = make_shared <GameRoom>();
}



#pragma region Server Thread =======================================================================================
/**
 * @brief	���������� :: ���׿�, ������ ����
 * @detail	1�ʸ��� ȣ���
*/
void ServerThreadLoop(void*)
{
	if (s_GameRunning == false)
		return;
	// ���ּ� ��� ���� �ð�

	if (server.GetSpaceShipLockTime() > 0 )
	{
		server.SetSpaceShipLockTime(server.GetSpaceShipLockTime() - 1);
		server.GetProxy()->NotifySpaceShipLockTime(server.GetP2PID(), 
			RmiContext::ReliableSend, server.GetSpaceShipLockTime());
	}
	
	for (int i = 0; i < 1; i++)
	{
		if (s_meteorCommingTime[i] > 0)
		{
			s_meteorCommingTime[i] -= 1;
			if(s_meteorCommingTime[i] % 5)
				cout << "���׿� "<< i << "�� " << s_meteorCommingTime[i] << " �� ���� " << server.GetServer()->GetTimeMs() << endl;
			if (s_meteorCommingTime[i] <= 0.0f)
			{
				float anglex = RandomRange(-360.0f, 360.0f);
				float anglez = RandomRange(-360.0f, 360.0f);
				string meteorID = "meteor";
				
				meteorID += to_string(server.GetMeteorID());
				server.SetMeteorID(server.GetMeteorID() + 1);
				cout << "anglex " << anglex << " anglez " << anglez;

				server.GetProxy()->NotifyMeteorCreate(server.GetP2PID(), 
					RmiContext::ReliableSend, anglex, anglez, meteorID);

				s_meteorCommingTime[i] = 60;
			}
		}
	}
	
	if (server.GetDeathZoneCommingTime() >= 0)
	{
		server.SetDeathZoneCommingTime(server.GetDeathZoneCommingTime() - 1);
		
		string s = "deathZone";
		s += to_string(server.GetDeathZoneID());
		cout << server.GetDeathZoneCommingTime() << endl;

		server.GetProxy()->NotifyDeathZoneCommingTime(server.GetP2PID(),
			RmiContext::ReliableSend, server.GetDeathZoneCommingTime(),s);
	}

	// ����� ������ üũ
	if (server.GetDeathZoneCommingTime() == 0)
	{
		int index = (int)RandomRange(0, server.GetSpaceShipCount());
		string deathZoneID = "deathZone";
		deathZoneID += to_string(server.GetDeathZoneID());
		server.SetDeathZoneID(server.GetDeathZoneID() + 1);
		// ����
		server.GetProxy()->NotifyDeathZoneCreate(server.GetP2PID(), 
			RmiContext::ReliableSend, index, deathZoneID);

		if (server.GetGameRoom() == nullptr)
			return;
		auto clientMap = server.GetGameRoom()->GetClientMap();

		// �����̴� ��ü ����
		auto iter = clientMap.begin();
		while (iter != clientMap.end())
		{
			if (iter->second->GetState() == ALIVE)
			{
				// �����̴� ��ü �뺸
				server.GetProxy()->NotifyDeathZoneMoveHostAndIndexSetup(
					server.GetP2PID(),
					RmiContext::ReliableSend,
					(int)iter->second->GetHostID(), 
					server.GetDeathZoneIndex());
				server.SetDeathZoneHostID((int)iter->second->GetHostID());
				break;
			}
			iter++;
		}
		server.SetDeathZoneCommingTime(server.GetDeathZoneCommingTime() - 1);
		
	}
	// �������� �����̴� ���� 
	else if (server.GetDeathZoneCommingTime() == -1)
	{
		if (server.GetGameRoom() == nullptr)
			return;
		auto clientMap = server.GetGameRoom()->GetClientMap();
		if(clientMap.size() <= 0)
			return;
		if (clientMap.find((HostID)server.GetDeathZoneHostID()) == clientMap.end())
			return;
		if (clientMap[(HostID)server.GetDeathZoneHostID()]->GetState() != ALIVE)
		{
			// �����̴� ��ü ����
			auto iter = clientMap.begin();
			while (iter != clientMap.end())
			{
				if (iter->second->GetState() == ALIVE)
				{
					// �����̴� ��ü �뺸
					server.GetProxy()->NotifyDeathZoneMoveHostAndIndexSetup(
						server.GetP2PID(),
						RmiContext::ReliableSend,
						(int)iter->second->GetHostID(),
						server.GetDeathZoneIndex());
					server.SetDeathZoneHostID((int)iter->second->GetHostID());
					break;
				}
				iter++;
			}
		}
	}

}
#pragma endregion ==================================================================================================



#pragma region Sever Class Logic ===================================================================================

/**
 * @brief	Sever �⺻ ���� ���� �ε�
 * @detail	server.properties �� �ε��� ���̺� ���� ���� �Ѵ�.
*/
void Server::ServerFileLoad()
{
	// ���� �б�
	ifstream infile("server.properties");

	if (infile.is_open() == false)
	{
		cout << "Server.properties is nothing :: " << endl;
		return;
	}

	string data = "";
	string line = "";

	while (!infile.eof())
	{
		getline(infile, line);
		data += line;
	}
	
	infile.close();
	Reader reader;
	
	bool parseCheck = reader.parse(data.c_str(), m_serverPropertiesData);

	if (!parseCheck)
	{
		cout << "JSON File Parse Failed..." << reader.getFormattedErrorMessages() << endl;
		return ;
	}

	cout << "Server Properties Load Success.. " << endl;
	//
}

/**
 * @brief	Server ���� ������ ���� ���̺� ���� �ε��Ѵ�.
 * @todo	JSON �׽�Ʈ �ʿ� ( ���� ���׽�Ʈ )
 * @detail	�⺻���� �ε��� �����Ѵ�.
*/
bool Server::ServerTableSetup()
{
	if (m_serverPropertiesData.empty())
	{
		cout << "json ERROR ! " << endl;
		return false;
	}

	SetDeathZoneCommingTime(m_serverPropertiesData["DeathZoneFirstComming"].asInt());
	SetSpaceShipLockTime(m_serverPropertiesData["SpaceShipLockTime"].asInt());

	return true;
}
/**
 * @brief	���� ���� ����
 * @detail	���� ������ �������� ���ư��� ��.
*/
void Server::ServerRun()
{
	m_netServer = shared_ptr<CNetServer>(CNetServer::Create());
	// ���� ���� �ε�
	ServerTableSetup();
	// ������ ������
	void(*func)(void*);
	func = &ServerThreadLoop;
	CTimerThread serverThread(func, 1000, nullptr);
	// ���� ������ ���� 
	serverThread.Start();

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
	catch (Proud::Exception &e)
	{
		cout << "Server Start Failed : " << e.what() << endl;
		return;
	}

	cout << "Time For Escape :: Server 20171022 Build -------------------------" << endl;
	
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
	m_netServer->OnException = [](const Proud::Exception &e) {
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
	serverThread.Stop();
}

/**
 * @brief	Ŭ���̾�Ʈ�� �������� ��
 * @detail	p2p �׷쿡 �־��ش�.
*/
void Server::OnClientJoin(CNetClientInfo* clientInfo)
{
	cout << "OnClientJoin " << clientInfo->m_HostID << endl;
	
	m_netServer->JoinP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);
}

/**
 * @brief	Ŭ���̾�Ʈ�� ������ ��
 * @detail	���� ȣ���� �ʿ䰡 ���� ������ ������ ȣ�����ش�.
*/
void Server::OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment)
{
	cout << "OnClientLeave " << clientInfo->m_HostID << endl;

	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.find((clientInfo->m_HostID));
	m_netServer->LeaveP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);

	if (iter == clientMap.end())
	{
		cout << "�α������� ���� Ŭ���̾�Ʈ�� �������ϴ�. " << clientInfo->m_HostID << endl;

		return;
	}
	auto oxyIter = m_oxyChargerMap.begin();
	while (oxyIter != m_oxyChargerMap.end())
	{
		if (oxyIter->second->GetUseHostID() == (int)clientInfo->m_HostID)
		{
			m_oxyChargerMap.erase(oxyIter);
			break;
		}
		oxyIter++;
	}



	m_proxy.NotifyPlayerLost(m_playerP2PGroup, RmiContext::ReliableSend, clientInfo->m_HostID);
	m_gameRoom->LogOutClient(iter->second->GetHostID());

	clientMap.erase(iter);

	if (clientMap.size() <= 0)
		ServerReset();
}

/**
 * @brief	���� ����
 * @detail	������ ������ �ʿ䰡 ���� ��� ȣ��
*/
void Server::ServerReset()
{
	cout << " Server Reset " << endl;
	m_gameRoom->ClearRoom();
	s_GameRunning = false;
	m_itemMap.clear();
	m_itemBoxMap.clear();
	m_oxyChargerMap.clear();
	m_spaceShipMap.clear();

	if (ServerTableSetup() == false)
	{
		SetDeathZoneCommingTime(180);
		SetSpaceShipLockTime(10);
	}

	// ��������� ��
	for (int i = 0; i < 10; i++)
		s_meteorCommingTime[i] = 30 + (i * 20);
}
#pragma region Get / Set Method ------------------------------------------------------------------------------------
/**
 * @brief	���� ��ü�� ��´�.
 * @return	���� ��ü ����
*/
shared_ptr<CNetServer> Server::GetServer()
{
	return m_netServer;
}
/**
 * @brief	Proxy ��ü�� ���´�.
*/
SpaceWar::Proxy* Server::GetProxy()
{
	return &m_proxy;
}

/**
 * @brief	P2P�׷��� HostID �� ��´�.
 * @return	P2P�׷��� HostID �� ����
*/
HostID Server::GetP2PID()
{
	return m_playerP2PGroup;
}

/**
 * @brief	GameRoom ���
 * @return	GameRoom �� �����Ѵ�.
*/
shared_ptr<GameRoom> Server::GetGameRoom()
{
	return m_gameRoom;
}

/**
 * @brief	���ּ��� �ʱ� ��� �ð����� �����Ѵ�.
 * @param	lockTime ��� �ð��� �� 
*/
void Server::SetSpaceShipLockTime(int lockTime)
{
	m_spaceShipLockTime = lockTime;
}

/**
 * @brief	���ּ� �ʱ� ��� �ð����� ���´�.
 * @return	���ּ� �ʱ� ��� �ð����� ����
*/
int Server::GetSpaceShipLockTime()
{
	return m_spaceShipLockTime;
}

/**
 * @brief	���������� ���� �ð��� �����Ѵ�.
 * @param	sec ������ ���� ���� �ð�
*/
void Server::SetDeathZoneCommingTime(int sec)
{
	m_deathZoneCommingSec = sec;
}

/**
 * @brief	���������� ���� �ð��� ���´�.
 * @return	���������� ���� �ð��� ����
*/
int Server::GetDeathZoneCommingTime()
{
	return m_deathZoneCommingSec;
}

/**
 * @brief	DeathZone ���� �������� �ε��� ����
 * @param	curIndex ���� �������� ������ �ε���
*/
void Server::SetDeathZoneIndex(int curIndex)
{
	m_deathZoneIndex = curIndex;
}

/**
 * @brief	���� �������� DeathZone �ε��� ���
 * @return	���� �������� DeathZone �ε��� ��ȯ
*/
int Server::GetDeathZoneIndex()
{
	return m_deathZoneIndex;
}

/**
 * @brief	DeathZone�� �����Ű�� �ִ� Ŭ���̾�Ʈ
 * @param	DeathZone�� �����Ű�� �ִ� Ŭ���̾�Ʈ�� HostID
*/
void Server::SetDeathZoneHostID(int hostID)
{
	m_deathZoneHostID = hostID;
}

/**
 * @brief	DeathZone�� �����Ű�� �ִ� Ŭ���̾�Ʈ�� HostID�� ��´�.
 * @return	DeathZone�� �����Ű�� �ִ� Ŭ���̾�Ʈ�� HostID.
*/
int Server::GetDeathZoneHostID()
{
	return m_deathZoneHostID;
}

/**
 * @brief	DeathZone�� ��Ʈ��ũ �ĺ� ���̵� ����
 * @param	�ĺ� ���̵�
*/
void Server::SetDeathZoneID(int id)
{
	m_deathZoneID = id;
}

/**
 * @brief	DeathZone �� �ĺ� ���̵� ��´�.
 * @return	DeathZone�� �ĺ� ���̵� ���� -1�� ��� �̼���
*/
int Server::GetDeathZoneID()
{
	return m_deathZoneID;
}

/**
* @brief	Meteor�� ��Ʈ��ũ �ĺ� ���̵� ����
* @param	�ĺ� ���̵�
*/
void Server::SetMeteorID(int id)
{
	m_meteorID = id;
}

/**
* @brief	Meteor �� �ĺ� ���̵� ��´�.
* @return	Meteor�� �ĺ� ���̵� ���� -1�� ��� �̼���
*/
int Server::GetMeteorID()
{
	return m_meteorID;
}
#pragma endregion --------------------------------------------------------------------------------------------------
#pragma endregion

#pragma region C2S Method ==========================================================================================

#pragma region �ʱ� ���� ===========================================================================================
#pragma region ���� ���� -------------------------------------------------------------------------------------------
/**
 * @brief	���� ���� ��û
 * @detail	���� ���� ���� ��û
*/
DEFRMI_SpaceWar_RequestServerConnect(Server)
{
	cout << "C2S-- RequestServerConnect " << id << " remote " << remote << endl;

	//���� �� ����
	if (m_gameRoom != nullptr)
	{
		if (m_gameRoom->NewClientConnect(remote, id, m_critSec) == false)
		{
			cout << "Login Failed .. �� �̻� ������ �� �����ϴ�. " << endl;
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend, "�� �̻� ������ �� �����ϴ�.");
			return true;
		}
	}


	int count = GetGameRoom()->GetClientMap().size();
	cout << "Server : LoginSuccess "<< count << endl;
	

	m_proxy.NotifyLoginSuccess(remote, RmiContext::ReliableSend,(int)remote,(count == 1));
	return true;
}

/**
 * @brief	�� ���� ��û
 * @detail	�������� ���� �� �ش� ���� ��ε�ĳ����
*/
DEFRMI_SpaceWar_RequestNetworkGameTeamSelect(Server)
{
	cout << "Team ���� " << endl;
	m_gameRoom->TeamChange(remote, teamRed);

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkGameTeamChange(*iter, RmiContext::ReliableSend, (int)remote, teamRed);
		iter++;
	}


	return true;
}

/**
 * @brief ���� ���� ��û
*/
DEFRMI_SpaceWar_RequestGameExit(Server)
{
	ServerReset();
	return true;
}
#pragma endregion

#pragma region ȣ��Ʈ �ƴ� ���� ------------------------------------------------------------------------------------
/**
 * @brief	���� ���� ��û
 * @detail	�������� ��û�� �ް� �ش� ���� ��ε�ĳ����
*/
DEFRMI_SpaceWar_RequestNetworkGameReady(Server)
{
	cout << " Ready ���� " << endl;

	m_gameRoom->SetReady(remote, ready);

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		m_proxy.NotifyNetworkReady(*iter, RmiContext::ReliableSend, (int)remote, m_gameRoom->GetClient(remote)->GetName(), ready);
		iter++;
	}
	return true;
}

/**
 * @brief	�κ���� ���Դ�.
 * @detail	������ �����ִ� ������ �������� �����ش�. / �� �ڽ��� ������ ������.
*/
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
		// ���� ������ ������.
		m_proxy.NotifyNetworkUserSetup(remote, RmiContext::ReliableSend, (int)iter->GetHostID(), iter->GetName(), iter->IsReady(), iter->IsRedTeam());

		// �� ������ ������ ���� �˷��ش�.
		if (remote != iter->GetHostID())
			m_proxy.NotifyNetworkConnectUser(iter->GetHostID(), RmiContext::ReliableSend, remote, m_gameRoom->GetClient(remote)->GetName());
		iter++;
	}
	return true;
}

/**
 * @brief	���� �� �ε� �Ϸ� �޽���
 * @detail	���� ���ӽ� ���� ������( �ʱ� ��ǥ �� )�� ������.
*/
DEFRMI_SpaceWar_RequestGameSceneJoin(Server)
{
	shared_ptr<RoomClient> client = m_gameRoom->GetClient(remote);
	// ���� ���� ���Դ�.
	client->SetGameScene(true);
	client->SetPosition(pos);

	// ��� Ŭ���̾�Ʈ�� �غ�Ǿ����� Ȯ�� 

	cout << "Game Scene Join :: " << m_gameRoom->IsGameSceneAllReady() << endl;
	if (m_gameRoom->IsGameSceneAllReady())
	{
		// �� ��� ���� ��� Ŭ�󿡰� ���� ������ ������.
		forward_list<HostID> list = m_gameRoom->GetAllClient();
		s_GameRunning = true;
		auto iter = list.begin();
		while (iter != list.end())
		{
			forward_list<RoomClient> otherUsers = m_gameRoom->GetOtherClientInfos(*iter);

			auto iter2 = otherUsers.begin();
			while (iter2 != otherUsers.end())
			{
				Vector3 pos = iter2->GetPosition();
				// ���� ������ ������.
				m_proxy.NotifyOtherClientJoin(*iter, RmiContext::ReliableSend,
					iter2->GetHostID(), iter2->GetName(), pos.x, pos.y, pos.z);
				iter2++;
			}

			// ȣ��Ʈ�� �ƴϸ� ������ ���� ��ɵ� ������ ��
			if (m_gameRoom->GetClient(*iter)->IsHost() == false)
			{
				// ������ ���� ����
				for each(auto item in m_itemMap)
				{
					m_proxy.NotifyCreateItem(*iter, RmiContext::ReliableSend, (int)HostID_Server,
						item.second->GetItemID(), item.second->GetNetworkID(), item.second->GetPosition(), item.second->GetRotation());
				}
			}


			iter++;
		}
	}


	return true;
}
#pragma endregion

#pragma region ���� ȣ��Ʈ ���� ------------------------------------------------------------------------------------
/**
 * @brief	������ ���� �ٲ��.
 * @detail	���� �ٲ� ���� ��ε�ĳ���� ���ش�.
*/
DEFRMI_SpaceWar_RequestNetworkChangeMap(Server)
{
	cout << " Map Change :: " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// �ٲ� �� ���� ����
		m_proxy.NotifyNetworkGameChangeMap(*iter, RmiContext::ReliableSend, mapName);
		iter++;
	}

	return true;
}

/**
 * @brief	�÷��� ������ �÷��̾� ���� �����ߴ�.
 * @detail	�÷��̾� �� ������ ��ε�ĳ�����Ѵ�.
*/
DEFRMI_SpaceWar_RequestNetworkPlayerCount(Server)
{
	cout << " Player Count ���� " << playerCount << endl;

	m_gameRoom->SetPlayerLimitCount(playerCount);
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// �÷��̾� ī��Ʈ ���� ����
		m_proxy.NotifyNetworkGamePlayerCountChange(*iter, RmiContext::ReliableSend, playerCount);
		iter++;
	}
	return true;
}

/**
 * @brief	���� ��� ����
 * @detail	���� ��� ���� ������ ��ε�ĳ����
 * @todo	���� ������ġ ��� ���� ���� �߰�
*/
DEFRMI_SpaceWar_RequestNetworkGameModeChange(Server)
{
	cout << " Game Mode ���� " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	m_gameRoom->SetGameMode(gameMode);
	m_gameRoom->SetTeamMode(teamMode);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// ���� ��� ����
		m_proxy.NotifyNetworkGameModeChange(*iter, RmiContext::ReliableSend, gameMode, teamMode);
		iter++;
	}
	return true;
}

/**
 * @brief	���� ��ŸƮ ��û
 * @detail	���� ����.
*/
DEFRMI_SpaceWar_RequestNetworkGameStart(Server)
{
	cout << " Game Start ���� " << endl;
	m_itemMap.clear();
	// ������ ������ �� �ִ� ȯ������ üũ
	//if (!m_gameRoom->GameStartCheck())
	//{
	//	//����
	//	m_proxy.NotifyNetworkGameStartFailed(m_playerP2PGroup, RmiContext::ReliableSend);
	//	return true;
	//	
	//}

	// ���� �ð� ����� ���� ���� �ð��� ����
	m_gameStartTime = m_netServer->GetTimeMs();
	
	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.begin();
	while (iter != clientMap.end())
	{
		m_proxy.NotifyNetworkGameStart(iter->second->GetHostID(), RmiContext::ReliableSend);
		iter++;
	}
	return true;
}

/**
 * @brief	������ ������.
 * @detail	�ٵ� Ÿ��Ʋ�� ƨ���� ������ �ϹǷ� �ش� �޽��� ��ε�ĳ��Ʈ
*/
DEFRMI_SpaceWar_RequestNetworkHostOut(Server)
{
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// ������ �������� �˸�
		m_proxy.NotifyNetworkGameHostOut(*iter, RmiContext::ReliableSend);
		iter++;
	}


	ServerReset();
	cout << "ȣ��Ʈ�� �������Ƿ� ���� �� Ŭ���� " << endl;

	return true;
}


#pragma endregion
#pragma endregion

#pragma region �ΰ��� ==============================================================================================
#pragma region �ΰ��� :: �÷��̾� �޽��� ---------------------------------------------------------------------------
#pragma region [�÷��̾� ���� ���� ����] 

/**
 * @brief	ü�� ���� ��û
 * @detail	�������� ������ ü�� ���� ��û
*/
DEFRMI_SpaceWar_RequestHpUpdate(Server)
{
	cout << "HP Update ��û " << endl;

	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.begin();

	float prevHp = 0.0f;
	while (iter != clientMap.end())
	{
		if (iter->second->GetHostID() == remote)
		{
			prevHp = iter->second->GetHp();
			iter->second->SetHp(hp);
			break;
		}
		iter++;
	}

	iter = clientMap.begin();
	while (iter != clientMap.end())
	{
		m_proxy.NotifyPlayerChangeHP(iter->second->GetHostID(), RmiContext::ReliableSend, (int)remote, "HpUpdate", hp, prevHp, MAX_HP, Vector3(0.0, 0.0, 0.0));
		iter++;
	}
	return true;
}

/**
 * @brief	�������� �Ծ���.
 * @detail	���� / ��ý�Ʈ � ���� ó��
*/
DEFRMI_SpaceWar_RequestPlayerDamage(Server)
{
	auto clientMap = GetGameRoom()->GetClientMap();
	cout << "Request Player Damage ������ " << sendHostID << " ������ " << targetHostID << " ������ " << damage << " ���� �̸� " << weaponName << endl;
	cout << "���� ���ο� " << clientMap.size() << endl;

	if (clientMap[(HostID)targetHostID]->GetState() == DEATH)
		return true;

	float prevHp = clientMap[(HostID)targetHostID]->GetHp();
	clientMap[(HostID)targetHostID]->SetHp(prevHp - damage);

	if (clientMap[(HostID)targetHostID]->GetHp() <= 0.0f)
	{
		cout << " �׾��� " + targetHostID << endl;
		clientMap[(HostID)targetHostID]->SetHp(0.0f);
		clientMap[(HostID)targetHostID]->PlayerDead(m_netServer->GetTimeMs());

		// ���� �Ŀ� ��ý�Ʈ ��� ����
		forward_list<int> list = clientMap[(HostID)targetHostID]->GetAssistClientList();
		for (auto value : list)
		{
			//ų�ѳ�
			if ((HostID)sendHostID == (HostID)value)
				continue;
			clientMap[(HostID)value]->SetAssistCount(clientMap[(HostID)value]->GetAssistCount());
			m_proxy.NotifyKillInfo((HostID)value, RmiContext::ReliableSend, 
				clientMap[(HostID)targetHostID]->GetName(), false, 
				clientMap[(HostID)value]->GetDeathCount(), 
				clientMap[(HostID)value]->GetAssistCount());
		}

		if (sendHostID != targetHostID)
			clientMap[(HostID)sendHostID]->SetKillCount(clientMap[(HostID)sendHostID]->GetKillCount()+1);

		m_proxy.NotifyKillInfo((HostID)sendHostID, 
			RmiContext::ReliableSend, 
			clientMap[(HostID)targetHostID]->GetName(), true, 
			clientMap[(HostID)sendHostID]->GetKillCount(),
			clientMap[(HostID)sendHostID]->GetAssistCount());

		// �� �κп��� �� �׾����� üũ
		int deadCheck = 0;
		auto iter = clientMap.begin();
		while (iter != clientMap.end())
		{
			deadCheck += (iter->second->GetState() == DEATH) ? 0 : 1;
			iter++;
		}

		// ������ - �� ��ο� ���� 
		if (deadCheck == 0)
		{
			iter = clientMap.begin();
			while (iter != clientMap.end())
			{
				// ��ο� �������� ������
				m_proxy.NotifyDrawGame(iter->second->GetHostID(), RmiContext::ReliableSend);
				iter++;
			}
		}
	}
	else
	{
		// �̳༮�� ��ý�Ʈ üũ�� �ؾ���
		if (targetHostID != sendHostID)
			clientMap[(HostID)targetHostID]->DamageClient((int)sendHostID, m_netServer->GetTimeMs());
	}


	m_proxy.NotifyPlayerChangeHP(m_playerP2PGroup, RmiContext::ReliableSend, targetHostID, weaponName, clientMap[(HostID)targetHostID]->GetHp(), prevHp, MAX_HP, dir);
	return true;
}

/**
 * @brief	��Ҹ� ����ߴ�.
 * @
*/
DEFRMI_SpaceWar_RequestPlayerUseOxy(Server)
{
	//cout << "RequestPlayerUseOxy " << useOxy << endl;
	auto clientMap = GetGameRoom()->GetClientMap();
	float prevOxy = clientMap[(HostID)sendHostID]->GetOxy();

	if (prevOxy - useOxy < 0)
	{
		clientMap[(HostID)sendHostID]->SetOxy(0.0f);
		m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup,
			RmiContext::ReliableSend, sendHostID, 
			clientMap[(HostID)sendHostID]->GetName(), 0.0f, prevOxy, MAX_OXY);
		return true;
	}


	clientMap[(HostID)sendHostID]->SetOxy(prevOxy - useOxy);

	m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup,
		RmiContext::ReliableSend, sendHostID, 
		clientMap[(HostID)sendHostID]->GetName(), 
		clientMap[(HostID)sendHostID]->GetOxy(), prevOxy, MAX_OXY);

	return true;
}

/**
 * @brief	�÷��̾ �̵������� �˸�
*/
DEFRMI_SpaceWar_NotifyPlayerMove(Server)
{
	auto clientMap = GetGameRoom()->GetClientMap();
	clientMap[(HostID)hostID]->SetPosition(curX, curY, curZ);

	m_gameRoom->GetClient((HostID)hostID)->SetPosition(curX, curY, curZ);
	return true;
}

/**
 * @brief	�÷��̾ �������� ����ߴ�.
*/
DEFRMI_SpaceWar_NotifyPlayerEquipItem(Server)
{
	cout << "NotifyPlayerEquipItem " << hostID << " item " << itemID << endl;
	return true;
}
#pragma endregion

#pragma endregion
#pragma region �ΰ��� :: ��� ������ -------------------------------------------------------------------------------

/**
 * @brief	���� �� ��� �����⸦ ����Ѵ�.
 * @detail	����ϰ���� ��� �����⸦ ����� �� �ִ�.
*/
DEFRMI_SpaceWar_RequestOxyChargerStartSetup(Server)
{
	CriticalSectionLock lock(m_critSec, true);
	auto newOxyCharger = make_shared<OxyCharger>();
	m_oxyChargerMap[oxyChargerID] = newOxyCharger;
	return true;
}

/**
 * @brief	��� ������ ���� ��û
 * @detail	����ִ��� üũ �� �˸°� ó��
*/
DEFRMI_SpaceWar_RequestUseOxyChargerStart(Server)
{
	// ����ִٸ� �ź�
	if (m_oxyChargerMap[oxyChargerIndex]->IsLocked())
	{
		m_proxy.NotifyUseFailedOxyCharger(remote, RmiContext::ReliableSend, 
			m_oxyChargerMap[oxyChargerIndex]->GetUseHostID(), oxyChargerIndex);
	}
	// ������� �ʴٸ� ���
	else
	{
		//��ٴ�.
		m_oxyChargerMap[oxyChargerIndex]->LockOxyCharger((int)remote);
		m_proxy.NotifyUseSuccessedOxyCharger(remote, 
			RmiContext::ReliableSend, (int)remote, oxyChargerIndex);
	}
	return true;
}

/**
 * @brief	��� ������ ������ 
*/
DEFRMI_SpaceWar_RequestUseOxyCharger(Server)
{
	cout << "RequestUseOxyCharger " << endl;

	if (m_oxyChargerMap[oxyChargerIndex]->IsLocked() 
		&& m_oxyChargerMap[oxyChargerIndex]->GetUseHostID() == (int)remote)
	{
		auto clientMap = GetGameRoom()->GetClientMap();
		float prevOxy = clientMap[(HostID)sendHostID]->GetOxy();
		clientMap[(HostID)sendHostID]->SetOxy(prevOxy + userOxy);

		m_proxy.NotifyPlayerChangeOxygen(m_playerP2PGroup, 
			RmiContext::ReliableSend, sendHostID,
			clientMap[(HostID)sendHostID]->GetName(), 
			clientMap[(HostID)sendHostID]->GetOxy(), prevOxy, MAX_OXY);

		m_proxy.NotifyUseOxyCharger(m_playerP2PGroup, 
			RmiContext::ReliableSend, sendHostID,
			oxyChargerIndex, userOxy);
	}

	return true;
}

/**
 * @brief	��� ������ ��� ��
 * @detail	��� ���� �� ��������� �ǵ���
*/
DEFRMI_SpaceWar_RequestUseOxyChargerEnd(Server)
{
	cout << "Request Use Oxy Charger End " << oxyChargerIndex << endl;
	if (m_oxyChargerMap[oxyChargerIndex]->GetUseHostID() == (int)remote)
		m_oxyChargerMap[oxyChargerIndex]->UnLockOxyCharger();
	return true;
}

#pragma endregion

#pragma region �ΰ��� :: ������ �ڽ� ���� --------------------------------------------------------------------------
/**
 * @brief	������ �ڽ� ��� ��û
 * @detail	�������� ������ �ڵ� ������ ��ε�ĳ����
*/
DEFRMI_SpaceWar_RequestUseItemBox(Server)
{
	cout << "RequestUseItemBox " << itemBoxIndex << endl;

	// TESTCODE
	//	if (m_itemBoxMap.find(itemBoxIndex) != m_itemBoxMap.end())
	{
		/*Reader reader;
		Value val;
		bool parseCheck = reader.parse(m_serverPropertiesData.c_str(), val);

		if (!parseCheck)
		{
			cout << "JSON File Parse Failed..." << reader.getFormattedErrorMessages() << endl;
			return true;
		}


		s_deathZoneCommingSec = val["DeathZoneFirstComming"].asInt();
		s_spaceShipLockTime = val["SpaceShipLockTime"].asInt();

		
		int size = val["Items"].size();*/

		string itemID = "temp";//val["Items"][RandomRange(0, size)]["Id"].asString();
		// ù ���
		m_itemBoxMap[itemBoxIndex] = sendHostID;
		cout << "item Code " << itemID << endl;
		string networkID = "server_" + itemBoxIndex;
		networkID += "_" + sendHostID;

		m_proxy.NotifyUseItemBox(m_playerP2PGroup, RmiContext::ReliableSend,
			sendHostID, itemBoxIndex, itemID, networkID);
		return true;
	}

	// �̹� �����
	return true;
}
#pragma endregion

#pragma region �ΰ��� :: ���� ���� ---------------------------------------------------------------------------------

/**
 * @brief	���� ���
 * @detail	����ϰ���� ���͸� ����� �� �ִ�.
*/
DEFRMI_SpaceWar_RequestShelterStartSetup(Server)
{
	cout << "RequestShelterStartSetup " << shelterID << endl;

	auto newshelter = make_shared<Shelter>();
	newshelter->SetShelterID(shelterID);
	m_shelterMap[shelterID] = newshelter;
	return true;
}

/**
 * @brief	���� �� ����
 * @detail	���� ���� ���� ������ �ް� �ݿ��� �� ��ε�ĳ����
*/
DEFRMI_SpaceWar_RequestShelterDoorControl(Server)
{
	cout << "RequestShelterDoorControl " << shelterID << " state " << doorState << endl;

	m_shelterMap[shelterID]->ShelterDoorStateChange(doorState);

	//// p2p �� ���� TODO
	//auto iter = m_clientMap.begin();
	//while (iter != m_clientMap.end())
	//{
	//	if ((int)iter->second->m_hostID != sendHostID)
	//		m_proxy.NotifyShelterInfo(iter->second->m_hostID,
	//			RmiContext::ReliableSend, sendHostID, shelterID,
	//			doorState, m_shelterMap[shelterID]->m_lightState);

	//	iter++;
	//}


	return true;
}
 
/**
 * @brief	���Ϳ� �����ߴ�.
 * @detail	������ ���� �Ѱ� �ϴ� ó���� ����
*/
DEFRMI_SpaceWar_RequestShelterEnter(Server)
{
	cout << "RequestShelterEnter " << sendHostID << endl;

	// ���� ���� 
	bool prevState = m_shelterMap[shelterID]->IsLightOn();
	bool prevDoorState = m_shelterMap[shelterID]->IsDoorOpen();
	if (enter)
		m_shelterMap[shelterID]->ShelterEnter();
	else
		m_shelterMap[shelterID]->ShelterExit();

	//// ���� ���¿� �޶����� ���
	//if (m_shelterMap[shelterID]->m_lightState != prevState ||
	//	m_shelterMap[shelterID]->m_doorState != prevDoorState)
	//{
	//	// ���� ����
	//	m_proxy.NotifyShelterInfo(m_playerP2PGroup,
	//		RmiContext::ReliableSend, sendHostID,
	//		shelterID, m_shelterMap[shelterID]->m_doorState, m_shelterMap[shelterID]->m_lightState);
	//}


	return true;
}

#pragma endregion

#pragma region �ΰ��� :: ������ ���� -------------------------------------------------------------------------------

/**
 * @brief	������ ���� ��û
*/
DEFRMI_SpaceWar_RequestWorldCreateItem(Server)
{
	cout << "RequestWorldCreateItem  itemID : " << itemID << " x : " << pos.x << " y : " << pos.y << " z : " << pos.z << endl;
	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.find((HostID)hostID);

	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		auto newItem = make_shared<Item>(itemID,networkID);
		newItem->SetPosition(pos);
		newItem->SetRotation(rot);
		m_itemMap[networkID] = newItem;
	}

	if (iter == clientMap.end())
	{
		cout << "�߸��� ��û" << endl;
	}

	// ���� ��� �ܿ� ���� ��û ������
	for each(auto c in clientMap)
	{
		if (c.first != iter->first)
		{
			m_proxy.NotifyCreateItem(c.first, RmiContext::ReliableSend,
				(int)(iter->first), itemID, networkID, pos, rot);
		}
	}
	return true;
}

/**
 * @brief	������ ���� ��û
 * @detail	���� ������ Ŭ���̾�Ʈ�� ����� ���� ó��
*/
DEFRMI_SpaceWar_RequestItemDelete(Server)
{
	cout << "Request Item Delete " << networkID << endl;
	// p2p �� �ѷ�����, �������� ���� ���� ����

	m_proxy.NotifyDeleteItem(m_playerP2PGroup, RmiContext::ReliableSend, networkID);
	return true;
}

/**
 * @brief	���忡�� �������� �����Ǿ����� ����
 * @detail	���� �޸𸮿����� ����
*/
DEFRMI_SpaceWar_NotifyDeleteItem(Server)
{
	cout << "NotifyDeleteItem " << networkID << endl;

	auto iter = m_itemMap.find(networkID);

	if (iter == m_itemMap.end())
	{
		cout << "�߸��� ��û" << endl;
		return true;
	}
	{
		// �߰�����
		CriticalSectionLock lock(m_critSec, true);
		m_itemMap.erase(iter);
	}
	return true;
}
#pragma endregion

// ���׿��� �˸��� ���� //

#pragma region �ΰ��� :: ���ּ� ------------------------------------------------------------------------------------

/**
 * @brief	���ּ��� ����Ѵ�.
 * @detail	����ؾ� ���ּ��� ����� �� �ִ�.
*/
DEFRMI_SpaceWar_RequestSpaceShipSetup(Server)
{
	cout << "SpaceShip ID Setting " << spaceShipID << endl;
	CriticalSectionLock lock(m_critSec, true);
	
	for (int i = 0; i < spaceShipID; i++)
	{
		auto newSpaceShip = make_shared<SpaceShip>();
		m_spaceShipMap[i] = newSpaceShip;
	}

	return true;
};

/**
 * @brief	���ּ� ���Ḧ ���� �������� �� ����
 * @detail	�����̹� - ���ּ� Ż�� �¸�
*/
DEFRMI_SpaceWar_RequestSpaceShip(Server)
{
	if (m_spaceShipLockTime > 0)
		return true;
	cout << "��ģ�� ���ּ� ���� " << winPlayerID << endl;
	GetGameRoom()->GetClientMap()[(HostID)winPlayerID]->PlayerWin();
	return true;
}

/**
 * @brief	���ּ� ��� ��û
 * @detail	������� ���� �� ��� �����ϰ� ó��
*/
DEFRMI_SpaceWar_RequestUseSpaceShip(Server)
{
	// ����ִٸ� �ź�
	if (m_spaceShipMap[spaceShipID]->IsLock())
	{
		cout << "�������:: ��� �Ұ� " << endl;
		m_proxy.NotifyUseSpaceShipFailed(remote, RmiContext::ReliableSend, spaceShipID, m_spaceShipMap[spaceShipID]->GetTargetHostID());
	}
	else
	{
		cout << "������� ���� " << endl;
		// ��� �����ϴٸ� ���
		m_spaceShipMap[spaceShipID]->LockSpaceShip((int)remote);
		// ���� �޽��� ����
		m_proxy.NotifyUseSpaceShipSuccess(remote, RmiContext::ReliableSend, spaceShipID);
	}
	return true;
}

/**
 * @brief	���ּ� ��� ���
 * @detail	���ּ� ��� ����
*/
DEFRMI_SpaceWar_RequestUseSpaceShipCancel(Server)
{
	// ��� ����̹Ƿ� ����
	m_spaceShipMap[spaceShipID]->UnLockSpaceShip();
	cout << "�ߵ� ��� �̺�Ʈ " << endl;
	return true;
}
#pragma endregion 

#pragma region �ΰ��� :: ������ ------------------------------------------------------------------------------------

/**
 * @brief	�������� ���� �ִ� �༺ �ܰ踦 �˷���
 * @detail	Ŭ�󿡼� �����̰� - ������ �˷��ش�.
*/
DEFRMI_SpaceWar_RequestDeathZoneMoveIndex(Server)
{
	cout << "Death Zone Move Target Index " << moveIndex << endl;
	m_deathZoneIndex = moveIndex;

	// �ٲ� �뺸�� ���־����
	m_proxy.NotifyDeathZoneMoveHostAndIndexSetup(m_playerP2PGroup, 
		RmiContext::ReliableSend, m_deathZoneHostID, m_deathZoneIndex);

	return true;
};
#pragma endregion 
#pragma endregion

#pragma region ���â ==============================================================================================

/**
 * @brief	���ºν� ���� ��� ��û
*/
DEFRMI_SpaceWar_RequestDrawGameResult(Server)
{
	cout << "��ο� ��� ��û " << endl;
	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.begin();

	float playTime = m_netServer->GetTimeMs() - m_gameStartTime;

	while (iter != clientMap.end())
	{
		HostID targetID = iter->first;
		int winState = 2;

		m_proxy.NotifyGameResultInfoMe(targetID, 
			RmiContext::ReliableSend, "test", winState,
			playTime, iter->second->GetKillCount(),
			iter->second->GetAssistCount(), 
			iter->second->GetDeathCount(), 100);

		auto resultIter = clientMap.begin();
		while (resultIter != clientMap.end())
		{
			if (resultIter->first != iter->first)
			{
				cout << " ���� ������ " << endl;
				m_proxy.NotifyGameResultInfoOther(targetID, 
					RmiContext::ReliableSend,
					resultIter->second->GetName(), resultIter->second->GetState());
			}

			resultIter++;
		}
		m_proxy.NotifyGameResultShow(targetID, RmiContext::ReliableSend);

		iter++;
	}

	return true;
}

/**
 * @brief	�Ϲ����� �¸��� ���â ��û
 * @todo	���� ���� ( ��� )
*/
DEFRMI_SpaceWar_RequestGameEnd(Server)
{
	cout << "RequestGameEnd -- ���� ����  - " << endl;
	s_GameRunning = false;

	auto clientMap = GetGameRoom()->GetClientMap();
	// ���⼭ ���� �������

	if (clientMap[remote]->GetState() != SPACESHIP)
		return true;


	auto iter = clientMap.begin();

	float playTime = m_netServer->GetTimeMs() - m_gameStartTime;

	while (iter != clientMap.end())
	{

		HostID targetID = iter->first;

		int winState = (clientMap[targetID]->GetState() == SPACESHIP) ? 1 : 0;

		m_proxy.NotifyGameResultInfoMe(targetID, 
			RmiContext::ReliableSend, "test", winState,
			playTime, iter->second->GetKillCount(), 
			iter->second->GetAssistCount() , 
			iter->second->GetDeathCount(), 100);

		auto resultIter = clientMap.begin();
		while (resultIter != clientMap.end())
		{
			//	if (resultIter->first != iter->first)
			{
				cout << " ���� ������ " << endl;
				m_proxy.NotifyGameResultInfoOther(targetID, 
					RmiContext::ReliableSend,
					resultIter->second->GetName(), 
					resultIter->second->GetState());
			}

			resultIter++;
		}
		m_proxy.NotifyGameResultShow(targetID, RmiContext::ReliableSend);

		iter++;
	}



	return true;
}
#pragma endregion

//// TODO ���� ���� �������� ���� ���
//DEFRMI_SpaceWar_RequestClientJoin(Server)
//{
//	cout << "Server : RequestClientJoin "<<name << endl;
//	
//	auto iter = m_clientMap.find((HostID)hostID);
//	if (iter == m_clientMap.end())
//	{
//		cout << "�߸��� Ŭ���̾�Ʈ " << endl;
//		return true;
//	}
//	// ��ο��� ��Ÿ���� �ؾ��� 
//	for each (auto c in m_clientMap)
//	{
//		if (c.first != iter->first)
//		{
//			auto otherClient = c.second;
//			m_proxy.NotifyOtherClientJoin(c.first, 
//				RmiContext::ReliableSend,
//				iter->second->m_hostID,
//				iter->second->m_userName,
//				iter->second->x,
//				iter->second->y,
//				iter->second->z);
//
//			m_proxy.NotifyOtherClientJoin(iter->first,
//				RmiContext::ReliableSend,
//				c.second->m_hostID,
//				c.second->m_userName,
//				c.second->x,
//				c.second->y,
//				c.second->z);
//		}
//	}
//
//
//	return true;
//}

#pragma endregion