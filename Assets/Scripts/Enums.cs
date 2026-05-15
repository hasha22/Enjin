using UnityEngine;

public class Enums : MonoBehaviour
{

}
public enum CharacterKeywords
{
    TechLiteracy,
    EnjinIntegration,
    Organization,
    Stability,
    Sustainability,
    Privacy,
    Wealth,
    Productivity,
}
public enum PolicyKeywords
{
    Innovation,
    RiskManagement,
    WorkerMorale,
    Profit
}
public enum VoteTypes
{
    NoVote = 0,
    MostlyDisagree = 1,
    Disagree = 2,
    Neutral = 3,
    Agree = 4,
    MostlyAgree = 5
    //this enum has been made because using an enum would make more sense from a unity perspective
}

public enum GameScreens
{
    CharacterIntroScreen = 0,
    SituationExplanationScreen = 1,
    FirstPolicyVotingScreen = 2,
    DiscussionScreen = 3,
    EnjinUpdateScreen = 4,
    SecondPolicyVotingScreen = 5,
    ResultsScreen = 6
}
