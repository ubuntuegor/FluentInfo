#include "pch.h"

#include <shlobj.h>
#include <shlwapi.h>
#pragma comment(lib, "Shlwapi.lib")

HINSTANCE g_hInst = 0;

static const wchar_t* supportedExts = L".aiff.aif.avi.m2ts.mts.flv.heif.heic.avif.ts.mov.mp4.m4v.m4a.3gpp.3gp.qt.ogg.opus.swf.wma.wmv.avc.h264.hevc.h265.aac.ac3.ac4.amr.dts.dtshd.eac3.ec3.flac.mp1.mp2.mp3.wav.bmp.gif.ico.jpg.jpeg.png.tif.tiff.mkv.webm.";

inline std::wstring get_module_folder(HMODULE mod = nullptr)
{
	wchar_t buffer[MAX_PATH + 1];
	DWORD actual_length = GetModuleFileNameW(mod, buffer, MAX_PATH);
	if (GetLastError() == ERROR_INSUFFICIENT_BUFFER)
	{
		const DWORD long_path_length = 0xFFFF; // should be always enough
		std::wstring long_filename(long_path_length, L'\0');
		actual_length = GetModuleFileNameW(mod, long_filename.data(), long_path_length);
		PathRemoveFileSpecW(long_filename.data());
		long_filename.resize(std::wcslen(long_filename.data()));
		long_filename.shrink_to_fit();
		return long_filename;
	}

	PathRemoveFileSpecW(buffer);
	return { buffer, static_cast<size_t>(lstrlenW(buffer)) };
}

struct __declspec(uuid("5C9CE5A1-7DC5-4613-A61B-75A47E86A41C"))
	CExplorerCommandVerb : winrt::implements<CExplorerCommandVerb, IExplorerCommand>
{

	IFACEMETHODIMP GetTitle(IShellItemArray* psiItemArray, LPWSTR* ppszName) noexcept override
	{
		return SHStrDupW(L"FluentInfo", ppszName);
	}

	IFACEMETHODIMP GetIcon(IShellItemArray* psiItemArray, LPWSTR* ppszIcon) noexcept override
	{
		std::wstring path = get_module_folder(g_hInst);
		path += L"\\Assets\\fluentinfo.ico";
		return SHStrDupW(path.c_str(), ppszIcon);
	}

	IFACEMETHODIMP GetToolTip(IShellItemArray* psiItemArray, LPWSTR* ppszInfotip) noexcept override
	{
		*ppszInfotip = NULL;
		return E_NOTIMPL;
	}

	IFACEMETHODIMP GetCanonicalName(GUID* pguidCommandName) noexcept override
	{
		*pguidCommandName = __uuidof(this);
		return S_OK;
	}

	// compute the visibility of the verb here, respect "fOkToBeSlow" if this is slow (does IO for example)
	// when called with fOkToBeSlow == FALSE return E_PENDING and this object will be called
	// back on a background thread with fOkToBeSlow == TRUE
	IFACEMETHODIMP GetState(IShellItemArray* psiItemArray, BOOL fOkToBeSlow, EXPCMDSTATE* pCmdState) noexcept override
	{
		*pCmdState = ECS_HIDDEN;

		if (psiItemArray == nullptr) {
			return S_OK;
		}

		IShellItem* shellItem = nullptr;
		HRESULT getItemResult = psiItemArray->GetItemAt(0, &shellItem);

		if (S_OK != getItemResult || nullptr == shellItem) {
			return S_OK;
		}

		LPWSTR pszPath;
		HRESULT getDisplayResult = shellItem->GetDisplayName(SIGDN_FILESYSPATH, &pszPath);
		if (S_OK != getDisplayResult || nullptr == pszPath)
		{
			return E_FAIL;
		}

		LPWSTR pszExt = PathFindExtensionW(pszPath);
		if (nullptr == pszExt)
		{
			CoTaskMemFree(pszPath);
			return E_FAIL;
		}

		if (pszExt[0] == 0) {
			CoTaskMemFree(pszPath);
			return S_OK;
		}

		std::wstring needle{ pszExt };
		needle += L".";

		if (StrStrIW(supportedExts, needle.c_str()) != nullptr) {
			*pCmdState = ECS_ENABLED;
		}

		CoTaskMemFree(pszPath);

		return S_OK;
	}

	IFACEMETHODIMP Invoke(IShellItemArray* psiItemArray, IBindCtx* pbc) noexcept override {
		if (psiItemArray == nullptr) {
			return E_FAIL;
		}

		IShellItem* shellItem = nullptr;
		HRESULT getItemResult = psiItemArray->GetItemAt(0, &shellItem);

		if (S_OK != getItemResult || nullptr == shellItem) {
			return E_FAIL;
		}

		LPWSTR pszPath;
		HRESULT getDisplayResult = shellItem->GetDisplayName(SIGDN_FILESYSPATH, &pszPath);
		if (S_OK != getDisplayResult || nullptr == pszPath)
		{
			return E_FAIL;
		}

		std::wstring exePath = get_module_folder(g_hInst);
		exePath += L"\\FluentInfo.exe";

		std::wstring cmdline{ L"\"" };
		cmdline += exePath;
		cmdline += L"\" \"";
		cmdline += pszPath;
		cmdline += L"\"";

		CoTaskMemFree(pszPath);

		STARTUPINFO startupInfo{ 0 };

		startupInfo.cb = sizeof(STARTUPINFO);
		startupInfo.dwFlags = STARTF_USESHOWWINDOW;
		startupInfo.wShowWindow = SW_SHOWNORMAL;

		PROCESS_INFORMATION processInformation;

		bool result = CreateProcessW(
			exePath.c_str(),
			cmdline.data(),
			nullptr,
			nullptr,
			false,
			0,
			nullptr,
			nullptr,
			&startupInfo,
			&processInformation
		);

		if (!CloseHandle(processInformation.hProcess))
		{
			return HRESULT_FROM_WIN32(GetLastError());
		}
		if (!CloseHandle(processInformation.hThread))
		{
			return HRESULT_FROM_WIN32(GetLastError());
		}

		return S_OK;
	}

	IFACEMETHODIMP GetFlags(EXPCMDFLAGS* pFlags) noexcept override
	{
		*pFlags = ECF_DEFAULT;
		return S_OK;
	}

	IFACEMETHODIMP EnumSubCommands(IEnumExplorerCommand** ppEnum) noexcept override
	{
		*ppEnum = NULL;
		return E_NOTIMPL;
	}
};

struct __declspec(uuid("BEBB29FA-FBE2-406B-8CF3-BFA47C45D545"))
	CClassFactory : winrt::implements<CClassFactory, IClassFactory>
{
	IFACEMETHODIMP CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) noexcept override
	{
		try
		{
			return winrt::make<CExplorerCommandVerb>()->QueryInterface(riid, ppvObject);
		}
		catch (...)
		{
			return winrt::to_hresult();
		}
	}

	IFACEMETHODIMP LockServer(BOOL fLock) noexcept override
	{
		if (fLock)
		{
			++winrt::get_module_lock();
		}
		else
		{
			--winrt::get_module_lock();
		}

		return S_OK;
	}
};

STDAPI_(BOOL) DllMain(_In_opt_ HINSTANCE hinst, DWORD reason, _In_opt_ void*)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		g_hInst = hinst;
		DisableThreadLibraryCalls(hinst);
	}

	return TRUE;
}

STDAPI DllCanUnloadNow()
{
	if (winrt::get_module_lock())
	{
		return S_FALSE;
	}

	winrt::clear_factory_cache();
	return S_OK;
}

STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _COM_Outptr_ void** ppv)
{
	try
	{
		*ppv = nullptr;

		if (rclsid == __uuidof(CClassFactory))
		{
			return winrt::make<CClassFactory>()->QueryInterface(riid, ppv);
		}

		return winrt::hresult_class_not_available().to_abi();
	}
	catch (...)
	{
		return winrt::to_hresult();
	}
}
