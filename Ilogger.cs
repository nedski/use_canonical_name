using System;
using System.Diagnostics;


/// <summary>
/// Summary description for Class1
/// </summary>
interface ILogger
{

    void Debug(string text);
    void Warn(string text);
    void Error(string text);
    void Error(string text, Exception ex);

}