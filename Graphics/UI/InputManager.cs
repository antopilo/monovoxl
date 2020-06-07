
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMonoGame.Graphics.UI
{
    public static class InputManager
    {
        public static KeyboardState Keyboard;
        public static MouseState Mouse;

        public static void Update()
        {
            Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }

        public static Point GetMousePosition()
        {
            return Mouse.Position;
        }

        public static bool IsKeyDown(Keys key)
        {
            return Keyboard.IsKeyDown(key);
        }
        public static bool IsKeyUp(Keys key)
        {
            return Keyboard.IsKeyUp(key);
        }
    }
}
