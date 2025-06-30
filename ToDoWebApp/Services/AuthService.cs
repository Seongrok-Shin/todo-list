using Supabase.Gotrue;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ToDoWebApp.Services
{
    public class AuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<AuthService> _logger;

        public User? CurrentUser { get; private set; } // Current logged-in user information
        public event Action? OnAuthStateChanged; // Event to subscribe to authentication state changes

        public AuthService(Supabase.Client supabaseClient, NavigationManager navigationManager, ILogger<AuthService> logger)
        {
            _supabaseClient = supabaseClient;
            _navigationManager = navigationManager;
            _logger = logger;

            // Initialize current user on service creation
            InitializeCurrentUser();
        }

        // Initialize current user from existing session
        private void InitializeCurrentUser()
        {
            try
            {
                CurrentUser = _supabaseClient.Auth.CurrentUser;
                OnAuthStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initial user");
            }
        }

        // Sign up method
        public async Task<(User? User, string? ErrorMessage)> SignUp(string email, string password)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignUp(email, password);
                if (response?.User != null)
                {
                    CurrentUser = response.User;
                    OnAuthStateChanged?.Invoke();
                    // Email confirmation may be required depending on Supabase settings
                    return (response.User, response.User.EmailConfirmedAt == null ? "Sign up successful! Email confirmation required." : null);
                }
                return (null, "Sign up failed: Unknown error");
            }
            catch (Exception ex)
            {
                return (null, $"Sign up failed: {ex.Message}");
            }
        }

        // Sign in method
        public async Task<(User? User, string? ErrorMessage)> SignIn(string email, string password)
        {
            try
            {
                var response = await _supabaseClient.Auth.SignIn(email, password);
                if (response?.User != null)
                {
                    CurrentUser = response.User;
                    OnAuthStateChanged?.Invoke();
                    return (response.User, null);
                }
                return (null, "Sign in failed: Invalid email or password");
            }
            catch (Exception ex)
            {
                return (null, $"Sign in failed: {ex.Message}");
            }
        }

        // Sign out method
        public async Task SignOut()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sign out");
                // Continue with cleanup even if sign out fails
            }
            finally
            {
                // Always clear user state and navigate, regardless of success or failure
                CurrentUser = null;
                OnAuthStateChanged?.Invoke();
                _navigationManager.NavigateTo("/", forceLoad: true); // Navigate to home and refresh entire app
            }
        }

        // Check if user is logged in
        public bool IsLoggedIn => CurrentUser != null;

        // Refresh current user state
        public Task RefreshUser()
        {
            try
            {
                // Try to get current user from the client
                CurrentUser = _supabaseClient.Auth.CurrentUser;
                OnAuthStateChanged?.Invoke();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing user");
                return Task.CompletedTask;
            }
        }
    }
}