using Modding;
using UnityEngine;

namespace CagneyCarnation
{
    public class SaveSettings : ModSettings, ISerializationCallbackReceiver
    {
        public BossStatue.Completion CompletionFlower = new BossStatue.Completion
        {
            isUnlocked = true,
            hasBeenSeen = true
        };
        
        public void OnBeforeSerialize()
        {
            StringValues["CompletionFlower"] = JsonUtility.ToJson(CompletionFlower);
        }

        public void OnAfterDeserialize()
        {
            StringValues.TryGetValue("CompletionFlower", out string @out);
            if (string.IsNullOrEmpty(@out)) return;
            CompletionFlower = JsonUtility.FromJson<BossStatue.Completion>(@out);
        }
    }
}