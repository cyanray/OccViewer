#pragma once

#include <AIS_SelectionScheme.hxx>

public enum class SelectionScheme
{
	Replace = AIS_SelectionScheme::AIS_SelectionScheme_Replace,
	Xor = AIS_SelectionScheme::AIS_SelectionScheme_XOR
};