// stdafx.h : 자주 사용하지만 자주 변경되지는 않는
// 표준 시스템 포함 파일 또는 프로젝트 관련 포함 파일이
// 들어 있는 포함 파일입니다.
//

#pragma once

#include <iostream>
#include <sstream>
#include <unordered_map>
#include <string>
#include <stdlib.h>
#include <time.h>
#include <forward_list>


// TODO: 프로그램에 필요한 추가 헤더는 여기에서 참조합니다.
#include <ProudNetServer.h>
#include <AdoWrap.h>
#include <iostream>
#include <fstream>
#include "json\json.h"

using namespace std;
using namespace Proud;
using namespace Json;

enum GameMode
{
	DEATH_MATCH = 100,
	SURVIVAL
};

typedef enum PlayerState
{
	ALIVE = 0,
	DEATH,
	DEATH_ZONE_DEAD,
	SPACESHIP
}PLAYER_STATE;

#define MAX_HP 100.0f
#define MAX_OXY 100.0f
