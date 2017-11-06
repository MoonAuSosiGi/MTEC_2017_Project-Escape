#ifndef OXYCHARGER_H
#define OXYCHARGER_H

/**
* @brief		OxyCharger :: 네트워크 오브젝트
* @details		산소 충전기 / 잠금 처리 등
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			OxyCharger.h
* @version		0.0.1
*/
class OxyCharger
{
private:
	int m_oxyChargerID; ///< 산소 충전기 네트워크 식별 아이디
	bool m_isLocked;	///< 잠금 여부
	int m_useHostID;	///< 사용중인 플레이어의 식별 아이디
public:
	OxyCharger(); ///< 기본 생성자

#pragma region Get / Set Method =========================================================
	void SetOxyChargerID(int oxyChargerID); ///< 산소 충전기 식별 아이디 세팅
	int GetOxyChargerID(); ///< 산소 충전기 식별 아이디 얻기
	bool IsLocked();	///< 잠김 유무 얻기
	int GetUseHostID();	///< 사용 중인 유저의 HostID 얻기
#pragma endregion =======================================================================

	
	void LockOxyCharger(int hostID) { m_isLocked = true; m_useHostID = hostID; } ///< 산소 충전기 잠금
	void UnLockOxyCharger() { m_isLocked = false; m_useHostID = -1; } ///< 산소 충전기 잠금 해제
private:


};

#endif