using UnityEngine;

/// <summary>
/// Optional interface for interactable objects to specify their interaction ID
/// </summary>
public interface IInteractionIdentifier
{
    string GetInteractionID();
}