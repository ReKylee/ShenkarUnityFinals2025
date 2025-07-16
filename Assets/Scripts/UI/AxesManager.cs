// using Interfaces.Resettable;
// using Managers;
// using TMPro;
// using UnityEngine;
// using Weapons.Models;
//
// public class AxesManager : MonoBehaviour, IResettable
// {
//     [SerializeField] private TextMeshProUGUI axesText;
//     private void Start()
//     {
//         ResetManager.Instance?.Register(this);
//     }
//     private void OnEnable()
//     {
//         AxeWeapon.OnAxeCollected += OnAxeCollision;
//         AxeWeapon.OnAxeFired += OnAxeFired;
//
//     }
//
//     private void OnDisable()
//     {
//         AxeWeapon.OnAxeCollected -= OnAxeCollision;
//         AxeWeapon.OnAxeFired -= OnAxeFired;
//     }
//     public void ResetState()
//     {
//         OnAxeCollision(0);
//     }
//
//     private void OnAxeCollision(int axes)
//     {
//         axesText.text = $"Axes: {axes}";
//     }
//
//     private void OnAxeFired(int axes)
//     {
//         axesText.text = $"Axes: {axes}";
//     }
// }


