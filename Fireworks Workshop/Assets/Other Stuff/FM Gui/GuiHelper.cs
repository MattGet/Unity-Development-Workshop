using FireworksMania.Core.Messaging;

public static class GuiHelper
{
    public static void SetGameLockMode(GameLockMode mode)
    {
        switch (mode)
        {
            case GameLockMode.None:
                UILock(false, false);
                break;
            case GameLockMode.ShowCursor:
                UILock(true, false);
                break;
            case GameLockMode.Full:
                UILock(true, true);
                break;
        }
    }

    private static void UILock(bool showCursor, bool freezePlayer)
    {
        Messenger.Broadcast(new MessengerEventChangeUIMode(showCursor, !freezePlayer));
    }
}

public enum GameLockMode : byte
{
    Full,
    ShowCursor,
    None
}