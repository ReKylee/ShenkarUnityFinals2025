using UnityEngine;

namespace LocksAndKeys
{
    public class BasicKey : MonoBehaviour, IKey
    {
        [SerializeField] private string keyId;
        public string KeyId => keyId;
    }
}
