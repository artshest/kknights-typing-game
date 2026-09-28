using UnityEngine;

public class TypingFacade : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string TargetText;

    [SerializeField]
    private TypingView _typingView;

    private ITypingInput _typingInput;
    private TypingSession _typingSession;
    private TypingViewModel _typingViewModel;

    private void Start()
    {
        // Здесть коннектить все зависимости   

        _typingInput = new TextInputHandler();

        _typingSession = new TypingSession(TargetText);
        _typingViewModel = new TypingViewModel(_typingInput, _typingSession);
        _typingView.Bind(_typingViewModel);
    }
}
