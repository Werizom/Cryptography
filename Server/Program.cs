using Server;

public class Program
{

    public static void Main()
    {
        Console.WriteLine("Hello, I'm Server!");

        TCPServer server = new TCPServer();
        server.Server();
    }
}