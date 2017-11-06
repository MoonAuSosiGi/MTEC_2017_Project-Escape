#ifndef ROOM_CLIENT_H
#define ROOM_CLIENT_H
#include "stdafx.h"

/**
* @brief		RoomClient :: 대기실 , 게임방에 들어와있는 클라이언트
* @details		대기실 / 게임 내 모든 클라이언트의 정보가 담겨있다.
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			RoomClient.h
* @version		0.0.1
*/
class RoomClient
{
private:

	HostID m_hostID;	///< 클라이언트의 HostID
	string m_userName;	///< 클라이언트의 유저 이름

	bool m_redTeam;		///< 적색 팀인지 (팀전일 경우에만 해당)
	bool m_isReady;		///< 레디 상태인지 ( 대기실, 방장이 아닐때 )
	bool m_isHost;		///< 방장인지 아닌지
	bool m_isGameScene;	///< 현재 게임씬 안인지

	Vector3 m_pos;		///< 현재 위치
	float m_hp;			///< 현재 클라이언트의 체력		
	float m_oxy;		///< 현재 클라이언트의 산소량

	int m_state;		///< 살아있는지 죽어있는지 우주선인지 등에 대한 PlayerState
	int m_deathCount;	///< 죽은 횟수 
	int m_killCount;	///< 죽인 횟수
	int m_assistCount;	///< 어시스트 횟수

	unordered_map<int, float> m_assistCheck; ///< 어시스트 관련 체크용 맵
	
public:

	RoomClient(HostID hostID, string userName, bool host = false); ///< 생성자, HostID / 유저 이름 / host 유무

#pragma region Get / Set Method Waiting Room ========================================================================
	void SetTeamColor(bool red);	///< 팀 컬러 세팅 
	void SetReady(bool ready);		///< 레디 정보 세팅
	void SetHost(bool host);		///< 방장 정보 세팅
	void SetGameScene(bool gameScene); ///< 현재 게임씬인지에 대한 정보 세팅
	void SetAssistCount(int assist); ///< 어시스트 정보 세팅
	void SetKillCount(int killCount);	///< 킬 카운트 세팅
	void SetDeathCount(int death); ///< 데스 카운트 세팅
	int GetDeathCount(); ///< 데스 카운트 얻기
	int GetKillCount(); ///< 킬 카운트 얻기
	int GetAssistCount(); ///< 어시스트 정보 얻기
	bool IsHost(); ///< 이 클라이언트가 방장인지
	bool IsReady(); ///< 레디 중인지 
	bool IsRedTeam(); ///< 레드팀인지 
	bool IsGameScene(); ///< 게임씬인지 아닌지

	HostID GetHostID(); ///< Host ID 얻기
	string GetName(); ///< 유저 이름 얻기 
#pragma endregion ===================================================================================================

#pragma region Get / Set Method InGame ==============================================================================
	void SetPosition(Vector3 pos);	///< 현재 포지션 세팅
	void SetPosition(float x, float y, float z); ///< 현재 포지션 세팅
	Vector3 GetPosition(); ///< 현재 포지션 얻기

	void SetHp(float newHp); ///< Hp 값 세팅
	void HpUpdate(float val); ///< 더해질 hp
	float GetHp(); ///< hp 리턴

	void SetOxy(float newOxy); ///< Oxy 값 세팅
	void OxyUpdate(float val); ///< 더해질 oxy
	float GetOxy(); ///< Oxy 리턴

	void SetState(int state); ///< State 변경
	int GetState();	///< State 리턴
#pragma endregion ===================================================================================================
	
#pragma region InGame Method ========================================================================================
	void GameReset(); ///< 게임 리셋시 호출
	void DamageClient(int hostID, float time); ///< 어시스트 계산을 위해 때린 녀석 기록
	void PlayerDead(float deadTime); ///< 플레이어가 죽었을 때의 처리
	void PlayerWin(); ///< 이 플레이어가 승리했을 경우에 대한 처리
	forward_list<int> GetAssistClientList(); ///< 이 클라이언트가 죽었을때, 어시스트한 클라이언트 리스트
#pragma endregion
};

#endif