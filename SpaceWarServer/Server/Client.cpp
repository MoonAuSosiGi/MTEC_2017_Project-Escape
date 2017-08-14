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
	cout << "����� ��ý�Ʈ üũ" << endl;
	m_state = DEATH;
	// ��ý�Ʈ�� ����� ������ ��
	auto iter = m_assistCheck.begin();
	
	while (iter != m_assistCheck.end())
	{	
		float timeCheck = deadTime - iter->second;
		
		if (timeCheck > 7000)
		{
			//��ý�Ʈ ��� �ð��� �Ѿ���!
			// ����
			iter = m_assistCheck.erase(iter);
			cout << "��ý�Ʈ ���� " << endl;
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