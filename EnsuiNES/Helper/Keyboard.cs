using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace EnsuiNES.Helper
{
    class Keyboard
    {
        public static KeyboardState currentState;
        public static KeyboardState previousState;

        public static KeyboardState GetState()
        {
            previousState = currentState;
            currentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            return currentState;
        }

        public static bool IsPressed(Keys key, bool singlePress = false)
        {
            if (!singlePress) return currentState.IsKeyDown(key);

            return currentState.IsKeyDown(key) && !previousState.IsKeyDown(key);
        }
    }
}
