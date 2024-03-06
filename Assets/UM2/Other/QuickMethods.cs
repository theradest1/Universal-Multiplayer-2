using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QuickMethods : MonoBehaviour
{
    //a bunch of small processing methods that I got annoyed with re-writing

    public static object ParseValue(string token, Type type)
    {   
        if(type == typeof(Vector3)){
            // Remove parentheses and split string into components
            string[] components = token.Replace("(", "").Replace(")", "").Split(',');

            // Parse components to floats
            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            return new Vector3(x, y, z);
        }
        else if(type == typeof(Quaternion)){
            // Remove parentheses and split string into components
            string[] components = token.Replace("(", "").Replace(")", "").Split(',');

            // Parse components to floats
            float w = float.Parse(components[0]);
            float x = float.Parse(components[1]);
            float y = float.Parse(components[2]);
            float z = float.Parse(components[3]);

            return new Quaternion(w, x, y, z);
        }
        else if (type == typeof(int))
        {
            return int.Parse(token);
        }
        else if (type == typeof(float))
        {
            return float.Parse(token);
        }
        else if (type == typeof(bool))
        {
            return bool.Parse(token);
        }
        else if (type == typeof(string))
        {
            return token;
        }
        else if (type == typeof(System.Type)){
            return Type.GetType(token);
        }
        else
        {
            throw new ArgumentException("Unsupported parameter type: " + type + ". Add to ParseValue method in QuickMethods");
        }
    }

    public static string ListToString<T>(List<T> list)
    {
        string finalString = "{";
        foreach (T element in list)
        {
            finalString += element + ", ";
        }
        return finalString.Substring(0, finalString.Length - 2) + "}";
    }
    public static string ArrayTypesToString<T>(T[] array)
    {
        string finalString = "{";
        foreach (T element in array)
        {
            finalString += element.GetType() + ", ";
        }
        return finalString.Substring(0, finalString.Length - 2) + "}";
    }

    public static string ArrayToString<T>(T[] array)
    {
        string finalString = "{";
        foreach (T element in array)
        {
            finalString += element + ", ";
        }
        return finalString.Substring(0, finalString.Length - 2) + "}";
    }
}
