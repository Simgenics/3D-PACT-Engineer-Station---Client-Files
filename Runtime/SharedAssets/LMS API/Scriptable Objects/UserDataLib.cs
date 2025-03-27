using UnityEngine;

[CreateAssetMenu(fileName = "User Data", menuName = "LMS/User Data Library")]
public class UserDataLib : ScriptableObject
{
    public string baseAPIURI = "baseAPIURI";
    public string tenantName = "tenant";

    public string accessToken = "accessToken";
    public string encryptedAccessToken = "encryptedAccessToken";
    public string expireInSeconds = "expireInSeconds";
    public string shouldResetPassword = "shouldResetPassword";
    public string passwordResetCode = "passwordResetCode";
    public string userId = "userId";
    public string userFirstName = "userFirstName";
    public string userLastName = "userLastName";
    public string requiresTwoFactorVerification = "requiresTwoFactorVerification";
    //public List<string> twoFactorAuthProviders;
    public string twoFactorRememberClientToken = "twoFactorRememberClientToken";
    public string returnUrl = "returnUrl";
    public string refreshToken = "refreshToken";
    public string refreshTokenExpireInSeconds = "refreshTokenExpireInSeconds";


    public string numberOfAttempts = "numberOfAttempts";
    public string resourcesAccessed = "resourcesAccessed";
    public string pdfResults = "pdfResults";
    public string stepsFailed = "stepsFailed";
    public string stepsPassed = "stepsPassed";
    public string score = "score";
    public string endTime = "endTime";
    public string startTime = "startTime";
    public string executionDate = "executionDate";
    public string assignedScenarioId = "assignedScenarioId";
    public string scenarioID = "scenarioID";
    public string PDFPath = "PDFPath";

    public string validAssignedScenarioDetails = "validAssignedScenarioDetails";

    public void clearUserData()
    {
        Debug.Log("Clearing User Data");
        PlayerPrefs.SetString(accessToken, "");
        PlayerPrefs.SetString(encryptedAccessToken, "");

        PlayerPrefs.SetInt(assignedScenarioId, 0);
        PlayerPrefs.SetInt(scenarioID, 0);
        PlayerPrefs.SetString(userFirstName, "");
        PlayerPrefs.SetString(userLastName, "");
        PlayerPrefs.SetInt(scenarioID, 0);
        PlayerPrefs.SetString(PDFPath, "");

        PlayerPrefs.SetString(resourcesAccessed, "");
        PlayerPrefs.SetString(pdfResults, "");
        PlayerPrefs.SetInt(stepsFailed, 0);
        PlayerPrefs.SetInt(stepsPassed, 0);
        PlayerPrefs.SetInt(score, 0);
        PlayerPrefs.SetString(endTime, "");
        PlayerPrefs.SetString(startTime, "");
        PlayerPrefs.SetString(executionDate, "");

        PlayerPrefs.SetString(validAssignedScenarioDetails, "");
    }

    public void clearAPISettings()
    {
        PlayerPrefs.SetString(baseAPIURI, "");
        PlayerPrefs.SetString(tenantName, "");
    }
}
