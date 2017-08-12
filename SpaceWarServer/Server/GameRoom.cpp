#include "stdafx.h"
#include "GameRoom.h"


GameRoom::GameRoom()
{
	m_currentPlayerCount = 0;
	m_limitPlayerCount = 10;
	m_gameMode = DEATH_MATCH;
	m_teamMode = false;
}

bool GameRoom::NewClientConnect(HostID hostID, string userName, CriticalSection& critSection)
{
	if (m_currentPlayerCount >= m_limitPlayerCount)
		return false;

	{
		// 추가로직
		CriticalSectionLock lock(critSection, true);
		auto newClient = make_shared<RoomClient>(hostID,userName);
		m_clientMap[hostID] = newClient;
		m_currentPlayerCount++;
	}
	return true;
}

void GameRoom::LogOutClient(HostID hostID)
{
	auto iter = m_clientMap.find(hostID);
	
	if (iter != m_clientMap.end())
	{
		m_clientMap.erase(iter);
		m_currentPlayerCount--;
	}
	cout << " 몇개야 ? " << m_currentPlayerCount << endl;
	
	if (m_currentPlayerCount <= 0)
	{
		cout << "다 나갔으니 클리어 " << endl;
		this->ClearRoom();
	}
}

bool GameRoom::IsRoomClient(HostID hostID)
{
	auto iter = m_clientMap.find(hostID);

	return  iter != m_clientMap.end();
}

void GameRoom::SetReady(HostID hostID, bool ready)
{
	m_clientMap[hostID]->SetReady(ready);
}

void GameRoom::TeamChange(HostID hostID, bool red)
{
	m_clientMap[hostID]->SetTeamColor(red);
}

bool GameRoom::GameStartCheck()
{
	bool gameStart = false;

	m_redTeamCount = m_blueTeamCount = 0;

	auto iter = m_clientMap.begin();
	while (iter != m_clientMap.end())
	{
		// 호스트가 아닐때
		if (!iter->second->IsHost())
		{
			gameStart = iter->second->IsReady();
		}

		if (iter->second->IsRedTeam())
			m_redTeamCount++;
		else
			m_blueTeamCount++;
		

		iter++;
	}

	if (gameStart == false)
		return false;

	if (m_teamMode)
		gameStart = (m_redTeamCount == m_blueTeamCount);

	return gameStart;
}

void GameRoom::ClearRoom()
{
	m_clientMap.clear();
	m_currentPlayerCount = 0;
	m_limitPlayerCount = 10;
	m_gameMode = DEATH_MATCH;
	m_teamMode = false;
}

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