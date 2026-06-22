using UnityEngine;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    #region Fields
    private RectTransform _rectTransform;
    private RectTransform _parentRectTransform;
    private Canvas _canvas;
    private Image _image;
    private Vector2 _startScreenPosition;
    private Vector2 _currentScreenPosition;
    private Vector2 _startLocalPosition;
    private Vector2 _currentLocalPosition;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = _rectTransform.parent as RectTransform;
        _canvas = GetComponentInParent<Canvas>();
        _image = GetComponent<Image>();

        // The selection rectangle is positioned from its bottom-left corner.
        // Its parent Canvas may stretch and scale freely.
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.zero;
        _rectTransform.pivot = Vector2.zero;
        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.sizeDelta = Vector2.zero;
        _image.enabled = false;
    }

    public void BeginDrag(Vector2 screenStart)
    {
        _startScreenPosition = screenStart;
        _currentScreenPosition = screenStart;
        _startLocalPosition = ScreenToParentLocalPosition(screenStart);
        _currentLocalPosition = _startLocalPosition;
        UpdateVisual();
        _image.enabled = true;
    }

    public void UpdateDrag(Vector2 screenCurrentPosition)
    {
        _currentScreenPosition = screenCurrentPosition;
        _currentLocalPosition = ScreenToParentLocalPosition(screenCurrentPosition);
        UpdateVisual();
    }

    public Rect EndDrag()
    {
        _image.enabled = false;
        return Rect.MinMaxRect(
            Mathf.Min(_startScreenPosition.x, _currentScreenPosition.x),
            Mathf.Min(_startScreenPosition.y, _currentScreenPosition.y),
            Mathf.Max(_startScreenPosition.x, _currentScreenPosition.x),
            Mathf.Max(_startScreenPosition.y, _currentScreenPosition.y)
        );
    }

    private void UpdateVisual()
    {
        Vector2 minimum = Vector2.Min(_startLocalPosition, _currentLocalPosition);
        Vector2 maximum = Vector2.Max(_startLocalPosition, _currentLocalPosition);
        _rectTransform.anchoredPosition = minimum;
        _rectTransform.sizeDelta = maximum - minimum;
    }

    private Vector2 ScreenToParentLocalPosition(Vector2 screenPosition)
    {
        if (_parentRectTransform == null) return screenPosition;

        Camera eventCamera = null;
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            eventCamera = _canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRectTransform,
                screenPosition,
                eventCamera,
                out Vector2 localPosition))
            return Vector2.zero;

        // ScreenPointToLocalPointInRectangle returns a point relative to the
        // parent's pivot. anchoredPosition uses the bottom-left anchor instead.
        return localPosition - _parentRectTransform.rect.min;
    }

    #endregion
}
