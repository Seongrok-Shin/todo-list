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
                // First, check if user already exists by trying to sign in
                var existingUserCheck = await _supabaseClient.Auth.SignIn(email, password);
                if (existingUserCheck?.User != null)
                {
                    // User exists and password is correct - sign them out immediately
                    await _supabaseClient.Auth.SignOut();
                    CurrentUser = null;
                    OnAuthStateChanged?.Invoke();
                    return (null, "User already registered");
                }
            }
            catch
            {
                // If sign in fails, it means user doesn't exist or password is wrong
                // Continue with signup process
            }

            try
            {
                var response = await _supabaseClient.Auth.SignUp(email, password);
                if (response?.User != null)
                {
                    // Don't automatically log in the user after signup
                    // Just return the user info for confirmation but don't set CurrentUser
                    await _supabaseClient.Auth.SignOut(); // Ensure we're signed out
                    CurrentUser = null;
                    OnAuthStateChanged?.Invoke();
                    
                    return (response.User, response.User.EmailConfirmedAt == null ? "Sign up successful! Email confirmation required." : "Sign up successful!");
                }
                return (null, "Sign up failed: Unknown error");
            }
            catch (Exception ex)
            {
                // Check if the error message indicates user already exists
                if (ex.Message.Contains("User already registered") || 
                    ex.Message.Contains("already exists") || 
                    ex.Message.Contains("duplicate"))
                {
                    return (null, "User already registered");
                }
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
                // Always clear user state first
                CurrentUser = null;
                OnAuthStateChanged?.Invoke();
            }
            
            // Small delay to allow UI state changes to complete before navigation
            await Task.Delay(100);
            
            // Navigate without forceLoad to avoid TaskCanceledException
            _navigationManager.NavigateTo("/", false);
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

        // Add this new method to handle session from email confirmation
        public async Task<bool> SetSessionFromTokens(string accessToken, string refreshToken)
        {
            try
            {
                await _supabaseClient.Auth.SetSession(accessToken, refreshToken);
                CurrentUser = _supabaseClient.Auth.CurrentUser;
                OnAuthStateChanged?.Invoke();
                return CurrentUser != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session from tokens");
                return false;
            }
        }
    }
}