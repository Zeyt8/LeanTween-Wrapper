using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tweener : MonoBehaviour
{
    private enum UIAnimationTypes
    {
        Move,
        Scale,
        Rotate,
        Fade
    }

    [SerializeField] GameObject objectToAnimate;

    [SerializeField] UIAnimationTypes animationType;
    public LeanTweenType easeType;
    [SerializeField] AnimationCurve curve;
    [SerializeField, Tooltip("When to trigger")] Trigger trigger;
    [SerializeField] private bool _useUnscaledTime = true;

    [SerializeField] float duration;
    [SerializeField] float delay;

    [SerializeField] bool loop;
    [SerializeField] bool pingpong;
    [SerializeField] bool destroyOnFinish;
    [SerializeField] bool destroyRootOnFinish;
    [SerializeField] GameObject root;

    [SerializeField, Tooltip("If true the value to animate will be set to from on begin")] bool startPositionOffset;
    [SerializeField, Tooltip("Absolute: move to to. Relative: move to current + to. Scaled: move to current * to")] MoveType moveType;
    private enum MoveType
    {
        Absolute,
        Relative,
        Scaled
    }
    public Vector3 from;
    public Vector3 to;
    public Color fromColor;
    public Color toColor;

    [SerializeField] UnityEvent onComplete;
    [SerializeField, Tooltip("Trigger onComplete also on start")] bool onCompleteOnStart;
    [SerializeField, Tooltip("Trigger onComplete at the end of each loop")] bool onCompleteOnRepeat;

    [System.Flags]
    private enum Trigger
    {
        OnEnable = 1,
        OnStart = 2
    }

    LTDescr _tweenObject;
    RectTransform _rectTransform;
    Image _image;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        if (trigger.HasFlag(Trigger.OnStart))
        {
            Play();
        }
    }

    private void OnEnable()
    {
        if (trigger.HasFlag(Trigger.OnEnable))
        {
            Play();
        }
    }

    public void Play()
    {
        if (_tweenObject != null)
        {
            LeanTween.cancel(_tweenObject.uniqueId);
        }
        HandleTween();
    }

    private void HandleTween()
    {
        if (objectToAnimate == null)
        {
            objectToAnimate = gameObject;
        }

        switch (animationType)
        {
            case UIAnimationTypes.Move:
                Move();
                break;
            case UIAnimationTypes.Rotate:
                Rotate();
                break;
            case UIAnimationTypes.Scale:
                Scale();
                break;
            case UIAnimationTypes.Fade:
                Fade();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _tweenObject.setDelay(delay);
        if (easeType == LeanTweenType.animationCurve)
        {
            _tweenObject.setEase(curve);
        }
        else
        {
            _tweenObject.setEase(easeType);
        }
        _tweenObject.setDestroyOnComplete(destroyOnFinish);
        _tweenObject.setOnComplete(OnComplete);
        _tweenObject.setOnCompleteOnStart(onCompleteOnStart);
        _tweenObject.setOnCompleteOnRepeat(onCompleteOnRepeat);

        if (loop)
        {
            _tweenObject.setLoopCount(int.MaxValue);
        }
        if (pingpong)
        {
            _tweenObject.setLoopPingPong();
        }

        _tweenObject.setIgnoreTimeScale(_useUnscaledTime);
    }

    private void Fade()
    {
        if (startPositionOffset)
        {
            _image.color = fromColor;
        }

        Color dest = moveType switch
        {
            MoveType.Absolute => toColor,
            MoveType.Relative => new Color(_image.color.r + toColor.r, _image.color.g + toColor.g,
                _image.color.b + toColor.b, _image.color.a + toColor.a),
            MoveType.Scaled => new Color(_image.color.r * toColor.r, _image.color.g * toColor.g,
                _image.color.b * toColor.b, _image.color.a * toColor.a),
            _ => Color.white
        };
        _tweenObject = LeanTween.color(_rectTransform, dest, duration);
    }

    private void Move()
    {
        if (startPositionOffset)
        {
            _rectTransform.anchoredPosition = from;
        }

        Vector3 dest = moveType switch
        {
            MoveType.Absolute => to,
            MoveType.Relative => new Vector3(_rectTransform.anchoredPosition.x + to.x,
                _rectTransform.anchoredPosition.y + to.y, to.z),
            MoveType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * to.x,
                _rectTransform.anchoredPosition.y * to.y, to.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.move(_rectTransform, dest, duration);
    }

    private void Rotate()
    {
        if (startPositionOffset)
        {
            _rectTransform.rotation = Quaternion.Euler(from);
        }

        Vector3 dest = moveType switch
        {
            MoveType.Absolute => to,
            MoveType.Relative => _rectTransform.rotation.eulerAngles + to,
            MoveType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * to.x,
                _rectTransform.anchoredPosition.y * to.y, to.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.rotate(_rectTransform, dest, duration);
    }

    private void Scale()
    {
        if (startPositionOffset)
        {
            _rectTransform.localScale = from;
        }

        Vector3 dest = moveType switch
        {
            MoveType.Absolute => to,
            MoveType.Relative => _rectTransform.localScale + to,
            MoveType.Scaled => new Vector3(_rectTransform.anchoredPosition.x * to.x,
                _rectTransform.anchoredPosition.y * to.y, to.z),
            _ => Vector3.zero
        };
        _tweenObject = LeanTween.scale(objectToAnimate, dest, duration);
    }

    private void OnComplete()
    {
        onComplete.Invoke();
        if (destroyRootOnFinish && root != null)
        {
            Destroy(root);
        }
    }
}