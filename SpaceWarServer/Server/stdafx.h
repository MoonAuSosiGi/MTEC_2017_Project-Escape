// stdafx.h : ���� ��������� ���� ��������� �ʴ�
// ǥ�� �ý��� ���� ���� �Ǵ� ������Ʈ ���� ���� ������
// ��� �ִ� ���� �����Դϴ�.
//

#pragma once

#include <iostream>
#include <sstream>
#include <unordered_map>
#include <string>
#include <stdlib.h>
#include <time.h>
#include <forward_list>


// TODO: ���α׷��� �ʿ��� �߰� ����� ���⿡�� �����մϴ�.
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
