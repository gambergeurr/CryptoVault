namespace CryptoVault;

/// <summary>
/// Provides utility methods to check different heuristics of password strength.
/// </summary>
public static class PasswordChecker
{
    /// <summary>
    /// Checks if the password contains at least one uppercase letter.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>True if it contains an uppercase letter, otherwise false.</returns>
    public static bool CheckCaps(string password)
    {
        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the password contains at least one lowercase letter.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>True if it contains a lowercase letter, otherwise false.</returns>
    public static bool CheckLower(string password)
    {
        foreach (char c in password)
        {
            if (char.IsLower(c))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the password length is at least 10 characters.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>True if length is greater than or equal to 10, otherwise false.</returns>
    public static bool CheckLenght(string password)
    {
        return password.Length >= 10;
    }

    /// <summary>
    /// Checks if the password contains at least one numeric digit.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>True if it contains a digit, otherwise false.</returns>
    public static bool CheckNumber(string password)
    {
        foreach (char c in password)
        {
            if (char.IsDigit(c))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the password contains at least one special character (symbol).
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <returns>True if it contains a special character, otherwise false.</returns>
    public static bool CheckSpecial(string password)
    {
        foreach (char c in password)
        {
            if (!char.IsLetter(c) && !char.IsNumber(c))
            {
                return true;
            }
        }
        return false;
    }
}