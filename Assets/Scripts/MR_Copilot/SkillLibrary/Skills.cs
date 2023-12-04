using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class Skill : MonoBehaviour
{
    public string textToBuilder;
    public string textToArchitect;

    // Start is called before the first frame update
    void Start()
    {
        textToBuilder = "";
    }


    // A method that uses reflection to get all method declarations inside a given type as strings
    List<string> GetMethodDeclarations(Type type)
    {
        // TODO: see if this works for async methods

        // Create an empty list to store the declarations
        List<string> declarations = new List<string>();
        // Get all the methods of the type, including public, non-public, static and instance methods
        //MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        // only get public methods
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        // For each method
        foreach (MethodInfo method in methods)
        {
            // ignore any methods defined in inherited classes, which include some generic stuff like Invoke(), GetComponent(), etc.
            if (method.DeclaringType != type)
            {
                continue;
            }

            // Get the name of the method
            string name = method.Name;
            // Get the return type of the method
            string returnType = method.ReturnType.Name;
            // Get the parameters of the method
            ParameterInfo[] parameters = method.GetParameters();
            // Create a list to store the parameter types and names
            List<string> parameterList = new List<string>();
            // For each parameter
            foreach (ParameterInfo parameter in parameters)
            {
                // Get the type and name of the parameter
                string parameterType = parameter.ParameterType.Name;
                string parameterName = parameter.Name;
                // Add the type and name to the parameter list
                parameterList.Add(parameterType + " " + parameterName);
            }
            // Join the parameter list with commas
            string parameterString = string.Join(", ", parameterList);
            // Create the declaration string by combining the return type, name and parameters
            string declaration = returnType + " " + name + "(" + parameterString + ")";

            // Check if the method is static
            if (method.IsStatic)
            {
                // Add the static keyword to the declaration
                declaration = "static " + declaration;
            }

            // Check if the method is public
            if (method.IsPublic)
            {
                // Add the static keyword to the declaration
                declaration = "public " + declaration;
            }

            // Add the declaration to the declarations list
            declarations.Add(declaration);
        }
        // Return the declarations list
        return declarations;
    }

}
