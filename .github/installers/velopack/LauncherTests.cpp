#include "LaunchApplication.hpp"
#include <iostream>

static unsigned calls;
static DWORD result;
static bool valid;
static BOOL WINAPI FakeShell(SHELLEXECUTEINFOW* info)
{
    ++calls;
    valid = std::wstring(info->lpVerb) == L"open" &&
        std::wstring(info->lpFile) == L"C:\\Users\\O'Brien Ω\\EDHM\\current\\EDHM-UI-V3.exe" &&
        std::wstring(info->lpDirectory) == L"C:\\Users\\O'Brien Ω\\EDHM\\current" &&
        std::wstring(info->lpParameters) == L"--example \"quoted value\"" &&
        info->nShow == SW_SHOWNORMAL;
    SetLastError(result);
    return result == ERROR_SUCCESS;
}

int main()
{
    for (const DWORD error : { ERROR_SUCCESS, ERROR_CANCELLED, ERROR_FILE_NOT_FOUND, ERROR_ELEVATION_REQUIRED }) {
        calls = 0;
        result = error;
        valid = false;
        const auto actual = LaunchApplication(L"C:\\Users\\O'Brien Ω\\EDHM\\current",
            L"--example \"quoted value\"", &FakeShell);
        if (!valid || calls != 1 || actual != (error == ERROR_CANCELLED ? ERROR_SUCCESS : error)) {
            std::cerr << "Launch/cancellation regression: " << error << "\n";
            return 1;
        }
    }
    std::cout << "Shell launch preserves arguments and working directory; cancellation never retries.\n";
}
