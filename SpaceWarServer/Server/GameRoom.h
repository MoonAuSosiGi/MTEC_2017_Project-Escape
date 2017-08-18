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
	bool m_isGameScene;

	Vector3 m_pos;
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

	void SetGameScene(bool gameScene)
	{
		m_isGameScene = gameScene;
	}

	void SetPosition(Vector3 pos)
	{
		m_pos = pos;
	}

	void SetPosition(float x, float y, float z)
	{
		m_pos.x = x; m_pos.y = y; m_pos.z = z;
	}

	bool IsHost() { return m_isHost; }
	bool IsReady() { return m_isReady; }
	bool IsRedTeam() { return m_redTeam; }
	bool IsLobby() { return m_isLobby; }
	bool IsGameScene() { return m_isGameScene; }
	Vector3 GetPos() { return m_pos; }
	HostID GetHostID() { return m_hostID; }
	string GetName() { return m_userName; }

	void GameReset()
	{
		m_isGameScene = false;
		m_isReady = false;
		m_isLobby = false;
	}


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

	// �÷��̾� �� ����
	void SetPlayerLimitCount(int count) { m_limitPlayerCount = count; }
	int GetPlayerLimitCount() { return m_limitPlayerCount; }

	// ���� ��� ����
	void SetGameMode(int gameMode) { m_gameMode = gameMode; }

	// �� ������� ����
	void SetTeamMode(bool teamMode) { m_teamMode = teamMode; }

	bool GetTeamMode() { return m_teamMode; }
	int GetGameMode() { return m_gameMode; }

	// �� Ŭ���̾�Ʈ�� ����
	bool NewClientConnect(HostID hostID, string userName, CriticalSection& critSection);

	// Ŭ���̾�Ʈ�� ����
	void LogOutClient(HostID hostID);

	// �濡 �����ִ��� üũ
	bool IsRoomClient(HostID hostID);

	// ���� 
	void SetReady(HostID hostID,bool ready);

	// �� ����
	void TeamChange(HostID hostID,bool red);

	// GameStart üũ
	bool GameStartCheck();

	// Ŭ����
	void ClearRoom();

	//�ٽ� ����ӽ� ����
	void ResetGame();

	// ��� �غ� �Ǿ����� 
	bool IsGameSceneAllReady();

	shared_ptr<RoomClient> GetClient(HostID hostID) { return m_clientMap[hostID]; }

	forward_list<HostID> GetOtherClients(HostID hostID);
	forward_list<RoomClient> GetOtherClientInfos(HostID hostID);
	forward_list<HostID> GetAllClient();

};

#endif
