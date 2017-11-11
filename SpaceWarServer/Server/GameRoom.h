#ifndef GameRoom_H
#define GameRoom_H
#include "RoomClient.h"

/**
 * @brief		GameRoom :: 게임이 수행되는 방 관리
 * @details		대기실, 게임방 관리 클래스 / 레디 정보 / 게임 모드 / 플레이어 수 제한 / 팀 정보  등에 대한 처리를 한다.
 * @author		이훈 (MoonAuSosiGi@gmail.com)
 * @date		2017-11-04
 * @file		GameRoom.h
 * @version		0.0.1
*/
class GameRoom
{
private:

	int m_limitPlayerCount;		///< 플레이어 수 제한 (기본값 10)
	int m_currentPlayerCount;	///< 현재 접속한 플레이어 수
	int m_gameMode;				///< 현재 게임 모드
	bool m_teamMode;			///< 현재 게임이 팀전인지 개인전인지
	bool m_isGameRunning;		///< 현재 게임중인지
	unordered_map<HostID,shared_ptr<RoomClient>> m_clientMap; ///< 접속한 클라이언트 맵
public:

	GameRoom();					///< 기본 생성자, 초깃값 세팅
	unordered_map<HostID, shared_ptr<RoomClient>> GetClientMap(); ///< 클라이언트 맵 얻기

#pragma region Room Setting Method =====================================================================================
	void SetPlayerLimitCount(int count);	///< 플레이어 수 제한두기
	int GetPlayerLimitCount();				///< 플레이 가능한 플레이어 수 얻기
	void SetGameMode(int gameMode);			///< 게임 모드 세팅
	void SetTeamMode(bool teamMode);		///< 팀 모드 세팅
	bool GetTeamMode();						///< 팀 모드 얻기
	int GetGameMode();						///< 게임 모드 얻기
	void ClearRoom();						///< 게임 룸 클리어
#pragma endregion ======================================================================================================

#pragma region Client Method ===========================================================================================
	bool NewClientConnect(HostID hostID, string userName, CriticalSection& critSection); ///< 새 클라이언트 접속
	void LogOutClient(HostID hostID); ///< 클라이언트가 접속을 끊었다.
	bool IsRoomClient(HostID hostID); ///< 방에 제대로 접속했는지 체크
	void SetReady(HostID hostID, bool ready); ///< 클라이언트 레디 정보 전송
	void TeamChange(HostID hostID, bool red); ///< 클라이언트 팀 변경 정보 전송
#pragma endregion ======================================================================================================

#pragma region Game Method =============================================================================================
	bool GameStartCheck(); ///< 게임 스타트가 가능한지 체크 
	bool IsGameSceneAllReady(); ///< 게임씬에 들어와서 게임 가능한 상태인지 체크
	bool IsGameRunning(); ///< 현재 게임 중인지를 리턴
	void SetGameRunning(bool running); ///< 현재 게임 중인지를 세팅

	forward_list<HostID> GetOtherClients(HostID hostID); ///< 특정 클라이언트 제외한 HostID 리스트 얻기
	forward_list<RoomClient> GetOtherClientInfos(HostID hostID); ///< 특정 클라이언트 제외한 리스트 얻기
	forward_list<HostID> GetAllClient(); ///< 접속해있는 모든 클라이언트의 HostID 리스트 얻기
	shared_ptr<RoomClient> GetClient(HostID hostID); ///< 특정 클라이언트의 정보 얻기

#pragma endregion ======================================================================================================

};

#endif
