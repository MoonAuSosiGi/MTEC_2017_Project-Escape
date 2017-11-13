#include "stdafx.h"
#include "GameRoom.h"

/**
 * @brief	�⺻ ������ (�ʱ�ȭ)
*/
GameRoom::GameRoom()
{
	m_limitPlayerCount = 10;
	m_gameMode = DEATH_MATCH;
	m_teamMode = false;
}

/**
 * @brief	Ŭ���̾�Ʈ �� ���
 * @return	Ŭ���̾�Ʈ ���� ����
*/
unordered_map<HostID, shared_ptr<RoomClient>> GameRoom::GetClientMap()
{
	return m_clientMap;
}

#pragma region Room Setting Method =======================================================================================
/** 
 * @brief	�÷��̾��� �� ������ �δ� �޼���
 * @param	count ������ �� ��
*/
void GameRoom::SetPlayerLimitCount(int count)
{
	m_limitPlayerCount = count;
}

/**
 * @brief	���� �÷��̾� ���� �� ��������
 * @return	���� ������ �÷��̾��� ���� �����Ѵ�.
*/
int GameRoom::GetPlayerLimitCount()
{
	return m_limitPlayerCount;
}

/**
 * @brief	���� ��� ����
 * @param	gameMode ������ ���� ���
*/
void GameRoom::SetGameMode(int gameMode)
{
	m_gameMode = gameMode;
}

/**
 * @brief	�� ��� ����
 * @param	teamMode  true �� ���, false ������ ���
*/
void GameRoom::SetTeamMode(bool teamMode)
{
	m_teamMode = teamMode;
}

/**
 * @brief	�� ��� ���
 * @return	�� ��������� �˾ƿ� �� �ִ�.
*/
bool GameRoom::GetTeamMode() 
{ 
	return m_teamMode; 
}

/**
 * @brief	���� ��� ���
 * @return	���� ��������� ���� �� �ִ�.
*/
int GameRoom::GetGameMode() 
{ 
	return m_gameMode; 
}

/**
 * @brief	���� �� Ŭ����
 * @detail	������ ����ǰų� / ������ �� �����ų� �� ��� ������ �ʱ�ȭ�Ѵ�.
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
 * @brief	���ο� Ŭ���̾�Ʈ�� �����ߴ�.
 * @detail	���ο� Ŭ���̾�Ʈ�� ���� �� �ʿ� �߰��ϴ� ����
 * @param	hostID ������ Ŭ���̾�Ʈ�� HostID
 * @param	userName ������ Ŭ���̾�Ʈ�� ���� �̸�
 * @param	critSection ũ��Ƽ�� ���� / �߰� ������ ����ϴ� ����
 * @return	���������� ������ ����Ǿ������� ���� ������ ����. true �� ���� �߰�, false �� ���� �Ұ�
*/
bool GameRoom::NewClientConnect(HostID hostID, string userName, CriticalSection& critSection)
{
	// ���� �����ο��� ���� ���� ���� �ʰ��ߴ���
	if (m_clientMap.size() >= m_limitPlayerCount)
		return false;

	{
		// Ŭ���̾�Ʈ�� map �� �߰��ϱ� ���� ũ��Ƽ�� ������ ���
		CriticalSectionLock lock(critSection, true);
		// Ŭ���̾�Ʈ ���� �� �ʿ� �߰�
		auto newClient = make_shared<RoomClient>(hostID, userName);
		m_clientMap[hostID] = newClient;
		// ���� �÷��̾� ���� ������Ŵ
		m_currentPlayerCount++;
	}
	return true;
}

/**
 * @brief	Ŭ���̾�Ʈ�� ������ ������.
 * @detail	Ŭ���̾�Ʈ�� ������ ���� �ش� Ŭ���̾�Ʈ ��ü�� �����ϴ� ����
 * @param	hostID ������ ���� Ŭ���̾�Ʈ�� HostID
*/
void GameRoom::LogOutClient(HostID hostID)
{
	// ���� Ŭ���̾�Ʈ�� Ŭ���̾�Ʈ �ʿ� �ִ��� üũ
	auto iter = m_clientMap.find(hostID);

	if (iter != m_clientMap.end())
	{
		// �ش� Ŭ���̾�Ʈ�� �ִٸ� ���������� ����
		m_clientMap.erase(iter);
	}
	
	// �� ������ ��� Ŭ����
	if (m_clientMap.size() <= 0)
	{
		cout << "Server :: ������ Ŭ���̾�Ʈ�� ���� 0�� �Ǿ����ϴ�. " << endl;
		this->ClearRoom();
	}
}

/**
 * @brief	Ŭ���̾�Ʈ�� �濡 �������ִ���
 * @param	hostID �˻��� Ŭ���̾�Ʈ�� HostID
 * @return	�� ���� ���� ����
*/
bool GameRoom::IsRoomClient(HostID hostID)
{
	auto iter = m_clientMap.find(hostID);

	return  iter != m_clientMap.end();
}

/**
 * @brief	Ŭ���̾�Ʈ�� ���� ���� ����
 * @param	hostID ���� ������ ������ HostID
 * @param	ready ���õ� ���� ����
*/
void GameRoom::SetReady(HostID hostID, bool ready)
{
	m_clientMap[hostID]->SetReady(ready);
}

/**
 * @brief	�� ���� ���� 
 * @param	hostID �� ������ �� Ŭ���̾�Ʈ�� HostID
 * @param	red ���� ��/û �ۿ� �����Ƿ� ��������?
*/
void GameRoom::TeamChange(HostID hostID, bool red)
{
	m_clientMap[hostID]->SetTeamColor(red);
}
#pragma endregion =========================================================================================================

#pragma region Game Method ================================================================================================
/**
 * @brief	���� ��ŸƮ ���� ����
 * @detail	���� ��ŸƮ ���� ���� : ���� �� �����ִ��� / ������ ��� �뷱���� �´���
 * @return	true ���Ͻ� ���� ���� ���� / false ���Ͻ� ���� ���� �Ұ�
*/
bool GameRoom::GameStartCheck()
{
	// ���� ���� ���� ���Ͽ�
	bool gameStart = true;

	// ���� û�� ī��Ʈ��
	int redTeam,  blueTeam;
	redTeam = blueTeam = 0;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		// ȣ��Ʈ�� �����̹Ƿ�, ȣ��Ʈ�� �ƴ� ���� ���� üũ
		if (!iter->second->IsHost())
		{
			gameStart = iter->second->IsReady();

			// �Ѹ��̶� ���� ���� �ʾ����� �� üũ�� �ʿ䰡 ����
			if (gameStart == false)
				return false;
		}

		// ������ ī��Ʈ�ϱ�
		if (iter->second->IsRedTeam())
			redTeam++;
		else
			blueTeam++;


		iter++;
	}


	// �� ����� ��� �뷱�� �´��� üũ
	if (m_teamMode)
		gameStart = (redTeam == blueTeam);

	return gameStart;
}

/**
 * @brief	��� ���Ӿ��� �Դ��� üũ
 * @detail	��� ���Ӿ��� �Դ��� üũ �� , ���� ������ ��� / �����ۿ� ���� ���� ���� �����Ѵ�.
*/
bool GameRoom::IsGameSceneAllReady()
{
	auto iter = m_clientMap.begin();

	while (iter != m_clientMap.end())
	{
		bool ready = iter->second->IsGameScene();

		// �ϳ��� �Ҹ����� �غ� �ȵǾ�����
		if (!ready)
			return false;

		iter++;
	}

	return true;
}

/**
 * @brief	���� ������������ ���´�.
 * @return	���� �������� ��� true �� ����
*/
bool GameRoom::IsGameRunning()
{
	return m_isGameRunning;
}

/**
 * @brief	���� ���� �������� �����Ѵ�.
 * @param	running ���� �������� ���� ����
*/
void GameRoom::SetGameRunning(bool running)
{
	m_isGameRunning = running;
}

/**
 * @brief	Ư�� Ŭ�� ������ HostID ����Ʈ ���
 * @param	hostID ������ Ŭ���̾�Ʈ�� HostID
 * @return	�ش� Ŭ���̾�Ʈ�� ������ HostID ����Ʈ�� ����
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
 * @brief	Ư�� Ŭ�� ������ ����Ʈ ���
 * @detail	GetOtherClients(HostID) �ʹ� �ٸ��� RoomClient ��ü ������ ���� �� �ִ�.
 * @param	hostID ������ Ŭ���̾�Ʈ�� HostID
 * @return	�ش� Ŭ���̾�Ʈ�� ������ ����Ʈ�� ����
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
 * @brief	�������ִ� ��� Ŭ���̾�Ʈ�� HostID ���
 * @detail	��� Ŭ���̾�Ʈ�� HostID �� �ʿ��� �� ���
 * @return	��� Ŭ���̾�Ʈ�� HostID ����Ʈ
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
 * @brief	Ư�� Ŭ���̾�Ʈ�� ������ ���´�.
 * @detail	Ư�� Ŭ���̾�Ʈ �ϳ��� RoomClient ������ ��´�.
 * @param	hostID ������ ���� Ŭ���̾�Ʈ�� HostID
 * @return	Ư�� Ŭ���̾�Ʈ�� ������ RoomClient ���·� �޾ƿ´�. nullptr �� �� �ִ�.
*/
shared_ptr<RoomClient> GameRoom::GetClient(HostID hostID)
{ 
	// �ʿ� ���� ��ϵ� hostID ���� üũ �� ����
	if (IsRoomClient(hostID))
		return m_clientMap[hostID];
	else
		return nullptr;
}

#pragma endregion =========================================================================================================







