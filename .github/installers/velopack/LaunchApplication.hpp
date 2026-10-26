#pragma once
#include <windows.h>
#include <shellapi.h>
#include <string>

using ShellExecuteFunction = BOOL (WINAPI *)(SHELLEXECUTEINFOW*);

inline DWORD LaunchApplication(const std::wstring& directory, const wchar_t* arguments,
    ShellExecuteFunction execute = &ShellExecuteExW)
{
    const auto executable = directory + L"\\EDHM-UI-V3.exe";
    SHELLEXECUTEINFOW info{};
    info.cbSize = sizeof(info);
    info.fMask = SEE_MASK_NOASYNC | SEE_MASK_FLAG_NO_UI;
    // 'open' honors the application's existing highestAvailable manifest.
    // Do not force 'runas', which would require different credentials for a
    // standard user and could switch their per-user settings/install identity.
    info.lpVerb = L"open";
    info.lpFile = executable.c_str();
    info.lpParameters = arguments;
    info.lpDirectory = directory.c_str();
    info.nShow = SW_SHOWNORMAL;
    if (execute(&info)) return ERROR_SUCCESS;
    const auto error = GetLastError();
    // Declining elevation closes the launcher; never retry without elevation.
    return error == ERROR_CANCELLED ? ERROR_SUCCESS : error;
}
