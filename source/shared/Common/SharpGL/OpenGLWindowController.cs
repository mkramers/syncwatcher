using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.SceneGraph;
using SharpGL.WPF;
using SharpGL.RenderContextProviders;
using SharpGL.Version;

namespace Common
{
    public abstract class OpenGLWindowController
    {
        public virtual void Initialized(object sender, OpenGLEventArgs args)
        {
            OpenGLWindow.Initialized(sender, args);
        }

        public virtual void Resized(object sender, OpenGLEventArgs args)
        {
            OpenGLControl control = (OpenGLControl)sender;

           OpenGLWindow.BasicResize(OpenGLWindow, control.RenderSize);
        }

        public virtual void Draw(object sender, OpenGLEventArgs args)
        {
        }

        protected OpenGLWindow OpenGLWindow { get; set; }

        //private InputHandler m_inputHandler = new InputHandler();
    }
}
