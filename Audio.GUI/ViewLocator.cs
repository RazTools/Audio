using Audio.GUI.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace Audio.GUI;
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param != null)
        {
            string name = param.GetType().FullName!.Replace("ViewModel", "View");
            Type? type = Type.GetType(name);

            if (type != null)
            {
                return (Control?)Activator.CreateInstance(type);
            }
        }

        return null;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}