using System;

namespace CowMouse
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CowMouseGame game = new CowMouseGame())
            {
                game.Run();
            }
        }
    }
#endif
}

