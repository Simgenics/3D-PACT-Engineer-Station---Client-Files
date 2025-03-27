using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EntityFetcher : MonoBehaviour
{
    public UserDataLib userDataLib;
    public APILib apiLib;
    public NotificationBanner notificationBanner;

    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public GameObject loadingObject;

    public void fetchAllCompanyAndFacilityData()
    {
        loadingObject.SetActive(true);

        EntityFetcher auth = this;
        StartCoroutine(auth.fetchCompanies());
    }

    public IEnumerator fetchCompanies()
    {
        CoroutineWithData cd = new CoroutineWithData(this, fetchTreeOfCompaniesForUser(apiLib));
        yield return cd.coroutine;

        if (errorHandlerUI(cd.result.ToString()))
        {
            Debug.Log("Fetched all companies");
        }
    }

    public IEnumerator fetchTreeOfCompaniesForUser(APILib apiLibrary)
    {
        var req = new UnityWebRequest(PlayerPrefs.GetString(userDataLib.baseAPIURI) + apiLibrary.GetTreeOfFacilitiesForUser, "GET");
        req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        req.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(userDataLib.accessToken));

        yield return req.SendWebRequest();
        string response = req.downloadHandler.text;
        if (req.result == UnityWebRequest.Result.ConnectionError || response.Substring(0, 15) == "<!DOCTYPE html>")
            yield return req.result;

        else
        {
            req.Dispose();
            var parsedObject = JObject.Parse(response);
            if (JsonConvert.SerializeObject(parsedObject["success"]) == "false")
            {
                yield return response;
            }
            else
            {
                yield return true;
            }
        }
    }

    public bool errorHandlerUI(string CoroutineResult)
    {
        loadingObject.SetActive(false);
        switch (CoroutineResult)
        {
            case "True":
                return true;
            case "ConnectionError":
                notificationBanner.displayNotificationBanner(MessageTemplate.Error, "CONNECTION ERROR", "Troubleshooting:\r\n1) Ensure that this computer is connected to the local network\r\n2) Ensure the correct 3DPact LMS Configurations are set\r\n3) Ensure that you have the correct role assigned to you", "OKAY", null);
                return false;
            case "<!DOCTYPE html>*":
                notificationBanner.displayNotificationBanner(MessageTemplate.Error, "CONNECTION ERROR", "Troubleshooting:\r\n1) Ensure that this computer is connected to the local network\r\n2) Ensure the correct 3DPact LMS Configurations are set\r\n3) Ensure that you have the correct role assigned to you", "OKAY", null);
                return false;
            default:
                try
                {
                    var parsedObject = JObject.Parse(CoroutineResult);
                    if (JsonConvert.SerializeObject(parsedObject["success"]) == "false")
                    {
                        string result;
                        result = JsonConvert.SerializeObject(parsedObject["error"]["message"]);
                        result = result.Substring(1, result.Length - 2);
                        notificationBanner.displayNotificationBanner(MessageTemplate.Error, "ERROR OCCURED", result, "OKAY", null);
                        return false;
                    }
                }
                catch (JsonReaderException jex)
                {
                    notificationBanner.displayNotificationBanner(MessageTemplate.Error, "UNEXPECTED ERROR OCCURED", "Refer to the 3DPact User Manual, and try again.", "OKAY", null);
                    return false;
                }
                notificationBanner.displayNotificationBanner(MessageTemplate.Error, "UNEXPECTED ERROR OCCURED", "Refer to the 3DPact User Manual, and try again.", "OKAY", null);
                return false;
        }


    }

    public class CoroutineWithData
    {
        public Coroutine coroutine { get; private set; }
        public object result;
        private IEnumerator target;
        public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
        {
            this.target = target;
            this.coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }
}
