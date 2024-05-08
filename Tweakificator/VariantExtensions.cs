using TinyJSON;

namespace Tweakificator
{
    public static class VariantExtensions
    {
        public static Variant Merge(this Variant self, Variant other)
        {
            if (self == null) return other;
            if (other == null) return self;

            if (self is ProxyObject selfObject && other is ProxyObject otherObject)
            {
                foreach (var key in otherObject.Keys)
                {
                    if (selfObject.TryGetValue(key, out var selfValue))
                    {
                        selfObject[key] = selfValue.Merge(otherObject[key]);
                    }
                    else
                    {
                        selfObject[key] = otherObject[key];
                    }
                }

                return self;
            }

            return other;
        }
    }
}
