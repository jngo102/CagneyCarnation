using UnityEngine;
namespace CagneyCarnation
{
   public static class Util
    {
        public static GameObject FindGameObjectInChildren(this GameObject parent,string name)
        {
            foreach(var child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child.gameObject;
            }
            return null;
        }
    }
}
