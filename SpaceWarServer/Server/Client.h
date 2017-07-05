#ifndef CLIENT_H
#define CLIENT_H
#include "stdafx.h"

#define MAX_HP 100.0f
#define MAX_OXY 100.0f

class Client
{
public:
	HostID m_hostID;
	string m_userName;
	float x;
	float y;
	float z;
	float hp;
	float oxy;
public:
	Client();
	~Client();
};

#endif