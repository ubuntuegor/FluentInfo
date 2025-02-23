use std::{
    collections::HashSet,
    ffi::OsString,
    os::windows::ffi::{OsStrExt, OsStringExt},
    path::{Path, PathBuf},
    ptr::null_mut,
    sync::LazyLock,
};

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

static mut DLL_INSTANCE: Option<HINSTANCE> = None;
static SUPPORTED_EXTENSIONS: LazyLock<HashSet<&str>> = LazyLock::new(|| {
    HashSet::from([
        "aiff", "aif", "avi", "m2ts", "mts", "flv", "heif", "heic", "avif", "ts", "mov", "mp4",
        "m4v", "m4a", "3gpp", "3gp", "qt", "ogg", "opus", "swf", "wma", "wmv", "avc", "h264",
        "hevc", "h265", "aac", "ac3", "ac4", "amr", "dts", "dtshd", "eac3", "ec3", "flac", "mp1",
        "mp2", "mp3", "wav", "bmp", "gif", "ico", "jpg", "jpeg", "png", "tif", "tiff", "mkv",
        "webm", "oga", "ogv",
    ])
});

fn get_module_folder() -> PathBuf {
    let mut buf: [u16; 261] = [0; (MAX_PATH + 1) as usize];
    let len = unsafe { GetModuleFileNameW(DLL_INSTANCE.map(|x| x.into()), &mut buf) as usize };

    let path = if unsafe { GetLastError() } == ERROR_INSUFFICIENT_BUFFER {
        let mut buf = vec![0; 0xFFFF]; // should be always enough
        let len = unsafe { GetModuleFileNameW(DLL_INSTANCE.map(|x| x.into()), &mut buf) as usize };
        OsString::from_wide(&buf[..len])
    } else {
        OsString::from_wide(&buf[..len])
    };

    Path::new(&path).parent().unwrap().to_path_buf()
}

trait ToWideWithNul {
    fn to_wide_with_nul(&self) -> Vec<u16>;
}

impl<T> ToWideWithNul for T
where
    T: ?Sized + OsStrExt,
{
    fn to_wide_with_nul(&self) -> Vec<u16> {
        self.encode_wide().chain(Some(0)).collect()
    }
}

#[implement(IExplorerCommand)]
struct COpenFluentInfo;

impl IExplorerCommand_Impl for COpenFluentInfo_Impl {
    fn GetTitle(&self, _psiitemarray: Ref<IShellItemArray>) -> Result<PWSTR> {
        unsafe { SHStrDupW(w!("FluentInfo")) }
    }

    fn GetIcon(&self, _psiitemarray: Ref<IShellItemArray>) -> Result<PWSTR> {
        let mut path = get_module_folder();
        path.push("Assets");
        path.push("fluentinfo.ico");

        let path = path.as_os_str().to_wide_with_nul();
        unsafe { SHStrDupW(PCWSTR::from_raw(path.as_ptr())) }
    }

    fn GetToolTip(&self, _psiitemarray: Ref<IShellItemArray>) -> Result<PWSTR> {
        Err(E_NOTIMPL.into())
    }

    fn GetCanonicalName(&self) -> Result<GUID> {
        Ok(GUID::zeroed())
    }

    fn GetState(&self, psiitemarray: Ref<IShellItemArray>, _foktobeslow: BOOL) -> Result<u32> {
        let array = psiitemarray.ok()?;
        let count = unsafe { array.GetCount() }?;

        if count == 0 || count > 1 {
            return Ok(ECS_HIDDEN.0 as u32);
        }

        let item = unsafe { array.GetItemAt(0) }?;
        let filepath = unsafe { item.GetDisplayName(SIGDN_FILESYSPATH) }?;
        let my_filepath = OsString::from_wide(unsafe { filepath.as_wide() });
        unsafe { CoTaskMemFree(Some(filepath.as_ptr() as *const core::ffi::c_void)) };

        let extension = Path::new(&my_filepath).extension().ok_or(E_FAIL)?;
        if SUPPORTED_EXTENSIONS.contains(extension.to_str().unwrap()) {
            Ok(ECS_ENABLED.0 as u32)
        } else {
            Ok(ECS_HIDDEN.0 as u32)
        }
    }

    fn Invoke(&self, psiitemarray: Ref<IShellItemArray>, _pbc: Ref<IBindCtx>) -> Result<()> {
        let array = psiitemarray.ok()?;
        let item = unsafe { array.GetItemAt(0) }?;
        let filepath = unsafe { item.GetDisplayName(SIGDN_FILESYSPATH) }?;

        let mut exe_path = get_module_folder();
        exe_path.push("FluentInfo.exe");
        let mut cmd_line = OsString::new();
        cmd_line.push("\"");
        cmd_line.push(&exe_path);
        cmd_line.push("\" \"");
        cmd_line.push(OsString::from_wide(unsafe { filepath.as_wide() }));
        cmd_line.push("\"");

        unsafe { CoTaskMemFree(Some(filepath.as_ptr() as *const core::ffi::c_void)) };

        let startup_info = STARTUPINFOW {
            cb: size_of::<STARTUPINFOW>() as u32,
            dwFlags: STARTF_USESHOWWINDOW,
            wShowWindow: SW_SHOWNORMAL.0 as u16,
            ..Default::default()
        };

        let mut process_information = PROCESS_INFORMATION::default();

        let exe_path = exe_path.as_os_str().to_wide_with_nul();
        let mut cmd_line = cmd_line.to_wide_with_nul();

        unsafe {
            CreateProcessW(
                PCWSTR::from_raw(exe_path.as_ptr()),
                Some(PWSTR::from_raw(cmd_line.as_mut_ptr())),
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
        punkouter: Ref<IUnknown>,
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
        unsafe { DLL_INSTANCE = Some(hinstdll) };
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
