using UnityEngine;
using DG.Tweening;

public class Rotation_Test : MonoBehaviour
{
    [SerializeField] private GameObject[] rotationObjects;
    [SerializeField] private GameObject arrowIndicator;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float arrowFloatHeight = 0.5f;
    [SerializeField] private float arrowFloatSpeed = 1f;
    [SerializeField] private float arrowMoveSpeed = 0.3f;
    [SerializeField] private float selectionScaleAmount = 1.05f;
    [SerializeField] private float selectionScaleDuration = 0.2f;
    [SerializeField] private float resetDelay = 2f;
    [SerializeField] private float resetSpeed = 1f;
    
    private int currentObjectIndex = 0;
    private bool isRotating = false;
    private bool isSelected = false;
    private GameObject currentSelectedObject;
    private Vector3[] originalScales;
    private Vector3[] originalRotations;
    private Tween[] resetTweens;
    
    void Start()
    {
        if (rotationObjects.Length > 0)
        {
            originalScales = new Vector3[rotationObjects.Length];
            originalRotations = new Vector3[rotationObjects.Length];
            resetTweens = new Tween[rotationObjects.Length];
            
            for (int i = 0; i < rotationObjects.Length; i++)
            {
                originalScales[i] = rotationObjects[i].transform.localScale;
                originalRotations[i] = rotationObjects[i].transform.eulerAngles;
            }
        }
        
        if (rotationObjects.Length > 0 && arrowIndicator != null)
        {
            MoveArrowToCurrentObject();
            StartArrowFloating();
        }
    }
    
    void Update()
    {
        if (rotationObjects.Length == 0) return;
        
        if (!isSelected)
        {
            HandleNavigation();
            HandleSelection();
        }
        else
        {
            HandleRotation();
            HandleDeselection();
        }
    }
    
    private void HandleNavigation()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentObjectIndex = (currentObjectIndex + 1) % rotationObjects.Length;
            MoveArrowToCurrentObject();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentObjectIndex = (currentObjectIndex - 1 + rotationObjects.Length) % rotationObjects.Length;
            MoveArrowToCurrentObject();
        }
    }
    
    private void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            isSelected = true;
            currentSelectedObject = rotationObjects[currentObjectIndex];
            DoSelectionEffect();
        }
    }
    
    private void HandleRotation()
    {
        if (currentSelectedObject == null || isRotating) return;
        
        Vector3 currentRotation = currentSelectedObject.transform.eulerAngles;
        Vector3 targetRotation = currentRotation;
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetRotation.z = currentRotation.z - 90f;
            RotateToTarget(targetRotation);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetRotation.z = currentRotation.z + 90f;
            RotateToTarget(targetRotation);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            targetRotation.x = currentRotation.x - 90f;
            RotateToTarget(targetRotation);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            targetRotation.x = currentRotation.x + 90f;
            RotateToTarget(targetRotation);
        }
    }
    
    private void HandleDeselection()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            isSelected = false;
            currentSelectedObject.transform.DOScale(originalScales[currentObjectIndex], selectionScaleDuration);
            currentSelectedObject = null;
        }
    }
    
    private void MoveArrowToCurrentObject()
    {
        if (arrowIndicator != null && rotationObjects[currentObjectIndex] != null)
        {
            Vector3 targetPos = rotationObjects[currentObjectIndex].transform.position + Vector3.up * 2f;
            arrowIndicator.transform.DOMove(targetPos, arrowMoveSpeed).SetEase(Ease.OutQuad);
        }
    }
    
    private void StartArrowFloating()
    {
        if (arrowIndicator != null)
        {
            arrowIndicator.transform.DOMoveY(arrowIndicator.transform.position.y + arrowFloatHeight, arrowFloatSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    
    private void DoSelectionEffect()
    {
        if (currentSelectedObject != null)
        {
            Vector3 originalScale = originalScales[currentObjectIndex];
            Vector3 targetScale = originalScale * selectionScaleAmount;
            
            Sequence selectionSequence = DOTween.Sequence();
            selectionSequence.Append(currentSelectedObject.transform.DOScale(targetScale, selectionScaleDuration));
            selectionSequence.Append(currentSelectedObject.transform.DOScale(originalScale, selectionScaleDuration));
            selectionSequence.Append(currentSelectedObject.transform.DOScale(targetScale, selectionScaleDuration));
            selectionSequence.Append(currentSelectedObject.transform.DOScale(originalScale, selectionScaleDuration));
        }
    }
    
    private void RotateToTarget(Vector3 targetRotation)
    {
        isRotating = true;

        currentSelectedObject.transform.DORotate(targetRotation, rotationSpeed).OnComplete(() => {
            isRotating = false;
            StartResetTimer(currentObjectIndex); // wait before slowly resetting
        });
    }

    
    private void StartResetTimer(int objectIndex)
    {
        if (resetTweens[objectIndex] != null)
        {
            resetTweens[objectIndex].Kill();
        }
        
        resetTweens[objectIndex] = DOVirtual.DelayedCall(resetDelay, () => {
            ResetObjectRotation(objectIndex);
        });
    }
    
    private void ResetObjectRotation(int objectIndex)
    {
        if (objectIndex < rotationObjects.Length && rotationObjects[objectIndex] != null)
        {
            rotationObjects[objectIndex].transform.DORotate(originalRotations[objectIndex], resetSpeed);
        }
    }
}