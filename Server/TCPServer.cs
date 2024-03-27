using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server;

public class TCPServer
{
    public void Server()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8888);
        listener.Start();

        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client added");

            using (SslStream sslStream = new SslStream(client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => true))
            {
                AuthenticateAsServer(sslStream, client);

                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    var rsa = GetPublicKey(reader);

                    var encryptedText = EncryptText(rsa);

                    SendMessageToServer(writer, encryptedText);

                    Console.WriteLine("Text has just sent");
                }
            }

            client.Close();
        }
    }

    private void AuthenticateAsServer(SslStream sslStream, TcpClient client)
    {
        try
        {
            X509Certificate2 certificate = LoadCertificate("localhost");

            sslStream.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error authentication TLS: " + ex.Message);
            client.Close();
        }
    }

    private X509Certificate2 LoadCertificate(string certificateName)
    {
        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindBySubjectName, certificateName, true);

        if (certificates.Count > 0)
        {
            return certificates[0];
        }
        else
        {
            throw new Exception("Certificate not found.");
        }
    }

    private RSACryptoServiceProvider GetPublicKey(StreamReader reader)
    {
        string publicKey = reader.ReadLine();
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(publicKey);

        return rsa;
    }

    private string EncryptText(RSACryptoServiceProvider rsa)
    {
        string message = File.ReadAllText("example.txt");
        byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(message), true);
        string encryptedText = Convert.ToBase64String(encryptedBytes);

        return encryptedText;
    }
    private void SendMessageToServer(StreamWriter writer, string encryptedText)
    {
        writer.WriteLine(encryptedText);
        writer.Flush();
    }

}