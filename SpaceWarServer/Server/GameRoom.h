#ifndef GameRoom_H
#define GameRoom_H

enum GameMode
{
	DEATH_MATCH = 100,
	SURVIVAL
};

class RoomClient
{
private:
	HostID m_hostID;
	string m_userName;
	bool m_isLobby;
	bool m_redTeam;
	bool m_isReady;
	bool m_isHost;
public:
	RoomClient(HostID hostID, string userName,bool host = false)
	{
		m_hostID = hostID;
		m_userName = userName;
		m_redTeam = true;
		m_isReady = false;
		m_isHost = host;
		m_isLobby = false;
	}

	void SetTeamColor(bool red)
	{
		m_redTeam = red;
	}
	
	void SetReady(bool ready)
	{
		m_isReady = ready;
	}

	void SetHost(bool host)
	{
		m_isHost = host;
	}

	void SetLobby(bool lobby)
	{
		m_isLobby = lobby;
	}

	bool IsHost() { return m_isHost; }
	bool IsReady() { return m_isReady; }
	bool IsRedTeam() { return m_redTeam; }
	bool IsLobby() { return m_isLobby; }
	HostID GetHostID() { return m_hostID; }
	string GetName() { return m_userName; }
};

class GameRoom
{
private:
	int m_limitPlayerCount;
	int m_currentPlayerCount;
	int m_gameMode;
	bool m_teamMode;
	int m_redTeamCount;
	int m_blueTeamCount;
	unordered_map<HostID,shared_ptr<RoomClient>> m_clientMap;
public:
	GameRoom();
	~GameRoom() {}

	// 플레이어 수 조절
	void SetPlayerLimitCount(int count) { m_limitPlayerCount = count; }
	int GetPlayerLimitCount() { return m_limitPlayerCount; }

	// 게임 모드 세팅
	void SetGameMode(int gameMode) { m_gameMode = gameMode; }

	// 팀 모드인지 세팅
	void SetTeamMode(bool teamMode) { m_teamMode = teamMode; }

	bool GetTeamMode() { return m_teamMode; }
	int GetGameMode() { return m_gameMode; }

	// 새 클라이언트가 들어옴
	bool NewClientConnect(HostID hostID, string userName, CriticalSection& critSection);

	// 클라이언트가 나감
	void LogOutClient(HostID hostID);

	// 방에 들어와있는지 체크
	bool IsRoomClient(HostID hostID);

	// 레디 
	void SetReady(HostID hostID,bool ready);

	// 팀 변경
	void TeamChange(HostID hostID,bool red);

	// GameStart 체크
	bool GameStartCheck();

	// 클리어
	void ClearRoom();

	shared_ptr<RoomClient> GetClient(HostID hostID) { return m_clientMap[hostID]; }

	forward_list<HostID> GetOtherClients(HostID hostID);
	forward_list<RoomClient> GetOtherClientInfos(HostID hostID);

};

#endif
