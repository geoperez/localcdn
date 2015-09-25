namespace LocalCdn
{
    class Program
    {
        /// <summary>
        /// This app should run as Admin because it needs to open port 80
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            LocalCdn.RunServer();
        }
    }
}
