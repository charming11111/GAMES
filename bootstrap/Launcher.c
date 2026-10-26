/* MIT license. Native Windows entry point: no CLR import or .NET prerequisite.
 * Checks Microsoft's v4 Full Release registry value before loading the CLR.
 * The managed payload is appended by build-native.ps1 and SHA-256 checked.
 */
#define UNICODE
#define _UNICODE
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <wchar.h>
#include "Messages.h"

#pragma pack(push, 1)
typedef struct BundleFooter {
    char magic[8];
    DWORD version;
    unsigned long long offset;
    unsigned long long length;
    BYTE sha256[32];
} BundleFooter;
#pragma pack(pop)

typedef LONG (WINAPI *OpenKeyFn)(HKEY,LPCWSTR,DWORD,REGSAM,PHKEY);
typedef LONG (WINAPI *QueryValueFn)(HKEY,LPCWSTR,LPDWORD,LPDWORD,LPBYTE,LPDWORD);
typedef LONG (WINAPI *CloseKeyFn)(HKEY);
typedef BOOL (WINAPI *AcquireFn)(ULONG_PTR*,LPCWSTR,LPCWSTR,DWORD,DWORD);
typedef BOOL (WINAPI *CreateHashFn)(ULONG_PTR,DWORD,ULONG_PTR,DWORD,ULONG_PTR*);
typedef BOOL (WINAPI *HashDataFn)(ULONG_PTR,const BYTE*,DWORD,DWORD);
typedef BOOL (WINAPI *GetHashFn)(ULONG_PTR,DWORD,BYTE*,DWORD*,DWORD);
typedef BOOL (WINAPI *DestroyHashFn)(ULONG_PTR);
typedef BOOL (WINAPI *ReleaseCryptFn)(ULONG_PTR,DWORD);
typedef HINSTANCE (WINAPI *ShellExecuteFn)(HWND,LPCWSTR,LPCWSTR,LPCWSTR,LPCWSTR,INT);
typedef HRESULT (WINAPI *FolderFn)(HWND,int,HANDLE,DWORD,LPWSTR);
typedef HRESULT (WINAPI *CoInitFn)(LPVOID,DWORD);
typedef void (WINAPI *CoUninitFn)(void);
typedef HRESULT (WINAPI *CreateClrFn)(const GUID*,const GUID*,void**);
typedef HRESULT (WINAPI *GetRuntimeFn)(void*,LPCWSTR,const GUID*,void**);
typedef HRESULT (WINAPI *GetInterfaceFn)(void*,const GUID*,const GUID*,void**);
typedef HRESULT (WINAPI *StartRuntimeFn)(void*);
typedef HRESULT (WINAPI *ExecuteFn)(void*,LPCWSTR,LPCWSTR,LPCWSTR,LPCWSTR,DWORD*);
typedef ULONG (WINAPI *ReleaseFn)(void*);

static const GUID CLSID_MetaHost={0x9280188d,0x0e8e,0x4867,{0xb3,0x0c,0x7f,0xa8,0x38,0x84,0xe8,0xde}};
static const GUID IID_MetaHost={0xd332db9e,0xb9b3,0x4125,{0x82,0x07,0xa1,0x48,0x84,0xf5,0x32,0x16}};
static const GUID IID_RuntimeInfo={0xbd39d1d2,0xba2f,0x486a,{0x89,0xb0,0xb4,0xb0,0xcb,0x46,0x68,0x91}};
static const GUID CLSID_RuntimeHost={0x90f1a06e,0x7712,0x4762,{0x86,0xb5,0x7a,0x5e,0xba,0x6b,0xdb,0x02}};
static const GUID IID_RuntimeHost={0x90f1a06c,0x7712,0x4762,{0x86,0xb5,0x7a,0x5e,0xba,0x6b,0xdb,0x02}};

static HMODULE system_dll(LPCWSTR name) {
    WCHAR path[MAX_PATH]; UINT n=GetSystemDirectoryW(path,MAX_PATH);
    if(!n||n>MAX_PATH-64) return NULL;
    lstrcatW(path,L"\\");lstrcatW(path,name);return LoadLibraryW(path);
}
static int supported(DWORD release) {return release>=528040;}
static DWORD framework_release(void) {
    HMODULE dll=system_dll(L"advapi32.dll"); HKEY key; DWORD release=0,type=0,size;
    OpenKeyFn open; QueryValueFn query; CloseKeyFn close; int i;
    REGSAM views[2]={KEY_WOW64_64KEY,KEY_WOW64_32KEY};
    if(!dll)return 0;
    open=(OpenKeyFn)GetProcAddress(dll,"RegOpenKeyExW");
    query=(QueryValueFn)GetProcAddress(dll,"RegQueryValueExW");
    close=(CloseKeyFn)GetProcAddress(dll,"RegCloseKey");
    if(open&&query&&close)for(i=0;i<2;i++) {
        if(open(HKEY_LOCAL_MACHINE,L"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full",0,KEY_READ|views[i],&key)==ERROR_SUCCESS) {
            DWORD value=0;size=sizeof(value);
            if(query(key,L"Release",NULL,&type,(BYTE*)&value,&size)==ERROR_SUCCESS&&type==REG_DWORD&&size==sizeof(value)&&value>release)release=value;
            close(key);
        }
    }
    FreeLibrary(dll);return release;
}
static int sha256(const BYTE* data,DWORD length,BYTE out[32]) {
    HMODULE dll=system_dll(L"advapi32.dll");ULONG_PTR provider=0,hash=0;DWORD size=32;int ok=0;
    AcquireFn acquire;CreateHashFn create;HashDataFn add;GetHashFn finish;DestroyHashFn destroy;ReleaseCryptFn release;
    if(!dll)return 0;
    acquire=(AcquireFn)GetProcAddress(dll,"CryptAcquireContextW");create=(CreateHashFn)GetProcAddress(dll,"CryptCreateHash");
    add=(HashDataFn)GetProcAddress(dll,"CryptHashData");finish=(GetHashFn)GetProcAddress(dll,"CryptGetHashParam");
    destroy=(DestroyHashFn)GetProcAddress(dll,"CryptDestroyHash");release=(ReleaseCryptFn)GetProcAddress(dll,"CryptReleaseContext");
    if(acquire&&create&&add&&finish&&destroy&&release&&acquire(&provider,NULL,NULL,24,0xf0000000)) {
        if(create(provider,0x800c,0,0,&hash)){ok=add(hash,data,length,0)&&finish(hash,2,out,&size,0)&&size==32;destroy(hash);}
        release(provider,0);
    }
    FreeLibrary(dll);return ok;
}
static int query_file_size(HANDLE file,LARGE_INTEGER* size) {DWORD high=0,low;SetLastError(0);low=GetFileSize(file,&high);if(low==INVALID_FILE_SIZE&&GetLastError()!=0)return 0;size->QuadPart=((unsigned long long)high<<32)|low;return 1;}
static int read_bundle(LPCWSTR self,BundleFooter* footer,BYTE** payload) {
    HANDLE file=CreateFileW(self,GENERIC_READ,FILE_SHARE_READ|FILE_SHARE_DELETE,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
    LARGE_INTEGER size,pos;DWORD read;BYTE hash[32];int ok=0;*payload=NULL;
    if(file==INVALID_HANDLE_VALUE)return 0;
    if(!query_file_size(file,&size)||size.QuadPart<(LONGLONG)sizeof(*footer))goto done;
    pos.QuadPart=size.QuadPart-sizeof(*footer);
    if(!SetFilePointerEx(file,pos,NULL,FILE_BEGIN)||!ReadFile(file,footer,sizeof(*footer),&read,NULL)||read!=sizeof(*footer))goto done;
    if(memcmp(footer->magic,"GAMESPK1",8)||footer->version!=1||footer->length<128||footer->length>64*1024*1024||footer->offset+footer->length!=(unsigned long long)pos.QuadPart)goto done;
    *payload=(BYTE*)HeapAlloc(GetProcessHeap(),0,(SIZE_T)footer->length);if(!*payload)goto done;
    pos.QuadPart=footer->offset;
    if(!SetFilePointerEx(file,pos,NULL,FILE_BEGIN)||!ReadFile(file,*payload,(DWORD)footer->length,&read,NULL)||read!=footer->length)goto done;
    ok=(*payload)[0]=='M'&&(*payload)[1]=='Z'&&sha256(*payload,(DWORD)footer->length,hash)&&!memcmp(hash,footer->sha256,32);
done:
    CloseHandle(file);if(!ok&&*payload){HeapFree(GetProcessHeap(),0,*payload);*payload=NULL;}return ok;
}
static int cached_payload(LPCWSTR path,BundleFooter* footer) {
    HANDLE file;LARGE_INTEGER size;DWORD read;BYTE *data,hash[32];int ok=0;
    file=CreateFileW(path,GENERIC_READ,FILE_SHARE_READ|FILE_SHARE_DELETE,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
    if(file==INVALID_HANDLE_VALUE)return 0;
    if(!query_file_size(file,&size)||size.QuadPart!=footer->length){CloseHandle(file);return 0;}
    data=(BYTE*)HeapAlloc(GetProcessHeap(),0,(SIZE_T)footer->length);
    if(data){ok=ReadFile(file,data,(DWORD)footer->length,&read,NULL)&&read==footer->length&&sha256(data,read,hash)&&!memcmp(hash,footer->sha256,32);HeapFree(GetProcessHeap(),0,data);}
    CloseHandle(file);return ok;
}
static int write_verified(LPCWSTR path,const BYTE* data,BundleFooter* footer) {
    WCHAR temp[MAX_PATH];HANDLE file;DWORD written;int ok;
    if(cached_payload(path,footer))return 1;
    swprintf(temp,L"%ls.%lu.tmp",path,GetCurrentProcessId());
    file=CreateFileW(temp,GENERIC_WRITE,0,NULL,CREATE_ALWAYS,FILE_ATTRIBUTE_NORMAL,NULL);if(file==INVALID_HANDLE_VALUE)return 0;
    ok=WriteFile(file,data,(DWORD)footer->length,&written,NULL)&&written==footer->length&&FlushFileBuffers(file);CloseHandle(file);
    if(ok)ok=MoveFileExW(temp,path,MOVEFILE_REPLACE_EXISTING|MOVEFILE_WRITE_THROUGH);
    if(!ok)ok=cached_payload(path,footer);DeleteFileW(temp);return ok;
}
static const BYTE* bridge_bytes(DWORD* size) {
    HMODULE self=GetModuleHandleW(NULL);HRSRC resource=FindResourceW(self,MAKEINTRESOURCEW(101),MAKEINTRESOURCEW(10));HGLOBAL loaded;
    if(!resource)return NULL;*size=SizeofResource(self,resource);if(*size<128||*size>1024*1024)return NULL;
    loaded=LoadResource(self,resource);return loaded?(const BYTE*)LockResource(loaded):NULL;
}
static int extract_payload(BundleFooter* footer,const BYTE* payload,WCHAR path[MAX_PATH]) {
    HMODULE shell=system_dll(L"shell32.dll");FolderFn folder;WCHAR hex[65],core[MAX_PATH];DWORD size=0;BYTE keyInput[64],cacheKey[32];const BYTE* bridge;BundleFooter bridgeFooter;int i;
    bridge=bridge_bytes(&size);memset(&bridgeFooter,0,sizeof(bridgeFooter));bridgeFooter.length=size;
    if(!bridge||!sha256(bridge,size,bridgeFooter.sha256)||!shell)return 0;
    memcpy(keyInput,footer->sha256,32);memcpy(keyInput+32,bridgeFooter.sha256,32);if(!sha256(keyInput,64,cacheKey))return 0;
    folder=(FolderFn)GetProcAddress(shell,"SHGetFolderPathW");
    if(!folder||FAILED(folder(NULL,0x001c|0x8000,NULL,0,path))||lstrlenW(path)>MAX_PATH-130){FreeLibrary(shell);return 0;}
    FreeLibrary(shell);lstrcatW(path,L"\\GameShelf");CreateDirectoryW(path,NULL);
    lstrcatW(path,L"\\Runtime");CreateDirectoryW(path,NULL);
    for(i=0;i<32;i++)swprintf(hex+i*2,L"%02x",cacheKey[i]);hex[64]=0;
    lstrcatW(path,L"\\");lstrcatW(path,hex);if(!CreateDirectoryW(path,NULL)&&GetLastError()!=ERROR_ALREADY_EXISTS)return 0;
    lstrcpyW(core,path);lstrcatW(core,L"\\GAMES.Core.exe");if(!write_verified(core,payload,footer))return 0;
    lstrcatW(path,L"\\BootstrapBridge.dll");return write_verified(path,bridge,&bridgeFooter);
}
static int has_arg(LPCWSTR flag) {
    /* Only diagnostic flags use this helper; managed command-line parsing is exact. */
    LPCWSTR command=GetCommandLineW(),found=wcsstr(command,flag);size_t length=wcslen(flag);
    return found&&(found==command||found[-1]==' '||found[-1]=='\t')&&(found[length]==0||found[length]==' '||found[length]=='\t');
}
static void stdout_text(const char* value){DWORD written;HANDLE out=GetStdHandle(STD_OUTPUT_HANDLE);if(out&&out!=INVALID_HANDLE_VALUE)WriteFile(out,value,(DWORD)strlen(value),&written,NULL);}
static int missing_prompt(void) {
    if(MessageBoxW(NULL,MISSING_MESSAGE,WINDOW_TITLE,MB_YESNO|MB_ICONINFORMATION|MB_DEFBUTTON2)==IDYES){
        HMODULE shell=system_dll(L"shell32.dll");ShellExecuteFn open=shell?(ShellExecuteFn)GetProcAddress(shell,"ShellExecuteW"):NULL;
        if(!open||(INT_PTR)open(NULL,L"open",DOWNLOAD_URL,NULL,NULL,SW_SHOWNORMAL)<=32)MessageBoxW(NULL,DOWNLOAD_URL,WINDOW_TITLE,MB_OK|MB_ICONINFORMATION);
        if(shell)FreeLibrary(shell);
    }
    return 2;
}
static void release_interface(void* value){if(value)((ReleaseFn)(*(void***)value)[2])(value);}
static HRESULT run_managed(LPCWSTR path,DWORD* result) {
    HMODULE shim=system_dll(L"mscoree.dll"),ole=system_dll(L"ole32.dll");void *meta=NULL,*info=NULL,*host=NULL;HRESULT hr=E_FAIL;
    CreateClrFn create;CoInitFn init;CoUninitFn uninit;HRESULT apartment;
    if(!shim||!ole)goto done;
    create=(CreateClrFn)GetProcAddress(shim,"CLRCreateInstance");init=(CoInitFn)GetProcAddress(ole,"CoInitializeEx");uninit=(CoUninitFn)GetProcAddress(ole,"CoUninitialize");
    if(!create||!init||!uninit)goto done;apartment=init(NULL,2);if(FAILED(apartment))goto done;
    hr=create(&CLSID_MetaHost,&IID_MetaHost,&meta);
    if(SUCCEEDED(hr))hr=((GetRuntimeFn)(*(void***)meta)[3])(meta,L"v4.0.30319",&IID_RuntimeInfo,&info);
    if(SUCCEEDED(hr))hr=((GetInterfaceFn)(*(void***)info)[9])(info,&CLSID_RuntimeHost,&IID_RuntimeHost,&host);
    if(SUCCEEDED(hr))hr=((StartRuntimeFn)(*(void***)host)[3])(host);
    if(SUCCEEDED(hr))hr=((ExecuteFn)(*(void***)host)[11])(host,path,L"BootstrapEntry",L"Start",GetCommandLineW(),result);
    release_interface(host);release_interface(info);release_interface(meta);uninit();
done:
    /* The process exits immediately after the hosted application. Do not unload
       mscoree.dll manually: the CLR can still own background runtime threads. */
    return hr;
}
int WINAPI WinMain(HINSTANCE instance,HINSTANCE previous,LPSTR command,int show) {
    WCHAR self[MAX_PATH],managed[MAX_PATH];BundleFooter footer;BYTE* payload=NULL;DWORD code=0,release=framework_release();HRESULT hr;int valid,tests;DWORD bridgeSize;
    if(!GetModuleFileNameW(NULL,self,MAX_PATH)||lstrlenW(self)>=MAX_PATH-1)return 1;
    if(has_arg(L"--bootstrap-self-test")||has_arg(L"--diagnose-runtime")) {
        char report[256];valid=read_bundle(self,&footer,&payload);
        tests=!supported(0)&&!supported(528039)&&supported(528040)&&supported(528049)&&supported(533320)&&supported(600000)&&valid&&bridge_bytes(&bridgeSize)!=NULL;
        sprintf(report,"{\"native_bootstrap\":true,\"release\":%lu,\"framework_ready\":%s,\"payload_valid\":%s,\"checks_passed\":%s}\n",release,supported(release)?"true":"false",valid?"true":"false",tests?"true":"false");stdout_text(report);
        if(payload)HeapFree(GetProcessHeap(),0,payload);return tests?0:1;
    }
    if(has_arg(L"--simulate-missing-framework")||!supported(release))return missing_prompt();
    if(!read_bundle(self,&footer,&payload)||!extract_payload(&footer,payload,managed)) {
        if(payload)HeapFree(GetProcessHeap(),0,payload);MessageBoxW(NULL,BUNDLE_ERROR,WINDOW_TITLE,MB_OK|MB_ICONERROR);return 1;
    }
    HeapFree(GetProcessHeap(),0,payload);hr=run_managed(managed,&code);
    if(FAILED(hr)){WCHAR text[384];swprintf(text,L"%ls\nHRESULT: 0x%08lx",RUNTIME_ERROR,(unsigned long)hr);MessageBoxW(NULL,text,WINDOW_TITLE,MB_OK|MB_ICONERROR);return 1;}
    return (int)code;
}

/* No compiler CRT/runtime objects are linked: imports come from Windows DLLs. */
#ifdef __TINYC__
void _winstart(void) {ExitProcess((UINT)WinMain(GetModuleHandleW(NULL),NULL,NULL,SW_SHOWNORMAL));}
#endif
