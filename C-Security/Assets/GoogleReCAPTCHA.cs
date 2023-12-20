using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class GoogleReCAPTCHA : MonoBehaviour
{
    private string serverURL = "http://127.0.0.1:5500/grecap.html"; // Change to your local server address
    private string lastExtractedContent = ""; // Store the last extracted content

    // Variable to store the repeating method name
    private const string repeatingMethodName = "CheckForChanges";

    private void Start()
    {
        // Start checking for changes every 60 seconds (1 minute)
        InvokeRepeating(repeatingMethodName, 0f, 60f);
    }

    private void CheckForChanges()
    {
        StartCoroutine(LoadHTML());
    }

    public void StopRepeatingCheckForChanges()
    {
        // Stop the repeating invocation of CheckForChanges
        CancelInvoke(repeatingMethodName);
    }

    public void OpenLocalHTML()
    {
        Application.OpenURL(serverURL);
        StartCoroutine(LoadHTML());
    }

    IEnumerator LoadHTML()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverURL);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string htmlContent = www.downloadHandler.text;
            string extractedContent = ExtractContent(htmlContent);

            if (string.IsNullOrEmpty(extractedContent))
            {
                Debug.Log("No data inside <p id=\"displayStoredResponse\" class=\"px-6\"></p>");
            }
            else
            {
                if (extractedContent != lastExtractedContent)
                {
                    lastExtractedContent = extractedContent;
                    Debug.Log("Extracted Content: " + extractedContent);
                }
                else
                {
                    Debug.Log("No changes in content.");
                }
            }
        }
    }

    private string ExtractContent(string html)
    {
        Match match = Regex.Match(html, @"<p id=""displayStoredResponse"" class=""px-6"">(.*?)</p>");

        if (match.Success)
        {
            string extractedContent = match.Groups[1].Value.Trim();
            return extractedContent;
        }

        return "";
    }
}
