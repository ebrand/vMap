using System;

namespace vMap.MonoGame
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var vMapMain = new vMapMain())
				vMapMain.Run();
        }
    }
}