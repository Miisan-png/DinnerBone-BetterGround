using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

[RequireComponent(typeof(Selectable))]
public class Velocity_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [Header("Text Elements")]
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI leftDesignElement;
    public TextMeshProUGUI rightDesignElement;
    
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;
    
    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onHover;
    public UnityEvent onExit;
    
    [Header("Focus Navigation")]
    public Velocity_Button focusUp;
    public Velocity_Button focusDown;
    public Velocity_Button focusLeft;
    public Velocity_Button focusRight;
    
    [Header("Animation Settings")]
    public float appearDuration = 0.5f;
    public float hoverDuration = 0.2f;
    public float hoverScale = 1.1f;
    public float disperseDistance = 20f;
    
    [Header("Auto Selection")]
    public bool isDefaultSelected = false;
    
    private Vector3 originalScale;
    private Vector3 leftOriginalPosition;
    private Vector3 rightOriginalPosition;
    private Color originalMainColor;
    private Color originalLeftColor;
    private Color originalRightColor;
    private bool isPressed = false;
    private bool isHovered = false;
    private static Velocity_Button currentSelectedButton;
    private static bool hasSetInitialSelection = false;
    
    private PlayerInput playerInput;
    private InputAction navigateAction;
    private InputAction submitAction;
    private Selectable selectable;
    
    void Awake()
    {
        selectable = GetComponent<Selectable>();
        if (selectable == null)
        {
            selectable = gameObject.AddComponent<Button>();
        }
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        originalScale = transform.localScale;
        if (leftDesignElement) leftOriginalPosition = leftDesignElement.transform.localPosition;
        if (rightDesignElement) rightOriginalPosition = rightDesignElement.transform.localPosition;
        
        if (mainText) originalMainColor = mainText.color;
        if (leftDesignElement) originalLeftColor = leftDesignElement.color;
        if (rightDesignElement) originalRightColor = rightDesignElement.color;
        
        playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            navigateAction = playerInput.actions["Navigate"];
            submitAction = playerInput.actions["Submit"];
        }
    }
    
    void Start()
    {
        AppearAnimation();
        
        if (!hasSetInitialSelection && (isDefaultSelected || IsControllerConnected()))
        {
            DOVirtual.DelayedCall(appearDuration + 0.1f, () =>
            {
                SetSelected();
                hasSetInitialSelection = true;
            });
        }
        
        SetupNavigation();
    }
    
    void OnEnable()
    {
        if (navigateAction != null)
        {
            navigateAction.performed += OnNavigate;
        }
        if (submitAction != null)
        {
            submitAction.performed += OnSubmit;
        }
    }
    
    void OnDisable()
    {
        if (navigateAction != null)
        {
            navigateAction.performed -= OnNavigate;
        }
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmit;
        }
    }
    
    bool IsControllerConnected()
    {
        return Gamepad.current != null || Joystick.current != null;
    }
    
    void SetupNavigation()
    {
        if (selectable == null) return;
        
        Navigation nav = selectable.navigation;
        nav.mode = Navigation.Mode.Explicit;
        
        if (focusUp && focusUp.selectable) nav.selectOnUp = focusUp.selectable;
        if (focusDown && focusDown.selectable) nav.selectOnDown = focusDown.selectable;
        if (focusLeft && focusLeft.selectable) nav.selectOnLeft = focusLeft.selectable;
        if (focusRight && focusRight.selectable) nav.selectOnRight = focusRight.selectable;
        
        selectable.navigation = nav;
    }
    
    void AppearAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, appearDuration).SetEase(Ease.OutBack);
        
        if (mainText) mainText.color = Color.clear;
        if (leftDesignElement) leftDesignElement.color = Color.clear;
        if (rightDesignElement) rightDesignElement.color = Color.clear;
        
        DOVirtual.DelayedCall(appearDuration * 0.3f, () =>
        {
            if (mainText) mainText.DOColor(originalMainColor, appearDuration * 0.7f);
            if (leftDesignElement) leftDesignElement.DOColor(originalLeftColor, appearDuration * 0.7f);
            if (rightDesignElement) rightDesignElement.DOColor(originalRightColor, appearDuration * 0.7f);
        });
    }
    
    void OnNavigate(InputAction.CallbackContext context)
    {
        if (currentSelectedButton != this) return;
        
        Vector2 input = context.ReadValue<Vector2>();
        Velocity_Button targetButton = null;
        
        if (input.y > 0.5f && focusUp) targetButton = focusUp;
        else if (input.y < -0.5f && focusDown) targetButton = focusDown;
        else if (input.x < -0.5f && focusLeft) targetButton = focusLeft;
        else if (input.x > 0.5f && focusRight) targetButton = focusRight;
        
        if (targetButton != null)
        {
            targetButton.SetSelected();
        }
    }
    
    void OnSubmit(InputAction.CallbackContext context)
    {
        if (currentSelectedButton == this)
        {
            OnPressed();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetSelected();
        if (!isPressed)
        {
            OnHoverStart();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPressed)
        {
            OnHoverEnd();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        SetColors(pressedColor);
        transform.DOScale(originalScale * 0.95f, 0.1f);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        OnPressed();
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        currentSelectedButton = this;
        OnHoverStart();
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        if (currentSelectedButton == this)
        {
            currentSelectedButton = null;
        }
        OnHoverEnd();
    }
    
    void OnPressed()
    {
        if (isHovered)
        {
            OnHoverStart();
        }
        else
        {
            OnHoverEnd();
        }
        
        transform.DOScale(originalScale, 0.1f);
        onPressed?.Invoke();
    }
    
    void OnHoverStart()
    {
        isHovered = true;
        onHover?.Invoke();
        
        SetColors(hoverColor);
        transform.DOScale(originalScale * hoverScale, hoverDuration);
        
        if (leftDesignElement)
        {
            leftDesignElement.transform.DOLocalMove(
                leftOriginalPosition + Vector3.left * disperseDistance, 
                hoverDuration
            ).SetEase(Ease.OutQuart);
        }
        
        if (rightDesignElement)
        {
            rightDesignElement.transform.DOLocalMove(
                rightOriginalPosition + Vector3.right * disperseDistance, 
                hoverDuration
            ).SetEase(Ease.OutQuart);
        }
    }
    
    void OnHoverEnd()
    {
        isHovered = false;
        onExit?.Invoke();
        
        SetColors(normalColor);
        transform.DOScale(originalScale, hoverDuration);
        
        if (leftDesignElement)
        {
            leftDesignElement.transform.DOLocalMove(leftOriginalPosition, hoverDuration).SetEase(Ease.OutQuart);
        }
        
        if (rightDesignElement)
        {
            rightDesignElement.transform.DOLocalMove(rightOriginalPosition, hoverDuration).SetEase(Ease.OutQuart);
        }
    }
    
    void SetColors(Color color)
    {
        if (mainText) mainText.DOColor(color, 0.1f);
        if (leftDesignElement) leftDesignElement.DOColor(color, 0.1f);
        if (rightDesignElement) rightDesignElement.DOColor(color, 0.1f);
    }
    
    public void SetSelected()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
    
    void OnValidate()
    {
        if (Application.isPlaying && selectable != null)
        {
            SetupNavigation();
        }
    }
}