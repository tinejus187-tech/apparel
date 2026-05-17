namespace Justine_Apparel_E_Commerce_System;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Database.Initialize();
        Application.Run(new Form1());
    }
}
