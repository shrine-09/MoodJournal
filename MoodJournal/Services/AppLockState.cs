namespace MoodJournal.Services;

public class AppLockState
{
    public bool Unlocked { get; private set; }

    public void SetUnlocked(bool value) => Unlocked = value;
}