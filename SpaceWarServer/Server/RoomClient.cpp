#include "stdafx.h"
#include "RoomClient.h"

/**
 * @brief	RoomClient ����
 * @detail	���� / �ΰ��� ���� Ȱ��Ǵ� Ŭ���̾�Ʈ�� ���� ���� Ŭ���� ������
 * @param	hostID	Ŭ���̾�Ʈ HostID ( ������ )
 * @param	userName ���� �̸�
 * @param	host ���� ���� ( �⺻ false )
*/
RoomClient::RoomClient(HostID hostID, string userName, bool host)
{
	m_hostID = hostID;
	m_userName = userName;
	m_redTeam = true;
	m_isReady = false;
	m_isHost = host;
	m_state = ALIVE;
	m_hp = MAX_HP;
	m_oxy = MAX_OXY;
	m_damageCoolTime = 0;
	m_killCount = 0;
	m_assistCount = 0;
	m_deathCount = 0;
}

#pragma region Get / Set Method Waiting Room =========================================================================

/**
 * @brief	�� �÷� ����
 * @detail	�� / û �� ���� �޼���
 * @param	red ���� ������ / û�� ������
*/
void RoomClient::SetTeamColor(bool red)
{
	m_redTeam = red;
}

/**
 * @brief	Ŭ���̾�Ʈ ���� ���� ����
 * @param	ready ���� ���� ����
*/
void RoomClient::SetReady(bool ready)
{
	m_isReady = ready;
}

/**
 * @brief	Ŭ���̾�Ʈ�� �������� �ƴ��� ����
 * @param	host ���� ���� ����
*/
void RoomClient::SetHost(bool host)
{
	m_isHost = host;
}

/**
 * @brief	���� ���Ӿ����� �ƴ����� ���� ���� ����
 * @param	gameScene ���Ӿ��� ��� true
*/
void RoomClient::SetGameScene(bool gameScene)
{
	m_isGameScene = gameScene;
}

/**
 * @brief	��ý�Ʈ����
 * @param	������ ��ý�Ʈ ����
*/
void RoomClient::SetAssistCount(int assist)
{
	m_assistCount = assist;
}

/**
 * @brief	ų ī��Ʈ ����
 * @param	������ ų ī��Ʈ
*/
void RoomClient::SetKillCount(int killCount)
{
	m_killCount = killCount;
}

/**
 * @brief	���� Ƚ�� ����
 * @param	���� Ƚ���� �����Ѵ�.
*/
void RoomClient::SetDeathCount(int death)
{
	m_deathCount = death;
}

/**
 * @brief	���� Ƚ�� ���
 * @return	���� Ƚ���� �����Ѵ�.
*/
int RoomClient::GetDeathCount()
{
	return m_deathCount;
}

/**
 * @brief	ų ī��Ʈ ���
 * @return	ų ī��Ʈ ����
*/
int RoomClient::GetKillCount()
{
	return m_killCount;
}
/**
 * @brief	��ý�Ʈ ���
 * @return	��ý�Ʈ�� �󸶳� �÷ȴ��� ����
*/
int RoomClient::GetAssistCount()
{
	return m_assistCount;
}

/**
 * @brief	���� ���� ��ȯ
 * @return	������ ��� true �� ����
*/
bool RoomClient::IsHost() 
{
	return m_isHost;
}

/**
 * @brief	���� ���� ��ȯ
 * @return	���� ������ ��� true �� ����
*/
bool RoomClient::IsReady() 
{ 
	return m_isReady; 
}

/**
 * @brief	���� ������ ��ȯ
 * @detail	������ ��쿡�� ���
 * @return	���� ���� ��� true �� ����
*/
bool RoomClient::IsRedTeam() 
{
	return m_redTeam; 
}

/**
 * @brief	���Ӿ����� ��ȯ
 * @return	���� ���Ӿ��� ��� true ����
*/
bool RoomClient::IsGameScene() 
{
	return m_isGameScene; 
}

/**
 * @brief	Ŭ���̾�Ʈ�� HostID ���
 * @return	�ش� Ŭ���̾�Ʈ�� HostID �� ����
*/
HostID RoomClient::GetHostID() 
{
	return m_hostID; 
}

/**
 * @brief	Ŭ���̾�Ʈ�� ���� �̸� ��� 
 * @return	�ش� Ŭ���̾�Ʈ�� userName�� ����
*/
string RoomClient::GetName() 
{ 
	return m_userName; 
}
#pragma endregion ===================================================================================================

#pragma region Get / Set Method InGame ==============================================================================

/**
* @brief	���� ��ġ ����
* @detail	���� ��ġ���� �뷫���� ��ġ ( Ŭ���̾�Ʈ���� �̵��� ó���ϱ� ���� )
* @param	pos Vector������ ������
*/
void RoomClient::SetPosition(Vector3 pos)
{
	m_pos = pos;
}

/**
* @brief	���� ��ġ ����
* @detail	���� ��ġ���� �뷫���� ��ġ ( Ŭ���̾�Ʈ���� �̵��� ó���ϱ� ���� )
* @param	x �÷��̾��� x��ǥ
* @param	y �÷��̾��� y��ǥ
* @param	z �÷��̾��� z��ǥ
*/
void RoomClient::SetPosition(float x, float y, float z)
{
	m_pos.x = x; m_pos.y = y; m_pos.z = z;
}

/**
 * @brief	���� ������ ���
 * @detail	���� ��ġ���� �뷫���� ��ġ�� ����
 * @return	���� �÷��̾��� position �� ����
*/
Vector3 RoomClient::GetPosition() 
{ 
	return m_pos; 
}

/**
 * @brief	ü�� ����
 * @detail	ü�� �� ���� ����
 * @param	newHp �ٲ� hp��
*/
void RoomClient::SetHp(float newHp)
{
	m_hp = newHp;
}

/**
 * @brief	ü�� ������Ʈ
 * @detail	���� ü���� ���ο� ���� ����
 * @param	val hp�� ������ ��
*/
void RoomClient::HpUpdate(float val)
{
	m_hp += val;
}

/**
 * @brief	ü�� ��ȯ
 * @return	hp�� ����
*/
float RoomClient::GetHp()
{
	return m_hp;
}

/**
 * @brief	������ ��Ÿ�� ����
 * @param	value ������ ��Ÿ��
*/
void RoomClient::SetDamageCooltime(int value)
{
	m_damageCoolTime = value;
}

/**
 * @brief	������ ��Ÿ�� ���
 * @return	������ ��Ÿ�� ����
*/
int RoomClient::GetDamageCooltime()
{
	return m_damageCoolTime;
}

/**
 * @brief	��� ����
 * @detail	��Ұ� ���� ����
 * @param	newOxy 
*/
void RoomClient::SetOxy(float newOxy)
{
	m_oxy = newOxy;
}

/**
 * @brief	��� ������Ʈ
 * @detail	���� ��Ҹ� ���ο� ���� ����
 * @param	val oxy�� ������ ��
*/
void RoomClient::OxyUpdate(float val)
{
	m_oxy += val;
}

/**
 * @brief	��� ����
 * @return	oxy�� ����
*/
float RoomClient::GetOxy()
{
	return m_oxy;
}

/**
 * @brief	�÷��̾� ���� ����
 * @detail	ALIVE , DEAD , SPACESHIP ���� ����
 * @param	state PLAYER_STATE Enum ���
*/
void RoomClient::SetState(int state)
{
	m_state = state;
}

/**
 * @brief	�÷��̾� ���� ����
 * @return	state ���� ( PLAYER_STATE Enum )
*/
int RoomClient::GetState()
{
	return m_state;
}

#pragma endregion ===================================================================================================

#pragma region InGame Method ========================================================================================

/**
 * @brief	���� ���� ���� 
 * @todo	ü�°� ���� ���̺��� �޾ƿ� �� 
*/
void RoomClient::GameReset()
{
	m_isGameScene = false;
	m_isReady = false;

	m_pos.x = m_pos.y = m_pos.z = 0.0f;
/*
	m_hp = MAX_HP;
	m_oxy = MAX_OXY;*/
	m_killCount = 0;
	m_assistCount = 0;
	m_deathCount = 0;

	m_state = ALIVE;
}

/**
 * @brief	��ý�Ʈ ����Ʈ ����� ���� ���� �Լ�
 * @detail	���� Ŭ���̾�Ʈ���� �����ϴµ�, ���� �ð� ���
 * @param	hostID ���� Ŭ���̾�Ʈ
 * @param	time ���� �ð�
*/
void RoomClient::DamageClient(int hostID, float time)
{
	m_assistCheck[hostID] = time;
}

/**
 * @brief	�÷��̾ ����ÿ� ���� ó��
 * @detail	�÷��̾ �׾��� ��� ���â �����ֱ� ���� ó�� ( ��ý�Ʈ �� )
 * @todo	��ý�Ʈ ���ð� ���̺� ������ �������� / ��ý�Ʈ ���� ����
*/
void RoomClient::PlayerDead(float deadTime)
{
	cout << "Player Dead : " << (int)m_hostID << " Dead Time : " << deadTime << endl;
	m_state = DEATH;
	m_deathCount++;
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
		}
		else
			iter++;
	}
}

/**
 * @brief	�÷��̾ �̰��� ���� ó��
*/
void RoomClient::PlayerWin()
{
	m_state = SPACESHIP;
}

/**
 * @brief	���� �÷��̾��� ����� ������ Ŭ���̾�Ʈ ����Ʈ
 * @todo	���� ���� ���
*/
forward_list<int> RoomClient::GetAssistClientList()
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
#pragma endregion ===================================================================================================