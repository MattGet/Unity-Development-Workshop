using UnityEngine;

[AddComponentMenu("Fireworks Mania/Gui/GuiManager")]
public class GuiManager : MonoBehaviour
{
    [SerializeField]
    private Canvas[] guiScreens;
    [Header("Settings")]
    public GameLockMode lockMode;

    private bool isActive = false;
    private Canvas internalGuiCanvas;
    private ComponentBridge uiBridge;

    public ComponentBridge ComponentBridge { get => uiBridge; }
    public bool IsActive { get => isActive; }

    public void Show(int index)
    {
        if (isActive) Hide(false);
        internalGuiCanvas = Instantiate(guiScreens[index]);
        uiBridge = internalGuiCanvas.GetComponent<ComponentBridge>();
        GuiHelper.SetGameLockMode(lockMode);
        isActive = true;
    }

    public void Hide(bool disableUIMode = true)
    {
        if (isActive)
        {
            Destroy(internalGuiCanvas.gameObject);
            internalGuiCanvas = null;
            uiBridge = null;
            if (disableUIMode)
                GuiHelper.SetGameLockMode(GameLockMode.None);
            isActive = false;
        }
    }
}