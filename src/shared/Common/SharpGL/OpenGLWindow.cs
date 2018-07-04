using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Shaders;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace Common
{
    public class OpenGLWindow
    {
        public void Initialized(object sender, OpenGLEventArgs args)
        {
            //ThrowIfNull(args);

            Context = args.OpenGL;
        }

        //properties
        public OpenGL Context { get; set; }
        private ShaderProgram m_shaderProgram = new ShaderProgram();
        public Color ClearColor
        {
            get { return m_clearColor; }
            set
            {
                m_clearColor = value;
                SetClearColor(this, m_clearColor);
            }
        }

        //members
        private Color m_clearColor = Colors.Black;

        //utilities
        public static void SetClearColor(OpenGLWindow _window, Color _color)
        {
            //ThrowIfNull(_window);

            _window.Context.ClearColor(_color.R, _color.G, _color.B, _color.A);
        }
        public static void CreateBasicLighting(OpenGLWindow _window, Vector4 _position)
        {
            //ThrowIfNull(_window);

            OpenGL context = _window.Context;

            context.Enable(OpenGL.GL_DEPTH_TEST);

            float[] global_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            float[] light0ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            context.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            context.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            context.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, _position.ToFloatArray());
            context.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            context.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            context.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            context.Enable(OpenGL.GL_LIGHTING);
            context.Enable(OpenGL.GL_LIGHT0);

            context.ShadeModel(OpenGL.GL_SMOOTH);
        }
        public static void CreateBasicShader(OpenGLWindow _window)
        {
            //ThrowIfNull(_window);

            OpenGL context = _window.Context;

            //  Create a vertex shader.
            VertexShader vertexShader = new VertexShader();
            vertexShader.CreateInContext(context);
            vertexShader.SetSource(
                "void main()" + Environment.NewLine +
                "{" + Environment.NewLine +
                "gl_Position = ftransform();" + Environment.NewLine +
                "}" + Environment.NewLine);

            //  Create a fragment shader.
            FragmentShader fragmentShader = new FragmentShader();
            fragmentShader.CreateInContext(context);
            fragmentShader.SetSource(
                "void main()" + Environment.NewLine +
                "{" + Environment.NewLine +
                "gl_FragColor = vec4(0.4,0.4,0.8,1.0);" + Environment.NewLine +
                "}" + Environment.NewLine);

            //  Compile them both.
            vertexShader.Compile();
            fragmentShader.Compile();

            //  Build a program.
            _window.m_shaderProgram.CreateInContext(context);

            //  Attach the shaders.
            _window.m_shaderProgram.AttachShader(vertexShader);
            _window.m_shaderProgram.AttachShader(fragmentShader);
            _window.m_shaderProgram.Link();
        }
        public static void BasicResize(OpenGLWindow _window, Size _size)
        {
            //ThrowIfNull(_window);

            OpenGL context = _window.Context;

            //  Set the projection matrix.
            context.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            context.LoadIdentity();

            //  Create a perspective transformation.
            context.Perspective(60.0f, _size.Width / _size.Height, 0.01, 100.0);

            //  Use the 'look at' helper function to position and aim the camera.
            context.LookAt(0, 0, 2, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            context.MatrixMode(OpenGL.GL_MODELVIEW);
        }
        public static void DrawRotatedTeaPot(OpenGLWindow _window, float _angle)
        {
            //ThrowIfNull(_window);

            // Clear The Screen And The Depth Buffer
            _window.Context.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Move Left And Into The Screen    
            _window.Context.LoadIdentity();
            _window.Context.Rotate(_angle, 0.0f, 1.0f, 0.0f);

            Teapot teapot = new Teapot();
            teapot.Draw(_window.Context, 14, 1, OpenGL.GL_FILL);
        }
    }
}
