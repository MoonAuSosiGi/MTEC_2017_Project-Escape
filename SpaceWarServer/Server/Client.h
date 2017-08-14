#ifndef CLIENT_H
#define CLIENT_H
#include "stdafx.h"

#define MAX_HP 100.0f
#define MAX_OXY 100.0f

typedef enum PlayerState
{
	ALIVE = 0,
	DEATH,
	SPACESHIP
}PLAYER_STATE;

class Client
{
public:
	HostID m_hostID;
	string m_userName;
	float x;
	float y;
	float z;
	float hp;
	float oxy;
	float m_time;
	int m_state;
	int m_deathCount;
	int m_killCount;
	int m_assistCount;
	unordered_map<int, float> m_assistCheck;
public:
	Client();
	~Client();

	void DamageClient(int hostID, float time);

	void PlayerDead(float deadTime);

	void PlayerWin();

	void Reset();

	// 이 클라를 죽이는데 일조한 리스트 
	forward_list<int> GetAssistClientList();
	
};

#endif