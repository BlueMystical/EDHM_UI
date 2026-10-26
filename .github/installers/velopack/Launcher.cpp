#include "LaunchApplication.hpp"
#include "Velopack.hpp"
#include <filesystem>

int WINAPI wWinMain(HINSTANCE, HINSTANCE, PWSTR arguments, int)
{
    // This asInvoker process owns every Velopack fast hook. They exit here,
    // without launching Electron, requesting elevation, or loading user settings.
    Velopack::VelopackApp::Build().SetAutoApplyOnStartup(false).Run();

    std::wstring filename(32768, L'\0');
    const auto length = GetModuleFileNameW(nullptr, filename.data(), static_cast<DWORD>(filename.size()));
    if (length == 0 || length >= filename.size()) return ERROR_BAD_PATHNAME;
    filename.resize(length);
    const auto directory = std::filesystem::path(filename).parent_path().wstring();
    const auto error = LaunchApplication(directory, arguments);
    if (error != ERROR_SUCCESS) {
        const auto message = L"EDHM-UI could not be started. Windows error: " + std::to_wstring(error);
        MessageBoxW(nullptr, message.c_str(), L"EDHM-UI", MB_OK | MB_ICONERROR);
    }
    return static_cast<int>(error);
}
