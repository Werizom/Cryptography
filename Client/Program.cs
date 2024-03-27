using Client;

public class Program
{

    public static void Main()
    {
        Console.WriteLine("Hello, I'm Client!");

        TCPClient client = new TCPClient();
        client.GetMessage();
    }
}
