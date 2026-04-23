

namespace CryptoVault;

public static class PasswordChecker
{
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

    public static bool CheckLower(string password)
    {
        foreach (char c in password)
        {
            if (!char.IsUpper(c))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CheckLenght(string password)
    {
        return password.Length >= 10;
    }

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