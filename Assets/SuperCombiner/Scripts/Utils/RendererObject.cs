using UnityEngine;

namespace LunarCatsStudio.SuperCombiner
{
    /// <summary>
    /// RendererObject
    /// A simple class for renderer objects 
    /// </summary>
    public class RendererObject<T>
    {
        // The reference to the renderer 
        private T renderer;

        public T Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }

        // True if this renderer will be combined
        private bool willBeCombined = true;
        public bool WillBeCombined
        {
            get { return willBeCombined; }
            set { willBeCombined = value; }
        }

        public RendererObject(T renderer, bool willBeCombined = true)
        {
            Renderer = renderer;
            WillBeCombined = willBeCombined;
        }
    }
}
