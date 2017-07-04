#ifndef MARSHALER_H
#define MARSHALER_H

#include "stdafx.h"

namespace Proud
{
	// Proud::Vector3 (C++) <-> UnityEngine.Vector3 (C#)
	// 
	// write
	inline CMessage& operator<<(CMessage& a, const Proud::Vector3& b)
	{
		a << b.x;
		a << b.y;
		a << b.z;
		return a;
	}

	// read
	inline CMessage& operator >> (CMessage& a, Proud::Vector3& b)
	{
		a >> b.x;
		a >> b.y;
		a >> b.z;
		return a;
	}

	inline void AppendTextOut(String &a, Proud::Vector3& b)
	{
		String f;
		f.Format(L"{x=%f,y=%f,z=%f}", b.x, b.y, b.z);
		a += f;
	}
}

#endif
