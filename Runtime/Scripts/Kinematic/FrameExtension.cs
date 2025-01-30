using System.Collections.Generic;
using UnityEngine;

namespace Preliy.Flange
{
    public static class FrameExtension
    {
        public static Frame FindInChildren(this Transform transform, string name = "")
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<Frame>(out var frame))
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        return frame;
                    }

                    if (frame.name == name) return frame;
                }
            }
            return null;
        }

        public static List<Frame> FindAllInChildren(this Transform transform)
        {
            var result = new List<Frame>();
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<Frame>(out var frame))
                {
                    result.Add(frame);
                }
            }
            return result;
        }

        public static Frame GetOrCreate(this Transform transform, string name = "")
        {
            var frame = FindInChildren(transform, name);
            if (frame != null) return frame;
            return CreateFrame(transform, string.IsNullOrEmpty(name) ? "Frame" : name);
        }
        
        public static Frame CreateFrame(this Transform parent, string name)
        {
            var frameGameObject = new GameObject(name, typeof(Frame))
            {
                transform =
                {
                    parent = parent,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity
                }
            };

            return frameGameObject.GetComponent<Frame>();
        }

        public static Frame CreateMeshPlaceholder(this Frame frame)
        {
            for (var i = 0; i < frame.transform.childCount; i++)
            {
                if (frame.transform.GetChild(i).name == "Mesh")
                {
                    return frame;
                }
            }
            
            var _ = new GameObject("Mesh")
            {
                transform =
                {
                    parent = frame.transform ,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity
                }
            };

            return frame;
        }
    }
}
