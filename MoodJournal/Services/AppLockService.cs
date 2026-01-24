using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace MoodJournal.Services;

public sealed class AppLockService
{
    private const string KeyEnabled = "mj_lock_enabled";
    private const string KeySalt    = "mj_pin_salt";
    private const string KeyHash    = "mj_pin_hash";

    public bool IsLockEnabled { get; private set; }
    public bool IsUnlocked { get; private set; } = true;

    public bool IsLocked => IsLockEnabled && !IsUnlocked;

    public event Action? Changed;

    public Task InitAsync()
    {
        IsLockEnabled = Preferences.Default.Get(KeyEnabled, false);
        IsUnlocked = !IsLockEnabled; //if enabled then start locking
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public void EnableWithPin(string pin)
    {
        pin = (pin ?? "").Trim();
        if (pin.Length < 4) throw new ArgumentException("PIN must be at least 4 digits.");

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HashPin(pin, salt);

        Preferences.Default.Set(KeyEnabled, true);
        Preferences.Default.Set(KeySalt, Convert.ToBase64String(salt));
        Preferences.Default.Set(KeyHash, Convert.ToBase64String(hash));

        IsLockEnabled = true;
        IsUnlocked = false; //locks immediately
        Changed?.Invoke();
    }

    public void Disable()
    {
        Preferences.Default.Set(KeyEnabled, false);
        Preferences.Default.Remove(KeySalt);
        Preferences.Default.Remove(KeyHash);

        IsLockEnabled = false;
        IsUnlocked = true;

        Changed?.Invoke();
    }

    public void Lock()
    {
        if (!IsLockEnabled) return;
        IsUnlocked = false;
        Changed?.Invoke();
    }

    public bool Verify(string pin)
    {
        if (!IsLockEnabled) return true;

        var saltB64 = Preferences.Default.Get(KeySalt, "");
        var hashB64 = Preferences.Default.Get(KeyHash, "");
        if (string.IsNullOrWhiteSpace(saltB64) || string.IsNullOrWhiteSpace(hashB64))
            return false;

        var salt = Convert.FromBase64String(saltB64);
        var expected = Convert.FromBase64String(hashB64);
        var actual = HashPin((pin ?? "").Trim(), salt);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    public bool TryUnlock(string pin)
    {
        var ok = Verify(pin);
        if (ok && IsLockEnabled)
        {
            IsUnlocked = true;
            Changed?.Invoke();
        }
        return ok;
    }

    public void ChangePin(string currentPin, string newPin)
    {
        if (!Verify(currentPin))
            throw new InvalidOperationException("Incorrect current PIN.");

        EnableWithPin(newPin);
        TryUnlock(newPin);
    }

    private static byte[] HashPin(string pin, byte[] salt)
    {
        var bytes = Encoding.UTF8.GetBytes(pin);
        var input = new byte[salt.Length + bytes.Length];

        Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
        Buffer.BlockCopy(bytes, 0, input, salt.Length, bytes.Length);

        return SHA256.HashData(input);
    }
}
