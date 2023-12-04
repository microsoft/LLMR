using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class APIManager : ChatBot
{
    public GameObject apiManager;
    [TextArea(10, 100)]
    public string apiRefToBuilder;

    public string skill_desc;
    public string selected_skills = "N/A";

    //protected override void Start()
    protected override void Awake()
    {
        skill_desc = "";
        //get the string of skills
        foreach (UnityEngine.Component s in apiManager.GetComponents(typeof(Skills)))
        {
            string skill_name = s.GetType().Name;
            System.Type type = s.GetType();
            Skills skill = apiManager.GetComponent(type) as Skills;
            skill_desc += "skill name: " + skill_name + ", description: " + skill.textToArchitect + "\n";
        }
        //Debug.Log(skill_desc);
        base.Awake();
    }


    public async Task AnalyzeInput(string user_request)
    {
        input = skill_desc + "\n" +"Instruction: " + user_request;
        await SendChat();

        ParseSkill(output);
        

    }

    private void ParseSkill(string output)
    {
        apiRefToBuilder = "";
        foreach (UnityEngine.Component s in apiManager.GetComponents(typeof(Skills)))
        {
            string skill_name = s.GetType().Name;

            if (output.Contains(skill_name))
            {
                System.Type type = s.GetType();
                Skills skill = apiManager.GetComponent(type) as Skills;
                apiRefToBuilder += "skill name: " + skill_name + ", description: " + skill.textToBuilder + "\n";
            }
            
        }
        Debug.Log(apiRefToBuilder);

    }

}
