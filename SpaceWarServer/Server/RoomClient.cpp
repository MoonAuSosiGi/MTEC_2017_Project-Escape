#include "stdafx.h"
#include "RoomClient.h"

/**
 * @brief	RoomClient 생성
 * @detail	대기실 / 인게임 에서 활용되는 클라이언트에 대한 정보 클래스 생성자
 * @param	hostID	클라이언트 HostID ( 고유함 )
 * @param	userName 유저 이름
 * @param	host 방장 유무 ( 기본 false )
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
 * @brief	팀 컬러 세팅
 * @detail	적 / 청 팀 세팅 메서드
 * @param	red 적색 팀인지 / 청색 팀인지
*/
void RoomClient::SetTeamColor(bool red)
{
	m_redTeam = red;
}

/**
 * @brief	클라이언트 레디 정보 세팅
 * @param	ready 레디 유무 정보
*/
void RoomClient::SetReady(bool ready)
{
	m_isReady = ready;
}

/**
 * @brief	클라이언트가 방장인지 아닌지 세팅
 * @param	host 방장 유무 정보
*/
void RoomClient::SetHost(bool host)
{
	m_isHost = host;
}

/**
 * @brief	현재 게임씬인지 아닌지에 대한 정보 세팅
 * @param	gameScene 게임씬일 경우 true
*/
void RoomClient::SetGameScene(bool gameScene)
{
	m_isGameScene = gameScene;
}

/**
 * @brief	어시스트세팅
 * @param	세팅할 어시스트 변수
*/
void RoomClient::SetAssistCount(int assist)
{
	m_assistCount = assist;
}

/**
 * @brief	킬 카운트 세팅
 * @param	세팅할 킬 카운트
*/
void RoomClient::SetKillCount(int killCount)
{
	m_killCount = killCount;
}

/**
 * @brief	죽은 횟수 세팅
 * @param	죽은 횟수를 세팅한다.
*/
void RoomClient::SetDeathCount(int death)
{
	m_deathCount = death;
}

/**
 * @brief	죽은 횟수 얻기
 * @return	죽은 횟수를 리턴한다.
*/
int RoomClient::GetDeathCount()
{
	return m_deathCount;
}

/**
 * @brief	킬 카운트 얻기
 * @return	킬 카운트 리턴
*/
int RoomClient::GetKillCount()
{
	return m_killCount;
}
/**
 * @brief	어시스트 얻기
 * @return	어시스트를 얼마나 올렸는지 리턴
*/
int RoomClient::GetAssistCount()
{
	return m_assistCount;
}

/**
 * @brief	방장 유무 반환
 * @return	방장일 경우 true 를 리턴
*/
bool RoomClient::IsHost() 
{
	return m_isHost;
}

/**
 * @brief	레디 유무 반환
 * @return	레디 상태일 경우 true 를 리턴
*/
bool RoomClient::IsReady() 
{ 
	return m_isReady; 
}

/**
 * @brief	적색 팀인지 반환
 * @detail	팀전일 경우에만 사용
 * @return	적색 팀일 경우 true 를 리턴
*/
bool RoomClient::IsRedTeam() 
{
	return m_redTeam; 
}

/**
 * @brief	게임씬인지 반환
 * @return	현재 게임씬일 경우 true 리턴
*/
bool RoomClient::IsGameScene() 
{
	return m_isGameScene; 
}

/**
 * @brief	클라이언트의 HostID 얻기
 * @return	해당 클라이언트의 HostID 를 리턴
*/
HostID RoomClient::GetHostID() 
{
	return m_hostID; 
}

/**
 * @brief	클라이언트의 유저 이름 얻기 
 * @return	해당 클라이언트의 userName을 리턴
*/
string RoomClient::GetName() 
{ 
	return m_userName; 
}
#pragma endregion ===================================================================================================

#pragma region Get / Set Method InGame ==============================================================================

/**
* @brief	현재 위치 세팅
* @detail	현재 위치지만 대략적인 위치 ( 클라이언트에서 이동을 처리하기 때문 )
* @param	pos Vector형태의 포지션
*/
void RoomClient::SetPosition(Vector3 pos)
{
	m_pos = pos;
}

/**
* @brief	현재 위치 세팅
* @detail	현재 위치지만 대략적인 위치 ( 클라이언트에서 이동을 처리하기 때문 )
* @param	x 플레이어의 x좌표
* @param	y 플레이어의 y좌표
* @param	z 플레이어의 z좌표
*/
void RoomClient::SetPosition(float x, float y, float z)
{
	m_pos.x = x; m_pos.y = y; m_pos.z = z;
}

/**
 * @brief	현재 포지션 얻기
 * @detail	현재 위치지만 대략적인 위치를 리턴
 * @return	현재 플레이어의 position 을 리턴
*/
Vector3 RoomClient::GetPosition() 
{ 
	return m_pos; 
}

/**
 * @brief	체력 세팅
 * @detail	체력 값 강제 변경
 * @param	newHp 바꿀 hp값
*/
void RoomClient::SetHp(float newHp)
{
	m_hp = newHp;
}

/**
 * @brief	체력 업데이트
 * @detail	현재 체력을 새로운 값과 더함
 * @param	val hp에 더해질 값
*/
void RoomClient::HpUpdate(float val)
{
	m_hp += val;
}

/**
 * @brief	체력 반환
 * @return	hp를 리턴
*/
float RoomClient::GetHp()
{
	return m_hp;
}

/**
 * @brief	데미지 쿨타임 세팅
 * @param	value 세팅할 쿨타임
*/
void RoomClient::SetDamageCooltime(int value)
{
	m_damageCoolTime = value;
}

/**
 * @brief	데미지 쿨타임 얻기
 * @return	데미지 쿨타임 리턴
*/
int RoomClient::GetDamageCooltime()
{
	return m_damageCoolTime;
}

/**
 * @brief	산소 세팅
 * @detail	산소값 강제 변경
 * @param	newOxy 
*/
void RoomClient::SetOxy(float newOxy)
{
	m_oxy = newOxy;
}

/**
 * @brief	산소 업데이트
 * @detail	현재 산소를 새로운 값과 더함
 * @param	val oxy에 더해질 값
*/
void RoomClient::OxyUpdate(float val)
{
	m_oxy += val;
}

/**
 * @brief	산소 리턴
 * @return	oxy를 리턴
*/
float RoomClient::GetOxy()
{
	return m_oxy;
}

/**
 * @brief	플레이어 상태 변경
 * @detail	ALIVE , DEAD , SPACESHIP 등의 상태
 * @param	state PLAYER_STATE Enum 사용
*/
void RoomClient::SetState(int state)
{
	m_state = state;
}

/**
 * @brief	플레이어 상태 리턴
 * @return	state 리턴 ( PLAYER_STATE Enum )
*/
int RoomClient::GetState()
{
	return m_state;
}

#pragma endregion ===================================================================================================

#pragma region InGame Method ========================================================================================

/**
 * @brief	게임 상태 리셋 
 * @todo	체력값 등은 테이블에서 받아올 것 
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
 * @brief	어시스트 리스트 계산을 위한 저장 함수
 * @detail	때린 클라이언트들을 저장하는데, 때린 시간 기록
 * @param	hostID 때린 클라이언트
 * @param	time 때린 시간
*/
void RoomClient::DamageClient(int hostID, float time)
{
	m_assistCheck[hostID] = time;
}

/**
 * @brief	플레이어가 사망시에 따른 처리
 * @detail	플레이어가 죽었을 경우 결과창 보여주기 관련 처리 ( 어시스트 등 )
 * @todo	어시스트 허용시간 테이블 값으로 가져오기 / 어시스트 로직 점검
*/
void RoomClient::PlayerDead(float deadTime)
{
	cout << "Player Dead : " << (int)m_hostID << " Dead Time : " << deadTime << endl;
	m_state = DEATH;
	m_deathCount++;
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
		}
		else
			iter++;
	}
}

/**
 * @brief	플레이어가 이겼을 때의 처리
*/
void RoomClient::PlayerWin()
{
	m_state = SPACESHIP;
}

/**
 * @brief	현재 플레이어의 사망에 관여한 클라이언트 리스트
 * @todo	로직 점검 요망
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