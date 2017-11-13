#include "stdafx.h"
#include "Server.h"
Server server;
bool s_GameRunning = false;


/**
 * @brief 서버 프로그램의 시작 지점
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
 * @brief	Server 기본 생성자
 * @detail	GameRoom 생성 등의 기본 초기화 과정 수행
*/
Server::Server()
{
	m_gameRoom = make_shared <GameRoom>();
}



#pragma region Server Thread =======================================================================================
/**
 * @brief	서버스레드 :: 메테오, 데스존 관리
 * @detail	1초마다 호출됨
*/
void ServerThreadLoop(void*)
{
	if (s_GameRunning == false)
		return;
	// 우주선 잠금 해제 시간

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
				cout << "메테오 "<< i << "번 " << s_meteorCommingTime[i] << " 초 남음 " << server.GetServer()->GetTimeMs() << endl;
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

	// 여기는 데스존 체크
	if (server.GetDeathZoneCommingTime() == 0)
	{
		int index = (int)RandomRange(0, server.GetSpaceShipCount());
		string deathZoneID = "deathZone";
		deathZoneID += to_string(server.GetDeathZoneID());
		server.SetDeathZoneID(server.GetDeathZoneID() + 1);
		// 생성
		server.GetProxy()->NotifyDeathZoneCreate(server.GetP2PID(), 
			RmiContext::ReliableSend, index, deathZoneID);

		if (server.GetGameRoom() == nullptr)
			return;
		auto clientMap = server.GetGameRoom()->GetClientMap();

		// 움직이는 주체 설정
		auto iter = clientMap.begin();
		while (iter != clientMap.end())
		{
			if (iter->second->GetState() == ALIVE)
			{
				// 움직이는 주체 통보
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
	// 데스존이 움직이는 루프 
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
			// 움직이는 주체 설정
			auto iter = clientMap.begin();
			while (iter != clientMap.end())
			{
				if (iter->second->GetState() == ALIVE)
				{
					// 움직이는 주체 통보
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
 * @brief	Sever 기본 설정 파일 로드
 * @detail	server.properties 를 로드해 테이블 설정 등을 한다.
*/
void Server::ServerFileLoad()
{
	// 파일 읽기
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
 * @brief	Server 설정 파일을 토대로 테이블 값을 로드한다.
 * @todo	JSON 테스트 필요 ( 아직 미테스트 )
 * @detail	기본값을 로드해 세팅한다.
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
 * @brief	게임 서버 시작
 * @detail	서버 로직이 메인으로 돌아가는 곳.
*/
void Server::ServerRun()
{
	m_netServer = shared_ptr<CNetServer>(CNetServer::Create());
	// 서버 파일 로드
	ServerTableSetup();
	// 스레드 돌려라
	void(*func)(void*);
	func = &ServerThreadLoop;
	CTimerThread serverThread(func, 1000, nullptr);
	// 서버 스레드 시작 
	serverThread.Start();

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
	catch (Proud::Exception &e)
	{
		cout << "Server Start Failed : " << e.what() << endl;
		return;
	}

	cout << "Time For Escape :: Server 20171022 Build -------------------------" << endl;
	
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
	m_netServer->OnException = [](const Proud::Exception &e) {
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
	serverThread.Stop();
}

/**
 * @brief	클라이언트가 접속했을 때
 * @detail	p2p 그룹에 넣어준다.
*/
void Server::OnClientJoin(CNetClientInfo* clientInfo)
{
	cout << "OnClientJoin " << clientInfo->m_HostID << endl;
	
	m_netServer->JoinP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);
}

/**
 * @brief	클라이언트가 나갔을 때
 * @detail	따로 호출할 필요가 없이 프라우드 엔진이 호출해준다.
*/
void Server::OnClientLeave(CNetClientInfo* clientInfo, ErrorInfo* errorInfo, const ByteArray& comment)
{
	cout << "OnClientLeave " << clientInfo->m_HostID << endl;

	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.find((clientInfo->m_HostID));
	m_netServer->LeaveP2PGroup(clientInfo->m_HostID, m_playerP2PGroup);

	if (iter == clientMap.end())
	{
		cout << "로그인하지 않은 클라이언트가 나갔습니다. " << clientInfo->m_HostID << endl;

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
 * @brief	서버 리셋
 * @detail	게임을 진행할 필요가 없을 경우 호출
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

	// 파일입출력 전
	for (int i = 0; i < 10; i++)
		s_meteorCommingTime[i] = 30 + (i * 20);
}
#pragma region Get / Set Method ------------------------------------------------------------------------------------
/**
 * @brief	서버 객체를 얻는다.
 * @return	서버 객체 리턴
*/
shared_ptr<CNetServer> Server::GetServer()
{
	return m_netServer;
}
/**
 * @brief	Proxy 객체를 얻어온다.
*/
SpaceWar::Proxy* Server::GetProxy()
{
	return &m_proxy;
}

/**
 * @brief	P2P그룹의 HostID 를 얻는다.
 * @return	P2P그룹의 HostID 를 리턴
*/
HostID Server::GetP2PID()
{
	return m_playerP2PGroup;
}

/**
 * @brief	GameRoom 얻기
 * @return	GameRoom 을 리턴한다.
*/
shared_ptr<GameRoom> Server::GetGameRoom()
{
	return m_gameRoom;
}

/**
 * @brief	우주선의 초기 잠금 시간값을 세팅한다.
 * @param	lockTime 잠금 시간의 값 
*/
void Server::SetSpaceShipLockTime(int lockTime)
{
	m_spaceShipLockTime = lockTime;
}

/**
 * @brief	우주선 초기 잠금 시간값을 얻어온다.
 * @return	우주선 초기 잠금 시간값을 리턴
*/
int Server::GetSpaceShipLockTime()
{
	return m_spaceShipLockTime;
}

/**
 * @brief	데스존까지 남은 시간을 세팅한다.
 * @param	sec 데스존 까지 남은 시간
*/
void Server::SetDeathZoneCommingTime(int sec)
{
	m_deathZoneCommingSec = sec;
}

/**
 * @brief	데스존까지 남은 시간을 얻어온다.
 * @return	데스존까지 남은 시간을 리턴
*/
int Server::GetDeathZoneCommingTime()
{
	return m_deathZoneCommingSec;
}

/**
 * @brief	DeathZone 현재 진행중인 인덱스 세팅
 * @param	curIndex 현재 진행중인 데스존 인덱스
*/
void Server::SetDeathZoneIndex(int curIndex)
{
	m_deathZoneIndex = curIndex;
}

/**
 * @brief	현재 진행중인 DeathZone 인덱스 얻기
 * @return	현재 진행중인 DeathZone 인덱스 반환
*/
int Server::GetDeathZoneIndex()
{
	return m_deathZoneIndex;
}

/**
 * @brief	DeathZone을 진행시키고 있는 클라이언트
 * @param	DeathZone을 진행시키고 있는 클라이언트의 HostID
*/
void Server::SetDeathZoneHostID(int hostID)
{
	m_deathZoneHostID = hostID;
}

/**
 * @brief	DeathZone을 진행시키고 있는 클라이언트의 HostID를 얻는다.
 * @return	DeathZone을 진행시키고 있는 클라이언트의 HostID.
*/
int Server::GetDeathZoneHostID()
{
	return m_deathZoneHostID;
}

/**
 * @brief	DeathZone의 네트워크 식별 아이디를 설정
 * @param	식별 아이디
*/
void Server::SetDeathZoneID(int id)
{
	m_deathZoneID = id;
}

/**
 * @brief	DeathZone 의 식별 아이디를 얻는다.
 * @return	DeathZone의 식별 아이디를 리턴 -1일 경우 미설정
*/
int Server::GetDeathZoneID()
{
	return m_deathZoneID;
}

/**
* @brief	Meteor의 네트워크 식별 아이디를 설정
* @param	식별 아이디
*/
void Server::SetMeteorID(int id)
{
	m_meteorID = id;
}

/**
* @brief	Meteor 의 식별 아이디를 얻는다.
* @return	Meteor의 식별 아이디를 리턴 -1일 경우 미설정
*/
int Server::GetMeteorID()
{
	return m_meteorID;
}
#pragma endregion --------------------------------------------------------------------------------------------------
#pragma endregion

#pragma region C2S Method ==========================================================================================

#pragma region 초기 접속 ===========================================================================================
#pragma region 공통 구간 -------------------------------------------------------------------------------------------
/**
 * @brief	서버 연결 요청
 * @detail	게임 대기실 참가 요청
*/
DEFRMI_SpaceWar_RequestServerConnect(Server)
{
	cout << "C2S-- RequestServerConnect " << id << " remote " << remote << endl;

	//게임 룸 입장
	if (m_gameRoom != nullptr)
	{
		if (m_gameRoom->NewClientConnect(remote, id, m_critSec) == false)
		{
			cout << "Login Failed .. 더 이상 참여할 수 없습니다. " << endl;
			m_proxy.NotifyLoginFailed(remote, RmiContext::ReliableSend, "더 이상 참여할 수 없습니다.");
			return true;
		}
	}


	int count = GetGameRoom()->GetClientMap().size();
	cout << "Server : LoginSuccess "<< count << endl;
	

	m_proxy.NotifyLoginSuccess(remote, RmiContext::ReliableSend,(int)remote,(count == 1));
	return true;
}

/**
 * @brief	팀 변경 요청
 * @detail	서버에서 변경 후 해당 정보 브로드캐스팅
*/
DEFRMI_SpaceWar_RequestNetworkGameTeamSelect(Server)
{
	cout << "Team 변경 " << endl;
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
 * @brief 정상 종료 요청
*/
DEFRMI_SpaceWar_RequestGameExit(Server)
{
	ServerReset();
	return true;
}
#pragma endregion

#pragma region 호스트 아닌 유저 ------------------------------------------------------------------------------------
/**
 * @brief	레디 변경 요청
 * @detail	서버에서 요청을 받고 해당 정보 브로드캐스팅
*/
DEFRMI_SpaceWar_RequestNetworkGameReady(Server)
{
	cout << " Ready 변경 " << endl;

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
 * @brief	로비씬에 들어왔다.
 * @detail	기존에 들어와있는 유저의 정보들을 보내준다. / 내 자신의 정보도 날린다.
*/
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
		// 유저 정보를 날린다.
		m_proxy.NotifyNetworkUserSetup(remote, RmiContext::ReliableSend, (int)iter->GetHostID(), iter->GetName(), iter->IsReady(), iter->IsRedTeam());

		// 이 유저가 들어왔음 또한 알려준다.
		if (remote != iter->GetHostID())
			m_proxy.NotifyNetworkConnectUser(iter->GetHostID(), RmiContext::ReliableSend, remote, m_gameRoom->GetClient(remote)->GetName());
		iter++;
	}
	return true;
}

/**
 * @brief	게임 씬 로딩 완료 메시지
 * @detail	전부 접속시 접속 정보들( 초기 좌표 등 )을 보낸다.
*/
DEFRMI_SpaceWar_RequestGameSceneJoin(Server)
{
	shared_ptr<RoomClient> client = m_gameRoom->GetClient(remote);
	// 게임 씬에 들어왔다.
	client->SetGameScene(true);
	client->SetPosition(pos);

	// 모든 클라이언트가 준비되었는지 확인 

	cout << "Game Scene Join :: " << m_gameRoom->IsGameSceneAllReady() << endl;
	if (m_gameRoom->IsGameSceneAllReady())
	{
		// 이 경우 이제 모든 클라에게 접속 정보를 보낸다.
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
				// 접속 정보도 날린다.
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
						item.second->GetItemID(), item.second->GetNetworkID(), item.second->GetPosition(), item.second->GetRotation());
				}
			}


			iter++;
		}
	}


	return true;
}
#pragma endregion

#pragma region 방장 호스트 전용 ------------------------------------------------------------------------------------
/**
 * @brief	방장이 맵을 바꿨다.
 * @detail	맵이 바뀐 것을 브로드캐스팅 해준다.
*/
DEFRMI_SpaceWar_RequestNetworkChangeMap(Server)
{
	cout << " Map Change :: " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// 바뀐 맵 정보 전송
		m_proxy.NotifyNetworkGameChangeMap(*iter, RmiContext::ReliableSend, mapName);
		iter++;
	}

	return true;
}

/**
 * @brief	플레이 가능한 플레이어 수를 변경했다.
 * @detail	플레이어 수 제한을 브로드캐스팅한다.
*/
DEFRMI_SpaceWar_RequestNetworkPlayerCount(Server)
{
	cout << " Player Count 변경 " << playerCount << endl;

	m_gameRoom->SetPlayerLimitCount(playerCount);
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// 플레이어 카운트 정보 전송
		m_proxy.NotifyNetworkGamePlayerCountChange(*iter, RmiContext::ReliableSend, playerCount);
		iter++;
	}
	return true;
}

/**
 * @brief	게임 모드 변경
 * @detail	게임 모드 변경 사항을 브로드캐스팅
 * @todo	추후 데스매치 모드 관련 로직 추가
*/
DEFRMI_SpaceWar_RequestNetworkGameModeChange(Server)
{
	cout << " Game Mode 변경 " << endl;

	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	m_gameRoom->SetGameMode(gameMode);
	m_gameRoom->SetTeamMode(teamMode);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// 게임 모드 전송
		m_proxy.NotifyNetworkGameModeChange(*iter, RmiContext::ReliableSend, gameMode, teamMode);
		iter++;
	}
	return true;
}

/**
 * @brief	게임 스타트 요청
 * @detail	게임 시작.
*/
DEFRMI_SpaceWar_RequestNetworkGameStart(Server)
{
	cout << " Game Start 변경 " << endl;
	m_itemMap.clear();
	// 게임을 시작할 수 있는 환경인지 체크
	//if (!m_gameRoom->GameStartCheck())
	//{
	//	//실패
	//	m_proxy.NotifyNetworkGameStartFailed(m_playerP2PGroup, RmiContext::ReliableSend);
	//	return true;
	//	
	//}

	// 게임 시간 기록을 위해 시작 시간을 저장
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
 * @brief	방장이 나갔다.
 * @detail	다들 타이틀로 튕겨져 나가야 하므로 해당 메시지 브로드캐스트
*/
DEFRMI_SpaceWar_RequestNetworkHostOut(Server)
{
	forward_list<HostID> list = m_gameRoom->GetOtherClients(remote);

	auto iter = list.begin();
	while (iter != list.end())
	{
		// 방장이 나갔음을 알림
		m_proxy.NotifyNetworkGameHostOut(*iter, RmiContext::ReliableSend);
		iter++;
	}


	ServerReset();
	cout << "호스트가 나갔으므로 게임 룸 클리어 " << endl;

	return true;
}


#pragma endregion
#pragma endregion

#pragma region 인게임 ==============================================================================================
#pragma region 인게임 :: 플레이어 메시지 ---------------------------------------------------------------------------
#pragma region [플레이어 상태 관련 정보] 

/**
 * @brief	체력 변경 요청
 * @detail	데미지를 제외한 체력 변경 요청
*/
DEFRMI_SpaceWar_RequestHpUpdate(Server)
{
	cout << "HP Update 요청 " << endl;

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
 * @brief	데미지를 입었다.
 * @detail	죽음 / 어시스트 등에 대한 처리
*/
DEFRMI_SpaceWar_RequestPlayerDamage(Server)
{
	auto clientMap = GetGameRoom()->GetClientMap();
	cout << "Request Player Damage 때린놈 " << sendHostID << " 맞은놈 " << targetHostID << " 데미지 " << damage << " 무기 이름 " << weaponName << endl;
	cout << "현재 총인원 " << clientMap.size() << endl;

	if (clientMap[(HostID)targetHostID]->GetState() == DEATH)
		return true;

	float prevHp = clientMap[(HostID)targetHostID]->GetHp();
	clientMap[(HostID)targetHostID]->SetHp(prevHp - damage);

	if (clientMap[(HostID)targetHostID]->GetHp() <= 0.0f)
	{
		cout << " 죽었다 " + targetHostID << endl;
		clientMap[(HostID)targetHostID]->SetHp(0.0f);
		clientMap[(HostID)targetHostID]->PlayerDead(m_netServer->GetTimeMs());

		// 죽은 후에 어시스트 목록 갱신
		forward_list<int> list = clientMap[(HostID)targetHostID]->GetAssistClientList();
		for (auto value : list)
		{
			//킬한놈
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

		// 이 부분에서 다 죽었는지 체크
		int deadCheck = 0;
		auto iter = clientMap.begin();
		while (iter != clientMap.end())
		{
			deadCheck += (iter->second->GetState() == DEATH) ? 0 : 1;
			iter++;
		}

		// 다죽음 - 즉 드로우 게임 
		if (deadCheck == 0)
		{
			iter = clientMap.begin();
			while (iter != clientMap.end())
			{
				// 드로우 게임임을 날린다
				m_proxy.NotifyDrawGame(iter->second->GetHostID(), RmiContext::ReliableSend);
				iter++;
			}
		}
	}
	else
	{
		// 이녀석은 어시스트 체크를 해야함
		if (targetHostID != sendHostID)
			clientMap[(HostID)targetHostID]->DamageClient((int)sendHostID, m_netServer->GetTimeMs());
	}


	m_proxy.NotifyPlayerChangeHP(m_playerP2PGroup, RmiContext::ReliableSend, targetHostID, weaponName, clientMap[(HostID)targetHostID]->GetHp(), prevHp, MAX_HP, dir);
	return true;
}

/**
 * @brief	산소를 사용했다.
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
 * @brief	플레이어가 이동했음을 알림
*/
DEFRMI_SpaceWar_NotifyPlayerMove(Server)
{
	auto clientMap = GetGameRoom()->GetClientMap();
	clientMap[(HostID)hostID]->SetPosition(curX, curY, curZ);

	m_gameRoom->GetClient((HostID)hostID)->SetPosition(curX, curY, curZ);
	return true;
}

/**
 * @brief	플레이어가 아이템을 장비했다.
*/
DEFRMI_SpaceWar_NotifyPlayerEquipItem(Server)
{
	cout << "NotifyPlayerEquipItem " << hostID << " item " << itemID << endl;
	return true;
}
#pragma endregion

#pragma endregion
#pragma region 인게임 :: 산소 충전기 -------------------------------------------------------------------------------

/**
 * @brief	게임 내 산소 충전기를 등록한다.
 * @detail	등록하고부터 산소 충전기를 사용할 수 있다.
*/
DEFRMI_SpaceWar_RequestOxyChargerStartSetup(Server)
{
	CriticalSectionLock lock(m_critSec, true);
	auto newOxyCharger = make_shared<OxyCharger>();
	m_oxyChargerMap[oxyChargerID] = newOxyCharger;
	return true;
}

/**
 * @brief	산소 충전기 조작 요청
 * @detail	잠겨있는지 체크 후 알맞게 처리
*/
DEFRMI_SpaceWar_RequestUseOxyChargerStart(Server)
{
	// 잠겨있다면 거부
	if (m_oxyChargerMap[oxyChargerIndex]->IsLocked())
	{
		m_proxy.NotifyUseFailedOxyCharger(remote, RmiContext::ReliableSend, 
			m_oxyChargerMap[oxyChargerIndex]->GetUseHostID(), oxyChargerIndex);
	}
	// 잠겨있지 않다면 허용
	else
	{
		//잠근다.
		m_oxyChargerMap[oxyChargerIndex]->LockOxyCharger((int)remote);
		m_proxy.NotifyUseSuccessedOxyCharger(remote, 
			RmiContext::ReliableSend, (int)remote, oxyChargerIndex);
	}
	return true;
}

/**
 * @brief	산소 충전기 조작중 
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
 * @brief	산소 충전기 사용 끝
 * @detail	잠금 해제 및 사용해제로 되돌림
*/
DEFRMI_SpaceWar_RequestUseOxyChargerEnd(Server)
{
	cout << "Request Use Oxy Charger End " << oxyChargerIndex << endl;
	if (m_oxyChargerMap[oxyChargerIndex]->GetUseHostID() == (int)remote)
		m_oxyChargerMap[oxyChargerIndex]->UnLockOxyCharger();
	return true;
}

#pragma endregion

#pragma region 인게임 :: 아이템 박스 조작 --------------------------------------------------------------------------
/**
 * @brief	아이템 박스 사용 요청
 * @detail	랜덤으로 아이템 코드 생성해 브로드캐스팅
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
		// 첫 사용
		m_itemBoxMap[itemBoxIndex] = sendHostID;
		cout << "item Code " << itemID << endl;
		string networkID = "server_" + itemBoxIndex;
		networkID += "_" + sendHostID;

		m_proxy.NotifyUseItemBox(m_playerP2PGroup, RmiContext::ReliableSend,
			sendHostID, itemBoxIndex, itemID, networkID);
		return true;
	}

	// 이미 사용중
	return true;
}
#pragma endregion

#pragma region 인게임 :: 쉘터 조작 ---------------------------------------------------------------------------------

/**
 * @brief	쉘터 등록
 * @detail	등록하고부터 쉘터를 사용할 수 있다.
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
 * @brief	쉘터 문 조작
 * @detail	쉘터 문을 조작 정보를 받고 반영한 뒤 브로드캐스팅
*/
DEFRMI_SpaceWar_RequestShelterDoorControl(Server)
{
	cout << "RequestShelterDoorControl " << shelterID << " state " << doorState << endl;

	m_shelterMap[shelterID]->ShelterDoorStateChange(doorState);

	//// p2p 로 수정 TODO
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
 * @brief	쉘터에 진입했다.
 * @detail	조명을 끄고 켜고 하는 처리를 수행
*/
DEFRMI_SpaceWar_RequestShelterEnter(Server)
{
	cout << "RequestShelterEnter " << sendHostID << endl;

	// 기존 상태 
	bool prevState = m_shelterMap[shelterID]->IsLightOn();
	bool prevDoorState = m_shelterMap[shelterID]->IsDoorOpen();
	if (enter)
		m_shelterMap[shelterID]->ShelterEnter();
	else
		m_shelterMap[shelterID]->ShelterExit();

	//// 기존 상태와 달라졌을 경우
	//if (m_shelterMap[shelterID]->m_lightState != prevState ||
	//	m_shelterMap[shelterID]->m_doorState != prevDoorState)
	//{
	//	// 상태 전송
	//	m_proxy.NotifyShelterInfo(m_playerP2PGroup,
	//		RmiContext::ReliableSend, sendHostID,
	//		shelterID, m_shelterMap[shelterID]->m_doorState, m_shelterMap[shelterID]->m_lightState);
	//}


	return true;
}

#pragma endregion

#pragma region 인게임 :: 아이템 로직 -------------------------------------------------------------------------------

/**
 * @brief	아이템 생성 요청
*/
DEFRMI_SpaceWar_RequestWorldCreateItem(Server)
{
	cout << "RequestWorldCreateItem  itemID : " << itemID << " x : " << pos.x << " y : " << pos.y << " z : " << pos.z << endl;
	auto clientMap = GetGameRoom()->GetClientMap();
	auto iter = clientMap.find((HostID)hostID);

	{
		// 추가로직
		CriticalSectionLock lock(m_critSec, true);
		auto newItem = make_shared<Item>(itemID,networkID);
		newItem->SetPosition(pos);
		newItem->SetRotation(rot);
		m_itemMap[networkID] = newItem;
	}

	if (iter == clientMap.end())
	{
		cout << "잘못된 요청" << endl;
	}

	// 보낸 사람 외에 생성 요청 보내기
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
 * @brief	아이템 삭제 요청
 * @detail	실제 삭제는 클라이언트에 명령을 보내 처리
*/
DEFRMI_SpaceWar_RequestItemDelete(Server)
{
	cout << "Request Item Delete " << networkID << endl;
	// p2p 로 뿌려지고, 그제서야 실제 삭제 로직

	m_proxy.NotifyDeleteItem(m_playerP2PGroup, RmiContext::ReliableSend, networkID);
	return true;
}

/**
 * @brief	월드에서 아이템이 삭제되었음을 받음
 * @detail	실제 메모리에서도 삭제
*/
DEFRMI_SpaceWar_NotifyDeleteItem(Server)
{
	cout << "NotifyDeleteItem " << networkID << endl;

	auto iter = m_itemMap.find(networkID);

	if (iter == m_itemMap.end())
	{
		cout << "잘못된 요청" << endl;
		return true;
	}
	{
		// 추가로직
		CriticalSectionLock lock(m_critSec, true);
		m_itemMap.erase(iter);
	}
	return true;
}
#pragma endregion

// 메테오는 알림만 날림 //

#pragma region 인게임 :: 우주선 ------------------------------------------------------------------------------------

/**
 * @brief	우주선을 등록한다.
 * @detail	등록해야 우주선을 사용할 수 있다.
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
 * @brief	우주선 연료를 전부 충전했을 때 보냄
 * @detail	서바이벌 - 우주선 탈출 승리
*/
DEFRMI_SpaceWar_RequestSpaceShip(Server)
{
	if (m_spaceShipLockTime > 0)
		return true;
	cout << "이친구 우주선 탔다 " << winPlayerID << endl;
	GetGameRoom()->GetClientMap()[(HostID)winPlayerID]->PlayerWin();
	return true;
}

/**
 * @brief	우주선 사용 요청
 * @detail	잠겨있지 않을 때 사용 가능하게 처리
*/
DEFRMI_SpaceWar_RequestUseSpaceShip(Server)
{
	// 잠겨있다면 거부
	if (m_spaceShipMap[spaceShipID]->IsLock())
	{
		cout << "잠겨있음:: 사용 불가 " << endl;
		m_proxy.NotifyUseSpaceShipFailed(remote, RmiContext::ReliableSend, spaceShipID, m_spaceShipMap[spaceShipID]->GetTargetHostID());
	}
	else
	{
		cout << "잠겨있지 않음 " << endl;
		// 사용 가능하다면 잠금
		m_spaceShipMap[spaceShipID]->LockSpaceShip((int)remote);
		// 성공 메시지 보냄
		m_proxy.NotifyUseSpaceShipSuccess(remote, RmiContext::ReliableSend, spaceShipID);
	}
	return true;
}

/**
 * @brief	우주선 사용 취소
 * @detail	우주선 잠금 해제
*/
DEFRMI_SpaceWar_RequestUseSpaceShipCancel(Server)
{
	// 사용 취소이므로 해제
	m_spaceShipMap[spaceShipID]->UnLockSpaceShip();
	cout << "중도 취소 이벤트 " << endl;
	return true;
}
#pragma endregion 

#pragma region 인게임 :: 데스존 ------------------------------------------------------------------------------------

/**
 * @brief	데스존이 가고 있는 행성 단계를 알려줌
 * @detail	클라에서 움직이고 - 서버에 알려준다.
*/
DEFRMI_SpaceWar_RequestDeathZoneMoveIndex(Server)
{
	cout << "Death Zone Move Target Index " << moveIndex << endl;
	m_deathZoneIndex = moveIndex;

	// 바뀔때 통보도 해주어야함
	m_proxy.NotifyDeathZoneMoveHostAndIndexSetup(m_playerP2PGroup, 
		RmiContext::ReliableSend, m_deathZoneHostID, m_deathZoneIndex);

	return true;
};
#pragma endregion 
#pragma endregion

#pragma region 결과창 ==============================================================================================

/**
 * @brief	무승부시 게임 결과 요청
*/
DEFRMI_SpaceWar_RequestDrawGameResult(Server)
{
	cout << "드로우 결과 요청 " << endl;
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
				cout << " 정보 보내기 " << endl;
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
 * @brief	일반적인 승리시 결과창 요청
 * @todo	버그 존재 ( 결과 )
*/
DEFRMI_SpaceWar_RequestGameEnd(Server)
{
	cout << "RequestGameEnd -- 게임 종료  - " << endl;
	s_GameRunning = false;

	auto clientMap = GetGameRoom()->GetClientMap();
	// 여기서 문제 생겼었음

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
				cout << " 정보 보내기 " << endl;
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

//// TODO 서버 구조 변경으로 삭제 대상
//DEFRMI_SpaceWar_RequestClientJoin(Server)
//{
//	cout << "Server : RequestClientJoin "<<name << endl;
//	
//	auto iter = m_clientMap.find((HostID)hostID);
//	if (iter == m_clientMap.end())
//	{
//		cout << "잘못된 클라이언트 " << endl;
//		return true;
//	}
//	// 모두에게 나타나게 해야함 
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