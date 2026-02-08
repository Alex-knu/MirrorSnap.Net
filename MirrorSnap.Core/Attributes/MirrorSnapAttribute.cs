using System;

namespace MirrorSnap.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MirrorSnapAttribute : Attribute
    {
        public MirrorSnapAttribute(string jsonPath)
        {
            JsonPath = jsonPath;
        }

        public string JsonPath { get; }
    }
}