// greets to MCMrARM and dktapps from MCLauncher

#include <Unknwn.h>

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Security.Authentication.Web.Core.h>
#include <winrt/Windows.Security.Cryptography.h>
#include "winrt/Windows.Internal.Security.Authentication.Web.h"
#include <WebAuthenticationCoreManagerInterop.h>

#include <combaseapi.h>
#include <thread>

using namespace winrt;
using namespace Windows::Security::Authentication::Web::Core;
using namespace Windows::Internal::Security::Authentication::Web;
using namespace Windows::Security::Cryptography;

#define WU_NO_ACCOUNT MAKE_HRESULT(SEVERITY_ERROR, FACILITY_ITF, 0x200)
#define WU_TOKEN_FETCH_ERROR_BASE MAKE_HRESULT(SEVERITY_ERROR, FACILITY_ITF, 0x400)

extern "C" __declspec(dllexport) int __stdcall GetToken(wchar_t** outToken, HWND window)
{
    auto tokenBrokerStatics = get_activation_factory<TokenBrokerInternal, Windows::Foundation::IUnknown>();
    auto statics = tokenBrokerStatics.as<ITokenBrokerInternalStatics>();
    auto accounts = statics.FindAllAccountsAsync().get();
    if (accounts.Size() == 0)
        return WU_NO_ACCOUNT;
    
    auto account = accounts.GetAt(0);
    auto accountProvider = WebAuthenticationCoreManager::FindAccountProviderAsync(
        L"https://login.microsoft.com", L"consumers").get();
    
    WebTokenRequest request(accountProvider, L"service::dcat.update.microsoft.com::MBI_SSL",
        L"{28520974-CE92-4F36-A219-3F255AF7E61E}");

    // THIS IS WHERE THE CODE CHANGES
    auto result = WebAuthenticationCoreManager::GetTokenSilentlyAsync(request, account).get();
    
    if (result.ResponseStatus() == WebTokenRequestStatus::UserInteractionRequired)
    {
        // boy i sure hope this isn't 0 because if so i will be very sad
        HWND window = FindWindow(nullptr, L"Jiayi Launcher");

        // regular params but they are IInspectable
        IInspectable* requestInspectable = request.as<IInspectable>().get();
        IInspectable* accountInspectable = account.as<IInspectable>().get();

        // iid for some odd reason
        constexpr guid iid { guid_of<Windows::Foundation::IAsyncOperation<WebTokenRequestResult>>() };

        // get this instance
        auto instance = get_activation_factory<WebAuthenticationCoreManager,
            IWebAuthenticationCoreManagerStatics>().as<IWebAuthenticationCoreManagerInterop>();

        // go
        auto hresult = instance->RequestTokenWithWebAccountForWindowAsync(window, requestInspectable,
            accountInspectable, iid, put_abi(result));
    }

    if (result.ResponseStatus() != WebTokenRequestStatus::Success)
    {
        return WU_TOKEN_FETCH_ERROR_BASE | static_cast<int32_t>(result.ResponseStatus());
    }
    
    auto token = result.ResponseData().GetAt(0).Token();
    auto tokenBinary = CryptographicBuffer::ConvertStringToBinary(token, BinaryStringEncoding::Utf16LE);
    auto tokenBase64 = CryptographicBuffer::EncodeToBase64String(tokenBinary);

    *outToken = (wchar_t*)CoTaskMemAlloc((tokenBase64.size() + 1) * sizeof(wchar_t));
    memcpy(*outToken, tokenBase64.data(), (tokenBase64.size() + 1) * sizeof(wchar_t));
    
    return S_OK;
}