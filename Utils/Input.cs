﻿using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockEngine.Utils;

public static class Input
{
    public static KeyboardState KeyboardState { get; private set; } = null!;
    public static MouseState MouseState { get; private set; } = null!;


    public static void Update(KeyboardState keyboardState, MouseState mouseState)
    {
        KeyboardState = keyboardState;
        MouseState = mouseState;
    }
}