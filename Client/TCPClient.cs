using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client;

public class TCPClient
{
    private static RSACryptoServiceProvider rsa;

    public void GetMessage()
    {
        rsa = new RSACryptoServiceProvider();
        string publicKey = rsa.ToXmlString(false);

        TcpClient client = new TcpClient("localhost", 8888);
        Console.WriteLine("Connected to server");

        using (SslStream sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true))
        {

            AuthenticateAsClient(sslStream, client);

            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {

                SendPublicKey(publicKey, writer);

                var encryptedText = reader.ReadLine();

                var decryptedText = DecryptText(encryptedText);
                Console.WriteLine("Encrypt Text: " + encryptedText);
                Console.WriteLine("Received from the client: " + decryptedText);

                SaveFile(decryptedText);
                Console.ReadKey();
            }
        }

        client.Close();
    }

    private void AuthenticateAsClient(SslStream sslStream, TcpClient client)
    {
        try
        {
            sslStream.AuthenticateAsClient("localhost");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error authentication TLS: " + ex.Message);
        }
    }

    private void SendPublicKey(string publicKey, StreamWriter writer)
    {
        writer.WriteLine(publicKey);
        writer.Flush();
    }


    private void SaveFile(string message)
    {
        string pathToFile = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "receiveMessage.txt");
        using (StreamWriter writer = new StreamWriter(pathToFile))
        {
            writer.Write(message);
        }
        Console.WriteLine("Save this text in file: " + pathToFile);
    }


    private string DecryptText(string encryptedText)
    {
        byte[] decryptedBytes = rsa.Decrypt(Convert.FromBase64String(encryptedText), true);
        string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        return decryptedText;
    }
}