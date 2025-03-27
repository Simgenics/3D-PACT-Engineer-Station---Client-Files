using UnityEngine;

[CreateAssetMenu(fileName = "LMS API", menuName = "LMS/LMS API SO")]
public class APILib : ScriptableObject
{
    public string postTraineeResults;
    public string AuthenticateWithTenantName;
    public string GetAssignedAllScenarios;
    public string GetAssignedScenarios;
    public string GetAssignedScenariosForView;
    public string GetAllCurrentAssetBundles;
    public string GetAllCurrentScenarios;
    public string GetTreeOfFacilitiesForUser;
    public string GetTreeOfCompaniesForUser;

}
