using Supabase;
using Supabase.Gotrue;

namespace ToDoWebApp.Services;

public class AuthService
{
    private readonly Supabase.Client _supabaseClient;
    private User? _currentUser;

    public AuthService(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
        _currentUser = _supabaseClient.Auth.CurrentUser;
    }

    public event Action? AuthStateChanged;
    public event Action? OnAuthStateChanged
    {
        add => AuthStateChanged += value;
        remove => AuthStateChanged -= value;
    }

    public User? CurrentUser => _currentUser;

    public bool IsLoggedIn => _currentUser != null;

    public string? GetUserEmail() => _currentUser?.Email ?? "Unknown User";

    public string? GetUserId() => _currentUser?.Id;

    public async Task<(User? user, string? error)> SignUp(string email, string password)
    {
        try
        {
            var result = await _supabaseClient.Auth.SignUp(email, password);
            
            if (result?.User != null)
            {
                // 회원가입 후 자동 로그인하지 않음
                return (result.User, null);
            }
            
            return (null, "Failed to create account");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(User? user, string? error)> SignIn(string email, string password)
    {
        try
        {
            var result = await _supabaseClient.Auth.SignIn(email, password);
            
            if (result?.User != null)
            {
                _currentUser = result.User;
                // UI 업데이트를 위해 이벤트 발생
                AuthStateChanged?.Invoke();
                return (result.User, null);
            }
            
            return (null, "Invalid email or password");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task SignOut()
    {
        try
        {
            await _supabaseClient.Auth.SignOut();
        }
        catch (Exception)
        {
            // 서버 로그아웃 실패해도 계속 진행
        }
        finally
        {
            // 항상 로컬 상태 클리어하고 UI 업데이트
            _currentUser = null;
            AuthStateChanged?.Invoke();
        }
    }

    public async Task<bool> InitializeAuthState()
    {
        try
        {
            var session = _supabaseClient.Auth.CurrentSession;
            _currentUser = session?.User;
            AuthStateChanged?.Invoke();
            return _currentUser != null;
        }
        catch (Exception)
        {
            _currentUser = null;
            AuthStateChanged?.Invoke();
            return false;
        }
    }

    // AuthCallback.razor에서 사용하는 메서드 추가
    public async Task<bool> SetSessionFromTokens(string accessToken, string refreshToken)
    {
        try
        {
            await _supabaseClient.Auth.SetSession(accessToken, refreshToken);
            _currentUser = _supabaseClient.Auth.CurrentUser;
            AuthStateChanged?.Invoke();
            return _currentUser != null;
        }
        catch (Exception)
        {
            _currentUser = null;
            AuthStateChanged?.Invoke();
            return false;
        }
    }

    // 세션 갱신 메서드
    public async Task<bool> RefreshSession()
    {
        try
        {
            var result = await _supabaseClient.Auth.RefreshSession();
            if (result?.User != null)
            {
                _currentUser = result.User;
                AuthStateChanged?.Invoke();
                return true;
            }
            
            _currentUser = null;
            AuthStateChanged?.Invoke();
            return false;
        }
        catch (Exception)
        {
            _currentUser = null;
            AuthStateChanged?.Invoke();
            return false;
        }
    }
}