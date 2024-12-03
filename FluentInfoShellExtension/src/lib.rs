use std::ptr::null_mut;

use widestring::{u16str, U16CStr, U16CString, U16Str, U16String};
use windows::{
    core::*,
    Win32::{
        Foundation::*,
        System::{
            Com::*,
            LibraryLoader::GetModuleFileNameW,
            SystemServices::DLL_PROCESS_ATTACH,
            Threading::{
                CreateProcessW, PROCESS_CREATION_FLAGS, PROCESS_INFORMATION, STARTF_USESHOWWINDOW,
                STARTUPINFOW,
            },
        },
        UI::{Shell::*, WindowsAndMessaging::SW_SHOWNORMAL},
    },
};

static mut DLL_INSTANCE: HINSTANCE = HINSTANCE(null_mut());

fn get_module_folder() -> U16CString {
    let mut buf = [0; (MAX_PATH + 1) as usize];
    unsafe { GetModuleFileNameW(DLL_INSTANCE, &mut buf) };

    if unsafe { GetLastError() } == ERROR_INSUFFICIENT_BUFFER {
        let mut long_buf = vec![0; 0xFFFF]; // should be always enough
        unsafe { GetModuleFileNameW(DLL_INSTANCE, &mut long_buf) };
        unsafe {
            let _ = PathRemoveFileSpecW(PWSTR::from_raw(long_buf.as_mut_ptr()));
        };
        U16CString::from_vec_truncate(long_buf)
    } else {
        unsafe {
            let _ = PathRemoveFileSpecW(PWSTR::from_raw(buf.as_mut_ptr()));
        };
        U16CString::from_ustr_truncate(U16Str::from_slice(&buf))
    }
}

#[implement(IExplorerCommand)]
struct COpenFluentInfo;

impl IExplorerCommand_Impl for COpenFluentInfo_Impl {
    fn GetTitle(&self, _psiitemarray: Option<&IShellItemArray>) -> Result<PWSTR> {
        unsafe { SHStrDupW(w!("FluentInfo")) }
    }

    fn GetIcon(&self, _psiitemarray: Option<&IShellItemArray>) -> Result<PWSTR> {
        let mut path = get_module_folder().into_ustring();
        path += u16str!("\\Assets\\fluentinfo.ico");

        let path = unsafe { U16CString::from_ustr_unchecked(path) };
        unsafe { SHStrDupW(PCWSTR::from_raw(path.as_ptr())) }
    }

    fn GetToolTip(&self, _psiitemarray: Option<&IShellItemArray>) -> Result<PWSTR> {
        Err(E_NOTIMPL.into())
    }

    fn GetCanonicalName(&self) -> Result<GUID> {
        Ok(GUID::zeroed())
    }

    fn GetState(&self, psiitemarray: Option<&IShellItemArray>, _foktobeslow: BOOL) -> Result<u32> {
        let array = psiitemarray.ok_or(E_INVALIDARG)?;
        let count = unsafe { array.GetCount() }?;

        if count == 0 || count > 1 {
            return Ok(ECS_HIDDEN.0 as u32);
        }

        Ok(ECS_ENABLED.0 as u32)
    }

    fn Invoke(
        &self,
        psiitemarray: Option<&IShellItemArray>,
        _pbc: Option<&IBindCtx>,
    ) -> Result<()> {
        let array = psiitemarray.ok_or(E_INVALIDARG)?;
        let item = unsafe { array.GetItemAt(0) }?;
        let filepath = unsafe { item.GetDisplayName(SIGDN_FILESYSPATH) }?;

        let exe_path = get_module_folder().into_ustring() + u16str!("\\FluentInfo.exe");
        let mut cmd_line: U16String = u16str!("\"").into();
        cmd_line.push(&exe_path);
        cmd_line.push(u16str!("\" \""));
        cmd_line.push(unsafe { U16CStr::from_ptr_str(filepath.as_ptr()) });
        cmd_line.push(u16str!("\""));

        unsafe { CoTaskMemFree(Some(filepath.as_ptr() as *const core::ffi::c_void)) };

        let mut startup_info = STARTUPINFOW::default();
        startup_info.cb = size_of::<STARTUPINFOW>() as u32;
        startup_info.dwFlags = STARTF_USESHOWWINDOW;
        startup_info.wShowWindow = SW_SHOWNORMAL.0 as u16;

        let mut process_information = PROCESS_INFORMATION::default();

        let exe_path = unsafe { U16CString::from_ustr_unchecked(exe_path) };
        let mut cmd_line = unsafe { U16CString::from_ustr_unchecked(cmd_line) };

        unsafe {
            CreateProcessW(
                PCWSTR::from_raw(exe_path.as_ptr()),
                PWSTR::from_raw(cmd_line.as_mut_ptr()),
                None,
                None,
                false,
                PROCESS_CREATION_FLAGS::default(),
                None,
                None,
                &startup_info,
                &mut process_information,
            )?
        };

        let _ = unsafe { CloseHandle(process_information.hProcess) };
        let _ = unsafe { CloseHandle(process_information.hThread) };

        Ok(())
    }

    fn GetFlags(&self) -> Result<u32> {
        Ok(ECF_DEFAULT.0 as u32)
    }

    fn EnumSubCommands(&self) -> Result<IEnumExplorerCommand> {
        Err(E_NOTIMPL.into())
    }
}

#[implement(IClassFactory)]
struct CClassFactory;

impl IClassFactory_Impl for CClassFactory_Impl {
    fn CreateInstance(
        &self,
        punkouter: Option<&IUnknown>,
        riid: *const GUID,
        ppvobject: *mut *mut core::ffi::c_void,
    ) -> Result<()> {
        if punkouter.is_none() {
            let verb: IExplorerCommand = COpenFluentInfo {}.into();
            unsafe { verb.query(riid, ppvobject).ok() }
        } else {
            CLASS_E_NOAGGREGATION.ok()
        }
    }

    fn LockServer(&self, _flock: BOOL) -> Result<()> {
        S_OK.ok()
    }
}

#[no_mangle]
extern "system" fn DllMain(
    hinstdll: HINSTANCE,
    fdwreason: u32,
    _lpvreserved: *mut core::ffi::c_void,
) -> bool {
    if fdwreason == DLL_PROCESS_ATTACH {
        unsafe { DLL_INSTANCE = hinstdll };
    }

    true
}

// https://github.com/microsoft/windows-rs/issues/2472
// #[no_mangle]
// extern "system" fn DllCanUnloadNow() -> HRESULT {
//     S_FALSE
// }

const MODULE_ID: GUID = GUID::from_values(
    // {BEBB29FA-FBE2-406B-8CF3-BFA47C45D545}
    0xbebb29fa,
    0xfbe2,
    0x406b,
    [0x8c, 0xf3, 0xbf, 0xa4, 0x7c, 0x45, 0xd5, 0x45],
);

#[no_mangle]
extern "system" fn DllGetClassObject(
    rclsid: *const GUID,
    riid: *const GUID,
    ppv: *mut *mut core::ffi::c_void,
) -> HRESULT {
    unsafe {
        *ppv = null_mut();
    };

    if MODULE_ID == unsafe { *rclsid } {
        let class_factory: IClassFactory = CClassFactory {}.into();
        unsafe { class_factory.query(riid, ppv) }
    } else {
        CLASS_E_CLASSNOTAVAILABLE
    }
}
