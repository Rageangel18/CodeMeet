namespace CodeMeet.Services;

public class AuthModalState
{
    public bool IsLoginOpen { get; private set; }
    public bool IsRegisterOpen { get; private set; }

    public event Action? OnChange;

    public void OpenLogin()
    {
        IsLoginOpen = true;
        IsRegisterOpen = false;
        NotifyChanged();
    }

    public void OpenRegister()
    {
        IsRegisterOpen = true;
        IsLoginOpen = false;
        NotifyChanged();
    }

    public void Close()
    {
        IsLoginOpen = false;
        IsRegisterOpen = false;
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Console.WriteLine($"AuthModalState changed: login={IsLoginOpen}, register={IsRegisterOpen}");
        OnChange?.Invoke();
    }
}
