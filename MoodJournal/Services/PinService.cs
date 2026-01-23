using System.Security.Cryptography;
using System.Text;

namespace MoodJournal.Services;

public class PinService
{
    private const string KeyEnabled = "pin_enabled";
    private const string KeyHash = "pin_hash";

    public bool IsEnabled => Preferences.Get(KeyEnabled, false);

    public void Disable()
    {
        Preferences.Set(KeyEnabled, false);
        Preferences.Remove(KeyHash);
    }

    public void EnableWithPin(string pin)
    {
        Preferences.Set(KeyHash, Hash(pin));
        Preferences.Set(KeyEnabled, true);
    }

    public bool Verify(string pin)
    {
        if (!IsEnabled) return true;

        var saved = Preferences.Get(KeyHash, "");
        if (string.IsNullOrWhiteSpace(saved)) return false;

        return string.Equals(saved, Hash(pin), StringComparison.Ordinal);
    }

    public bool ChangePin(string oldPin, string newPin)
    {
        if (!Verify(oldPin)) return false;

        Preferences.Set(KeyHash, Hash(newPin));
        Preferences.Set(KeyEnabled, true);
        return true;
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}

