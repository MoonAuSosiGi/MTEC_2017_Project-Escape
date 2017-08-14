#include "stdafx.h"
#include "Client.h"


Client::Client()
{
	x = y = z = 0.0f;
	hp = MAX_HP;
	oxy = MAX_OXY;
	m_killCount = 0;
	m_assistCount = 0;
	m_deathCount = 0;

	m_state = ALIVE;

}


Client::~Client()
{
}

void Client::Reset()
{
	x = y = z = 0.0f;
	hp = MAX_HP;
	oxy = MAX_OXY;
	m_killCount = 0;
	m_assistCount = 0;
	m_deathCount = 0;

	m_state = ALIVE;
}

void Client::DamageClient(int hostID, float time)
{
	m_assistCheck[hostID] = time;
}

void Client::PlayerDead(float deadTime)
{
	cout << "사망시 어시스트 체크" << endl;
	m_state = DEATH;
	// 어시스트의 목록을 만들어야 함
	auto iter = m_assistCheck.begin();
	
	while (iter != m_assistCheck.end())
	{	
		float timeCheck = deadTime - iter->second;
		
		if (timeCheck > 7000)
		{
			//어시스트 허용 시간을 넘었다!
			// 삭제
			iter = m_assistCheck.erase(iter);
			cout << "어시스트 삭제 " << endl;
		}
		else
			iter++;
	}
}

void Client::PlayerWin()
{
	m_state = SPACESHIP;
}

forward_list<int> Client::GetAssistClientList()
{
	forward_list<int> list;
	
	auto iter = m_assistCheck.begin();

	while (iter != m_assistCheck.end())
	{
		list.push_front(iter->first);
		iter++;
	}

	return list;
}