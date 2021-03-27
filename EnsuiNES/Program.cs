using System;

namespace EnsuiNES
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.Write("Sup");
            using (var game = new Game1())
                game.Run();
        }
    }
}
