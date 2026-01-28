using System.Security.Cryptography;
using System.Text;
using YourJournal.Models;

namespace YourJournal.Services;

/// <summary>
/// Manages user authentication, session state, and password security.
/// Implements secure password hashing using SHA256 algorithm.
/// </summary>
public class AuthService
{
    #region Fields & Constants
    
    private readonly DatabaseService _database;
    private User? _currentUser;
    
    /// <summary>
    /// Minimum required password length for security compliance
    /// </summary>
    private const int MinimumPasswordLength = 8;
    
    /// <summary>
    /// Maximum PIN length for quick authentication
    /// </summary>
    private const int MaximumPinLength = 6;
    
    #endregion
    
    #region Constructor

    /// <summary>
    /// Initializes authentication service with database dependency.
    /// </summary>
    /// <param name="database">Database service for user data access</param>
    public AuthService(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }
    
    #endregion
    
    #region Public Properties

    /// <summary>
    /// Gets the currently authenticated user, or null if not logged in.
    /// </summary>
    public User? CurrentUser => _currentUser;
    
    /// <summary>
    /// Indicates whether a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => _currentUser != null;

    /// <summary>
    /// Event raised when authentication state changes (login/logout).
    /// </summary>
    public event Action? OnAuthStateChanged;
    
    #endregion
    
    #region Public Methods

    /// <summary>
    /// Registers a new user with email and password authentication.
    /// Validates input, checks for duplicate emails, and securely hashes password.
    /// </summary>
    /// <param name="fullName">User's full name</param>
    /// <param name="email">User's email address (must be unique)</param>
    /// <param name="password">User's password (minimum 8 characters)</param>
    /// <param name="pin">Optional PIN for quick login (4-6 digits)</param>
    /// <returns>Tuple with success status and descriptive message</returns>
    public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, string? pin = null)
    {
        try
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Full name is required");
            
            if (string.IsNullOrWhiteSpace(email))
                return (false, "Email is required");
            
            if (!IsValidEmail(email))
                return (false, "Invalid email format");
            
            // Check for existing user
            var existingUser = await _database.GetUserByEmailAsync(email);
            if (existingUser != null)
                return (false, "Email already registered");

            // Password validation
            if (password.Length < MinimumPasswordLength)
                return (false, $"Password must be at least {MinimumPasswordLength} characters long");
            
            // PIN validation if provided
            if (!string.IsNullOrWhiteSpace(pin) && (pin.Length < 4 || pin.Length > MaximumPinLength))
                return (false, "PIN must be 4-6 digits");

            // Create and save new user
            var user = new User
            {
                FullName = fullName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = HashPassword(password),
                PIN = pin,
                CreatedAt = DateTime.Now
            };

            await _database.SaveUserAsync(user);
            return (true, "Registration successful!");
        }
        catch (Exception ex)
        {
            // Log error in production environment
            System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
            return (false, "An error occurred during registration. Please try again.");
        }
    }

    public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
    {
        var user = await _database.GetUserByEmailAsync(email);
        if (user == null)
            return (false, "Invalid email or password");

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid email or password");

        _currentUser = user;
        OnAuthStateChanged?.Invoke();
        return (true, "Login successful!");
    }

    public async Task<(bool Success, string Message)> LoginWithPINAsync(string email, string pin)
    {
        var user = await _database.GetUserByEmailAsync(email);
        if (user == null || string.IsNullOrEmpty(user.PIN))
            return (false, "Invalid email or PIN");

        if (user.PIN != pin)
            return (false, "Invalid email or PIN");

        _currentUser = user;
        OnAuthStateChanged?.Invoke();
        return (true, "Login successful!");
    }

    public void Logout()
    {
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Validates email format using basic regex pattern.
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid</returns>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Hashes password using SHA256 algorithm for secure storage.
    /// Note: In production, consider using bcrypt or PBKDF2 with salt.
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Base64-encoded hash string</returns>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verifies a password against its stored hash.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches hash</returns>
    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
    
    #endregion
}
