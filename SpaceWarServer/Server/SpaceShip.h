#ifndef SPACESHIP_H
#define SPACESHIP_H

/**
* @brief		SpaceShip :: 네트워크 오브젝트
* @details		우주선 잠김 여부 / 누가 조작중인지 등에 대한 처리
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			SpaceShip.h
* @version		0.0.1
*/
class SpaceShip
{
private:
	int m_spaceShipID;	///< 우주선 네트워크 인덱스 ( 고유 )
	int m_targetHostID; ///< 현재 우주선에 접근중인 클라이언트의 HostID
	bool m_isLocked; ///< 우주선 조작 가능여부 ( 잠김 상태 여부 )
public:
	SpaceShip();	///< 기본 생성자

	void LockSpaceShip(int hostID); ///< 특정 클라이언트가 잠금을 시도했다.
	void UnLockSpaceShip();	///< 우주선 잠금이 끝났을 때의 처리
	bool IsLock();	///< 현재 우주선이 잠겨있는지
	int GetTargetHostID(); ///< 현재 우주선을 조작중인 클라이언트의 HostID
};

#endif